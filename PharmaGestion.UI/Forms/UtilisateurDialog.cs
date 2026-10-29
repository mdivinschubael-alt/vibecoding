using System.Drawing;
using System.Windows.Forms;
using PharmaGestion.Core.Common;
using PharmaGestion.Core.Models;
using PharmaGestion.UI.Common;

namespace PharmaGestion.UI.Forms;

public class UtilisateurDialog : Form
{
    private readonly TextBox _txtNom = new();
    private readonly TextBox _txtIdentifiant = new();
    private readonly TextBox _txtMotDePasse = new();
    private readonly ComboBox _cbRole = new();

    private readonly bool _isModification;
    public Utilisateur Utilisateur { get; private set; }
    public string NouveauMotDePasse => _txtMotDePasse.Text;

    public UtilisateurDialog(Utilisateur? utilisateurExistant = null)
    {
        _isModification = utilisateurExistant != null;
        Utilisateur = utilisateurExistant ?? new Utilisateur();

        Text = _isModification ? "Modifier l'Utilisateur" : "Nouvel Utilisateur";
        Size = new Size(500, 420);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.White;
        Font = UiTheme.BodyFont;

        BuildForm();
        LoadData();
    }

    private void BuildForm()
    {
        var panelHeader = new Panel
        {
            Dock = DockStyle.Top,
            Height = 60,
            BackColor = UiTheme.PrimaryLight,
            Padding = new Padding(20, 15, 20, 15)
        };
        var lblTitle = new Label
        {
            Text = Text,
            Font = UiTheme.SubHeaderFont,
            ForeColor = UiTheme.PrimaryDark,
            AutoSize = true,
            Location = new Point(20, 18)
        };
        panelHeader.Controls.Add(lblTitle);

        var panelContent = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(25, 20, 25, 20)
        };

        int y = 75;
        const int labelX = 25;
        const int inputX = 160;
        const int inputW = 290;
        const int rowGap = 40;

        AddRow(panelContent, "Nom complet *", _txtNom, labelX, inputX, inputW, ref y, rowGap);
        AddRow(panelContent, "Identifiant unique *", _txtIdentifiant, labelX, inputX, inputW, ref y, rowGap);

        _cbRole.DropDownStyle = ComboBoxStyle.DropDownList;
        _cbRole.Items.AddRange(RoleUtilisateur.Tous);
        _cbRole.SelectedItem = RoleUtilisateur.Pharmacien;
        AddRow(panelContent, "Rôle / Privilèges *", _cbRole, labelX, inputX, inputW, ref y, rowGap);

        _txtMotDePasse.UseSystemPasswordChar = true;
        string pwdLabel = _isModification ? "Nouveau mot de passe" : "Mot de passe initial *";
        AddRow(panelContent, pwdLabel, _txtMotDePasse, labelX, inputX, inputW, ref y, rowGap);

        if (_isModification)
        {
            var lblHint = new Label
            {
                Text = "(Laisser vide pour conserver le mot de passe actuel)",
                ForeColor = UiTheme.TextMuted,
                Font = UiTheme.SmallFont,
                Location = new Point(inputX, y - 10),
                AutoSize = true
            };
            panelContent.Controls.Add(lblHint);
            y += 20;
        }

        // Boutons
        var panelBottom = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 65,
            BackColor = Color.FromArgb(248, 250, 252),
            Padding = new Padding(20, 15, 20, 15)
        };

        var btnSave = UiTheme.CreatePrimaryButton("Enregistrer", 130, 36);
        btnSave.Location = new Point(330, 14);
        btnSave.Click += BtnSave_Click;

        var btnCancel = UiTheme.CreateSecondaryButton("Annuler", 100, 36);
        btnCancel.Location = new Point(215, 14);
        btnCancel.Click += (_, _) => DialogResult = DialogResult.Cancel;

        panelBottom.Controls.Add(btnCancel);
        panelBottom.Controls.Add(btnSave);

        Controls.Add(panelContent);
        Controls.Add(panelHeader);
        Controls.Add(panelBottom);
    }

    private static void AddRow(Panel panel, string labelText, Control inputControl, int lx, int ix, int iw, ref int y, int gap)
    {
        var lbl = new Label
        {
            Text = labelText,
            Location = new Point(lx, y + 3),
            AutoSize = true,
            ForeColor = UiTheme.TextMain
        };
        inputControl.Location = new Point(ix, y);
        inputControl.Width = iw;
        panel.Controls.Add(lbl);
        panel.Controls.Add(inputControl);
        y += gap;
    }

    private void LoadData()
    {
        _txtNom.Text = Utilisateur.Nom;
        _txtIdentifiant.Text = Utilisateur.Identifiant;
        if (!string.IsNullOrEmpty(Utilisateur.Role) && _cbRole.Items.Contains(Utilisateur.Role))
        {
            _cbRole.SelectedItem = Utilisateur.Role;
        }
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_txtNom.Text))
        {
            MessageBox.Show("Le nom complet est obligatoire.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _txtNom.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(_txtIdentifiant.Text) || _txtIdentifiant.Text.Trim().Length < 3)
        {
            MessageBox.Show("L'identifiant doit comporter au moins 3 caractères.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _txtIdentifiant.Focus();
            return;
        }

        if (!_isModification && (string.IsNullOrWhiteSpace(_txtMotDePasse.Text) || _txtMotDePasse.Text.Length < 6))
        {
            MessageBox.Show("Le mot de passe initial doit contenir au moins 6 caractères.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _txtMotDePasse.Focus();
            return;
        }

        if (_isModification && !string.IsNullOrWhiteSpace(_txtMotDePasse.Text) && _txtMotDePasse.Text.Length < 6)
        {
            MessageBox.Show("Le nouveau mot de passe doit comporter au moins 6 caractères.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _txtMotDePasse.Focus();
            return;
        }

        Utilisateur.Nom = _txtNom.Text.Trim();
        Utilisateur.Identifiant = _txtIdentifiant.Text.Trim();
        Utilisateur.Role = _cbRole.SelectedItem?.ToString() ?? RoleUtilisateur.Pharmacien;

        DialogResult = DialogResult.OK;
        Close();
    }
}
