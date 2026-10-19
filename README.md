# User Management API

A simple ASP.NET Core Web API for TechHive Solutions to manage internal user records (HR/IT).

## Endpoints

| Method | Route             | Description                       |
| ------ | ----------------- | --------------------------------- |
| GET    | `/api/users`      | Retrieve all users                |
| GET    | `/api/users/{id}` | Retrieve a single user by ID      |
| POST   | `/api/users`      | Create a new user                 |
| PUT    | `/api/users/{id}` | Update an existing user's details |
| DELETE | `/api/users/{id}` | Remove a user by ID               |

User model fields: `firstName`, `lastName`, `email`, `department` (`id` is server-generated).

Data is stored in-memory (`UsersController`) and resets whenever the app restarts.

## Middleware pipeline

All requests flow through three custom middleware components, registered in this order so each one can rely on the
guarantees of the one before it:

1. **`ExceptionHandlingMiddleware`** (outermost) — catches unhandled exceptions from every later middleware and
   controller, logs them, and returns a consistent `500` response: `{ "error": "Internal server error." }`.
2. **`TokenAuthenticationMiddleware`** — requires an `Authorization: Bearer <token>` header matching the configured
   `Authentication:ApiToken` value. Missing/invalid tokens get a `401` with `{ "error": "..." }` before any
   business logic runs. The OpenAPI document (`/openapi/*`) is excluded so tooling can still discover the API.
3. **`RequestLoggingMiddleware`** (innermost) — logs the HTTP method, path, and resulting status code for every
   authenticated request.

The API token is read from configuration (`Authentication:ApiToken`): set in [appsettings.Development.json](appsettings.Development.json)
for local development (`dev-secret-token`), and should be supplied via user secrets/environment variables/Key Vault
in production rather than committed to `appsettings.json`.

## Testing

Sample requests for all CRUD operations, including auth success/failure and validation edge cases, are in
[UserManagementAPI.http](UserManagementAPI.http). Run the project (`dotnet run`) and send the requests with the
VS Code REST Client extension, Postman, or `Invoke-RestMethod`/`curl`.

