using Npgsql;

namespace EventProcessor.Consumer;

public class DbContext : IAsyncDisposable
{
    public NpgsqlDataSource DataSource { get; }
    public NpgsqlBatch Batch => DataSource.CreateBatch(); 

    public DbContext(string connectionString)
    {
        DataSource = NpgsqlDataSource.Create(connectionString);
        var initSql = File.ReadAllText("init.sql");

        using var tablesCommand = DataSource.CreateCommand(initSql);
        tablesCommand.ExecuteNonQuery();
    }

    public async ValueTask DisposeAsync()
    {
        await DataSource.DisposeAsync();
    }
}