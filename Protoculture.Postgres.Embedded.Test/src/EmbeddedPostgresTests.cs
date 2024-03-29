using System.IO;
using FluentAssertions;
using Npgsql;
using Xunit;

namespace Protoculture.Postgres.Embedded.Test;

public class EmbeddedPostgresTests
{
    [Fact]
    public async void ItStarts()
    {
        await using var server = new EmbeddedPostgres();

        await server.Start();

        server.Running.Should().BeTrue();
    }

    [Fact]
    public async void ItStops()
    {
        var server = new EmbeddedPostgres();

        await server.Start();
        await server.Stop();

        server.Running.Should().BeFalse();
    }
    
    [Fact]
    public async void ItCanBeReached()
    {
        await using var server = new EmbeddedPostgres();
        
        var connectionString = server.Configuration.SupportsSockets
            ? server.Configuration.SocketConnectionString
            : server.Configuration.TcpConnectionString;

        await using var connection = new NpgsqlConnection(connectionString);
        await using var command = new NpgsqlCommand("select true", connection);

        await server.Start();
        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();

        result.Should().BeOfType<bool>().Subject.Should().BeTrue();
    }

    [Fact]
    public async void ItShouldCleanUpAfterItselfWhenTransient()
    {
        string basePath;
        
        await using (var server = new EmbeddedPostgres())
        {
            await server.Start();

            basePath = server.Configuration.BasePath;
        }

        Directory.Exists(basePath).Should().BeFalse();
    }
}
