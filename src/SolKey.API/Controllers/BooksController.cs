using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolKey.Application.Common;
using SolKey.Application.DTOs.Books;
using SolKey.Application.Interfaces;

namespace SolKey.API.Controllers;

[ApiController]
[Route("api/books")]
public class BooksController : ApiControllerBase
{
    private readonly IBookService _bookService;

    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    [HttpGet]
    public Task<ActionResult<ResponseEnvelope<PagedResponse<BookDto>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(() => _bookService.GetAllAsync(page, pageSize, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public Task<ActionResult<ResponseEnvelope<BookDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return ExecuteAsync(() => _bookService.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public Task<ActionResult<ResponseEnvelope<BookDto>>> Create(UpsertBookRequest request, CancellationToken cancellationToken)
    {
        return ExecuteAsync(() => _bookService.CreateAsync(request, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public Task<ActionResult<ResponseEnvelope<BookDto>>> Update(Guid id, UpsertBookRequest request, CancellationToken cancellationToken)
    {
        return ExecuteAsync(() => _bookService.UpdateAsync(id, request, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public Task<ActionResult<ResponseEnvelope<object>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        return ExecuteAsync(() => _bookService.DeleteAsync(id, cancellationToken));
    }
}
