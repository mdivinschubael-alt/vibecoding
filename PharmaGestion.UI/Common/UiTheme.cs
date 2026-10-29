using System.Drawing;
using System.Windows.Forms;

namespace PharmaGestion.UI.Common;

public static class UiTheme
{
    public static readonly Color Primary = Color.FromArgb(13, 148, 136);       // Teal 600
    public static readonly Color PrimaryDark = Color.FromArgb(15, 118, 110);    // Teal 700
    public static readonly Color PrimaryLight = Color.FromArgb(204, 251, 241);  // Teal 100
    
    public static readonly Color SidebarDark = Color.FromArgb(15, 23, 42);      // Slate 900
    public static readonly Color SidebarHover = Color.FromArgb(30, 41, 59);     // Slate 800
    public static readonly Color SidebarActive = Color.FromArgb(13, 148, 136);  // Teal 600
    
    public static readonly Color Background = Color.FromArgb(248, 250, 252);    // Slate 50
    public static readonly Color Card = Color.FromArgb(255, 255, 255);          // White
    public static readonly Color Border = Color.FromArgb(226, 232, 240);        // Slate 200
    
    public static readonly Color TextMain = Color.FromArgb(30, 41, 59);         // Slate 800
    public static readonly Color TextMuted = Color.FromArgb(100, 116, 139);     // Slate 500
    public static readonly Color TextLight = Color.FromArgb(241, 245, 249);     // Slate 100
    
    public static readonly Color Danger = Color.FromArgb(239, 68, 68);         // Red 500
    public static readonly Color DangerLight = Color.FromArgb(254, 226, 226);   // Red 100
    public static readonly Color Warning = Color.FromArgb(245, 158, 11);       // Amber 500
    public static readonly Color WarningLight = Color.FromArgb(254, 243, 199); // Amber 100
    public static readonly Color Success = Color.FromArgb(16, 185, 129);       // Emerald 500

    public static readonly Font HeaderFont = new("Segoe UI", 16f, FontStyle.Bold);
    public static readonly Font SubHeaderFont = new("Segoe UI", 12f, FontStyle.Bold);
    public static readonly Font BodyFont = new("Segoe UI", 9.5f, FontStyle.Regular);
    public static readonly Font BodyBoldFont = new("Segoe UI", 9.5f, FontStyle.Bold);
    public static readonly Font SmallFont = new("Segoe UI", 8.5f, FontStyle.Regular);

    public static Button CreatePrimaryButton(string text, int width = 140, int height = 36)
    {
        return new Button
        {
            Text = text,
            Width = width,
            Height = height,
            BackColor = Primary,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = BodyBoldFont,
            Cursor = Cursors.Hand,
            FlatAppearance = { BorderSize = 0 }
        };
    }

    public static Button CreateSecondaryButton(string text, int width = 120, int height = 36)
    {
        return new Button
        {
            Text = text,
            Width = width,
            Height = height,
            BackColor = Color.White,
            ForeColor = TextMain,
            FlatStyle = FlatStyle.Flat,
            Font = BodyFont,
            Cursor = Cursors.Hand,
            FlatAppearance = { BorderColor = Border, BorderSize = 1 }
        };
    }

    public static Button CreateDangerButton(string text, int width = 120, int height = 36)
    {
        return new Button
        {
            Text = text,
            Width = width,
            Height = height,
            BackColor = Danger,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = BodyBoldFont,
            Cursor = Cursors.Hand,
            FlatAppearance = { BorderSize = 0 }
        };
    }

    public static void StyleDataGridView(DataGridView dgv)
    {
        dgv.BackgroundColor = Color.White;
        dgv.BorderStyle = BorderStyle.None;
        dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        dgv.GridColor = Border;
        dgv.RowHeadersVisible = false;
        dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgv.MultiSelect = false;
        dgv.AllowUserToAddRows = false;
        dgv.AllowUserToDeleteRows = false;
        dgv.ReadOnly = true;
        dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgv.EnableHeadersVisualStyles = false;
        dgv.RowTemplate.Height = 36;

        dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(241, 245, 249);
        dgv.ColumnHeadersDefaultCellStyle.ForeColor = TextMain;
        dgv.ColumnHeadersDefaultCellStyle.Font = BodyBoldFont;
        dgv.ColumnHeadersHeight = 40;

        dgv.DefaultCellStyle.BackColor = Color.White;
        dgv.DefaultCellStyle.ForeColor = TextMain;
        dgv.DefaultCellStyle.Font = BodyFont;
        dgv.DefaultCellStyle.SelectionBackColor = PrimaryLight;
        dgv.DefaultCellStyle.SelectionForeColor = TextMain;
    }
}
