using System.Globalization;
using Microsoft.Data.Sqlite;
using PharmaGestion.Core.Common;
using PharmaGestion.Core.Security;

namespace PharmaGestion.DAL.Database;

public class DatabaseInitializer
{
    private readonly DatabaseConfig _dbConfig;
    private readonly IPasswordHasher _passwordHasher;

    public DatabaseInitializer(DatabaseConfig dbConfig, IPasswordHasher? passwordHasher = null)
    {
        _dbConfig = dbConfig;
        _passwordHasher = passwordHasher ?? new PasswordHasher();
    }

    public void Initialize()
    {
        using var connection = _dbConfig.CreateConnection();
        using var transaction = connection.BeginTransaction();

        try
        {
            CreateTables(connection, transaction);
            CreateIndexes(connection, transaction);
            SeedDefaultAdmin(connection, transaction);
            SeedInitialPharmacyDataIfEmpty(connection, transaction);

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private static void CreateTables(SqliteConnection connection, SqliteTransaction transaction)
    {
        const string sql = @"
            -- 1. Table Utilisateurs
            CREATE TABLE IF NOT EXISTS utilisateurs (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                nom TEXT NOT NULL,
                identifiant TEXT NOT NULL UNIQUE,
                mot_de_passe_hash TEXT NOT NULL,
                role TEXT NOT NULL,
                date_creation TEXT NOT NULL
            );

            -- 2. Table Fournisseurs
            CREATE TABLE IF NOT EXISTS fournisseurs (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                nom TEXT NOT NULL,
                telephone TEXT,
                adresse TEXT,
                email TEXT,
                date_creation TEXT NOT NULL
            );

            -- 3. Table Produits
            -- Remarque : prix_vente est stocké au format TEXT (décimal invariant) pour éviter toute imprécision d'arrondi binaire
            CREATE TABLE IF NOT EXISTS produits (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                reference_unique TEXT NOT NULL UNIQUE,
                designation TEXT NOT NULL,
                categorie TEXT NOT NULL,
                dosage_forme TEXT NOT NULL,
                unite_vente TEXT NOT NULL,
                prix_vente TEXT NOT NULL,
                stock_minimum INTEGER NOT NULL DEFAULT 0,
                actif INTEGER NOT NULL DEFAULT 1
            );

            -- 4. Table Lots
            -- Remarque : cout_achat est stocké au format TEXT (décimal invariant)
            CREATE TABLE IF NOT EXISTS lots (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                produit_id INTEGER NOT NULL,
                fournisseur_id INTEGER NULL,
                numero_lot TEXT NOT NULL,
                date_peremption TEXT NOT NULL,
                quantite_initiale INTEGER NOT NULL,
                quantite_actuelle INTEGER NOT NULL,
                cout_achat TEXT NOT NULL,
                date_reception TEXT NOT NULL,
                FOREIGN KEY (produit_id) REFERENCES produits(id) ON DELETE RESTRICT,
                FOREIGN KEY (fournisseur_id) REFERENCES fournisseurs(id) ON DELETE SET NULL
            );
        ";

        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private static void CreateIndexes(SqliteConnection connection, SqliteTransaction transaction)
    {
        const string sql = @"
            CREATE UNIQUE INDEX IF NOT EXISTS uq_utilisateurs_identifiant ON utilisateurs(identifiant);
            CREATE UNIQUE INDEX IF NOT EXISTS uq_produits_ref ON produits(reference_unique);
            CREATE INDEX IF NOT EXISTS idx_produits_designation ON produits(designation);
            CREATE INDEX IF NOT EXISTS idx_lots_produit ON lots(produit_id);
            CREATE INDEX IF NOT EXISTS idx_lots_peremption ON lots(date_peremption);
        ";

        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private void SeedDefaultAdmin(SqliteConnection connection, SqliteTransaction transaction)
    {
        using var checkCmd = connection.CreateCommand();
        checkCmd.Transaction = transaction;
        checkCmd.CommandText = "SELECT COUNT(*) FROM utilisateurs WHERE LOWER(identifiant) = 'admin'";
        long adminCount = (long)(checkCmd.ExecuteScalar() ?? 0L);

        if (adminCount == 0)
        {
            string hash = _passwordHasher.HashPassword("Admin123!");

            using var insertCmd = connection.CreateCommand();
            insertCmd.Transaction = transaction;
            insertCmd.CommandText = @"
                INSERT INTO utilisateurs (nom, identifiant, mot_de_passe_hash, role, date_creation)
                VALUES (@nom, @identifiant, @hash, @role, @date_creation);
            ";
            insertCmd.Parameters.AddWithValue("@nom", "Administrateur Système");
            insertCmd.Parameters.AddWithValue("@identifiant", "admin");
            insertCmd.Parameters.AddWithValue("@hash", hash);
            insertCmd.Parameters.AddWithValue("@role", RoleUtilisateur.Administrateur);
            insertCmd.Parameters.AddWithValue("@date_creation", DateTime.Now.ToString("o"));
            insertCmd.ExecuteNonQuery();
        }
    }

    private static void SeedInitialPharmacyDataIfEmpty(SqliteConnection connection, SqliteTransaction transaction)
    {
        using var countCmd = connection.CreateCommand();
        countCmd.Transaction = transaction;
        countCmd.CommandText = "SELECT COUNT(*) FROM produits";
        long prodCount = (long)(countCmd.ExecuteScalar() ?? 0L);

        if (prodCount > 0) return;

        // Insertion de fournisseurs modèles
        using var fCmd = connection.CreateCommand();
        fCmd.Transaction = transaction;
        fCmd.CommandText = @"
            INSERT INTO fournisseurs (id, nom, telephone, adresse, email, date_creation) VALUES 
            (1, 'PharmaDistri France', '+33 1 42 68 55 00', '12 Avenue des Laboratoires, Paris', 'contact@pharmadistri.fr', datetime('now')),
            (2, 'MédicaLogistique', '+33 4 72 00 11 22', '45 Rue de la Santé, Lyon', 'commandes@medicalogistique.com', datetime('now'));
        ";
        fCmd.ExecuteNonQuery();

        // Insertion de produits modèles avec prix décimaux précis
        using var pCmd = connection.CreateCommand();
        pCmd.Transaction = transaction;
        pCmd.CommandText = @"
            INSERT INTO produits (id, reference_unique, designation, categorie, dosage_forme, unite_vente, prix_vente, stock_minimum, actif) VALUES
            (1, 'PAR-500-CP', 'Paracétamol Biogaran', 'Analgésique / Antipyrétique', '500 mg - Comprimé', 'Boîte de 16', '2.1800', 20, 1),
            (2, 'AMO-1G-CP', 'Amoxicilline Sandoz', 'Antibiotique', '1 g - Comprimé dispersible', 'Boîte de 14', '4.4500', 15, 1),
            (3, 'IBU-400-CP', 'Ibuprofène Mylan', 'Anti-inflammatoire non stéroïdien', '400 mg - Comprimé pelliculé', 'Boîte de 20', '3.1200', 10, 1),
            (4, 'SPA-80-CP', 'Spasfon Lyoc', 'Antispasmodique', '80 mg - Lyophilisat oral', 'Boîte de 10', '3.0800', 15, 1),
            (5, 'DOL-1000-GE', 'Doliprane Caps', 'Analgésique', '1000 mg - Gélule', 'Boîte de 8', '2.4900', 25, 1);
        ";
        pCmd.ExecuteNonQuery();

        // Insertion de lots modèles avec suivi d'expiration et coûts d'achat décimaux
        using var lCmd = connection.CreateCommand();
        lCmd.Transaction = transaction;
        lCmd.CommandText = @"
            INSERT INTO lots (produit_id, fournisseur_id, numero_lot, date_peremption, quantite_initiale, quantite_actuelle, cout_achat, date_reception) VALUES
            (1, 1, 'LOT-PAR-2024-01', date('now', '+365 days'), 100, 75, '1.2000', datetime('now', '-30 days')),
            (1, 1, 'LOT-PAR-2023-99', date('now', '+45 days'), 50, 12, '1.1500', datetime('now', '-180 days')),
            (2, 2, 'LOT-AMO-8834', date('now', '+240 days'), 60, 42, '2.8000', datetime('now', '-15 days')),
            (3, 1, 'LOT-IBU-5521', date('now', '+15 days'), 30, 8, '1.7500', datetime('now', '-90 days')),
            (4, 2, 'LOT-SPA-4410', date('now', '+500 days'), 80, 50, '1.6500', datetime('now', '-5 days')),
            (5, 1, 'LOT-DOL-9901', date('now', '+400 days'), 120, 95, '1.3500', datetime('now', '-10 days'));
        ";
        lCmd.ExecuteNonQuery();
    }
}
