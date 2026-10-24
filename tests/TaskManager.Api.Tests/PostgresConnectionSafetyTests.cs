using Xunit;

namespace TaskManager.Api.Tests;

public sealed class PostgresConnectionSafetyTests
{
    [Theory]
    [InlineData("Host=localhost;Database=TaskManagerDb;Username=test;Password=secret")]
    [InlineData("Host=localhost;Database=postgres;Username=test;Password=secret")]
    [InlineData("Host=db.example.com;Database=TaskManagerTestDb;Username=test;Password=secret")]
    public void RejectsNonDedicatedOrNonLocalTargets(string connectionString)
    {
        var exception = Assert.Throws<InvalidOperationException>(() => DedicatedTestDatabase.Validate(connectionString));
        Assert.DoesNotContain("secret", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("Host=localhost;Database=TaskManagerTestDb;Username=test;Password=secret")]
    [InlineData("Host=127.0.0.1;Database=TaskManagerTestDb;Username=test;Password=secret")]
    [InlineData("Host=::1;Database=TaskManagerTestDb;Username=test;Password=secret")]
    public void AcceptsOnlyDedicatedLocalTestDatabase(string connectionString)
    {
        Assert.Equal(connectionString, DedicatedTestDatabase.Validate(connectionString));
    }

    [Fact]
    public void RejectsMalformedConnectionWithoutEchoingIt()
    {
        const string secret = "Password=secret;not-a-pair";
        var exception = Assert.Throws<InvalidOperationException>(() => DedicatedTestDatabase.Validate(secret));
        Assert.DoesNotContain("secret", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
}
