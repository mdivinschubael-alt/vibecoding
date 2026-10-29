using PharmaGestion.Core.Models;

namespace PharmaGestion.DAL.Repositories;

public interface IProduitRepository
{
    Produit? GetById(int id);
    Produit? GetByReference(string referenceUnique);
    List<Produit> GetAll(bool seulementActifs = false);
    List<Produit> Search(string terme);
    List<Produit> GetEnAlerteStock();
    int Insert(Produit produit);
    bool Update(Produit produit);
    bool Delete(int id);
    bool ExistsReference(string referenceUnique, int? excludeId = null);
}
