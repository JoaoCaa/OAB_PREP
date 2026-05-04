using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OabPrep.Application.Common.Models;
using OabPrep.Application.UseCases.Admin.Users;
using OabPrep.Domain.Constants;
using System.Security.Claims;

namespace OabPrep.API.Controllers;

[ApiController]
[Route("api/v1/admin/users")]
[Authorize(Roles = UserRoles.Admin)]
public sealed class AdminUsersController : ControllerBase
{
    private readonly ListUsersUseCase _listUsersUseCase;
    private readonly GetAdminUserUseCase _getAdminUserUseCase;
    private readonly BlockUserUseCase _blockUserUseCase;
    private readonly UnblockUserUseCase _unblockUserUseCase;
    private readonly ChangeUserRoleUseCase _changeUserRoleUseCase;

    public AdminUsersController(
        ListUsersUseCase listUsersUseCase,
        GetAdminUserUseCase getAdminUserUseCase,
        BlockUserUseCase blockUserUseCase,
        UnblockUserUseCase unblockUserUseCase,
        ChangeUserRoleUseCase changeUserRoleUseCase)
    {
        _listUsersUseCase = listUsersUseCase;
        _getAdminUserUseCase = getAdminUserUseCase;
        _blockUserUseCase = blockUserUseCase;
        _unblockUserUseCase = unblockUserUseCase;
        _changeUserRoleUseCase = changeUserRoleUseCase;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AdminUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> List(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _listUsersUseCase.ExecuteAsync(search, page, size, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AdminUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getAdminUserUseCase.ExecuteAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/block")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Block(Guid id, CancellationToken cancellationToken)
    {
        await _blockUserUseCase.ExecuteAsync(GetAdminId(), id, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:guid}/unblock")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Unblock(Guid id, CancellationToken cancellationToken)
    {
        await _unblockUserUseCase.ExecuteAsync(GetAdminId(), id, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:guid}/role")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ChangeRole(
        Guid id,
        [FromBody] ChangeUserRoleCommand command,
        CancellationToken cancellationToken)
    {
        await _changeUserRoleUseCase.ExecuteAsync(GetAdminId(), id, command, cancellationToken);
        return NoContent();
    }

    private Guid GetAdminId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
