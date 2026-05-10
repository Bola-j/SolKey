using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolKey.Application.Common;
using SolKey.Application.DTOs.Books;
using SolKey.Application.Interfaces;

namespace SolKey.API.Controllers;

[ApiController]
[Route("api/books")]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;

    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    [HttpGet]
    public async Task<ActionResult<ResponseEnvelope<IReadOnlyCollection<BookDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var response = await _bookService.GetAllAsync(cancellationToken);
        return Ok(ResponseEnvelope<IReadOnlyCollection<BookDto>>.Success(response));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ResponseEnvelope<BookDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var response = await _bookService.GetByIdAsync(id, cancellationToken);
        return Ok(ResponseEnvelope<BookDto>.Success(response));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ResponseEnvelope<BookDto>>> Create(UpsertBookRequest request, CancellationToken cancellationToken)
    {
        var response = await _bookService.CreateAsync(request, cancellationToken);
        return Ok(ResponseEnvelope<BookDto>.Success(response));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ResponseEnvelope<BookDto>>> Update(Guid id, UpsertBookRequest request, CancellationToken cancellationToken)
    {
        var response = await _bookService.UpdateAsync(id, request, cancellationToken);
        return Ok(ResponseEnvelope<BookDto>.Success(response));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ResponseEnvelope<object>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _bookService.DeleteAsync(id, cancellationToken);
        return Ok(ResponseEnvelope<object>.Success(new { }));
    }
}
