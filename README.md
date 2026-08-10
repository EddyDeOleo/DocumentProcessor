# AI-Powered Document Ingestion & Processing API

[![.NET 8.0](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Clean Architecture](https://img.shields.io/badge/Architecture-Clean%20Architecture-blue)](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

An enterprise-ready **C# .NET 8 REST API** designed for asynchronous document ingestion, text extraction, and automated metadata parsing using the official **OpenAI SDK**. 

Built following **Clean Architecture**, **SOLID principles**, and **Domain-Driven Design (DDD)** concepts to ensure maintainability, scalability, and high testability.

---

## Architecture Overview

The project is structured into 4 decoupled layers enforcing strict dependency rules:

```text
DocumentProcessor/
├── src/
│   ├── Core/
│   │   ├── DocumentProcessor.Domain/          # Enterprise Business Rules & Entities
│   │   └── DocumentProcessor.Application/     # Use Cases, Interfaces, DTOs & CQRS Logic
│   │
│   └── Infrastructure/
│       ├── DocumentProcessor.Infrastructure/  # EF Core, OpenAI API, PDF Reader & Storage
│       └── DocumentProcessor.Api/             # Controllers, Middlewares, Swagger UI
│
└── tests/
    └── DocumentProcessor.UnitTests/           # Unit & Integration Tests (xUnit + Moq)
        ├── Application/
        │   └── UseCases/
        ├── Domain/
        │   └── Entities/
        └── Infrastructure/
            └── Services/
```

- **Domain Layer:** Pure C# domain entities, enums, and value objects without external framework dependencies.
- **Application Layer:** Orchestrates business logic, use cases, interfaces (`IOpenAIService`, `IDocumentRepository`), and validation.
- **Infrastructure Layer:** Concrete implementations for database persistence (Entity Framework Core), file storage, PDF text extraction (`PdfPig`), and OpenAI API integration.
- **API Layer:** ASP.NET Core Web API with global exception handling, dependency injection wiring, and Swagger/OpenAPI documentation.

---

## Tech Stack & Dependencies

* **Framework:** .NET 8.0 (C#)
* **Architecture:** Clean Architecture + Repository Pattern
* **Database & ORM:** SQL Server + Entity Framework Core 8
* **AI Engine:** Official `OpenAI` C# SDK (GPT-4o Structured Outputs)
* **PDF Engine:** `PdfPig` (Lightweight text extraction)
* **Logging:** `Serilog` (Structured JSON logging)
* **Testing:** `xUnit`, `Moq`, `FluentAssertions`
* **API Documentation:** Swagger UI / OpenAPI

---

## Key Features

* **Asynchronous File Upload:** Handles PDF/Image document uploads with instant HTTP 202 status responses.
* **Text Extraction Pipeline:** Automated parsing of unstructured PDF content into raw text.
* **AI-Driven Data Extraction:** Leverages OpenAI API with JSON Schema enforcement to extract specific fields:
  * Document Category (Invoice, Receipt, Contract, Tax Form)
  * Vendor / Issuer Name
  * Tax ID (RNC / EIN / VAT)
  * Total Amount & Currency
  * Issue Date & AI Confidence Score
* **Resilience & Auditing:** Comprehensive audit logs, document status tracking (`Pending`, `Processing`, `Completed`, `Failed`), and global exception handling middleware.

---

## API Endpoints Summary

| Method | Endpoint | Description |
| :--- | :--- | :--- |
| `POST` | `/api/v1/documents/upload` | Upload a PDF/document for processing |
| `GET` | `/api/v1/documents/{id}/status` | Check the current processing status |
| `GET` | `/api/v1/documents/{id}/extracted-data` | Retrieve structured AI-extracted metadata |
| `GET` | `/api/v1/documents` | Fetch paginated list of processed documents |
| `DELETE`| `/api/v1/documents/{id}` | Remove document and associated metadata |

---

## Getting Started

### Prerequisites
* [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
* [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) (v17.8+) or VS Code
* [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (Express or LocalDB)
* OpenAI API Key

### Local Setup Instructions

1. **Clone the Repository:**
   
   git clone [https://github.com/EddyDeOleo/DocumentProcessor.git](https://github.com/EddyDeOleo/DocumentProcessor.git)
   cd DocumentProcessor


2. **Configure Environment Variables:**
Open src/Infrastructure/DocumentProcessor.Api/appsettings.json and set your database connection string and OpenAI API key:

```text
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=DocumentProcessorDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "OpenAI": {
    "ApiKey": "OPENAI-API-KEY-HERE"
  }
}
```

3. **Apply Database Migrations:**
In Visual Studio, open the Package Manager Console, set Default Project to DocumentProcessor.Infrastructure, and run:

Update-Database

4. **Run the Application:**

Set DocumentProcessor.Api as the Startup Project.

Press F5 or click Run.

Navigate to https://localhost:7123/swagger in your browser to test the endpoints interactively.


**Running Unit Tests**
To run the unit test suite across domain and application logic:

Open Test Explorer in Visual Studio (Ctrl + E, T).

Click Run All Tests.

Or run via CLI using:

dotnet test

