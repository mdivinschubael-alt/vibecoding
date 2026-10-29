using PharmaGestion.Core.Common;
using PharmaGestion.Core.Models;
using PharmaGestion.Core.Security;
using PharmaGestion.DAL.Repositories;

namespace PharmaGestion.BLL.Services;

public class UtilisateurService : IUtilisateurService
{
    private readonly IUtilisateurRepository _utilisateurRepo;
    private readonly IPasswordHasher _passwordHasher;

    public UtilisateurService(IUtilisateurRepository utilisateurRepo, IPasswordHasher? passwordHasher = null)
    {
        _utilisateurRepo = utilisateurRepo;
        _passwordHasher = passwordHasher ?? new PasswordHasher();
    }

    public List<Utilisateur> GetAll()
    {
        return _utilisateurRepo.GetAll();
    }

    public Utilisateur? GetById(int id)
    {
        return _utilisateurRepo.GetById(id);
    }

    public OperationResult<Utilisateur> CreerUtilisateur(string nom, string identifiant, string motDePasse, string role)
    {
        if (string.IsNullOrWhiteSpace(nom))
            return OperationResult<Utilisateur>.Echec("Le nom de l'utilisateur est obligatoire.");

        if (string.IsNullOrWhiteSpace(identifiant) || identifiant.Trim().Length < 3)
            return OperationResult<Utilisateur>.Echec("L'identifiant doit comporter au moins 3 caractères.");

        if (string.IsNullOrWhiteSpace(motDePasse) || motDePasse.Length < 6)
            return OperationResult<Utilisateur>.Echec("Le mot de passe doit comporter au moins 6 caractères.");

        if (!RoleUtilisateur.Tous.Contains(role))
            return OperationResult<Utilisateur>.Echec("Le rôle spécifié est invalide.");

        if (_utilisateurRepo.GetByIdentifiant(identifiant) != null)
            return OperationResult<Utilisateur>.Echec($"L'identifiant '{identifiant}' est déjà utilisé.");

        var nouvelUtilisateur = new Utilisateur
        {
            Nom = nom.Trim(),
            Identifiant = identifiant.Trim(),
            MotDePasseHash = _passwordHasher.HashPassword(motDePasse),
            Role = role,
            DateCreation = DateTime.Now
        };

        try
        {
            int newId = _utilisateurRepo.Insert(nouvelUtilisateur);
            nouvelUtilisateur.Id = newId;
            return OperationResult<Utilisateur>.Succes(nouvelUtilisateur, "Utilisateur créé avec succès.");
        }
        catch (Exception ex)
        {
            return OperationResult<Utilisateur>.Echec($"Erreur lors de la création de l'utilisateur : {ex.Message}");
        }
    }

    public OperationResult ModifierUtilisateur(int id, string nom, string identifiant, string role)
    {
        var existing = _utilisateurRepo.GetById(id);
        if (existing == null)
            return OperationResult.Echec("Utilisateur introuvable.");

        if (string.IsNullOrWhiteSpace(nom))
            return OperationResult.Echec("Le nom de l'utilisateur est obligatoire.");

        if (string.IsNullOrWhiteSpace(identifiant))
            return OperationResult.Echec("L'identifiant est obligatoire.");

        if (!RoleUtilisateur.Tous.Contains(role))
            return OperationResult.Echec("Rôle non reconnu.");

        var withSameIdent = _utilisateurRepo.GetByIdentifiant(identifiant);
        if (withSameIdent != null && withSameIdent.Id != id)
            return OperationResult.Echec($"L'identifiant '{identifiant}' est déjà attribué.");

        existing.Nom = nom.Trim();
        existing.Identifiant = identifiant.Trim();
        existing.Role = role;

        bool updated = _utilisateurRepo.Update(existing);
        return updated
            ? OperationResult.Succes("Informations utilisateur mises à jour.")
            : OperationResult.Echec("Erreur lors de la mise à jour.");
    }

    public OperationResult ReinitialiserMotDePasse(int id, string nouveauMotDePasse)
    {
        if (string.IsNullOrWhiteSpace(nouveauMotDePasse) || nouveauMotDePasse.Length < 6)
            return OperationResult.Echec("Le nouveau mot de passe doit contenir au moins 6 caractères.");

        var user = _utilisateurRepo.GetById(id);
        if (user == null)
            return OperationResult.Echec("Utilisateur introuvable.");

        string hash = _passwordHasher.HashPassword(nouveauMotDePasse);
        bool ok = _utilisateurRepo.UpdateMotDePasse(id, hash);

        return ok
            ? OperationResult.Succes("Mot de passe réinitialisé avec succès.")
            : OperationResult.Echec("Erreur lors de la réinitialisation.");
    }

    public OperationResult SupprimerUtilisateur(int id, int idUtilisateurConnecte)
    {
        if (id == idUtilisateurConnecte)
            return OperationResult.Echec("Vous ne pouvez pas supprimer votre propre compte utilisateur actif.");

        var user = _utilisateurRepo.GetById(id);
        if (user == null)
            return OperationResult.Echec("Utilisateur introuvable.");

        if (_utilisateurRepo.Count() <= 1)
            return OperationResult.Echec("Impossible de supprimer le dernier compte utilisateur du système.");

        bool ok = _utilisateurRepo.Delete(id);
        return ok
            ? OperationResult.Succes("Compte utilisateur supprimé avec succès.")
            : OperationResult.Echec("Erreur lors de la suppression.");
    }
}
