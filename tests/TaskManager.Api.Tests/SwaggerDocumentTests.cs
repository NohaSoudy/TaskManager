using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace TaskManager.Api.Tests;

public sealed class SwaggerDocumentTests
{
    private const string ConnectionSecretSentinel = "SwaggerSecretSentinel-DoNotPublish";

    [Fact]
    public async Task DevelopmentSwaggerDocumentsTaskRoutesContractsAndStatusesWithoutSecrets()
    {
        await using var factory = new SwaggerApiFactory(ConnectionSecretSentinel);
        using var client = factory.CreateClient();

        using var uiResponse = await client.GetAsync("/swagger/index.html");
        Assert.Equal(HttpStatusCode.OK, uiResponse.StatusCode);
        Assert.Contains("swagger-ui", await uiResponse.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);

        using var response = await client.GetAsync("/swagger/v1/swagger.json");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = document.RootElement;
        var paths = root.GetProperty("paths");

        AssertPath(paths, "/api/tasks", "get", "post");
        AssertPath(paths, "/api/tasks/{id}", "get", "put", "delete");
        AssertPath(paths, "/api/tasks/{id}/status", "patch");

        var schemas = root.GetProperty("components").GetProperty("schemas");
        Assert.True(schemas.TryGetProperty("CreateTaskRequest", out _));
        Assert.True(schemas.TryGetProperty("UpdateTaskRequest", out _));
        Assert.True(schemas.TryGetProperty("ChangeTaskStateRequest", out _));
        Assert.True(schemas.TryGetProperty("TaskResponse", out _));
        AssertStatus(paths.GetProperty("/api/tasks").GetProperty("post"), "201", "400");
        AssertStatus(paths.GetProperty("/api/tasks/{id}").GetProperty("get"), "200", "404");
        AssertStatus(paths.GetProperty("/api/tasks/{id}").GetProperty("put"), "200", "400", "404");
        AssertStatus(paths.GetProperty("/api/tasks/{id}/status").GetProperty("patch"), "200", "400", "404");
        AssertStatus(paths.GetProperty("/api/tasks/{id}").GetProperty("delete"), "204", "404");
        Assert.Contains("ValidationProblemDetails", schemas.EnumerateObject().Select(x => x.Name));
        Assert.DoesNotContain(ConnectionSecretSentinel, response.Content.Headers.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain(ConnectionSecretSentinel, root.ToString(), StringComparison.Ordinal);
    }

    private static void AssertPath(JsonElement paths, string path, params string[] methods)
    {
        Assert.True(paths.TryGetProperty(path, out var operations), $"Missing OpenAPI path {path}.");
        foreach (var method in methods)
            Assert.True(operations.TryGetProperty(method, out _), $"Missing {method.ToUpperInvariant()} operation for {path}.");
    }

    private static void AssertStatus(JsonElement operation, params string[] expectedCodes)
    {
        var responses = operation.GetProperty("responses");
        foreach (var code in expectedCodes)
            Assert.True(responses.TryGetProperty(code, out _), $"Missing response status {code}.");
        Assert.True(responses.TryGetProperty("500", out _));
    }

    private sealed class SwaggerApiFactory(string connectionSecret) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.UseSetting("ConnectionStrings:TaskManager", $"Host=localhost;Database=SwaggerDocsOnly;Username=docs;Password={connectionSecret}");
        }
    }
}
