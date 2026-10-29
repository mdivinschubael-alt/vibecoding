using System.Data;
using System.Globalization;
using Microsoft.Data.Sqlite;

namespace PharmaGestion.DAL.Database;

public static class SqliteExtensions
{
    /// <summary>
    /// Ajoute un paramètre décimal sécurisé stocké sous forme de chaîne exacte (InvariantCulture) 
    /// pour éviter toute imprécision de virgule flottante binaire.
    /// </summary>
    public static void AddDecimalParameter(this SqliteCommand command, string parameterName, decimal value)
    {
        var param = command.CreateParameter();
        param.ParameterName = parameterName;
        param.SqliteType = SqliteType.Text;
        param.Value = value.ToString("0.0000", CultureInfo.InvariantCulture);
        command.Parameters.Add(param);
    }

    /// <summary>
    /// Ajoute un paramètre typé ou null.
    /// </summary>
    public static void AddSafeParameter(this SqliteCommand command, string parameterName, object? value, SqliteType type = SqliteType.Text)
    {
        var param = command.CreateParameter();
        param.ParameterName = parameterName;
        param.SqliteType = type;
        param.Value = value ?? DBNull.Value;
        command.Parameters.Add(param);
    }

    /// <summary>
    /// Lecture sécurisée d'un décimal depuis la base SQLite sans perte d'arrondi.
    /// </summary>
    public static decimal GetSafeDecimal(this SqliteDataReader reader, string columnName)
    {
        int ordinal = reader.GetOrdinal(columnName);
        if (reader.IsDBNull(ordinal)) return 0.00m;

        // Si la colonne est stockée en TEXT
        if (reader.GetFieldType(ordinal) == typeof(string))
        {
            string strVal = reader.GetString(ordinal);
            if (decimal.TryParse(strVal, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
            {
                return result;
            }
        }

        // Sinon conversion directe
        try
        {
            return Convert.ToDecimal(reader.GetValue(ordinal), CultureInfo.InvariantCulture);
        }
        catch
        {
            return 0.00m;
        }
    }

    public static string GetSafeString(this SqliteDataReader reader, string columnName)
    {
        int ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
    }

    public static int GetSafeInt32(this SqliteDataReader reader, string columnName)
    {
        int ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? 0 : reader.GetInt32(ordinal);
    }

    public static int? GetSafeNullableInt32(this SqliteDataReader reader, string columnName)
    {
        int ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetInt32(ordinal);
    }

    public static bool GetSafeBoolean(this SqliteDataReader reader, string columnName)
    {
        int ordinal = reader.GetOrdinal(columnName);
        if (reader.IsDBNull(ordinal)) return false;
        return reader.GetInt32(ordinal) == 1;
    }

    public static DateTime GetSafeDateTime(this SqliteDataReader reader, string columnName)
    {
        int ordinal = reader.GetOrdinal(columnName);
        if (reader.IsDBNull(ordinal)) return DateTime.MinValue;

        string strVal = reader.GetString(ordinal);
        if (DateTime.TryParse(strVal, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dt))
        {
            return dt;
        }

        if (DateTime.TryParse(strVal, out dt))
        {
            return dt;
        }

        return DateTime.MinValue;
    }
}
