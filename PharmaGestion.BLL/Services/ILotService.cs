using PharmaGestion.Core.Common;
using PharmaGestion.Core.Models;

namespace PharmaGestion.BLL.Services;

public interface ILotService
{
    List<Lot> GetAll();
    List<Lot> GetByProduitId(int produitId);
    List<Lot> GetAlertesPeremption(int joursLimite = 90);
    OperationResult<Lot> ReceptionnerLot(Lot lot);
    OperationResult ModifierLot(Lot lot);
    OperationResult SortirStockLot(int lotId, int quantite, string motif);
    OperationResult SupprimerLot(int id);
}
