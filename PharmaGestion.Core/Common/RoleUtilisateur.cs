namespace PharmaGestion.Core.Common;

public static class RoleUtilisateur
{
    public const string Administrateur = "Administrateur";
    public const string Pharmacien = "Pharmacien";
    public const string Caissier = "Caissier";

    public static readonly string[] Tous = [Administrateur, Pharmacien, Caissier];
}
