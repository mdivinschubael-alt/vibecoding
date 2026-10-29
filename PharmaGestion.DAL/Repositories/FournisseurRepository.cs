using Microsoft.Data.Sqlite;
using PharmaGestion.Core.Models;
using PharmaGestion.DAL.Database;

namespace PharmaGestion.DAL.Repositories;

public class FournisseurRepository : IFournisseurRepository
{
    private readonly DatabaseConfig _dbConfig;

    public FournisseurRepository(DatabaseConfig dbConfig)
    {
        _dbConfig = dbConfig;
    }

    public Fournisseur? GetById(int id)
    {
        using var connection = _dbConfig.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT id, nom, telephone, adresse, email, date_creation FROM fournisseurs WHERE id = @id";
        command.Parameters.AddWithValue("@id", id);

        using var reader = command.ExecuteReader();
        return reader.Read() ? MapReader(reader) : null;
    }

    public List<Fournisseur> GetAll()
    {
        var list = new List<Fournisseur>();
        using var connection = _dbConfig.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT id, nom, telephone, adresse, email, date_creation FROM fournisseurs ORDER BY nom ASC";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            list.Add(MapReader(reader));
        }

        return list;
    }

    public List<Fournisseur> Search(string terme)
    {
        var list = new List<Fournisseur>();
        using var connection = _dbConfig.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT id, nom, telephone, adresse, email, date_creation 
            FROM fournisseurs 
            WHERE nom LIKE @t OR telephone LIKE @t OR email LIKE @t
            ORDER BY nom ASC";

        command.Parameters.AddWithValue("@t", $"%{terme.Trim()}%");

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            list.Add(MapReader(reader));
        }

        return list;
    }

    public int Insert(Fournisseur fournisseur)
    {
        using var connection = _dbConfig.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO fournisseurs (nom, telephone, adresse, email, date_creation)
            VALUES (@nom, @tel, @adr, @email, @date_creation);
            SELECT last_insert_rowid();";

        command.AddSafeParameter("@nom", fournisseur.Nom.Trim());
        command.AddSafeParameter("@tel", fournisseur.Telephone.Trim());
        command.AddSafeParameter("@adr", fournisseur.Adresse.Trim());
        command.AddSafeParameter("@email", fournisseur.Email.Trim());
        command.AddSafeParameter("@date_creation", fournisseur.DateCreation.ToString("o"));

        long newId = (long)(command.ExecuteScalar() ?? 0L);
        fournisseur.Id = (int)newId;
        return fournisseur.Id;
    }

    public bool Update(Fournisseur fournisseur)
    {
        using var connection = _dbConfig.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE fournisseurs 
            SET nom = @nom, telephone = @tel, adresse = @adr, email = @email 
            WHERE id = @id";

        command.AddSafeParameter("@nom", fournisseur.Nom.Trim());
        command.AddSafeParameter("@tel", fournisseur.Telephone.Trim());
        command.AddSafeParameter("@adr", fournisseur.Adresse.Trim());
        command.AddSafeParameter("@email", fournisseur.Email.Trim());
        command.Parameters.AddWithValue("@id", fournisseur.Id);

        return command.ExecuteNonQuery() > 0;
    }

    public bool Delete(int id)
    {
        using var connection = _dbConfig.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM fournisseurs WHERE id = @id";
        command.Parameters.AddWithValue("@id", id);

        return command.ExecuteNonQuery() > 0;
    }

    private static Fournisseur MapReader(SqliteDataReader reader)
    {
        return new Fournisseur
        {
            Id = reader.GetSafeInt32("id"),
            Nom = reader.GetSafeString("nom"),
            Telephone = reader.GetSafeString("telephone"),
            Adresse = reader.GetSafeString("adresse"),
            Email = reader.GetSafeString("email"),
            DateCreation = reader.GetSafeDateTime("date_creation")
        };
    }
}
