using System.Drawing;
using System.Windows.Forms;
using PharmaGestion.BLL.Services;
using PharmaGestion.UI.Common;

namespace PharmaGestion.UI.Forms;

public class LoginForm : Form
{
    private readonly IAuthService _authService;
    private readonly TextBox _txtIdentifiant = new();
    private readonly TextBox _txtMotDePasse = new();
    private readonly Label _lblErreur = new();
    private readonly Button _btnLogin;

    public LoginForm(IAuthService authService)
    {
        _authService = authService;

        Text = "PharmaGestion - Authentification Sécurisée";
        Size = new Size(460, 520);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        BackColor = UiTheme.Background;
        Font = UiTheme.BodyFont;

        _btnLogin = UiTheme.CreatePrimaryButton("Se connecter", 360, 42);
        BuildForm();
    }

    private void BuildForm()
    {
        // Carte centrale blanche avec ombre/bordure
        var card = new Panel
        {
            Size = new Size(390, 430),
            Location = new Point(28, 25),
            BackColor = Color.White,
            Padding = new Padding(25)
        };

        // Icône / En-tête
        var lblLogo = new Label
        {
            Text = "💊",
            Font = new Font("Segoe UI Emoji", 32f),
            AutoSize = true,
            Location = new Point(165, 15)
        };

        var lblAppTitle = new Label
        {
            Text = "PharmaGestion",
            Font = UiTheme.HeaderFont,
            ForeColor = UiTheme.PrimaryDark,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Size = new Size(340, 32),
            Location = new Point(25, 70)
        };

        var lblSubtitle = new Label
        {
            Text = "Gestion d'officine hors ligne • SQLite",
            Font = UiTheme.SmallFont,
            ForeColor = UiTheme.TextMuted,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Size = new Size(340, 20),
            Location = new Point(25, 102)
        };

        // Badge Hors Ligne
        var pnlBadge = new Panel
        {
            Size = new Size(220, 26),
            Location = new Point(85, 126),
            BackColor = Color.FromArgb(236, 253, 245),
        };
        var lblBadge = new Label
        {
            Text = "🟢 Mode 100% Hors Ligne",
            Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
            ForeColor = UiTheme.PrimaryDark,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter
        };
        pnlBadge.Controls.Add(lblBadge);

        // Champ Identifiant
        var lblUser = new Label
        {
            Text = "Identifiant",
            Font = UiTheme.BodyBoldFont,
            ForeColor = UiTheme.TextMain,
            Location = new Point(25, 165),
            AutoSize = true
        };
        _txtIdentifiant.Location = new Point(25, 188);
        _txtIdentifiant.Size = new Size(340, 30);
        _txtIdentifiant.Text = "admin";

        // Champ Mot de passe
        var lblPwd = new Label
        {
            Text = "Mot de passe",
            Font = UiTheme.BodyBoldFont,
            ForeColor = UiTheme.TextMain,
            Location = new Point(25, 228),
            AutoSize = true
        };
        _txtMotDePasse.Location = new Point(25, 251);
        _txtMotDePasse.Size = new Size(340, 30);
        _txtMotDePasse.UseSystemPasswordChar = true;
        _txtMotDePasse.Text = "Admin123!"; // Pré-rempli pour test immédiat du compte admin par défaut

        // Message d'erreur
        _lblErreur.Location = new Point(25, 290);
        _lblErreur.Size = new Size(340, 38);
        _lblErreur.ForeColor = UiTheme.Danger;
        _lblErreur.Font = UiTheme.SmallFont;
        _lblErreur.TextAlign = ContentAlignment.MiddleCenter;
        _lblErreur.Text = "";

        // Bouton de connexion
        _btnLogin.Location = new Point(25, 335);
        _btnLogin.Click += BtnLogin_Click;

        // Astuce par défaut
        var lblHint = new Label
        {
            Text = "Admin par défaut : admin / Admin123!",
            Font = UiTheme.SmallFont,
            ForeColor = UiTheme.TextMuted,
            TextAlign = ContentAlignment.MiddleCenter,
            Size = new Size(340, 20),
            Location = new Point(25, 385)
        };

        card.Controls.AddRange([
            lblLogo, lblAppTitle, lblSubtitle, pnlBadge,
            lblUser, _txtIdentifiant,
            lblPwd, _txtMotDePasse,
            _lblErreur, _btnLogin, lblHint
        ]);

        Controls.Add(card);

        AcceptButton = _btnLogin;
    }

    private void BtnLogin_Click(object? sender, EventArgs e)
    {
        _lblErreur.Text = "";
        _btnLogin.Enabled = false;

        try
        {
            var result = _authService.Login(_txtIdentifiant.Text, _txtMotDePasse.Text);
            if (result.EstSucces)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                _lblErreur.Text = result.Message;
                _txtMotDePasse.SelectAll();
                _txtMotDePasse.Focus();
            }
        }
        catch (Exception ex)
        {
            _lblErreur.Text = $"Erreur : {ex.Message}";
        }
        finally
        {
            _btnLogin.Enabled = true;
        }
    }
}
