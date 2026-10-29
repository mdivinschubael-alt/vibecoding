using System.Drawing;
using System.Windows.Forms;
using PharmaGestion.Core.Models;
using PharmaGestion.UI.Common;

namespace PharmaGestion.UI.Forms;

public class LotDialog : Form
{
    private readonly ComboBox _cbProduit = new();
    private readonly ComboBox _cbFournisseur = new();
    private readonly TextBox _txtNumeroLot = new();
    private readonly DateTimePicker _dtpPeremption = new();
    private readonly NumericUpDown _numQteInitiale = new();
    private readonly NumericUpDown _numQteActuelle = new();
    private readonly NumericUpDown _numCoutAchat = new();
    private readonly DateTimePicker _dtpReception = new();

    private readonly List<Produit> _produits;
    private readonly List<Fournisseur> _fournisseurs;
    private readonly bool _isModification;

    public Lot Lot { get; private set; }

    public LotDialog(List<Produit> produits, List<Fournisseur> fournisseurs, Lot? lotExistant = null, int? produitPreselectionneId = null)
    {
        _produits = produits;
        _fournisseurs = fournisseurs;
        _isModification = lotExistant != null;
        Lot = lotExistant ?? new Lot();

        if (!_isModification && produitPreselectionneId.HasValue)
        {
            Lot.ProduitId = produitPreselectionneId.Value;
        }

        Text = _isModification ? "Modifier les Informations du Lot" : "Réceptionner un Nouveau Lot de Stock";
        Size = new Size(540, 560);
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
        const int inputX = 180;
        const int inputW = 310;
        const int rowGap = 38;

        // ComboBox Produit
        _cbProduit.DropDownStyle = ComboBoxStyle.DropDownList;
        _cbProduit.DisplayMember = "Designation";
        _cbProduit.ValueMember = "Id";
        _cbProduit.DataSource = _produits;
        _cbProduit.Enabled = !_isModification; // Ne pas changer le produit d'un lot existant
        AddRow(panelContent, "Médicament associé *", _cbProduit, labelX, inputX, inputW, ref y, rowGap);

        // ComboBox Fournisseur
        _cbFournisseur.DropDownStyle = ComboBoxStyle.DropDownList;
        _cbFournisseur.DisplayMember = "Nom";
        _cbFournisseur.ValueMember = "Id";
        var listeFournisseursAvecVide = new List<Fournisseur>
        {
            new() { Id = 0, Nom = "-- Aucun / Non spécifié --" }
        };
        listeFournisseursAvecVide.AddRange(_fournisseurs);
        _cbFournisseur.DataSource = listeFournisseursAvecVide;
        AddRow(panelContent, "Fournisseur / Labo", _cbFournisseur, labelX, inputX, inputW, ref y, rowGap);

        AddRow(panelContent, "Numéro de Lot *", _txtNumeroLot, labelX, inputX, inputW, ref y, rowGap);

        _dtpPeremption.Format = DateTimePickerFormat.Short;
        _dtpPeremption.MinDate = DateTime.Today.AddDays(-365);
        _dtpPeremption.Value = DateTime.Today.AddYears(1);
        AddRow(panelContent, "Date de péremption *", _dtpPeremption, labelX, inputX, inputW, ref y, rowGap);

        _numQteInitiale.Maximum = 1_000_000;
        _numQteInitiale.Minimum = 1;
        _numQteInitiale.Value = 100;
        _numQteInitiale.ValueChanged += (_, _) =>
        {
            if (!_isModification)
            {
                _numQteActuelle.Value = _numQteInitiale.Value;
            }
        };
        AddRow(panelContent, "Quantité initiale *", _numQteInitiale, labelX, inputX, inputW, ref y, rowGap);

        _numQteActuelle.Maximum = 1_000_000;
        _numQteActuelle.Minimum = 0;
        _numQteActuelle.Value = 100;
        AddRow(panelContent, "Quantité restante en stock", _numQteActuelle, labelX, inputX, inputW, ref y, rowGap);

        // Coût d'achat décimal précis
        _numCoutAchat.DecimalPlaces = 2;
        _numCoutAchat.Maximum = 100_000m;
        _numCoutAchat.Minimum = 0.00m;
        _numCoutAchat.Increment = 0.50m;
        AddRow(panelContent, "Coût d'achat unitaire (€)", _numCoutAchat, labelX, inputX, inputW, ref y, rowGap);

        _dtpReception.Format = DateTimePickerFormat.Short;
        _dtpReception.Value = DateTime.Today;
        AddRow(panelContent, "Date de réception", _dtpReception, labelX, inputX, inputW, ref y, rowGap);

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
        if (Lot.ProduitId > 0)
        {
            _cbProduit.SelectedValue = Lot.ProduitId;
        }

        if (Lot.FournisseurId.HasValue && Lot.FournisseurId.Value > 0)
        {
            _cbFournisseur.SelectedValue = Lot.FournisseurId.Value;
        }

        _txtNumeroLot.Text = Lot.NumeroLot;
        if (Lot.DatePeremption > DateTime.MinValue)
        {
            _dtpPeremption.Value = Lot.DatePeremption;
        }
        _numQteInitiale.Value = Lot.QuantiteInitiale > 0 ? Lot.QuantiteInitiale : 100;
        _numQteActuelle.Value = Lot.QuantiteActuelle >= 0 ? Lot.QuantiteActuelle : _numQteInitiale.Value;
        _numCoutAchat.Value = Lot.CoutAchat;
        if (Lot.DateReception > DateTime.MinValue)
        {
            _dtpReception.Value = Lot.DateReception;
        }
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        if (_cbProduit.SelectedValue is not int pId || pId <= 0)
        {
            MessageBox.Show("Veuillez sélectionner un médicament.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(_txtNumeroLot.Text))
        {
            MessageBox.Show("Le numéro de lot est requis pour assurer la traçabilité.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _txtNumeroLot.Focus();
            return;
        }

        if (!_isModification && _dtpPeremption.Value.Date <= DateTime.Today)
        {
            MessageBox.Show("La date de péremption doit être dans le futur.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        int? fournisseurId = null;
        if (_cbFournisseur.SelectedValue is int fId && fId > 0)
        {
            fournisseurId = fId;
        }

        Lot.ProduitId = pId;
        Lot.FournisseurId = fournisseurId;
        Lot.NumeroLot = _txtNumeroLot.Text.Trim();
        Lot.DatePeremption = _dtpPeremption.Value.Date;
        Lot.QuantiteInitiale = (int)_numQteInitiale.Value;
        Lot.QuantiteActuelle = (int)_numQteActuelle.Value;
        Lot.CoutAchat = _numCoutAchat.Value;
        Lot.DateReception = _dtpReception.Value;

        DialogResult = DialogResult.OK;
        Close();
    }
}
