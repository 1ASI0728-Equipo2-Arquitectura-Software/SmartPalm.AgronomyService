using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPalm.AgronomyService.Application.DomainServices;
using SmartPalm.AgronomyService.Application.Ports;
using SmartPalm.AgronomyService.Domain.Services;
using SmartPalm.AgronomyService.Infrastructure.ExternalServices;

namespace SmartPalm.AgronomyService.Api.Controllers;

[ApiController, Authorize(Roles = "Administrator,Agronomist"), Route("api/v1/sectors/{sectorId:long}/recommendations/diagnosis")]
public class DiagnosisController(
    AgronomyAuthorizationService authorization,
    IDiagnosisProvider provider,
    ILogger<DiagnosisController> logger) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Analyze(long sectorId, [FromBody] DiagnosisRequest request, CancellationToken token)
    {
        if (!long.TryParse(User.FindFirst("sub")?.Value, out var actorId) || actorId <= 0) return Forbid();
        await authorization.EnsureAuthorAsync(actorId, sectorId, token);
        var summary = DiagnosisSummary.Create(request.From, request.To, request.Readings);
        try
        {
            var diagnosis = await provider.GenerateAsync(summary, token);
            return Ok(new { summary, diagnosis });
        }
        catch (DiagnosisProviderException exception)
        {
            logger.LogWarning("Diagnosis provider failure. Category={Category} SectorId={SectorId}",
                exception.Category, sectorId);
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "El servicio de diagnóstico no está disponible. Inténtalo más tarde." });
        }
    }
}

public sealed record DiagnosisRequest(DateTimeOffset From, DateTimeOffset To, IReadOnlyCollection<DiagnosisReading> Readings);
