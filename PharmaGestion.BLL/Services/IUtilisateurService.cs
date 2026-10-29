using PharmaGestion.Core.Common;
using PharmaGestion.Core.Models;

namespace PharmaGestion.BLL.Services;

public interface IUtilisateurService
{
    List<Utilisateur> GetAll();
    Utilisateur? GetById(int id);
    OperationResult<Utilisateur> CreerUtilisateur(string nom, string identifiant, string motDePasse, string role);
    OperationResult ModifierUtilisateur(int id, string nom, string identifiant, string role);
    OperationResult ReinitialiserMotDePasse(int id, string nouveauMotDePasse);
    OperationResult SupprimerUtilisateur(int id, int idUtilisateurConnecte);
}
