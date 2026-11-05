using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPalm.AgronomyService.Domain.Model.Commands;
using SmartPalm.AgronomyService.Domain.Model.Queries;
using SmartPalm.AgronomyService.Domain.Services;
using SmartPalm.AgronomyService.Interfaces.REST.Resources;
using SmartPalm.AgronomyService.Interfaces.REST.Transform;
using Swashbuckle.AspNetCore.Annotations;

namespace SmartPalm.AgronomyService.Interfaces.REST.Controllers;

[Authorize, ApiController, Route("api/v1"), SwaggerTag("Technical report endpoints")]
public class ReportsController(IReportCommandService commands, IReportQueryService queries) : ControllerBase
{
    [HttpGet("sectors/{sectorId:long}/reports")]
    public async Task<IActionResult> ListSectorReports(long sectorId, CancellationToken token)
    { var result = await queries.HandleAsync(new GetSectorReportsQuery(sectorId), token); return Ok(result.Select(AgronomyAssemblers.ToResource)); }
    [HttpGet("reports/{reportId:long}")]
    public async Task<IActionResult> GetReportById(long reportId, CancellationToken token)
    { var value = await queries.HandleAsync(new GetReportByIdQuery(reportId), token); return value is null ? NotFound(new { message = "Report not found." }) : Ok(AgronomyAssemblers.ToResource(value)); }
    [HttpPost("sectors/{sectorId:long}/reports")]
    public async Task<IActionResult> CreateReport(long sectorId, [FromBody] CreateReportResource resource, CancellationToken token)
    { var value = await commands.HandleAsync(AgronomyAssemblers.ToCommand(sectorId, resource), token); return Created($"/api/v1/reports/{value.Id}", AgronomyAssemblers.ToResource(value)); }
    [HttpPatch("reports/{reportId:long}")]
    public async Task<IActionResult> UpdateReportContent(long reportId, [FromBody] UpdateReportContentResource resource, CancellationToken token)
    { var value = await commands.HandleAsync(new UpdateReportContentCommand(reportId, resource.title, resource.content, resource.findings), token); return Ok(AgronomyAssemblers.ToResource(value)); }
    [HttpPatch("reports/{reportId:long}/publication"), Authorize(Roles = "Administrator,Agronomist")]
    public async Task<IActionResult> PublishReport(long reportId, CancellationToken token)
    { var value = await commands.HandleAsync(new PublishReportCommand(reportId), token); return Ok(AgronomyAssemblers.ToResource(value)); }
}
