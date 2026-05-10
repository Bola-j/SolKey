namespace SolKey.Application.Common;

public class PagedResponse<T>
{
    public bool Succeeded { get; init; }
    public string Message { get; init; } = string.Empty;
    public IReadOnlyCollection<T> Data { get; init; } = Array.Empty<T>();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }

    public static PagedResponse<T> Success(IReadOnlyCollection<T> data, int page, int pageSize, int totalCount, string message = "") =>
        new()
        {
            Succeeded = true,
            Data = data,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            Message = message
        };
}
