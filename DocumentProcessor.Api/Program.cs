using DocumentProcessor.Api.Middleware;
using DocumentProcessor.Application.Interfaces;
using DocumentProcessor.Application.UseCases;
using DocumentProcessor.Infrastructure.Persistence;
using DocumentProcessor.Infrastructure.Services;
using DocumentProcessor.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// =========================================================
// 1. SERVICE REGISTRATION (Before builder.Build())
// =========================================================

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS configuration for React dev servers (ports 5173 and 5174)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Database Context Configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)
              .EnableRetryOnFailure(
                  maxRetryCount: 5,
                  maxRetryDelay: TimeSpan.FromSeconds(10),
                  errorNumbersToAdd: null
              )
    )
);

// Infrastructure Services
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddTransient<IFileStorageService, LocalFileStorageService>();
builder.Services.AddTransient<ITextExtractorService, PdfPigExtractorService>();
builder.Services.AddHttpClient<IAiService, GeminiService>();

// Application Use Cases
builder.Services.AddScoped<ProcessDocumentUseCase>();
builder.Services.AddScoped<DeleteDocumentUseCase>();
builder.Services.AddScoped<GetDocumentStatusUseCase>();
builder.Services.AddScoped<GetExtractedDataUseCase>();
builder.Services.AddScoped<GetDocumentByIdUseCase>();
builder.Services.AddScoped<GetAllDocumentsUseCase>();

// =========================================================
// 2. BUILD APPLICATION
// =========================================================

var app = builder.Build();

// =========================================================
// 3. MIDDLEWARE PIPELINE
// =========================================================

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enable CORS Middleware (Must be invoked before MapControllers)
app.UseCors("AllowReactApp");

app.UseAuthorization();
app.MapControllers();

// Apply automatic database migrations on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

app.Run();