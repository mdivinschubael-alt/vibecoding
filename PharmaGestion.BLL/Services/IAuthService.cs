using PharmaGestion.Core.Common;
using PharmaGestion.Core.Models;

namespace PharmaGestion.BLL.Services;

public interface IAuthService
{
    Utilisateur? CurrentUser { get; }
    bool IsAuthenticated => CurrentUser != null;
    bool IsAdmin => CurrentUser?.Role == RoleUtilisateur.Administrateur;

    OperationResult<Utilisateur> Login(string identifiant, string motDePasse);
    void Logout();
    OperationResult ChangerMotDePasse(int userId, string motDePasseActuel, string nouveauMotDePasse);
}
