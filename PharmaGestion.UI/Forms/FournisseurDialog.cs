using System.Drawing;
using System.Windows.Forms;
using PharmaGestion.Core.Models;
using PharmaGestion.UI.Common;

namespace PharmaGestion.UI.Forms;

public class FournisseurDialog : Form
{
    private readonly TextBox _txtNom = new();
    private readonly TextBox _txtTelephone = new();
    private readonly TextBox _txtAdresse = new();
    private readonly TextBox _txtEmail = new();

    public Fournisseur Fournisseur { get; private set; }

    public FournisseurDialog(Fournisseur? fournisseurExistant = null)
    {
        Fournisseur = fournisseurExistant ?? new Fournisseur();

        Text = fournisseurExistant == null ? "Nouveau Fournisseur / Laboratoire" : "Modifier le Fournisseur";
        Size = new Size(520, 420);
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
        const int inputW = 320;
        const int rowGap = 38;

        AddRow(panelContent, "Nom ou Raison *", _txtNom, labelX, inputX, inputW, ref y, rowGap);
        AddRow(panelContent, "Téléphone", _txtTelephone, labelX, inputX, inputW, ref y, rowGap);
        AddRow(panelContent, "Email", _txtEmail, labelX, inputX, inputW, ref y, rowGap);
        
        _txtAdresse.Multiline = true;
        _txtAdresse.Height = 60;
        AddRow(panelContent, "Adresse", _txtAdresse, labelX, inputX, inputW, ref y, 70);

        // Boutons
        var panelBottom = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 65,
            BackColor = Color.FromArgb(248, 250, 252),
            Padding = new Padding(20, 15, 20, 15)
        };

        var btnSave = UiTheme.CreatePrimaryButton("Enregistrer", 130, 36);
        btnSave.Location = new Point(350, 14);
        btnSave.Click += BtnSave_Click;

        var btnCancel = UiTheme.CreateSecondaryButton("Annuler", 100, 36);
        btnCancel.Location = new Point(235, 14);
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
        _txtNom.Text = Fournisseur.Nom;
        _txtTelephone.Text = Fournisseur.Telephone;
        _txtAdresse.Text = Fournisseur.Adresse;
        _txtEmail.Text = Fournisseur.Email;
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_txtNom.Text))
        {
            MessageBox.Show("Le nom du fournisseur est obligatoire.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _txtNom.Focus();
            return;
        }

        Fournisseur.Nom = _txtNom.Text.Trim();
        Fournisseur.Telephone = _txtTelephone.Text.Trim();
        Fournisseur.Adresse = _txtAdresse.Text.Trim();
        Fournisseur.Email = _txtEmail.Text.Trim();

        DialogResult = DialogResult.OK;
        Close();
    }
}
