using PharmaGestion.Core.Models;

namespace PharmaGestion.DAL.Repositories;

public interface IUtilisateurRepository
{
    Utilisateur? GetById(int id);
    Utilisateur? GetByIdentifiant(string identifiant);
    List<Utilisateur> GetAll();
    int Insert(Utilisateur utilisateur);
    bool Update(Utilisateur utilisateur);
    bool UpdateMotDePasse(int id, string motDePasseHash);
    bool Delete(int id);
    int Count();
}
