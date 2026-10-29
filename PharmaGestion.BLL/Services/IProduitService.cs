using PharmaGestion.Core.Common;
using PharmaGestion.Core.Models;

namespace PharmaGestion.BLL.Services;

public interface IProduitService
{
    List<Produit> GetAll(bool seulementActifs = false);
    Produit? GetById(int id);
    Produit? GetByReference(string referenceUnique);
    List<Produit> Search(string terme);
    List<Produit> GetAlertesStock();
    OperationResult<Produit> CreerProduit(Produit produit);
    OperationResult ModifierProduit(Produit produit);
    OperationResult DesactiverProduit(int id);
    OperationResult SupprimerProduit(int id);
}
