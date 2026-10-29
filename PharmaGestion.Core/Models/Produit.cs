namespace PharmaGestion.Core.Models;

public class Produit
{
    public int Id { get; set; }
    public string ReferenceUnique { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public string Categorie { get; set; } = string.Empty;
    public string DosageForme { get; set; } = string.Empty;
    public string UniteVente { get; set; } = "Boîte";
    
    /// <summary>
    /// Prix de vente unitaire géré en type décimal de haute précision pour éviter les erreurs d'arrondi.
    /// </summary>
    public decimal PrixVente { get; set; } = 0.00m;
    
    public int StockMinimum { get; set; } = 0;
    public bool Actif { get; set; } = true;
    
    /// <summary>
    /// Stock total calculé à partir de la somme des quantités actuelles des lots.
    /// </summary>
    public int StockTotal { get; set; } = 0;

    /// <summary>
    /// Indique si le stock est en dessous ou égal au seuil minimum.
    /// </summary>
    public bool EnAlerteStock => Actif && StockTotal <= StockMinimum;
}
