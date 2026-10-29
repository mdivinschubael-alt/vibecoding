using Microsoft.Data.Sqlite;
using PharmaGestion.BLL.Services;
using PharmaGestion.Core.Common;
using PharmaGestion.Core.Models;
using PharmaGestion.Core.Security;
using PharmaGestion.DAL.Database;
using PharmaGestion.DAL.Repositories;
using Xunit;

namespace PharmaGestion.Tests;

public class DatabaseAndRepositoryTests : IDisposable
{
    private readonly string _tempDbFile;
    private readonly DatabaseConfig _dbConfig;
    private readonly IPasswordHasher _passwordHasher;

    public DatabaseAndRepositoryTests()
    {
        _tempDbFile = Path.Combine(Path.GetTempPath(), $"pharma_test_db_{Guid.NewGuid():N}.db");
        _dbConfig = new DatabaseConfig(_tempDbFile);
        _passwordHasher = new PasswordHasher();

        var initializer = new DatabaseInitializer(_dbConfig, _passwordHasher);
        initializer.Initialize();
    }

    public void Dispose()
    {
        try
        {
            if (File.Exists(_tempDbFile))
            {
                File.Delete(_tempDbFile);
            }
        }
        catch
        {
            // Ignorer si verrouillé temporairement
        }
    }

    [Fact]
    public void Initializer_ShouldCreateDefaultAdministrator()
    {
        var utilisateurRepo = new UtilisateurRepository(_dbConfig);
        var admin = utilisateurRepo.GetByIdentifiant("admin");

        Assert.NotNull(admin);
        Assert.Equal("admin", admin.Identifiant);
        Assert.Equal(RoleUtilisateur.Administrateur, admin.Role);
        Assert.True(_passwordHasher.VerifyPassword("Admin123!", admin.MotDePasseHash));
    }

    [Fact]
    public void AuthService_LoginWithDefaultAdmin_ShouldSucceed()
    {
        var utilisateurRepo = new UtilisateurRepository(_dbConfig);
        var authService = new AuthService(utilisateurRepo, _passwordHasher);

        var result = authService.Login("admin", "Admin123!");

        Assert.True(result.EstSucces);
        Assert.NotNull(result.Donnee);
        Assert.Equal("admin", result.Donnee.Identifiant);
        Assert.True(authService.IsAuthenticated);
        Assert.True(authService.IsAdmin);
    }

    [Fact]
    public void AuthService_LoginWithWrongPassword_ShouldFail()
    {
        var utilisateurRepo = new UtilisateurRepository(_dbConfig);
        var authService = new AuthService(utilisateurRepo, _passwordHasher);

        var result = authService.Login("admin", "FauxMotDePasse!");

        Assert.False(result.EstSucces);
        Assert.False(authService.IsAuthenticated);
    }

    [Fact]
    public void FournisseurService_CRUD_ShouldWorkCorrectly()
    {
        var fournisseurRepo = new FournisseurRepository(_dbConfig);
        var fournisseurService = new FournisseurService(fournisseurRepo);

        var nouveauFournisseur = new Fournisseur
        {
            Nom = "Laboratoire Pierre Fabre Test",
            Telephone = "+33 5 63 50 00 00",
            Adresse = "Castres, France",
            Email = "contact@fabre-test.fr"
        };

        var createResult = fournisseurService.CreerFournisseur(nouveauFournisseur);
        Assert.True(createResult.EstSucces);
        Assert.True(nouveauFournisseur.Id > 0);

        var retrieved = fournisseurService.GetById(nouveauFournisseur.Id);
        Assert.NotNull(retrieved);
        Assert.Equal("Laboratoire Pierre Fabre Test", retrieved.Nom);

        retrieved.Telephone = "+33 5 63 99 99 99";
        var updateResult = fournisseurService.ModifierFournisseur(retrieved);
        Assert.True(updateResult.EstSucces);

        var updated = fournisseurService.GetById(retrieved.Id);
        Assert.Equal("+33 5 63 99 99 99", updated?.Telephone);
    }

    [Fact]
    public void ProduitAndLotService_StockTotalAndAlerts_ShouldFunctionCorrectly()
    {
        var produitRepo = new ProduitRepository(_dbConfig);
        var lotRepo = new LotRepository(_dbConfig);
        var produitService = new ProduitService(produitRepo);
        var lotService = new LotService(lotRepo, produitRepo);

        // 1. Création d'un produit avec stock minimum = 20
        var prod = new Produit
        {
            ReferenceUnique = "TEST-DOLI-500",
            Designation = "Doliprane 500mg Test",
            Categorie = "Analgésique",
            DosageForme = "500 mg - Gélule",
            UniteVente = "Boîte de 16",
            PrixVente = 2.15m,
            StockMinimum = 20,
            Actif = true
        };

        var prodResult = produitService.CreerProduit(prod);
        Assert.True(prodResult.EstSucces);
        int prodId = prod.Id;

        // Stock initial = 0, donc alerte stock bas active
        var prodEnAlerte = produitService.GetAlertesStock().FirstOrDefault(p => p.Id == prodId);
        Assert.NotNull(prodEnAlerte);

        // 2. Réception d'un lot avec 15 unités (toujours < stock min 20)
        var lot1 = new Lot
        {
            ProduitId = prodId,
            NumeroLot = "LOT-DOLI-001",
            DatePeremption = DateTime.Today.AddDays(40), // Expire dans 40 jours (< 90j)
            QuantiteInitiale = 15,
            QuantiteActuelle = 15,
            CoutAchat = 1.10m,
            DateReception = DateTime.Now
        };
        var lotResult1 = lotService.ReceptionnerLot(lot1);
        Assert.True(lotResult1.EstSucces);

        // Le stock total doit maintenant être 15
        int stockTotal = lotRepo.GetStockTotalProduit(prodId);
        Assert.Equal(15, stockTotal);

        // Vérification de l'alerte péremption pour ce lot (< 90 jours)
        var alertesPeremption = lotService.GetAlertesPeremption(90);
        Assert.Contains(alertesPeremption, l => l.NumeroLot == "LOT-DOLI-001");

        // 3. Réception d'un second lot avec 25 unités (total 40 > stock min 20)
        var lot2 = new Lot
        {
            ProduitId = prodId,
            NumeroLot = "LOT-DOLI-002",
            DatePeremption = DateTime.Today.AddDays(365), // Expire dans 1 an
            QuantiteInitiale = 25,
            QuantiteActuelle = 25,
            CoutAchat = 1.05m,
            DateReception = DateTime.Now
        };
        var lotResult2 = lotService.ReceptionnerLot(lot2);
        Assert.True(lotResult2.EstSucces);

        // Le stock total doit être 40
        stockTotal = lotRepo.GetStockTotalProduit(prodId);
        Assert.Equal(40, stockTotal);

        // Le produit ne doit plus être en alerte de stock bas
        var alertesApresRecep = produitService.GetAlertesStock();
        Assert.DoesNotContain(alertesApresRecep, p => p.Id == prodId);

        // 4. Test d'une sortie de stock de 30 unités
        var sortieResult = lotService.SortirStockLot(lot2.Id, 20, "Délivrance ordonnance");
        Assert.True(sortieResult.EstSucces);

        var lot2ApresSortie = lotRepo.GetById(lot2.Id);
        Assert.NotNull(lot2ApresSortie);
        Assert.Equal(5, lot2ApresSortie.QuantiteActuelle);
    }

    [Fact]
    public void ForeignKeyConstraint_ShouldPreventOrphanLots()
    {
        var lotRepo = new LotRepository(_dbConfig);

        var orphanLot = new Lot
        {
            ProduitId = 999999, // Produit inexistant
            NumeroLot = "LOT-ORPHELIN-01",
            DatePeremption = DateTime.Today.AddYears(1),
            QuantiteInitiale = 10,
            QuantiteActuelle = 10,
            CoutAchat = 5.00m
        };

        // L'insertion doit échouer en raison de la contrainte SQLite FOREIGN KEY (produit_id)
        Assert.Throws<SqliteException>(() => lotRepo.Insert(orphanLot));
    }
}
