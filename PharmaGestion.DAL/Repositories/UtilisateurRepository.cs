using System.Data;
using Microsoft.Data.Sqlite;
using PharmaGestion.Core.Models;
using PharmaGestion.DAL.Database;

namespace PharmaGestion.DAL.Repositories;

public class UtilisateurRepository : IUtilisateurRepository
{
    private readonly DatabaseConfig _dbConfig;

    public UtilisateurRepository(DatabaseConfig dbConfig)
    {
        _dbConfig = dbConfig;
    }

    public Utilisateur? GetById(int id)
    {
        using var connection = _dbConfig.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT id, nom, identifiant, mot_de_passe_hash, role, date_creation FROM utilisateurs WHERE id = @id";
        command.Parameters.AddWithValue("@id", id);

        using var reader = command.ExecuteReader();
        return reader.Read() ? MapReader(reader) : null;
    }

    public Utilisateur? GetByIdentifiant(string identifiant)
    {
        using var connection = _dbConfig.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT id, nom, identifiant, mot_de_passe_hash, role, date_creation FROM utilisateurs WHERE LOWER(identifiant) = LOWER(@identifiant)";
        command.Parameters.AddWithValue("@identifiant", identifiant.Trim());

        using var reader = command.ExecuteReader();
        return reader.Read() ? MapReader(reader) : null;
    }

    public List<Utilisateur> GetAll()
    {
        var list = new List<Utilisateur>();
        using var connection = _dbConfig.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT id, nom, identifiant, mot_de_passe_hash, role, date_creation FROM utilisateurs ORDER BY nom ASC";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            list.Add(MapReader(reader));
        }

        return list;
    }

    public int Insert(Utilisateur utilisateur)
    {
        using var connection = _dbConfig.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO utilisateurs (nom, identifiant, mot_de_passe_hash, role, date_creation)
            VALUES (@nom, @identifiant, @mot_de_passe_hash, @role, @date_creation);
            SELECT last_insert_rowid();";

        command.AddSafeParameter("@nom", utilisateur.Nom);
        command.AddSafeParameter("@identifiant", utilisateur.Identifiant.Trim());
        command.AddSafeParameter("@mot_de_passe_hash", utilisateur.MotDePasseHash);
        command.AddSafeParameter("@role", utilisateur.Role);
        command.AddSafeParameter("@date_creation", utilisateur.DateCreation.ToString("o"));

        long newId = (long)(command.ExecuteScalar() ?? 0L);
        utilisateur.Id = (int)newId;
        return utilisateur.Id;
    }

    public bool Update(Utilisateur utilisateur)
    {
        using var connection = _dbConfig.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE utilisateurs 
            SET nom = @nom, identifiant = @identifiant, role = @role
            WHERE id = @id";

        command.AddSafeParameter("@nom", utilisateur.Nom);
        command.AddSafeParameter("@identifiant", utilisateur.Identifiant.Trim());
        command.AddSafeParameter("@role", utilisateur.Role);
        command.Parameters.AddWithValue("@id", utilisateur.Id);

        return command.ExecuteNonQuery() > 0;
    }

    public bool UpdateMotDePasse(int id, string motDePasseHash)
    {
        using var connection = _dbConfig.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE utilisateurs SET mot_de_passe_hash = @hash WHERE id = @id";
        command.AddSafeParameter("@hash", motDePasseHash);
        command.Parameters.AddWithValue("@id", id);

        return command.ExecuteNonQuery() > 0;
    }

    public bool Delete(int id)
    {
        using var connection = _dbConfig.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM utilisateurs WHERE id = @id";
        command.Parameters.AddWithValue("@id", id);

        return command.ExecuteNonQuery() > 0;
    }

    public int Count()
    {
        using var connection = _dbConfig.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM utilisateurs";
        return Convert.ToInt32(command.ExecuteScalar());
    }

    private static Utilisateur MapReader(SqliteDataReader reader)
    {
        return new Utilisateur
        {
            Id = reader.GetSafeInt32("id"),
            Nom = reader.GetSafeString("nom"),
            Identifiant = reader.GetSafeString("identifiant"),
            MotDePasseHash = reader.GetSafeString("mot_de_passe_hash"),
            Role = reader.GetSafeString("role"),
            DateCreation = reader.GetSafeDateTime("date_creation")
        };
    }
}
