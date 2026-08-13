using DocumentProcessor.Api.Middleware;
using DocumentProcessor.Application.Interfaces;
using DocumentProcessor.Application.UseCases;
using DocumentProcessor.Infrastructure.Persistence; 
using DocumentProcessor.Infrastructure.Services;   
using DocumentProcessor.Infrastructure.Persistence.Repositories;


using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Controllers, Exception Handling & OpenAPI/Swagger Setup
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// 2. Database Context Registration
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

// 3. Infrastructure Services Registration
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddTransient<IFileStorageService, LocalFileStorageService>();
builder.Services.AddTransient<ITextExtractorService, PdfPigExtractorService>();

// Typed HttpClient for AI Service (Gemini Implementation)
builder.Services.AddHttpClient<IAiService, GeminiService>();

// 4. Application Use Cases Registration
builder.Services.AddScoped<ProcessDocumentUseCase>();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

app.Run();
