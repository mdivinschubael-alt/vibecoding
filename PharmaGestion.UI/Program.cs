using System.Globalization;
using PharmaGestion.BLL.Services;
using PharmaGestion.Core.Security;
using PharmaGestion.DAL.Database;
using PharmaGestion.DAL.Repositories;
using PharmaGestion.UI.Forms;

namespace PharmaGestion.UI;

static class Program
{
    /// <summary>
    /// Point d'entrée principal de l'application pharmaceutique hors ligne.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // Configuration de la culture par défaut pour les formats monétaires et dates
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("fr-FR");

        ApplicationConfiguration.Initialize();

        try
        {
            // 1. Initialisation de la couche DAL & Base SQLite locale hors ligne
            var dbConfig = new DatabaseConfig();
            var passwordHasher = new PasswordHasher();
            var dbInitializer = new DatabaseInitializer(dbConfig, passwordHasher);
            
            // Crée les tables et injecte l'administrateur par défaut si nécessaire
            dbInitializer.Initialize();

            // 2. Initialisation des dépôts (DAL)
            IUtilisateurRepository utilisateurRepo = new UtilisateurRepository(dbConfig);
            IProduitRepository produitRepo = new ProduitRepository(dbConfig);
            IFournisseurRepository fournisseurRepo = new FournisseurRepository(dbConfig);
            ILotRepository lotRepo = new LotRepository(dbConfig);

            // 3. Initialisation des services métier (BLL)
            IAuthService authService = new AuthService(utilisateurRepo, passwordHasher);
            IProduitService produitService = new ProduitService(produitRepo);
            ILotService lotService = new LotService(lotRepo, produitRepo);
            IFournisseurService fournisseurService = new FournisseurService(fournisseurRepo);
            IUtilisateurService utilisateurService = new UtilisateurService(utilisateurRepo, passwordHasher);

            // 4. Lancement de l'écran d'authentification
            using var loginForm = new LoginForm(authService);
            if (loginForm.ShowDialog() == DialogResult.OK)
            {
                // 5. Lancement du tableau de bord principal après connexion
                Application.Run(new MainForm(
                    authService,
                    produitService,
                    lotService,
                    fournisseurService,
                    utilisateurService));
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Une erreur critique est survenue lors de l'initialisation de l'application hors ligne :\n\n{ex.Message}\n\nDétails : {ex.StackTrace}",
                "Erreur Système",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}