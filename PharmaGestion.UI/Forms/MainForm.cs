using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using PharmaGestion.BLL.Services;
using PharmaGestion.Core.Common;
using PharmaGestion.Core.Models;
using PharmaGestion.UI.Common;

namespace PharmaGestion.UI.Forms;

public class MainForm : Form
{
    private readonly IAuthService _authService;
    private readonly IProduitService _produitService;
    private readonly ILotService _lotService;
    private readonly IFournisseurService _fournisseurService;
    private readonly IUtilisateurService _utilisateurService;

    // Éléments d'interface
    private readonly Panel _panelSidebar = new();
    private readonly Panel _panelHeader = new();
    private readonly Panel _panelContainer = new();
    private readonly Label _lblUserBadge = new();

    // Onglets / Vues
    private readonly Panel _viewDashboard = new();
    private readonly Panel _viewProduits = new();
    private readonly Panel _viewLots = new();
    private readonly Panel _viewFournisseurs = new();
    private readonly Panel _viewUtilisateurs = new();

    // Boutons navigation
    private readonly Button _btnNavDashboard = new();
    private readonly Button _btnNavProduits = new();
    private readonly Button _btnNavLots = new();
    private readonly Button _btnNavFournisseurs = new();
    private readonly Button _btnNavUtilisateurs = new();

    // Grilles de données
    private readonly DataGridView _dgvProduits = new();
    private readonly DataGridView _dgvLots = new();
    private readonly DataGridView _dgvFournisseurs = new();
    private readonly DataGridView _dgvUtilisateurs = new();
    private readonly DataGridView _dgvAlertesStock = new();
    private readonly DataGridView _dgvAlertesPeremption = new();

    // KPI Labels
    private readonly Label _lblKpiProduits = new();
    private readonly Label _lblKpiValeurStock = new();
    private readonly Label _lblKpiAlertesStock = new();
    private readonly Label _lblKpiAlertesPeremption = new();

    // Filtres
    private readonly TextBox _txtSearchProduit = new();
    private readonly TextBox _txtSearchLot = new();
    private readonly TextBox _txtSearchFournisseur = new();

    public MainForm(
        IAuthService authService,
        IProduitService produitService,
        ILotService lotService,
        IFournisseurService fournisseurService,
        IUtilisateurService utilisateurService)
    {
        _authService = authService;
        _produitService = produitService;
        _lotService = lotService;
        _fournisseurService = fournisseurService;
        _utilisateurService = utilisateurService;

        Text = "PharmaGestion - Système de Gestion Pharmaceutique Hors Ligne (SQLite)";
        Size = new Size(1280, 800);
        MinimumSize = new Size(1100, 700);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = UiTheme.Background;
        Font = UiTheme.BodyFont;

        InitializeComponentsLayout();
        LoadUserSession();
        ShowView(_viewDashboard, _btnNavDashboard);
    }

    private void InitializeComponentsLayout()
    {
        // 1. Sidebar à gauche
        _panelSidebar.Dock = DockStyle.Left;
        _panelSidebar.Width = 240;
        _panelSidebar.BackColor = UiTheme.SidebarDark;
        BuildSidebar();

        // 2. Header en haut
        _panelHeader.Dock = DockStyle.Top;
        _panelHeader.Height = 65;
        _panelHeader.BackColor = Color.White;
        BuildHeader();

        // 3. Container central
        _panelContainer.Dock = DockStyle.Fill;
        _panelContainer.BackColor = UiTheme.Background;

        BuildDashboardView();
        BuildProduitsView();
        BuildLotsView();
        BuildFournisseursView();
        BuildUtilisateursView();

        _panelContainer.Controls.AddRange([
            _viewDashboard,
            _viewProduits,
            _viewLots,
            _viewFournisseurs,
            _viewUtilisateurs
        ]);

        Controls.Add(_panelContainer);
        Controls.Add(_panelHeader);
        Controls.Add(_panelSidebar);
    }

    private void BuildSidebar()
    {
        // Titre dans le menu
        var pnlLogo = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.FromArgb(11, 19, 36) };
        var lblBrand = new Label
        {
            Text = "💊 PharmaGestion",
            Font = new Font("Segoe UI", 13f, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(20, 26)
        };
        pnlLogo.Controls.Add(lblBrand);
        _panelSidebar.Controls.Add(pnlLogo);

        // Boutons de navigation
        var pnlNav = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10, 15, 10, 15) };
        int y = 10;
        const int btnH = 44;
        const int btnGap = 8;

        SetupNavButton(_btnNavDashboard, "📊  Tableau de bord", ref y, btnH, btnGap, pnlNav, () => ShowView(_viewDashboard, _btnNavDashboard));
        SetupNavButton(_btnNavProduits, "💊  Produits & Tarifs", ref y, btnH, btnGap, pnlNav, () => ShowView(_viewProduits, _btnNavProduits));
        SetupNavButton(_btnNavLots, "📦  Lots & Péremptions", ref y, btnH, btnGap, pnlNav, () => ShowView(_viewLots, _btnNavLots));
        SetupNavButton(_btnNavFournisseurs, "🏢  Fournisseurs", ref y, btnH, btnGap, pnlNav, () => ShowView(_viewFournisseurs, _btnNavFournisseurs));
        SetupNavButton(_btnNavUtilisateurs, "👥  Utilisateurs (Admin)", ref y, btnH, btnGap, pnlNav, () => ShowView(_viewUtilisateurs, _btnNavUtilisateurs));

        // Footer version & hors-ligne
        var pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 60, BackColor = Color.FromArgb(11, 19, 36) };
        var lblOffline = new Label
        {
            Text = "🟢 Mode 100% Hors Ligne\nBase SQLite Locale",
            ForeColor = Color.FromArgb(148, 163, 184),
            Font = UiTheme.SmallFont,
            Location = new Point(15, 12),
            AutoSize = true
        };
        pnlFooter.Controls.Add(lblOffline);

        _panelSidebar.Controls.Add(pnlNav);
        _panelSidebar.Controls.Add(pnlFooter);
    }

    private static void SetupNavButton(Button btn, string text, ref int y, int height, int gap, Panel parent, Action onClick)
    {
        btn.Text = text;
        btn.TextAlign = ContentAlignment.MiddleLeft;
        btn.Location = new Point(5, y);
        btn.Size = new Size(210, height);
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize = 0;
        btn.ForeColor = Color.FromArgb(203, 213, 225);
        btn.Font = UiTheme.BodyBoldFont;
        btn.BackColor = Color.Transparent;
        btn.Cursor = Cursors.Hand;
        btn.Click += (_, _) => onClick();
        parent.Controls.Add(btn);
        y += height + gap;
    }

    private void BuildHeader()
    {
        var lblTitle = new Label
        {
            Text = "Système de Gestion Officinale Sécurisé",
            Font = UiTheme.SubHeaderFont,
            ForeColor = UiTheme.TextMain,
            Location = new Point(25, 20),
            AutoSize = true
        };

        _lblUserBadge.Font = UiTheme.BodyBoldFont;
        _lblUserBadge.ForeColor = UiTheme.PrimaryDark;
        _lblUserBadge.Location = new Point(600, 22);
        _lblUserBadge.AutoSize = true;

        var btnLogout = UiTheme.CreateSecondaryButton("🚪 Déconnexion", 120, 34);
        btnLogout.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnLogout.Location = new Point(_panelHeader.Width - 140, 16);
        btnLogout.Click += BtnLogout_Click;

        _panelHeader.Controls.AddRange([lblTitle, _lblUserBadge, btnLogout]);
        _panelHeader.Resize += (_, _) => { btnLogout.Location = new Point(_panelHeader.Width - 140, 16); };
    }

    private void LoadUserSession()
    {
        var user = _authService.CurrentUser;
        if (user != null)
        {
            _lblUserBadge.Text = $"👤 {user.Nom} ({user.Role})";
            _btnNavUtilisateurs.Visible = _authService.IsAdmin;
        }
    }

    private void ShowView(Panel targetView, Button activeButton)
    {
        _viewDashboard.Visible = false;
        _viewProduits.Visible = false;
        _viewLots.Visible = false;
        _viewFournisseurs.Visible = false;
        _viewUtilisateurs.Visible = false;

        foreach (var btn in new[] { _btnNavDashboard, _btnNavProduits, _btnNavLots, _btnNavFournisseurs, _btnNavUtilisateurs })
        {
            btn.BackColor = Color.Transparent;
            btn.ForeColor = Color.FromArgb(203, 213, 225);
        }

        activeButton.BackColor = UiTheme.SidebarActive;
        activeButton.ForeColor = Color.White;

        targetView.Visible = true;
        targetView.BringToFront();

        // Rafraîchir les données de la vue
        if (targetView == _viewDashboard) RefreshDashboardData();
        else if (targetView == _viewProduits) RefreshProduitsData();
        else if (targetView == _viewLots) RefreshLotsData();
        else if (targetView == _viewFournisseurs) RefreshFournisseursData();
        else if (targetView == _viewUtilisateurs) RefreshUtilisateursData();
    }

    #region 1. Dashboard View
    private void BuildDashboardView()
    {
        _viewDashboard.Dock = DockStyle.Fill;
        _viewDashboard.AutoScroll = true;
        _viewDashboard.Padding = new Padding(25);

        // Panneau KPI
        var pnlKpis = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 110,
            WrapContents = false,
            AutoScroll = true
        };

        pnlKpis.Controls.Add(CreateKpiCard("💊 Produits au catalogue", _lblKpiProduits, UiTheme.Primary));
        pnlKpis.Controls.Add(CreateKpiCard("💰 Valeur stock estimée", _lblKpiValeurStock, Color.FromArgb(59, 130, 246)));
        pnlKpis.Controls.Add(CreateKpiCard("⚠️ Alertes stock bas", _lblKpiAlertesStock, UiTheme.Warning));
        pnlKpis.Controls.Add(CreateKpiCard("⏳ Alertes péremptions (≤90j)", _lblKpiAlertesPeremption, UiTheme.Danger));

        // Panneau central 2 tables d'alertes
        var splitAlerts = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(0, 15, 0, 0)
        };
        splitAlerts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        splitAlerts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));

        // Grille alerte stock
        var pnlStockAlert = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(15) };
        var lblStockAlertTitle = new Label
        {
            Text = "⚠️ Produits en rupture ou stock minimum atteint",
            Font = UiTheme.BodyBoldFont,
            ForeColor = UiTheme.Warning,
            Dock = DockStyle.Top,
            Height = 30
        };
        UiTheme.StyleDataGridView(_dgvAlertesStock);
        _dgvAlertesStock.Dock = DockStyle.Fill;
        pnlStockAlert.Controls.Add(_dgvAlertesStock);
        pnlStockAlert.Controls.Add(lblStockAlertTitle);

        // Grille alerte péremption
        var pnlPeremptionAlert = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(15) };
        var lblPeremptionAlertTitle = new Label
        {
            Text = "⏳ Lots proches de la péremption ou expirés",
            Font = UiTheme.BodyBoldFont,
            ForeColor = UiTheme.Danger,
            Dock = DockStyle.Top,
            Height = 30
        };
        UiTheme.StyleDataGridView(_dgvAlertesPeremption);
        _dgvAlertesPeremption.Dock = DockStyle.Fill;
        pnlPeremptionAlert.Controls.Add(_dgvAlertesPeremption);
        pnlPeremptionAlert.Controls.Add(lblPeremptionAlertTitle);

        splitAlerts.Controls.Add(pnlStockAlert, 0, 0);
        splitAlerts.Controls.Add(pnlPeremptionAlert, 1, 0);

        _viewDashboard.Controls.Add(splitAlerts);
        _viewDashboard.Controls.Add(pnlKpis);
    }

    private static Panel CreateKpiCard(string title, Label valLabel, Color accentColor)
    {
        var card = new Panel
        {
            Width = 230,
            Height = 90,
            BackColor = Color.White,
            Margin = new Padding(0, 0, 15, 0),
            Padding = new Padding(15, 12, 15, 12)
        };

        var pnlAccent = new Panel { Dock = DockStyle.Left, Width = 5, BackColor = accentColor };
        var lblTitle = new Label
        {
            Text = title,
            Font = UiTheme.SmallFont,
            ForeColor = UiTheme.TextMuted,
            Dock = DockStyle.Top,
            Height = 22
        };

        valLabel.Font = new Font("Segoe UI", 16f, FontStyle.Bold);
        valLabel.ForeColor = UiTheme.TextMain;
        valLabel.Dock = DockStyle.Fill;
        valLabel.Text = "...";

        card.Controls.AddRange([valLabel, lblTitle, pnlAccent]);
        return card;
    }

    private void RefreshDashboardData()
    {
        var produits = _produitService.GetAll();
        var lots = _lotService.GetAll();
        var alertesStock = _produitService.GetAlertesStock();
        var alertesPeremption = _lotService.GetAlertesPeremption(90);

        // Calcul décimal exact de la valeur totale du stock
        decimal valeurStock = 0.00m;
        foreach (var l in lots)
        {
            valeurStock += (decimal)l.QuantiteActuelle * l.CoutAchat;
        }

        _lblKpiProduits.Text = produits.Count.ToString();
        _lblKpiValeurStock.Text = valeurStock.ToString("C2", CultureInfo.CurrentCulture);
        _lblKpiAlertesStock.Text = alertesStock.Count.ToString();
        _lblKpiAlertesPeremption.Text = alertesPeremption.Count.ToString();

        // Remplir tableau alerte stock
        var dtStock = new DataTable();
        dtStock.Columns.Add("Référence");
        dtStock.Columns.Add("Désignation");
        dtStock.Columns.Add("Stock Actuel", typeof(int));
        dtStock.Columns.Add("Stock Mini", typeof(int));

        foreach (var p in alertesStock)
        {
            dtStock.Rows.Add(p.ReferenceUnique, p.Designation, p.StockTotal, p.StockMinimum);
        }
        _dgvAlertesStock.DataSource = dtStock;

        // Remplir tableau alerte péremption
        var dtPeremp = new DataTable();
        dtPeremp.Columns.Add("Produit");
        dtPeremp.Columns.Add("N° Lot");
        dtPeremp.Columns.Add("Péremption");
        dtPeremp.Columns.Add("Jours restants", typeof(int));
        dtPeremp.Columns.Add("Stock", typeof(int));

        foreach (var l in alertesPeremption)
        {
            dtPeremp.Rows.Add(l.ProduitDesignation, l.NumeroLot, l.DatePeremption.ToString("dd/MM/yyyy"), l.JoursRestants, l.QuantiteActuelle);
        }
        _dgvAlertesPeremption.DataSource = dtPeremp;
    }
    #endregion

    #region 2. Produits View
    private void BuildProduitsView()
    {
        _viewProduits.Dock = DockStyle.Fill;
        _viewProduits.Padding = new Padding(25);

        // Barre supérieure
        var topBar = new Panel { Dock = DockStyle.Top, Height = 50 };

        _txtSearchProduit.PlaceholderText = "🔍 Rechercher par désignation, référence ou catégorie...";
        _txtSearchProduit.Size = new Size(340, 32);
        _txtSearchProduit.Location = new Point(0, 8);
        _txtSearchProduit.TextChanged += (_, _) => SearchProduits();

        var btnAddProduit = UiTheme.CreatePrimaryButton("+ Nouveau Produit", 160, 36);
        btnAddProduit.Location = new Point(360, 6);
        btnAddProduit.Click += BtnAddProduit_Click;

        var btnEditProduit = UiTheme.CreateSecondaryButton("Modifier", 100, 36);
        btnEditProduit.Location = new Point(530, 6);
        btnEditProduit.Click += BtnEditProduit_Click;

        var btnDeleteProduit = UiTheme.CreateDangerButton("Supprimer", 100, 36);
        btnDeleteProduit.Location = new Point(640, 6);
        btnDeleteProduit.Click += BtnDeleteProduit_Click;

        var btnRefreshProduits = UiTheme.CreateSecondaryButton("Actualiser", 100, 36);
        btnRefreshProduits.Location = new Point(750, 6);
        btnRefreshProduits.Click += (_, _) => RefreshProduitsData();

        topBar.Controls.AddRange([_txtSearchProduit, btnAddProduit, btnEditProduit, btnDeleteProduit, btnRefreshProduits]);

        // Grille
        UiTheme.StyleDataGridView(_dgvProduits);
        _dgvProduits.Dock = DockStyle.Fill;
        _dgvProduits.DoubleClick += (_, _) => BtnEditProduit_Click(null, EventArgs.Empty);

        var pnlGrid = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(15) };
        pnlGrid.Controls.Add(_dgvProduits);

        _viewProduits.Controls.Add(pnlGrid);
        _viewProduits.Controls.Add(topBar);
    }

    private void RefreshProduitsData()
    {
        var list = _produitService.GetAll();
        BindProduitsGrid(list);
    }

    private void SearchProduits()
    {
        var list = _produitService.Search(_txtSearchProduit.Text);
        BindProduitsGrid(list);
    }

    private void BindProduitsGrid(List<Produit> list)
    {
        var dt = new DataTable();
        dt.Columns.Add("Id", typeof(int));
        dt.Columns.Add("Référence");
        dt.Columns.Add("Désignation");
        dt.Columns.Add("Catégorie");
        dt.Columns.Add("Dosage / Forme");
        dt.Columns.Add("Unité");
        dt.Columns.Add("Prix Vente (€)", typeof(string));
        dt.Columns.Add("Stock Mini", typeof(int));
        dt.Columns.Add("Stock Actuel", typeof(int));
        dt.Columns.Add("Statut");

        foreach (var p in list)
        {
            string statut = !p.Actif ? "Inactif" : (p.EnAlerteStock ? "⚠️ Stock Bas" : "OK");
            dt.Rows.Add(
                p.Id,
                p.ReferenceUnique,
                p.Designation,
                p.Categorie,
                p.DosageForme,
                p.UniteVente,
                p.PrixVente.ToString("0.00 €", CultureInfo.CurrentCulture),
                p.StockMinimum,
                p.StockTotal,
                statut
            );
        }

        _dgvProduits.DataSource = dt;
        if (_dgvProduits.Columns["Id"] is { } colIdProd)
        {
            colIdProd.Visible = false;
        }
    }

    private void BtnAddProduit_Click(object? sender, EventArgs e)
    {
        using var dlg = new ProduitDialog();
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            var res = _produitService.CreerProduit(dlg.Produit);
            if (res.EstSucces)
            {
                MessageBox.Show(res.Message, "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RefreshProduitsData();
            }
            else
            {
                MessageBox.Show(res.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }

    private void BtnEditProduit_Click(object? sender, EventArgs e)
    {
        if (_dgvProduits.SelectedRows.Count == 0) return;
        int id = Convert.ToInt32(_dgvProduits.SelectedRows[0].Cells["Id"].Value);
        var produit = _produitService.GetById(id);
        if (produit == null) return;

        using var dlg = new ProduitDialog(produit);
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            var res = _produitService.ModifierProduit(dlg.Produit);
            if (res.EstSucces)
            {
                MessageBox.Show(res.Message, "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RefreshProduitsData();
            }
            else
            {
                MessageBox.Show(res.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }

    private void BtnDeleteProduit_Click(object? sender, EventArgs e)
    {
        if (_dgvProduits.SelectedRows.Count == 0) return;
        int id = Convert.ToInt32(_dgvProduits.SelectedRows[0].Cells["Id"].Value);
        string nom = _dgvProduits.SelectedRows[0].Cells["Désignation"]?.Value?.ToString() ?? "";

        var confirm = MessageBox.Show($"Confirmez-vous la suppression du produit '{nom}' ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm == DialogResult.Yes)
        {
            var res = _produitService.SupprimerProduit(id);
            if (res.EstSucces)
            {
                MessageBox.Show(res.Message, "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RefreshProduitsData();
            }
            else
            {
                MessageBox.Show(res.Message, "Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
    #endregion

    #region 3. Lots & Stock View
    private void BuildLotsView()
    {
        _viewLots.Dock = DockStyle.Fill;
        _viewLots.Padding = new Padding(25);

        var topBar = new Panel { Dock = DockStyle.Top, Height = 50 };

        _txtSearchLot.PlaceholderText = "🔍 Filtrer par médicament ou N° de lot...";
        _txtSearchLot.Size = new Size(320, 32);
        _txtSearchLot.Location = new Point(0, 8);
        _txtSearchLot.TextChanged += (_, _) => FilterLots();

        var btnAddLot = UiTheme.CreatePrimaryButton("+ Réceptionner un Lot", 180, 36);
        btnAddLot.Location = new Point(340, 6);
        btnAddLot.Click += BtnAddLot_Click;

        var btnSortieStock = UiTheme.CreateSecondaryButton("Sortie de stock", 130, 36);
        btnSortieStock.Location = new Point(530, 6);
        btnSortieStock.Click += BtnSortieStock_Click;

        var btnEditLot = UiTheme.CreateSecondaryButton("Modifier", 90, 36);
        btnEditLot.Location = new Point(670, 6);
        btnEditLot.Click += BtnEditLot_Click;

        var btnDeleteLot = UiTheme.CreateDangerButton("Supprimer", 100, 36);
        btnDeleteLot.Location = new Point(770, 6);
        btnDeleteLot.Click += BtnDeleteLot_Click;

        var btnRefreshLots = UiTheme.CreateSecondaryButton("Actualiser", 100, 36);
        btnRefreshLots.Location = new Point(880, 6);
        btnRefreshLots.Click += (_, _) => RefreshLotsData();

        topBar.Controls.AddRange([_txtSearchLot, btnAddLot, btnSortieStock, btnEditLot, btnDeleteLot, btnRefreshLots]);

        UiTheme.StyleDataGridView(_dgvLots);
        _dgvLots.Dock = DockStyle.Fill;

        var pnlGrid = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(15) };
        pnlGrid.Controls.Add(_dgvLots);

        _viewLots.Controls.Add(pnlGrid);
        _viewLots.Controls.Add(topBar);
    }

    private void RefreshLotsData()
    {
        var list = _lotService.GetAll();
        BindLotsGrid(list);
    }

    private void FilterLots()
    {
        string term = _txtSearchLot.Text.Trim().ToLower();
        var list = _lotService.GetAll().Where(l =>
            string.IsNullOrEmpty(term) ||
            l.NumeroLot.ToLower().Contains(term) ||
            l.ProduitDesignation.ToLower().Contains(term) ||
            l.FournisseurNom.ToLower().Contains(term)).ToList();

        BindLotsGrid(list);
    }

    private void BindLotsGrid(List<Lot> list)
    {
        var dt = new DataTable();
        dt.Columns.Add("Id", typeof(int));
        dt.Columns.Add("Médicament");
        dt.Columns.Add("N° Lot");
        dt.Columns.Add("Date Péremption");
        dt.Columns.Add("Jours Restants", typeof(int));
        dt.Columns.Add("Quantité Initiale", typeof(int));
        dt.Columns.Add("Quantité Restante", typeof(int));
        dt.Columns.Add("Coût Achat (€)", typeof(string));
        dt.Columns.Add("Fournisseur");
        dt.Columns.Add("Date Réception");
        dt.Columns.Add("Statut");

        foreach (var l in list)
        {
            string statut = l.EstPerime ? "❌ Périmé" : (l.EstProchePeremption ? "⚠️ Expire bientôt" : "Conforme");
            dt.Rows.Add(
                l.Id,
                l.ProduitDesignation,
                l.NumeroLot,
                l.DatePeremption.ToString("dd/MM/yyyy"),
                l.JoursRestants,
                l.QuantiteInitiale,
                l.QuantiteActuelle,
                l.CoutAchat.ToString("0.00 €", CultureInfo.CurrentCulture),
                l.FournisseurNom,
                l.DateReception.ToString("dd/MM/yyyy"),
                statut
            );
        }

        _dgvLots.DataSource = dt;
        if (_dgvLots.Columns["Id"] is { } colIdLot)
        {
            colIdLot.Visible = false;
        }
    }

    private void BtnAddLot_Click(object? sender, EventArgs e)
    {
        var produits = _produitService.GetAll(true);
        var fournisseurs = _fournisseurService.GetAll();

        if (produits.Count == 0)
        {
            MessageBox.Show("Vous devez créer au moins un médicament avant de pouvoir réceptionner un lot.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dlg = new LotDialog(produits, fournisseurs);
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            var res = _lotService.ReceptionnerLot(dlg.Lot);
            if (res.EstSucces)
            {
                MessageBox.Show(res.Message, "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RefreshLotsData();
            }
            else
            {
                MessageBox.Show(res.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }

    private void BtnSortieStock_Click(object? sender, EventArgs e)
    {
        if (_dgvLots.SelectedRows.Count == 0) return;
        int lotId = Convert.ToInt32(_dgvLots.SelectedRows[0].Cells["Id"].Value);
        string numLot = _dgvLots.SelectedRows[0].Cells["N° Lot"]?.Value?.ToString() ?? "";

        using var inputForm = new Form
        {
            Text = $"Sortie de stock - Lot {numLot}",
            Size = new Size(380, 240),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = Color.White,
            Font = UiTheme.BodyFont
        };

        var numQte = new NumericUpDown { Minimum = 1, Maximum = 100000, Value = 1, Location = new Point(140, 25), Width = 190 };
        var txtMotif = new TextBox { Text = "Vente au comptoir", Location = new Point(140, 65), Width = 190 };
        var btnValider = UiTheme.CreatePrimaryButton("Confirmer", 120, 36);
        btnValider.Location = new Point(210, 130);
        btnValider.Click += (_, _) => { inputForm.DialogResult = DialogResult.OK; inputForm.Close(); };

        inputForm.Controls.AddRange([
            new Label { Text = "Quantité sortie :", Location = new Point(20, 27), AutoSize = true },
            numQte,
            new Label { Text = "Motif sortie :", Location = new Point(20, 67), AutoSize = true },
            txtMotif,
            btnValider
        ]);

        if (inputForm.ShowDialog(this) == DialogResult.OK)
        {
            var res = _lotService.SortirStockLot(lotId, (int)numQte.Value, txtMotif.Text);
            if (res.EstSucces)
            {
                MessageBox.Show(res.Message, "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RefreshLotsData();
            }
            else
            {
                MessageBox.Show(res.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }

    private void BtnEditLot_Click(object? sender, EventArgs e)
    {
        if (_dgvLots.SelectedRows.Count == 0) return;
        int lotId = Convert.ToInt32(_dgvLots.SelectedRows[0].Cells["Id"].Value);
        var lot = _lotService.GetAll().FirstOrDefault(l => l.Id == lotId);
        if (lot == null) return;

        var produits = _produitService.GetAll();
        var fournisseurs = _fournisseurService.GetAll();

        using var dlg = new LotDialog(produits, fournisseurs, lot);
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            var res = _lotService.ModifierLot(dlg.Lot);
            if (res.EstSucces)
            {
                MessageBox.Show(res.Message, "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RefreshLotsData();
            }
            else
            {
                MessageBox.Show(res.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }

    private void BtnDeleteLot_Click(object? sender, EventArgs e)
    {
        if (_dgvLots.SelectedRows.Count == 0) return;
        int lotId = Convert.ToInt32(_dgvLots.SelectedRows[0].Cells["Id"].Value);
        string numLot = _dgvLots.SelectedRows[0].Cells["N° Lot"]?.Value?.ToString() ?? "";

        var confirm = MessageBox.Show($"Confirmez-vous la suppression définitive du lot '{numLot}' ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm == DialogResult.Yes)
        {
            var res = _lotService.SupprimerLot(lotId);
            if (res.EstSucces)
            {
                MessageBox.Show(res.Message, "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RefreshLotsData();
            }
            else
            {
                MessageBox.Show(res.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
    #endregion

    #region 4. Fournisseurs View
    private void BuildFournisseursView()
    {
        _viewFournisseurs.Dock = DockStyle.Fill;
        _viewFournisseurs.Padding = new Padding(25);

        var topBar = new Panel { Dock = DockStyle.Top, Height = 50 };

        _txtSearchFournisseur.PlaceholderText = "🔍 Rechercher un laboratoire ou fournisseur...";
        _txtSearchFournisseur.Size = new Size(340, 32);
        _txtSearchFournisseur.Location = new Point(0, 8);
        _txtSearchFournisseur.TextChanged += (_, _) => SearchFournisseurs();

        var btnAddFournisseur = UiTheme.CreatePrimaryButton("+ Nouveau Fournisseur", 180, 36);
        btnAddFournisseur.Location = new Point(360, 6);
        btnAddFournisseur.Click += BtnAddFournisseur_Click;

        var btnEditFournisseur = UiTheme.CreateSecondaryButton("Modifier", 100, 36);
        btnEditFournisseur.Location = new Point(550, 6);
        btnEditFournisseur.Click += BtnEditFournisseur_Click;

        var btnDeleteFournisseur = UiTheme.CreateDangerButton("Supprimer", 100, 36);
        btnDeleteFournisseur.Location = new Point(660, 6);
        btnDeleteFournisseur.Click += BtnDeleteFournisseur_Click;

        var btnRefreshFournisseurs = UiTheme.CreateSecondaryButton("Actualiser", 100, 36);
        btnRefreshFournisseurs.Location = new Point(770, 6);
        btnRefreshFournisseurs.Click += (_, _) => RefreshFournisseursData();

        topBar.Controls.AddRange([_txtSearchFournisseur, btnAddFournisseur, btnEditFournisseur, btnDeleteFournisseur, btnRefreshFournisseurs]);

        UiTheme.StyleDataGridView(_dgvFournisseurs);
        _dgvFournisseurs.Dock = DockStyle.Fill;

        var pnlGrid = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(15) };
        pnlGrid.Controls.Add(_dgvFournisseurs);

        _viewFournisseurs.Controls.Add(pnlGrid);
        _viewFournisseurs.Controls.Add(topBar);
    }

    private void RefreshFournisseursData()
    {
        var list = _fournisseurService.GetAll();
        BindFournisseursGrid(list);
    }

    private void SearchFournisseurs()
    {
        var list = _fournisseurService.Search(_txtSearchFournisseur.Text);
        BindFournisseursGrid(list);
    }

    private void BindFournisseursGrid(List<Fournisseur> list)
    {
        var dt = new DataTable();
        dt.Columns.Add("Id", typeof(int));
        dt.Columns.Add("Nom / Laboratoire");
        dt.Columns.Add("Téléphone");
        dt.Columns.Add("Email");
        dt.Columns.Add("Adresse");
        dt.Columns.Add("Enregistré le");

        foreach (var f in list)
        {
            dt.Rows.Add(f.Id, f.Nom, f.Telephone, f.Email, f.Adresse, f.DateCreation.ToString("dd/MM/yyyy"));
        }

        _dgvFournisseurs.DataSource = dt;
        if (_dgvFournisseurs.Columns["Id"] is { } colIdFourn)
        {
            colIdFourn.Visible = false;
        }
    }

    private void BtnAddFournisseur_Click(object? sender, EventArgs e)
    {
        using var dlg = new FournisseurDialog();
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            var res = _fournisseurService.CreerFournisseur(dlg.Fournisseur);
            if (res.EstSucces)
            {
                MessageBox.Show(res.Message, "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RefreshFournisseursData();
            }
            else
            {
                MessageBox.Show(res.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }

    private void BtnEditFournisseur_Click(object? sender, EventArgs e)
    {
        if (_dgvFournisseurs.SelectedRows.Count == 0) return;
        int id = Convert.ToInt32(_dgvFournisseurs.SelectedRows[0].Cells["Id"].Value);
        var f = _fournisseurService.GetById(id);
        if (f == null) return;

        using var dlg = new FournisseurDialog(f);
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            var res = _fournisseurService.ModifierFournisseur(dlg.Fournisseur);
            if (res.EstSucces)
            {
                MessageBox.Show(res.Message, "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RefreshFournisseursData();
            }
            else
            {
                MessageBox.Show(res.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }

    private void BtnDeleteFournisseur_Click(object? sender, EventArgs e)
    {
        if (_dgvFournisseurs.SelectedRows.Count == 0) return;
        int id = Convert.ToInt32(_dgvFournisseurs.SelectedRows[0].Cells["Id"].Value);
        string nom = _dgvFournisseurs.SelectedRows[0].Cells["Nom / Laboratoire"]?.Value?.ToString() ?? "";

        var confirm = MessageBox.Show($"Confirmez-vous la suppression du fournisseur '{nom}' ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm == DialogResult.Yes)
        {
            var res = _fournisseurService.SupprimerFournisseur(id);
            if (res.EstSucces)
            {
                MessageBox.Show(res.Message, "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RefreshFournisseursData();
            }
            else
            {
                MessageBox.Show(res.Message, "Attention", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
    #endregion

    #region 5. Utilisateurs View (Admin Only)
    private void BuildUtilisateursView()
    {
        _viewUtilisateurs.Dock = DockStyle.Fill;
        _viewUtilisateurs.Padding = new Padding(25);

        var topBar = new Panel { Dock = DockStyle.Top, Height = 50 };

        var btnAddUser = UiTheme.CreatePrimaryButton("+ Nouvel Utilisateur", 180, 36);
        btnAddUser.Location = new Point(0, 6);
        btnAddUser.Click += BtnAddUser_Click;

        var btnEditUser = UiTheme.CreateSecondaryButton("Modifier Rôle / Identifiant", 200, 36);
        btnEditUser.Location = new Point(190, 6);
        btnEditUser.Click += BtnEditUser_Click;

        var btnDeleteUser = UiTheme.CreateDangerButton("Supprimer", 100, 36);
        btnDeleteUser.Location = new Point(400, 6);
        btnDeleteUser.Click += BtnDeleteUser_Click;

        var btnRefreshUsers = UiTheme.CreateSecondaryButton("Actualiser", 100, 36);
        btnRefreshUsers.Location = new Point(510, 6);
        btnRefreshUsers.Click += (_, _) => RefreshUtilisateursData();

        topBar.Controls.AddRange([btnAddUser, btnEditUser, btnDeleteUser, btnRefreshUsers]);

        UiTheme.StyleDataGridView(_dgvUtilisateurs);
        _dgvUtilisateurs.Dock = DockStyle.Fill;

        var pnlGrid = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(15) };
        pnlGrid.Controls.Add(_dgvUtilisateurs);

        _viewUtilisateurs.Controls.Add(pnlGrid);
        _viewUtilisateurs.Controls.Add(topBar);
    }

    private void RefreshUtilisateursData()
    {
        var list = _utilisateurService.GetAll();
        var dt = new DataTable();
        dt.Columns.Add("Id", typeof(int));
        dt.Columns.Add("Nom");
        dt.Columns.Add("Identifiant");
        dt.Columns.Add("Rôle");
        dt.Columns.Add("Date Création");

        foreach (var u in list)
        {
            dt.Rows.Add(u.Id, u.Nom, u.Identifiant, u.Role, u.DateCreation.ToString("dd/MM/yyyy HH:mm"));
        }

        _dgvUtilisateurs.DataSource = dt;
        if (_dgvUtilisateurs.Columns["Id"] is { } colIdUser)
        {
            colIdUser.Visible = false;
        }
    }

    private void BtnAddUser_Click(object? sender, EventArgs e)
    {
        using var dlg = new UtilisateurDialog();
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            var res = _utilisateurService.CreerUtilisateur(dlg.Utilisateur.Nom, dlg.Utilisateur.Identifiant, dlg.NouveauMotDePasse, dlg.Utilisateur.Role);
            if (res.EstSucces)
            {
                MessageBox.Show(res.Message, "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RefreshUtilisateursData();
            }
            else
            {
                MessageBox.Show(res.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }

    private void BtnEditUser_Click(object? sender, EventArgs e)
    {
        if (_dgvUtilisateurs.SelectedRows.Count == 0) return;
        int id = Convert.ToInt32(_dgvUtilisateurs.SelectedRows[0].Cells["Id"].Value);
        var u = _utilisateurService.GetById(id);
        if (u == null) return;

        using var dlg = new UtilisateurDialog(u);
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            var res = _utilisateurService.ModifierUtilisateur(id, dlg.Utilisateur.Nom, dlg.Utilisateur.Identifiant, dlg.Utilisateur.Role);
            if (!string.IsNullOrWhiteSpace(dlg.NouveauMotDePasse))
            {
                _utilisateurService.ReinitialiserMotDePasse(id, dlg.NouveauMotDePasse);
            }

            if (res.EstSucces)
            {
                MessageBox.Show(res.Message, "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RefreshUtilisateursData();
            }
            else
            {
                MessageBox.Show(res.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }

    private void BtnDeleteUser_Click(object? sender, EventArgs e)
    {
        if (_dgvUtilisateurs.SelectedRows.Count == 0) return;
        int id = Convert.ToInt32(_dgvUtilisateurs.SelectedRows[0].Cells["Id"].Value);
        string nom = _dgvUtilisateurs.SelectedRows[0].Cells["Nom"]?.Value?.ToString() ?? "";

        int currentId = _authService.CurrentUser?.Id ?? 0;
        var confirm = MessageBox.Show($"Confirmez-vous la suppression du compte de '{nom}' ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm == DialogResult.Yes)
        {
            var res = _utilisateurService.SupprimerUtilisateur(id, currentId);
            if (res.EstSucces)
            {
                MessageBox.Show(res.Message, "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RefreshUtilisateursData();
            }
            else
            {
                MessageBox.Show(res.Message, "Attention", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
    #endregion

    private void BtnLogout_Click(object? sender, EventArgs e)
    {
        var confirm = MessageBox.Show("Êtes-vous sûr de vouloir vous déconnecter ?", "Déconnexion", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm == DialogResult.Yes)
        {
            _authService.Logout();
            Hide();
            using var loginForm = new LoginForm(_authService);
            if (loginForm.ShowDialog() == DialogResult.OK)
            {
                LoadUserSession();
                ShowView(_viewDashboard, _btnNavDashboard);
                Show();
            }
            else
            {
                Close();
            }
        }
    }
}
