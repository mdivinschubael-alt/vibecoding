using System.Drawing;
using System.Windows.Forms;
using PharmaGestion.Core.Models;
using PharmaGestion.UI.Common;

namespace PharmaGestion.UI.Forms;

public class ProduitDialog : Form
{
    private readonly TextBox _txtReference = new();
    private readonly TextBox _txtDesignation = new();
    private readonly ComboBox _cbCategorie = new();
    private readonly TextBox _txtDosage = new();
    private readonly ComboBox _cbUnite = new();
    private readonly NumericUpDown _numPrixVente = new();
    private readonly NumericUpDown _numStockMini = new();
    private readonly CheckBox _chkActif = new();

    public Produit Produit { get; private set; }

    public ProduitDialog(Produit? produitExistant = null)
    {
        Produit = produitExistant ?? new Produit();

        Text = produitExistant == null ? "Nouveau Produit Pharmaceutique" : "Modifier le Produit";
        Size = new Size(540, 520);
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
        const int inputX = 170;
        const int inputW = 320;
        const int rowGap = 38;

        AddRow(panelContent, "Référence unique *", _txtReference, labelX, inputX, inputW, ref y, rowGap);
        AddRow(panelContent, "Désignation *", _txtDesignation, labelX, inputX, inputW, ref y, rowGap);

        _cbCategorie.DropDownStyle = ComboBoxStyle.DropDown;
        _cbCategorie.Items.AddRange(["Analgésique / Antipyrétique", "Antibiotique", "Anti-inflammatoire", "Antispasmodique", "Cardiologie", "Dermatologie", "Vitamines & Compléments", "Autre"]);
        AddRow(panelContent, "Catégorie *", _cbCategorie, labelX, inputX, inputW, ref y, rowGap);

        AddRow(panelContent, "Dosage / Forme", _txtDosage, labelX, inputX, inputW, ref y, rowGap);

        _cbUnite.DropDownStyle = ComboBoxStyle.DropDown;
        _cbUnite.Items.AddRange(["Boîte", "Flacon", "Plaquette", "Ampoule", "Tube", "Sachet"]);
        AddRow(panelContent, "Unité de vente", _cbUnite, labelX, inputX, inputW, ref y, rowGap);

        // Configuration pour la précision décimale (2 à 4 décimales pour les prix)
        _numPrixVente.DecimalPlaces = 2;
        _numPrixVente.Maximum = 1_000_000m;
        _numPrixVente.Minimum = 0.00m;
        _numPrixVente.Increment = 0.50m;
        AddRow(panelContent, "Prix de vente (€) *", _numPrixVente, labelX, inputX, inputW, ref y, rowGap);

        _numStockMini.Maximum = 100_000;
        _numStockMini.Minimum = 0;
        AddRow(panelContent, "Stock minimum", _numStockMini, labelX, inputX, inputW, ref y, rowGap);

        _chkActif.Text = "Produit actif (disponible à la vente)";
        _chkActif.Checked = true;
        _chkActif.Location = new Point(inputX, y);
        _chkActif.AutoSize = true;
        panelContent.Controls.Add(_chkActif);
        y += rowGap;

        // Boutons
        var panelBottom = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 65,
            BackColor = Color.FromArgb(248, 250, 252),
            Padding = new Padding(20, 15, 20, 15)
        };

        var btnSave = UiTheme.CreatePrimaryButton("Enregistrer", 130, 36);
        btnSave.Location = new Point(370, 14);
        btnSave.Click += BtnSave_Click;

        var btnCancel = UiTheme.CreateSecondaryButton("Annuler", 100, 36);
        btnCancel.Location = new Point(255, 14);
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
        _txtReference.Text = Produit.ReferenceUnique;
        _txtDesignation.Text = Produit.Designation;
        _cbCategorie.Text = Produit.Categorie;
        _txtDosage.Text = Produit.DosageForme;
        _cbUnite.Text = string.IsNullOrEmpty(Produit.UniteVente) ? "Boîte" : Produit.UniteVente;
        _numPrixVente.Value = Produit.PrixVente;
        _numStockMini.Value = Produit.StockMinimum;
        _chkActif.Checked = Produit.Actif;
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_txtReference.Text))
        {
            MessageBox.Show("La référence unique est requise.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _txtReference.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(_txtDesignation.Text))
        {
            MessageBox.Show("La désignation commerciale est requise.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _txtDesignation.Focus();
            return;
        }

        Produit.ReferenceUnique = _txtReference.Text.Trim();
        Produit.Designation = _txtDesignation.Text.Trim();
        Produit.Categorie = _cbCategorie.Text.Trim();
        Produit.DosageForme = _txtDosage.Text.Trim();
        Produit.UniteVente = _cbUnite.Text.Trim();
        Produit.PrixVente = _numPrixVente.Value;
        Produit.StockMinimum = (int)_numStockMini.Value;
        Produit.Actif = _chkActif.Checked;

        DialogResult = DialogResult.OK;
        Close();
    }
}
