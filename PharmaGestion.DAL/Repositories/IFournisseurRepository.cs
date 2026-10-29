using PharmaGestion.Core.Models;

namespace PharmaGestion.DAL.Repositories;

public interface IFournisseurRepository
{
    Fournisseur? GetById(int id);
    List<Fournisseur> GetAll();
    List<Fournisseur> Search(string terme);
    int Insert(Fournisseur fournisseur);
    bool Update(Fournisseur fournisseur);
    bool Delete(int id);
}
