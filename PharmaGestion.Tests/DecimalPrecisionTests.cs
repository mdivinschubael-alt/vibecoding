using System.Globalization;
using PharmaGestion.Core.Models;
using PharmaGestion.DAL.Database;
using PharmaGestion.DAL.Repositories;
using Xunit;

namespace PharmaGestion.Tests;

public class DecimalPrecisionTests : IDisposable
{
    private readonly string _tempDbFile;
    private readonly DatabaseConfig _dbConfig;

    public DecimalPrecisionTests()
    {
        _tempDbFile = Path.Combine(Path.GetTempPath(), $"pharma_test_dec_{Guid.NewGuid():N}.db");
        _dbConfig = new DatabaseConfig(_tempDbFile);
        var initializer = new DatabaseInitializer(_dbConfig);
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
    public void PrixVente_ShouldMaintainExactDecimalPrecision_WithoutFloatRoundingErrors()
    {
        var produitRepo = new ProduitRepository(_dbConfig);

        // Tester un montant avec des décimales précises (ex: 12.3456 €)
        decimal exactPrice = 12.3456m;

        var produit = new Produit
        {
            ReferenceUnique = "TEST-DEC-001",
            Designation = "Test Produit Précision",
            Categorie = "Test",
            DosageForme = "10mg",
            UniteVente = "Boîte",
            PrixVente = exactPrice,
            StockMinimum = 5,
            Actif = true
        };

        int id = produitRepo.Insert(produit);
        var retrieved = produitRepo.GetById(id);

        Assert.NotNull(retrieved);
        Assert.Equal(exactPrice, retrieved.PrixVente);

        // Vérification qu'aucune dérive de virgule flottante IEEE 754 n'a altéré la valeur
        Assert.Equal("12.3456", retrieved.PrixVente.ToString("0.0000", CultureInfo.InvariantCulture));
    }

    [Fact]
    public void CoutAchat_InLots_ShouldMaintainExactDecimalPrecision()
    {
        var produitRepo = new ProduitRepository(_dbConfig);
        var lotRepo = new LotRepository(_dbConfig);

        var produit = new Produit
        {
            ReferenceUnique = "TEST-DEC-LOT-01",
            Designation = "Test Produit Pour Lot",
            Categorie = "Test",
            DosageForme = "50mg",
            UniteVente = "Flacon",
            PrixVente = 29.99m,
            StockMinimum = 10,
            Actif = true
        };
        int prodId = produitRepo.Insert(produit);

        decimal exactCoutAchat = 18.7895m;

        var lot = new Lot
        {
            ProduitId = prodId,
            NumeroLot = "LOT-PRECISION-99",
            DatePeremption = DateTime.Today.AddYears(2),
            QuantiteInitiale = 50,
            QuantiteActuelle = 50,
            CoutAchat = exactCoutAchat,
            DateReception = DateTime.Now
        };

        int lotId = lotRepo.Insert(lot);
        var retrievedLot = lotRepo.GetById(lotId);

        Assert.NotNull(retrievedLot);
        Assert.Equal(exactCoutAchat, retrievedLot.CoutAchat);

        // Calcul de valeur stock totale
        decimal valeurTotale = retrievedLot.QuantiteActuelle * retrievedLot.CoutAchat;
        Assert.Equal(50 * 18.7895m, valeurTotale);
    }
}
