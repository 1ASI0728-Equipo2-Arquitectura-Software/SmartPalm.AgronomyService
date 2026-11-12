using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPalm.AgronomyService.Application.Services;

namespace SmartPalm.AgronomyService.Api.Controllers;

[ApiController, Authorize(Roles = "Administrator"), Route("api/v1/admin/projections")]
public sealed class ProjectionReconciliationController(
    AgronomyProjectionReconciliationService reconciliation) : ControllerBase
{
    [HttpPost("reconcile")]
    public async Task<IActionResult> Reconcile(CancellationToken token) =>
        Ok(await reconciliation.ReconcileAsync(token));
}
