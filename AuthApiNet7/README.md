# Studio Medico Auth API (.NET 7)

Web API with **JWT** auth, **EF Core + PostgreSQL**, **CORS** for Vite/React on `http://localhost:5173`.

## Prerequisites

- [.NET 7 SDK](https://dotnet.microsoft.com/download/dotnet/7.0)
- PostgreSQL (local or Docker)
- `dotnet-ef` tool: `dotnet tool install --global dotnet-ef`

## Configuration

`appsettings.json` includes the example connection string:

`Host=localhost;Port=5432;Database=studio_medico_auth;Username=postgres;Password=postgres`

Set **`Jwt:SecretKey`** to a random string **≥ 32 characters** before production.

## Database

From this folder (`AuthApiNet7`):

```bash
dotnet ef migrations add InitialCreate --context AppDbContext
dotnet ef database update --context AppDbContext
```

## Run

```bash
dotnet run
```

- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger (Development): `/swagger`

## Endpoints

| Method | Path | Auth |
|--------|------|------|
| POST | `/auth/register` | No |
| POST | `/auth/login` | No |
| GET | `/profile/me` | Yes (Bearer JWT) |

### Register body

```json
{
  "email": "user@example.com",
  "password": "yourpassword",
  "role": "User"
}
```

Allowed `role` values: `User`, `Admin`, `Doctor`, `Secretary` (case-insensitive).

### Login body

```json
{
  "email": "user@example.com",
  "password": "yourpassword"
}
```

Response: `{ "token": "..." }`

### Frontend (Vite on port 5173)

Point API calls to `http://localhost:5000` (avoids dev certificate issues) and send:

`Authorization: Bearer <token>`

Example:

```ts
await fetch('http://localhost:5000/auth/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ email, password }),
});
```

## Error responses

JSON shape: `{ "message": "...", "status": 400 }`

- **401** – invalid login
- **409** – email already registered (or unique constraint)
- **400** – invalid role / validation
- **500** – unexpected (details only in Development)
