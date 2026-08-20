# Document Processor AI

[![.NET 8.0](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-18-61DAFB?logo=react)](https://react.dev/)
[![Tailwind CSS](https://img.shields.io/badge/Tailwind-3.0-38BDF8?logo=tailwindcss)](https://tailwindcss.com/)
[![Clean Architecture](https://img.shields.io/badge/Architecture-Clean%20Architecture-blue)](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

An enterprise-ready Full-Stack application designed for asynchronous document ingestion, text extraction, and automated metadata parsing using **Google Gemini AI**. 

Built following **Clean Architecture**, **SOLID principles**, and **Domain-Driven Design (DDD)** concepts on the backend, paired with a modern dark-themed **React SPA** on the frontend.

---

## Architecture Overview

The project is structured as a decoupled monorepo containing a **Clean Architecture C# Backend** and a **React Single Page Application (SPA)**:

```text
DocumentProcessor/
├── client/                                    # Frontend (React + Vite + Tailwind)
│   ├── src/
│   │   ├── api/                               # Axios Client & Document Endpoints
│   │   ├── types/                             # TypeScript DTOs & Interfaces
│   │   ├── App.tsx                            # Router, Views & UI Components
│   │   └── main.tsx                           # Application Entrypoint
│   ├── package.json
│   └── vite.config.ts
│
├── src/                                       # Backend (.NET 8 Clean Architecture)
│   ├── Core/
│   │   ├── DocumentProcessor.Domain/          # Enterprise Business Rules & Entities
│   │   └── DocumentProcessor.Application/     # Use Cases, Interfaces, DTOs & CQRS Logic
│   │
│   └── Infrastructure/
│       ├── DocumentProcessor.Infrastructure/  # EF Core, Gemini AI Integration & Storage
│       └── DocumentProcessor.Api/             # Controllers, Middlewares, Swagger UI
│
└── tests/
    └── DocumentProcessor.UnitTests/           # Unit & Integration Tests (xUnit + Moq)
        ├── Application/
        ├── Domain/
        └── Infrastructure/
```

### Layer Responsibilities
- **Frontend (Client):** Modern dashboard built with React, React Router DOM, Tailwind CSS, and Lucide Icons. Handles drag-and-drop file upload, custom delete confirmation modals, and JSON payload inspect views.
- **Domain Layer:** Pure C# domain entities, enums, and value objects without external framework dependencies.
- **Application Layer:** Orchestrates business logic, use cases, interfaces (`IGeminiService`, `IDocumentRepository`), and validation.
- **Infrastructure Layer:** Concrete implementations for database persistence (Entity Framework Core), file storage, and **Google Gemini AI API** integration (`GoogleGenAI`).
- **API Layer:** ASP.NET Core Web API with global exception handling, dependency injection wiring, CORS configuration, and Swagger/OpenAPI documentation.

---

## Tech Stack & Dependencies

### Backend
* **Framework:** .NET 8.0 (C#)
* **Architecture:** Clean Architecture + Repository Pattern
* **Database & ORM:** SQL Server + Entity Framework Core 8
* **AI Engine:** Official `GoogleGenAI` SDK (Gemini AI Structured Outputs)
* **Logging:** `Serilog` (Structured JSON logging)
* **Testing:** `xUnit`, `Moq`, `FluentAssertions`
* **API Documentation:** Swagger UI / OpenAPI

### Frontend
* **UI Framework:** React 18 + TypeScript
* **Build Tool:** Vite
* **Styling:** Tailwind CSS (Dark Mode Design System)
* **Icons:** Lucide React
* **HTTP Client:** Axios
* **Routing:** React Router DOM v6

---

## Key Features

* **Asynchronous File Processing:** Seamless PDF upload with real-time feedback and processing status (`Pending`, `Processing`, `Completed`, `Failed`).
* **AI-Driven Data Extraction:** Leverages **Google Gemini AI** to structure unstructured document text into structured JSON schema outputs:
  * Document Category (*Invoice, Receipt, Contract, Tax Form*)
  * Vendor / Issuer Name
  * Tax ID (*RNC / EIN / VAT*)
  * Total Amount & Currency
  * Issue Date & AI Confidence Score
* **Interactive Document Dashboard:** Visual table displaying uploaded files, processing badges, date filters, and custom deletion confirmation dialogs.
* **JSON Payload Inspector:** View Gemini AI extracted raw and structured JSON responses per document.
* **Resilience & Auditing:** Comprehensive audit logs, database persistence via EF Core, global exception handling, and full unit test coverage.

---

## API Endpoints Summary

| Method | Endpoint | Description |
| :--- | :--- | :--- |
| `POST` | `/api/v1/documents/upload` | Upload a PDF document for processing |
| `GET` | `/api/v1/documents/{id}/status` | Check the current processing status |
| `GET` | `/api/v1/documents/{id}/extracted-data` | Retrieve structured AI-extracted metadata |
| `GET` | `/api/v1/documents` | Fetch all processed documents |
| `GET` | `/api/v1/documents/{id}` | Fetch document details by ID |
| `DELETE`| `/api/v1/documents/{id}` | Remove document and associated metadata |

---

## Getting Started

### Prerequisites
* [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
* [Node.js](https://nodejs.org/) (v18.x or higher)
* [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) (v17.8+) or VS Code
* [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (Express, LocalDB, or Docker container)
* [Google Gemini API Key](https://aistudio.google.com/)

---

### Local Setup Instructions

#### 1. Clone the Repository

```text
git clone https://github.com/EddyDeOleo/DocumentProcessor.git
cd DocumentProcessor
```

#### 2. Backend Setup (.NET 8 API)

1. Open `src/Infrastructure/DocumentProcessor.Api/appsettings.json` and configure your Database Connection String and Gemini API Key:

```text
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=DocumentProcessorDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "Gemini": {
    "ApiKey": "YOUR_GEMINI_API_KEY_HERE"
  }
}
```

2. Apply Entity Framework Database Migrations:

```text
dotnet ef database update --project src/Infrastructure/DocumentProcessor.Infrastructure --startup-project src/Infrastructure/DocumentProcessor.Api
```

3. Run the API:

```text
dotnet run --project src/Infrastructure/DocumentProcessor.Api
```

> The API will start on `https://localhost:7123`. You can inspect endpoints via Swagger at `https://localhost:7123/swagger`.

---

#### 3. Frontend Setup (React SPA)

1. Navigate to the frontend directory:

```text
cd client
```

2. Install dependencies:

```text
npm install
```

3. Create a `.env` file in the `client/` root directory:

```text
VITE_API_BASE_URL=https://localhost:7123/api/v1
```

4. Start the Vite development server:

```text
npm run dev
```

> Open your browser at `http://localhost:5173` to interact with the application.

---

## Running Tests

To run the backend unit test suite across domain, application, and infrastructure layers:

```text
dotnet test
```

Or open **Test Explorer** in Visual Studio (`Ctrl + E, T`) and click **Run All Tests**.

---

## License

This project is licensed under the MIT License - see the LICENSE file for details.
