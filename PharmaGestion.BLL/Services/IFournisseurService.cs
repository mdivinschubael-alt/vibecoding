using PharmaGestion.Core.Common;
using PharmaGestion.Core.Models;

namespace PharmaGestion.BLL.Services;

public interface IFournisseurService
{
    List<Fournisseur> GetAll();
    Fournisseur? GetById(int id);
    List<Fournisseur> Search(string terme);
    OperationResult<Fournisseur> CreerFournisseur(Fournisseur f);
    OperationResult ModifierFournisseur(Fournisseur f);
    OperationResult SupprimerFournisseur(int id);
}
