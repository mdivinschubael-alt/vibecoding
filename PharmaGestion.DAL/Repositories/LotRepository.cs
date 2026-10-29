using Microsoft.Data.Sqlite;
using PharmaGestion.Core.Models;
using PharmaGestion.DAL.Database;

namespace PharmaGestion.DAL.Repositories;

public class LotRepository : ILotRepository
{
    private readonly DatabaseConfig _dbConfig;

    public LotRepository(DatabaseConfig dbConfig)
    {
        _dbConfig = dbConfig;
    }

    private const string BaseSelectQuery = @"
        SELECT 
            l.id,
            l.produit_id,
            l.fournisseur_id,
            l.numero_lot,
            l.date_peremption,
            l.quantite_initiale,
            l.quantite_actuelle,
            l.cout_achat,
            l.date_reception,
            p.reference_unique AS produit_ref,
            p.designation AS produit_nom,
            COALESCE(f.nom, 'Non assigné') AS fournisseur_nom
        FROM lots l
        INNER JOIN produits p ON p.id = l.produit_id
        LEFT JOIN fournisseurs f ON f.id = l.fournisseur_id";

    public Lot? GetById(int id)
    {
        using var connection = _dbConfig.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = $"{BaseSelectQuery} WHERE l.id = @id";
        command.Parameters.AddWithValue("@id", id);

        using var reader = command.ExecuteReader();
        return reader.Read() ? MapReader(reader) : null;
    }

    public List<Lot> GetAll()
    {
        var list = new List<Lot>();
        using var connection = _dbConfig.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = $"{BaseSelectQuery} ORDER BY l.date_peremption ASC, l.id ASC";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            list.Add(MapReader(reader));
        }

        return list;
    }

    public List<Lot> GetByProduitId(int produitId, bool seulementDisponibles = true)
    {
        var list = new List<Lot>();
        using var connection = _dbConfig.CreateConnection();
        using var command = connection.CreateCommand();

        command.CommandText = seulementDisponibles
            ? $"{BaseSelectQuery} WHERE l.produit_id = @pid AND l.quantite_actuelle > 0 ORDER BY l.date_peremption ASC"
            : $"{BaseSelectQuery} WHERE l.produit_id = @pid ORDER BY l.date_peremption ASC";

        command.Parameters.AddWithValue("@pid", produitId);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            list.Add(MapReader(reader));
        }

        return list;
    }

    public List<Lot> GetAlertesPeremption(int joursLimite = 90)
    {
        var list = new List<Lot>();
        using var connection = _dbConfig.CreateConnection();
        using var command = connection.CreateCommand();

        // Lots non épuisés dont la date de péremption est dépassée ou arrive à échéance sous N jours
        command.CommandText = $@"
            {BaseSelectQuery} 
            WHERE l.quantite_actuelle > 0 
              AND DATE(l.date_peremption) <= DATE('now', '+{joursLimite} days')
            ORDER BY l.date_peremption ASC";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            list.Add(MapReader(reader));
        }

        return list;
    }

    public int Insert(Lot lot)
    {
        using var connection = _dbConfig.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO lots (produit_id, fournisseur_id, numero_lot, date_peremption, quantite_initiale, quantite_actuelle, cout_achat, date_reception)
            VALUES (@pid, @fid, @num, @peremption, @q_init, @q_act, @cout, @reception);
            SELECT last_insert_rowid();";

        command.Parameters.AddWithValue("@pid", lot.ProduitId);
        command.AddSafeParameter("@fid", lot.FournisseurId.HasValue ? lot.FournisseurId.Value : DBNull.Value, SqliteType.Integer);
        command.AddSafeParameter("@num", lot.NumeroLot.Trim());
        command.AddSafeParameter("@peremption", lot.DatePeremption.ToString("yyyy-MM-dd"));
        command.Parameters.AddWithValue("@q_init", lot.QuantiteInitiale);
        command.Parameters.AddWithValue("@q_act", lot.QuantiteActuelle);
        command.AddDecimalParameter("@cout", lot.CoutAchat);
        command.AddSafeParameter("@reception", lot.DateReception.ToString("o"));

        long newId = (long)(command.ExecuteScalar() ?? 0L);
        lot.Id = (int)newId;
        return lot.Id;
    }

    public bool Update(Lot lot)
    {
        using var connection = _dbConfig.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE lots 
            SET produit_id = @pid,
                fournisseur_id = @fid,
                numero_lot = @num,
                date_peremption = @peremption,
                quantite_initiale = @q_init,
                quantite_actuelle = @q_act,
                cout_achat = @cout,
                date_reception = @reception
            WHERE id = @id";

        command.Parameters.AddWithValue("@pid", lot.ProduitId);
        command.AddSafeParameter("@fid", lot.FournisseurId.HasValue ? lot.FournisseurId.Value : DBNull.Value, SqliteType.Integer);
        command.AddSafeParameter("@num", lot.NumeroLot.Trim());
        command.AddSafeParameter("@peremption", lot.DatePeremption.ToString("yyyy-MM-dd"));
        command.Parameters.AddWithValue("@q_init", lot.QuantiteInitiale);
        command.Parameters.AddWithValue("@q_act", lot.QuantiteActuelle);
        command.AddDecimalParameter("@cout", lot.CoutAchat);
        command.AddSafeParameter("@reception", lot.DateReception.ToString("o"));
        command.Parameters.AddWithValue("@id", lot.Id);

        return command.ExecuteNonQuery() > 0;
    }

    public bool UpdateQuantite(int id, int nouvelleQuantite)
    {
        using var connection = _dbConfig.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE lots SET quantite_actuelle = @q WHERE id = @id";
        command.Parameters.AddWithValue("@q", nouvelleQuantite);
        command.Parameters.AddWithValue("@id", id);

        return command.ExecuteNonQuery() > 0;
    }

    public bool Delete(int id)
    {
        using var connection = _dbConfig.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM lots WHERE id = @id";
        command.Parameters.AddWithValue("@id", id);

        return command.ExecuteNonQuery() > 0;
    }

    public int GetStockTotalProduit(int produitId)
    {
        using var connection = _dbConfig.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COALESCE(SUM(quantite_actuelle), 0) FROM lots WHERE produit_id = @pid";
        command.Parameters.AddWithValue("@pid", produitId);

        return Convert.ToInt32(command.ExecuteScalar());
    }

    private static Lot MapReader(SqliteDataReader reader)
    {
        return new Lot
        {
            Id = reader.GetSafeInt32("id"),
            ProduitId = reader.GetSafeInt32("produit_id"),
            FournisseurId = reader.GetSafeNullableInt32("fournisseur_id"),
            NumeroLot = reader.GetSafeString("numero_lot"),
            DatePeremption = reader.GetSafeDateTime("date_peremption"),
            QuantiteInitiale = reader.GetSafeInt32("quantite_initiale"),
            QuantiteActuelle = reader.GetSafeInt32("quantite_actuelle"),
            CoutAchat = reader.GetSafeDecimal("cout_achat"),
            DateReception = reader.GetSafeDateTime("date_reception"),
            ProduitReference = reader.GetSafeString("produit_ref"),
            ProduitDesignation = reader.GetSafeString("produit_nom"),
            FournisseurNom = reader.GetSafeString("fournisseur_nom")
        };
    }
}
