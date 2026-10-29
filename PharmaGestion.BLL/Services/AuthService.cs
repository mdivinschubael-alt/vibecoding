using PharmaGestion.Core.Common;
using PharmaGestion.Core.Models;
using PharmaGestion.Core.Security;
using PharmaGestion.DAL.Repositories;

namespace PharmaGestion.BLL.Services;

public class AuthService : IAuthService
{
    private readonly IUtilisateurRepository _utilisateurRepo;
    private readonly IPasswordHasher _passwordHasher;

    public Utilisateur? CurrentUser { get; private set; }
    public bool IsAuthenticated => CurrentUser != null;
    public bool IsAdmin => CurrentUser?.Role == RoleUtilisateur.Administrateur;

    public AuthService(IUtilisateurRepository utilisateurRepo, IPasswordHasher? passwordHasher = null)
    {
        _utilisateurRepo = utilisateurRepo;
        _passwordHasher = passwordHasher ?? new PasswordHasher();
    }

    public OperationResult<Utilisateur> Login(string identifiant, string motDePasse)
    {
        if (string.IsNullOrWhiteSpace(identifiant) || string.IsNullOrWhiteSpace(motDePasse))
        {
            return OperationResult<Utilisateur>.Echec("L'identifiant et le mot de passe sont obligatoires.");
        }

        var utilisateur = _utilisateurRepo.GetByIdentifiant(identifiant);
        if (utilisateur == null)
        {
            return OperationResult<Utilisateur>.Echec("Identifiant ou mot de passe incorrect.");
        }

        bool motDePasseValide = _passwordHasher.VerifyPassword(motDePasse, utilisateur.MotDePasseHash);
        if (!motDePasseValide)
        {
            return OperationResult<Utilisateur>.Echec("Identifiant ou mot de passe incorrect.");
        }

        CurrentUser = utilisateur;
        return OperationResult<Utilisateur>.Succes(utilisateur, $"Connexion réussie. Bienvenue, {utilisateur.Nom} ({utilisateur.Role}) !");
    }

    public void Logout()
    {
        CurrentUser = null;
    }

    public OperationResult ChangerMotDePasse(int userId, string motDePasseActuel, string nouveauMotDePasse)
    {
        if (string.IsNullOrWhiteSpace(nouveauMotDePasse) || nouveauMotDePasse.Length < 6)
        {
            return OperationResult.Echec("Le nouveau mot de passe doit comporter au moins 6 caractères.");
        }

        var user = _utilisateurRepo.GetById(userId);
        if (user == null)
        {
            return OperationResult.Echec("Utilisateur introuvable.");
        }

        if (!_passwordHasher.VerifyPassword(motDePasseActuel, user.MotDePasseHash))
        {
            return OperationResult.Echec("Le mot de passe actuel est incorrect.");
        }

        string nouveauHash = _passwordHasher.HashPassword(nouveauMotDePasse);
        bool misAJour = _utilisateurRepo.UpdateMotDePasse(userId, nouveauHash);

        return misAJour
            ? OperationResult.Succes("Mot de passe mis à jour avec succès.")
            : OperationResult.Echec("Erreur lors de la mise à jour du mot de passe en base.");
    }
}
