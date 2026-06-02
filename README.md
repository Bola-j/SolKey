# SolKey

SolKey is a .NET Web API for an education platform where students ask questions, teachers answer and upload solution/session videos, and admins manage curriculum, content approval, payments, and access.

The project follows a layered architecture:

- `SolKey.API`: ASP.NET Core controllers, middleware, Swagger, authentication, dependency injection.
- `SolKey.Application`: DTOs, validators, service interfaces, shared response models.
- `SolKey.Domain`: entities, enums, and domain base types.
- `SolKey.Infrastructure`: EF Core persistence, migrations, seed data, identity helpers, storage, email, and application service implementations.

## Current Feature Set

- JWT authentication with refresh tokens, logout, email verification, and resend cooldown.
- Roles: `Student`, `Teacher`, `Admin`.
- Student profile read/update.
- Curriculum management for books, chapters, and lessons.
- Questions and answers:
  - verified students can ask questions.
  - verified teachers can answer.
  - authenticated users can upvote/downvote answers.
  - votes persist only aggregate counts (`Upvotes`, `Downvotes`); voter identity is not stored.
- Explanation sessions:
  - teachers create sessions.
  - admins approve sessions indirectly through admin/content workflows.
  - public users list approved sessions.
  - students can receive session access through approved payments.
- QA subscriptions:
  - students can check subscription status.
  - approved QA subscription payments grant or extend access by 1 month.
- Temporary payment flow:
  - students submit payment proof screenshots.
  - admins manually approve payments until third-party payment provider integration is finished.
  - approval grants either QA subscription access or session access depending on `PaymentType`.
- Video upload and secure playback:
  - upload tickets and presigned S3-compatible URLs.
  - teacher video registration.
  - secure signed read URLs gated by approval, subscription, or session purchase.
- Tag management.
- Serilog request logging.
- Central exception handling with response envelopes.
- Mailpit support for local development email.

## Tech Stack

- .NET 10 / ASP.NET Core
- Entity Framework Core 10
- PostgreSQL via Npgsql
- JWT Bearer authentication
- FluentValidation
- Swagger / OpenAPI
- Serilog
- AWS S3 SDK, configured for S3-compatible storage such as Cloudflare R2
- Mailpit for local SMTP testing

## Repository Layout

```text
src/
  SolKey.API/
    Controllers/
    Middleware/
    Program.cs
  SolKey.Application/
    DTOs/
    Interfaces/
    Validators/
  SolKey.Domain/
    Entities/
    Enums/
  SolKey.Infrastructure/
    Identity/
    Migrations/
    Persistence/
    Services/
```

Useful project files:

- `SolKey.slnx`: solution file.
- `docker-compose.yml`: local Mailpit service.
- `solkey_user_stories_and_actors.md`: current actors, permissions, and user stories.
- `plan.md`: delivery/milestone notes.
- `src/SolKey.API/SolKey.API.http`: HTTP scratch file.

## Getting Started

### Prerequisites

- .NET SDK 10.x
- PostgreSQL database
- Docker, optional but recommended for Mailpit
- EF Core CLI tools, if you need to create or apply migrations:

```bash
dotnet tool install --global dotnet-ef
```

### Restore And Build

```bash
dotnet restore SolKey.slnx
dotnet build SolKey.slnx
```

### Configure The API

Configuration is read from `appsettings.json`, `appsettings.Development.json`, environment variables, or user secrets. Do not commit production secrets.

Required configuration keys:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=solkey;Username=postgres;Password=your-password"
  },
  "Jwt": {
    "Issuer": "SolKey",
    "Audience": "SolKey",
    "SigningKey": "replace-with-a-long-secret",
    "AccessTokenMinutes": 20,
    "RefreshTokenDays": 7
  },
  "Storage": {
    "BucketName": "solkey",
    "ServiceUrl": "https://your-s3-compatible-endpoint",
    "AccessKey": "your-access-key",
    "SecretKey": "your-secret-key"
  },
  "Email": {
    "FromEmail": "no-reply@solkey.dev",
    "FromName": "SolKey",
    "VerificationBaseUrl": "https://localhost:7284",
    "TokenExpiryHours": 24,
    "ResendCooldownMinutes": 60,
    "Smtp": {
      "Host": "localhost",
      "Port": 1025,
      "Username": "",
      "Password": "",
      "UseSsl": false
    }
  }
}
```

Environment variable examples:

```bash
ConnectionStrings__DefaultConnection="Host=localhost;Database=solkey;Username=postgres;Password=your-password"
Jwt__SigningKey="replace-with-a-long-secret"
Storage__BucketName="solkey"
Storage__ServiceUrl="https://your-s3-compatible-endpoint"
Storage__AccessKey="your-access-key"
Storage__SecretKey="your-secret-key"
```

### Database Migrations

Apply migrations:

```bash
dotnet ef database update --project src/SolKey.Infrastructure --startup-project src/SolKey.API
```

Create a new migration:

```bash
dotnet ef migrations add MigrationName --project src/SolKey.Infrastructure --startup-project src/SolKey.API --output-dir Migrations
```

Current migrations include the initial schema, email verification tokens, upload tickets, and the payment-to-session link used to grant session access after payment approval.

### Development Email With Mailpit

Start Mailpit:

```bash
docker compose up -d mailpit
```

Open the inbox UI:

```text
http://localhost:8025
```

SMTP defaults for development:

```text
Host: localhost
Port: 1025
SSL: false
```

### Run The API

```bash
dotnet run --project src/SolKey.API
```

Swagger UI is enabled at the application root in development and normal runs:

```text
https://localhost:{port}/
```

## API Overview

Authentication and identity:

- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/auth/refresh`
- `POST /api/auth/logout`
- `GET /api/auth/verify-email`
- `POST /api/auth/resend-verification`
- `GET /api/auth/test-auth`
- `GET /api/auth/whoami`

User profile:

- `GET /api/user/profile`
- `PUT /api/user/profile`

Curriculum:

- `GET /api/books`
- `GET /api/books/{id}`
- `POST /api/books` admin
- `PUT /api/books/{id}` admin
- `DELETE /api/books/{id}` admin
- `GET /api/chapters/book/{bookId}`
- `GET /api/chapters/{id}`
- `POST /api/chapters` admin
- `PUT /api/chapters/{id}` admin
- `DELETE /api/chapters/{id}` admin
- `GET /api/lessons/chapter/{chapterId}`
- `GET /api/lessons/{id}`
- `POST /api/lessons` admin
- `PUT /api/lessons/{id}` admin
- `DELETE /api/lessons/{id}` admin

Questions and answers:

- `POST /api/questions/ask` authenticated student
- `POST /api/questions/answer` teacher
- `POST /api/questions/vote` authenticated user

Sessions:

- `GET /api/sessions`
- `POST /api/sessions` teacher
- `POST /api/sessions/purchase` student

Subscriptions and payments:

- `GET /api/subscription/status` student
- `POST /api/subscription/payment` student

Uploads and videos:

- `POST /api/uploads/presign` authenticated user
- `POST /api/videos/upload` teacher
- `GET /api/videos/{id}/secure-url` authenticated user

Tags:

- `GET /api/tags`
- `POST /api/tags` admin
- `PUT /api/tags/{id}` admin
- `DELETE /api/tags/{id}` admin

Admin:

- `POST /api/admin/verify-teacher/{teacherId}`
- `POST /api/admin/approve-video/{videoId}`
- `POST /api/admin/approve-payment/{paymentId}`

## Payment And Access Rules

`PaymentType` values:

- `QaSubscription`: admin approval creates or extends an active subscription by 1 month.
- `SessionPurchase`: payment must include `SessionId`; admin approval creates a `SessionPurchase` for the approved session.

The manual admin approval step is temporary and exists only until the third-party payment provider integration is completed.

Access checks:

- Free approved videos can be viewed by authenticated users.
- Premium question solution videos require an active QA subscription.
- Premium session videos require an active session purchase.
- Unapproved videos are not accessible.

## Voting Rules

Answer voting stores aggregate counts only:

- `Upvotes`
- `Downvotes`

The system does not persist who voted or how each user voted. A short in-memory cooldown key prevents rapid repeat votes on the same answer by the same authenticated user.

## Validation And Error Handling

- Request validation uses FluentValidation.
- Controller responses are wrapped in `ResponseEnvelope<T>`.
- Paginated responses use `PagedResponse<T>`.
- Unhandled exceptions flow through `ExceptionHandlingMiddleware`.

## Seed Data

The infrastructure project seeds sample data for:

- one student, teacher, and admin
- a book, chapter, lesson, question, answer, and tag
- an approved explanation session
- a sample session purchase
- a sample video
- a sample subscription and payment

Seed password hashes are placeholders and are intended for development/demo data only.

## Verification

Useful checks before committing:

```bash
dotnet build SolKey.slnx
dotnet ef migrations list --project src/SolKey.Infrastructure --startup-project src/SolKey.API
```
