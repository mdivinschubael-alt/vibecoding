using Microsoft.Data.Sqlite;
using PharmaGestion.Core.Models;
using PharmaGestion.DAL.Database;

namespace PharmaGestion.DAL.Repositories;

public class ProduitRepository : IProduitRepository
{
    private readonly DatabaseConfig _dbConfig;

    public ProduitRepository(DatabaseConfig dbConfig)
    {
        _dbConfig = dbConfig;
    }

    private const string BaseSelectQuery = @"
        SELECT 
            p.id, 
            p.reference_unique, 
            p.designation, 
            p.categorie, 
            p.dosage_forme, 
            p.unite_vente, 
            p.prix_vente, 
            p.stock_minimum, 
            p.actif,
            COALESCE((SELECT SUM(l.quantite_actuelle) FROM lots l WHERE l.produit_id = p.id), 0) AS stock_total
        FROM produits p";

    public Produit? GetById(int id)
    {
        using var connection = _dbConfig.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = $"{BaseSelectQuery} WHERE p.id = @id";
        command.Parameters.AddWithValue("@id", id);

        using var reader = command.ExecuteReader();
        return reader.Read() ? MapReader(reader) : null;
    }

    public Produit? GetByReference(string referenceUnique)
    {
        using var connection = _dbConfig.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = $"{BaseSelectQuery} WHERE LOWER(p.reference_unique) = LOWER(@ref)";
        command.Parameters.AddWithValue("@ref", referenceUnique.Trim());

        using var reader = command.ExecuteReader();
        return reader.Read() ? MapReader(reader) : null;
    }

    public List<Produit> GetAll(bool seulementActifs = false)
    {
        var list = new List<Produit>();
        using var connection = _dbConfig.CreateConnection();
        using var command = connection.CreateCommand();
        
        command.CommandText = seulementActifs 
            ? $"{BaseSelectQuery} WHERE p.actif = 1 ORDER BY p.designation ASC" 
            : $"{BaseSelectQuery} ORDER BY p.designation ASC";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            list.Add(MapReader(reader));
        }

        return list;
    }

    public List<Produit> Search(string terme)
    {
        var list = new List<Produit>();
        using var connection = _dbConfig.CreateConnection();
        using var command = connection.CreateCommand();
        
        command.CommandText = $@"
            {BaseSelectQuery} 
            WHERE p.reference_unique LIKE @t 
               OR p.designation LIKE @t 
               OR p.categorie LIKE @t 
               OR p.dosage_forme LIKE @t
            ORDER BY p.designation ASC";

        command.Parameters.AddWithValue("@t", $"%{terme.Trim()}%");

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            list.Add(MapReader(reader));
        }

        return list;
    }

    public List<Produit> GetEnAlerteStock()
    {
        var list = new List<Produit>();
        using var connection = _dbConfig.CreateConnection();
        using var command = connection.CreateCommand();

        command.CommandText = $@"
            {BaseSelectQuery}
            WHERE p.actif = 1 
              AND COALESCE((SELECT SUM(l.quantite_actuelle) FROM lots l WHERE l.produit_id = p.id), 0) <= p.stock_minimum
            ORDER BY stock_total ASC, p.designation ASC";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            list.Add(MapReader(reader));
        }

        return list;
    }

    public int Insert(Produit produit)
    {
        using var connection = _dbConfig.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO produits (reference_unique, designation, categorie, dosage_forme, unite_vente, prix_vente, stock_minimum, actif)
            VALUES (@ref, @desig, @cat, @dosage, @unite, @prix, @min, @actif);
            SELECT last_insert_rowid();";

        command.AddSafeParameter("@ref", produit.ReferenceUnique.Trim());
        command.AddSafeParameter("@desig", produit.Designation.Trim());
        command.AddSafeParameter("@cat", produit.Categorie.Trim());
        command.AddSafeParameter("@dosage", produit.DosageForme.Trim());
        command.AddSafeParameter("@unite", produit.UniteVente.Trim());
        command.AddDecimalParameter("@prix", produit.PrixVente);
        command.Parameters.AddWithValue("@min", produit.StockMinimum);
        command.Parameters.AddWithValue("@actif", produit.Actif ? 1 : 0);

        long newId = (long)(command.ExecuteScalar() ?? 0L);
        produit.Id = (int)newId;
        return produit.Id;
    }

    public bool Update(Produit produit)
    {
        using var connection = _dbConfig.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE produits 
            SET reference_unique = @ref, 
                designation = @desig, 
                categorie = @cat, 
                dosage_forme = @dosage, 
                unite_vente = @unite, 
                prix_vente = @prix, 
                stock_minimum = @min, 
                actif = @actif
            WHERE id = @id";

        command.AddSafeParameter("@ref", produit.ReferenceUnique.Trim());
        command.AddSafeParameter("@desig", produit.Designation.Trim());
        command.AddSafeParameter("@cat", produit.Categorie.Trim());
        command.AddSafeParameter("@dosage", produit.DosageForme.Trim());
        command.AddSafeParameter("@unite", produit.UniteVente.Trim());
        command.AddDecimalParameter("@prix", produit.PrixVente);
        command.Parameters.AddWithValue("@min", produit.StockMinimum);
        command.Parameters.AddWithValue("@actif", produit.Actif ? 1 : 0);
        command.Parameters.AddWithValue("@id", produit.Id);

        return command.ExecuteNonQuery() > 0;
    }

    public bool Delete(int id)
    {
        using var connection = _dbConfig.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM produits WHERE id = @id";
        command.Parameters.AddWithValue("@id", id);

        return command.ExecuteNonQuery() > 0;
    }

    public bool ExistsReference(string referenceUnique, int? excludeId = null)
    {
        using var connection = _dbConfig.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = excludeId.HasValue
            ? "SELECT COUNT(*) FROM produits WHERE LOWER(reference_unique) = LOWER(@ref) AND id <> @excludeId"
            : "SELECT COUNT(*) FROM produits WHERE LOWER(reference_unique) = LOWER(@ref)";

        command.Parameters.AddWithValue("@ref", referenceUnique.Trim());
        if (excludeId.HasValue)
        {
            command.Parameters.AddWithValue("@excludeId", excludeId.Value);
        }

        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    private static Produit MapReader(SqliteDataReader reader)
    {
        return new Produit
        {
            Id = reader.GetSafeInt32("id"),
            ReferenceUnique = reader.GetSafeString("reference_unique"),
            Designation = reader.GetSafeString("designation"),
            Categorie = reader.GetSafeString("categorie"),
            DosageForme = reader.GetSafeString("dosage_forme"),
            UniteVente = reader.GetSafeString("unite_vente"),
            PrixVente = reader.GetSafeDecimal("prix_vente"),
            StockMinimum = reader.GetSafeInt32("stock_minimum"),
            Actif = reader.GetSafeBoolean("actif"),
            StockTotal = reader.GetSafeInt32("stock_total")
        };
    }
}
