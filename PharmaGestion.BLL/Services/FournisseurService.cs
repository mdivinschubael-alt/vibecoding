using PharmaGestion.Core.Common;
using PharmaGestion.Core.Models;
using PharmaGestion.DAL.Repositories;

namespace PharmaGestion.BLL.Services;

public class FournisseurService : IFournisseurService
{
    private readonly IFournisseurRepository _fournisseurRepo;

    public FournisseurService(IFournisseurRepository fournisseurRepo)
    {
        _fournisseurRepo = fournisseurRepo;
    }

    public List<Fournisseur> GetAll()
    {
        return _fournisseurRepo.GetAll();
    }

    public Fournisseur? GetById(int id)
    {
        return _fournisseurRepo.GetById(id);
    }

    public List<Fournisseur> Search(string terme)
    {
        return string.IsNullOrWhiteSpace(terme) ? _fournisseurRepo.GetAll() : _fournisseurRepo.Search(terme);
    }

    public OperationResult<Fournisseur> CreerFournisseur(Fournisseur f)
    {
        if (string.IsNullOrWhiteSpace(f.Nom))
        {
            return OperationResult<Fournisseur>.Echec("Le nom du fournisseur ou laboratoire est obligatoire.");
        }

        try
        {
            int newId = _fournisseurRepo.Insert(f);
            f.Id = newId;
            return OperationResult<Fournisseur>.Succes(f, "Fournisseur enregistré avec succès.");
        }
        catch (Exception ex)
        {
            return OperationResult<Fournisseur>.Echec($"Erreur lors de l'enregistrement du fournisseur : {ex.Message}");
        }
    }

    public OperationResult ModifierFournisseur(Fournisseur f)
    {
        if (string.IsNullOrWhiteSpace(f.Nom))
        {
            return OperationResult.Echec("Le nom du fournisseur est obligatoire.");
        }

        bool updated = _fournisseurRepo.Update(f);
        return updated
            ? OperationResult.Succes("Fournisseur mis à jour avec succès.")
            : OperationResult.Echec("Erreur lors de la mise à jour du fournisseur.");
    }

    public OperationResult SupprimerFournisseur(int id)
    {
        try
        {
            bool ok = _fournisseurRepo.Delete(id);
            return ok
                ? OperationResult.Succes("Fournisseur supprimé avec succès.")
                : OperationResult.Echec("Fournisseur introuvable.");
        }
        catch (Exception)
        {
            return OperationResult.Echec("Impossible de supprimer ce fournisseur car des lots y sont rattachés.");
        }
    }
}
