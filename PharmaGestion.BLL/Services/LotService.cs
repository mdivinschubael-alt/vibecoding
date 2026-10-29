using PharmaGestion.Core.Common;
using PharmaGestion.Core.Models;
using PharmaGestion.DAL.Repositories;

namespace PharmaGestion.BLL.Services;

public class LotService : ILotService
{
    private readonly ILotRepository _lotRepo;
    private readonly IProduitRepository _produitRepo;

    public LotService(ILotRepository lotRepo, IProduitRepository produitRepo)
    {
        _lotRepo = lotRepo;
        _produitRepo = produitRepo;
    }

    public List<Lot> GetAll()
    {
        return _lotRepo.GetAll();
    }

    public List<Lot> GetByProduitId(int produitId)
    {
        return _lotRepo.GetByProduitId(produitId);
    }

    public List<Lot> GetAlertesPeremption(int joursLimite = 90)
    {
        return _lotRepo.GetAlertesPeremption(joursLimite);
    }

    public OperationResult<Lot> ReceptionnerLot(Lot lot)
    {
        if (lot.ProduitId <= 0)
        {
            return OperationResult<Lot>.Echec("Veuillez sélectionner un produit valide.");
        }

        var produit = _produitRepo.GetById(lot.ProduitId);
        if (produit == null)
        {
            return OperationResult<Lot>.Echec("Le produit associé à ce lot n'existe pas.");
        }

        if (string.IsNullOrWhiteSpace(lot.NumeroLot))
        {
            return OperationResult<Lot>.Echec("Le numéro de lot est obligatoire pour la traçabilité sanitaire.");
        }

        if (lot.QuantiteInitiale <= 0)
        {
            return OperationResult<Lot>.Echec("La quantité initiale du lot doit être strictement supérieure à 0.");
        }

        if (lot.CoutAchat < 0.00m)
        {
            return OperationResult<Lot>.Echec("Le coût d'achat unitaire ne peut pas être négatif.");
        }

        if (lot.DatePeremption.Date <= DateTime.Today)
        {
            return OperationResult<Lot>.Echec("La date de péremption doit être ultérieure à la date du jour pour une réception en stock.");
        }

        // Par défaut lors de la réception, la quantité actuelle est égale à la quantité initiale
        if (lot.QuantiteActuelle <= 0)
        {
            lot.QuantiteActuelle = lot.QuantiteInitiale;
        }

        if (lot.DateReception == DateTime.MinValue)
        {
            lot.DateReception = DateTime.Now;
        }

        try
        {
            int newId = _lotRepo.Insert(lot);
            lot.Id = newId;
            return OperationResult<Lot>.Succes(lot, $"Lot '{lot.NumeroLot}' réceptionné avec succès ({lot.QuantiteInitiale} unités).");
        }
        catch (Exception ex)
        {
            return OperationResult<Lot>.Echec($"Erreur lors de la réception du lot : {ex.Message}");
        }
    }

    public OperationResult ModifierLot(Lot lot)
    {
        if (string.IsNullOrWhiteSpace(lot.NumeroLot))
        {
            return OperationResult.Echec("Le numéro de lot ne peut pas être vide.");
        }

        if (lot.QuantiteActuelle < 0)
        {
            return OperationResult.Echec("La quantité actuelle ne peut pas être négative.");
        }

        if (lot.CoutAchat < 0.00m)
        {
            return OperationResult.Echec("Le coût d'achat ne peut pas être négatif.");
        }

        bool updated = _lotRepo.Update(lot);
        return updated
            ? OperationResult.Succes("Lot mis à jour avec succès.")
            : OperationResult.Echec("Erreur lors de la mise à jour du lot.");
    }

    public OperationResult SortirStockLot(int lotId, int quantite, string motif)
    {
        if (quantite <= 0)
        {
            return OperationResult.Echec("La quantité à déduire doit être supérieure à zéro.");
        }

        var lot = _lotRepo.GetById(lotId);
        if (lot == null)
        {
            return OperationResult.Echec("Lot introuvable.");
        }

        if (lot.QuantiteActuelle < quantite)
        {
            return OperationResult.Echec($"Stock insuffisant pour ce lot. Quantité disponible : {lot.QuantiteActuelle}.");
        }

        int nouvelleQte = lot.QuantiteActuelle - quantite;
        bool ok = _lotRepo.UpdateQuantite(lotId, nouvelleQte);

        return ok
            ? OperationResult.Succes($"Sortie de {quantite} unité(s) validée. Motif : {motif}. Reste en lot : {nouvelleQte}.")
            : OperationResult.Echec("Erreur lors de la mise à jour du stock.");
    }

    public OperationResult SupprimerLot(int id)
    {
        var lot = _lotRepo.GetById(id);
        if (lot == null)
        {
            return OperationResult.Echec("Lot introuvable.");
        }

        bool ok = _lotRepo.Delete(id);
        return ok
            ? OperationResult.Succes("Lot supprimé avec succès.")
            : OperationResult.Echec("Erreur lors de la suppression du lot.");
    }
}
