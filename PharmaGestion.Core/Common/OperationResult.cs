namespace PharmaGestion.Core.Common;

public class OperationResult
{
    public bool EstSucces { get; set; }
    public string Message { get; set; } = string.Empty;

    public static OperationResult Succes(string message = "Opération réussie.") 
        => new() { EstSucces = true, Message = message };

    public static OperationResult Echec(string message) 
        => new() { EstSucces = false, Message = message };
}

public class OperationResult<T> : OperationResult
{
    public T? Donnee { get; set; }

    public static OperationResult<T> Succes(T donnee, string message = "Opération réussie.") 
        => new() { EstSucces = true, Message = message, Donnee = donnee };

    public new static OperationResult<T> Echec(string message) 
        => new() { EstSucces = false, Message = message, Donnee = default };
}
