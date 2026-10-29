using PharmaGestion.Core.Common;
using PharmaGestion.Core.Models;
using PharmaGestion.DAL.Repositories;

namespace PharmaGestion.BLL.Services;

public class ProduitService : IProduitService
{
    private readonly IProduitRepository _produitRepo;

    public ProduitService(IProduitRepository produitRepo)
    {
        _produitRepo = produitRepo;
    }

    public List<Produit> GetAll(bool seulementActifs = false)
    {
        return _produitRepo.GetAll(seulementActifs);
    }

    public Produit? GetById(int id)
    {
        return _produitRepo.GetById(id);
    }

    public Produit? GetByReference(string referenceUnique)
    {
        return _produitRepo.GetByReference(referenceUnique);
    }

    public List<Produit> Search(string terme)
    {
        return string.IsNullOrWhiteSpace(terme) ? _produitRepo.GetAll() : _produitRepo.Search(terme);
    }

    public List<Produit> GetAlertesStock()
    {
        return _produitRepo.GetEnAlerteStock();
    }

    public OperationResult<Produit> CreerProduit(Produit produit)
    {
        var validation = ValiderProduit(produit);
        if (!validation.EstSucces)
        {
            return OperationResult<Produit>.Echec(validation.Message);
        }

        if (_produitRepo.ExistsReference(produit.ReferenceUnique))
        {
            return OperationResult<Produit>.Echec($"La référence unique '{produit.ReferenceUnique}' existe déjà dans le catalogue.");
        }

        try
        {
            int newId = _produitRepo.Insert(produit);
            produit.Id = newId;
            return OperationResult<Produit>.Succes(produit, "Produit ajouté avec succès.");
        }
        catch (Exception ex)
        {
            return OperationResult<Produit>.Echec($"Erreur lors de l'enregistrement du produit : {ex.Message}");
        }
    }

    public OperationResult ModifierProduit(Produit produit)
    {
        var validation = ValiderProduit(produit);
        if (!validation.EstSucces)
        {
            return validation;
        }

        if (_produitRepo.ExistsReference(produit.ReferenceUnique, produit.Id))
        {
            return OperationResult.Echec($"La référence unique '{produit.ReferenceUnique}' est déjà utilisée par un autre produit.");
        }

        bool updated = _produitRepo.Update(produit);
        return updated
            ? OperationResult.Succes("Produit mis à jour avec succès.")
            : OperationResult.Echec("Erreur lors de la mise à jour du produit.");
    }

    public OperationResult DesactiverProduit(int id)
    {
        var produit = _produitRepo.GetById(id);
        if (produit == null)
        {
            return OperationResult.Echec("Produit introuvable.");
        }

        produit.Actif = false;
        bool updated = _produitRepo.Update(produit);
        return updated 
            ? OperationResult.Succes("Produit désactivé.") 
            : OperationResult.Echec("Impossible de désactiver le produit.");
    }

    public OperationResult SupprimerProduit(int id)
    {
        try
        {
            bool deleted = _produitRepo.Delete(id);
            return deleted
                ? OperationResult.Succes("Produit supprimé du catalogue.")
                : OperationResult.Echec("Produit introuvable ou déjà supprimé.");
        }
        catch (Exception)
        {
            return OperationResult.Echec("Impossible de supprimer ce produit car des lots ou mouvements de stock y sont encore associés. Désactivez-le plutôt.");
        }
    }

    private static OperationResult ValiderProduit(Produit p)
    {
        if (string.IsNullOrWhiteSpace(p.ReferenceUnique))
            return OperationResult.Echec("La référence unique du produit est obligatoire.");

        if (string.IsNullOrWhiteSpace(p.Designation))
            return OperationResult.Echec("La désignation commerciale du produit est obligatoire.");

        if (string.IsNullOrWhiteSpace(p.Categorie))
            return OperationResult.Echec("La catégorie thérapeutique est requise.");

        if (p.PrixVente < 0.00m)
            return OperationResult.Echec("Le prix de vente ne peut pas être négatif.");

        if (p.StockMinimum < 0)
            return OperationResult.Echec("Le stock minimum ne peut pas être inférieur à 0.");

        return OperationResult.Succes();
    }
}
