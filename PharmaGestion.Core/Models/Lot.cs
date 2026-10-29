namespace PharmaGestion.Core.Models;

public class Lot
{
    public int Id { get; set; }
    public int ProduitId { get; set; }
    public int? FournisseurId { get; set; }
    public string NumeroLot { get; set; } = string.Empty;
    public DateTime DatePeremption { get; set; } = DateTime.Today.AddYears(1);
    public int QuantiteInitiale { get; set; } = 0;
    public int QuantiteActuelle { get; set; } = 0;

    /// <summary>
    /// Coût d'achat unitaire ou total géré en type décimal de haute précision pour éviter les erreurs d'arrondi.
    /// </summary>
    public decimal CoutAchat { get; set; } = 0.00m;
    
    public DateTime DateReception { get; set; } = DateTime.Now;

    // Propriétés de jointure / affichage
    public string ProduitReference { get; set; } = string.Empty;
    public string ProduitDesignation { get; set; } = string.Empty;
    public string FournisseurNom { get; set; } = string.Empty;

    /// <summary>
    /// Indique le statut de péremption :
    /// - Périmé (<= 0 jours)
    /// - Alerte imminente (<= 90 jours)
    /// - Normal (> 90 jours)
    /// </summary>
    public bool EstPerime => DatePeremption.Date <= DateTime.Today;
    public bool EstProchePeremption => !EstPerime && (DatePeremption.Date - DateTime.Today).TotalDays <= 90;
    public int JoursRestants => (int)(DatePeremption.Date - DateTime.Today).TotalDays;
}
