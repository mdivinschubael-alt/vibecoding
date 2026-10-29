namespace PharmaGestion.Core.Models;

public class Utilisateur
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Identifiant { get; set; } = string.Empty;
    public string MotDePasseHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Pharmacien";
    public DateTime DateCreation { get; set; } = DateTime.Now;
}
