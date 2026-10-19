# UserManagementAPI

A sample ASP.NET Core Web API for managing users. It targets .NET 10, stores user data in memory, protects `/api` endpoints with a development Bearer token, and includes middleware for request logging and unhandled errors.

## Prerequisites

- .NET 10 SDK
- Optional: Visual Studio Code with the [REST Client extension](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) for running `UserManagementAPI.http`

Check that the SDK is installed:

```bash
dotnet --version
```

## Run the API

From the repository root, run:

```bash
dotnet run --project UserManagementAPI.csproj --launch-profile http
```

The HTTP profile listens at `http://localhost:5126`. It sets the Development environment, which loads the local token from `appsettings.Development.json`.

Interactive API documentation (Scalar):

- UI: <http://localhost:5126/scalar/v1>
- OpenAPI JSON: <http://localhost:5126/openapi/v1.json>

Scalar and OpenAPI are enabled only in the Development environment.

## Authentication

All `/api` endpoints require this header:

```http
Authorization: Bearer local-development-token-user-management-2026
```

The example token in `appsettings.Development.json` is for local testing only. Do not reuse it in shared, staging, or production environments. Configure a secret of at least 32 characters through the `Auth__ApiToken` environment variable instead:

```bash
# PowerShell
$env:Auth__ApiToken = "replace-with-a-private-token-of-at-least-32-characters"

# Bash
export Auth__ApiToken="replace-with-a-private-token-of-at-least-32-characters"
```

Set the variable before starting the application. A missing or too-short token causes startup validation to fail. Requests without a valid token receive `401 Unauthorized`.

## API endpoints

| Method | Route | Success response |
| --- | --- | --- |
| `GET` | `/api/users` | `200 OK` with the user list |
| `GET` | `/api/users/{id}` | `200 OK`, or `404 Not Found` |
| `POST` | `/api/users` | `201 Created` with a `Location` header |
| `PUT` | `/api/users/{id}` | `200 OK`, or `404 Not Found` |
| `DELETE` | `/api/users/{id}` | `204 No Content`, or `404 Not Found` |

Create and update requests use this JSON shape:

```json
{
  "name": "An Nguyen",
  "email": "an.nguyen@example.com",
  "department": "Engineering"
}
```

Invalid input, such as a malformed email, returns `400 Bad Request`.

## Run the HTTP requests manually

1. Start the API using the command in [Run the API](#run-the-api).
2. Open `UserManagementAPI.http` in VS Code.
3. Install and enable the REST Client extension if the **Send Request** links are not shown.
4. Send `createUser` first. Then send `getCreatedUser`, `updateCreatedUser`, and `deleteCreatedUser`; these requests use the ID returned by the create request.
5. `getDeletedUser` checks that the deleted user returns `404`. `getNonexistentUser` is another missing-user example.

The HTTP file uses the development token configured in `appsettings.Development.json`. If you override the token with an environment variable, update the HTTP file's `@apiToken` value to match.

## Run the automated tests

From the repository root:

```bash
dotnet test tests/UserManagementAPI.Tests/UserManagementAPI.Tests.csproj
```

The xUnit integration tests use ASP.NET Core's in-memory test server and a test-only token supplied by the test fixture. They cover the CRUD flow, invalid email validation, missing-token authentication, and the generic JSON error response from the exception middleware. They do not require the API to be started separately.

## Middleware behavior

- Unhandled exceptions are logged server-side and return `500 Internal Server Error` with a generic JSON body: `{"error":"Internal server error."}`.
- Requests to `/api` require a valid Bearer token.
- Request logs include the HTTP method, path, and response status for requests that reach the logging middleware. Authentication rejections are logged by the authentication middleware.

## In-memory data

Users are stored in memory and are not persisted. The list is empty again after the application restarts.
