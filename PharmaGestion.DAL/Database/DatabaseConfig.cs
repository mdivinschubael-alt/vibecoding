using Microsoft.Data.Sqlite;

namespace PharmaGestion.DAL.Database;

public class DatabaseConfig
{
    private readonly string _connectionString;
    public string DatabasePath { get; }

    public DatabaseConfig(string? customDbPath = null)
    {
        if (string.IsNullOrWhiteSpace(customDbPath))
        {
            // Emplacement par défaut dans le dossier AppData local pour garantir l'accès en écriture
            string appDataDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "PharmaGestion");

            Directory.CreateDirectory(appDataDir);
            DatabasePath = Path.Combine(appDataDir, "pharmacie.db");
        }
        else
        {
            DatabasePath = customDbPath;
            var directory = Path.GetDirectoryName(DatabasePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = DatabasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            ForeignKeys = true,
            Cache = SqliteCacheMode.Shared
        };

        _connectionString = builder.ConnectionString;
    }

    public SqliteConnection CreateConnection()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Active explicitement les contraintes de clés étrangères et le mode WAL pour de meilleures performances
        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA foreign_keys = ON; PRAGMA journal_mode = WAL;";
        command.ExecuteNonQuery();

        return connection;
    }
}
