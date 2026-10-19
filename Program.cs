var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Pipeline order matters: each middleware wraps the ones registered after it.
// 1. Error handling runs outermost so it can catch exceptions from authentication, logging, and the controllers.
app.UseMiddleware<UserManagementAPI.Middleware.ExceptionHandlingMiddleware>();
// 2. Authentication runs next so unauthenticated requests are rejected before any business logic executes.
app.UseMiddleware<UserManagementAPI.Middleware.TokenAuthenticationMiddleware>();
// 3. Logging runs last (closest to the endpoint) so it only logs requests that passed authentication.
app.UseMiddleware<UserManagementAPI.Middleware.RequestLoggingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
