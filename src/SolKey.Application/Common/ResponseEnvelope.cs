namespace SolKey.Application.Common;

public class ResponseEnvelope<T>
{
    public bool Succeeded { get; init; }
    public string Message { get; init; } = string.Empty;
    public T? Data { get; init; }
    public IReadOnlyCollection<string>? Errors { get; init; }

    public static ResponseEnvelope<T> Success(T data, string message = "") =>
        new() { Succeeded = true, Data = data, Message = message };

    public static ResponseEnvelope<T> Failure(IEnumerable<string> errors, string message = "") =>
        new() { Succeeded = false, Message = message, Errors = errors.ToArray() };

    public static ResponseEnvelope<T> Failure(string error, string message = "") =>
        new() { Succeeded = false, Message = message, Errors = new[] { error } };
}
