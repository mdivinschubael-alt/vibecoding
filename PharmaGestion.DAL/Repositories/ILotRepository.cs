using PharmaGestion.Core.Models;

namespace PharmaGestion.DAL.Repositories;

public interface ILotRepository
{
    Lot? GetById(int id);
    List<Lot> GetAll();
    List<Lot> GetByProduitId(int produitId, bool seulementDisponibles = true);
    List<Lot> GetAlertesPeremption(int joursLimite = 90);
    int Insert(Lot lot);
    bool Update(Lot lot);
    bool UpdateQuantite(int id, int nouvelleQuantite);
    bool Delete(int id);
    int GetStockTotalProduit(int produitId);
}
