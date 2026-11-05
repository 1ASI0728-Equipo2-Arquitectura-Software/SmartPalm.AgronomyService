using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPalm.AgronomyService.Domain.Model.Commands;
using SmartPalm.AgronomyService.Domain.Model.Queries;
using SmartPalm.AgronomyService.Domain.Services;
using SmartPalm.AgronomyService.Interfaces.REST.Resources;
using SmartPalm.AgronomyService.Interfaces.REST.Transform;
using Swashbuckle.AspNetCore.Annotations;

namespace SmartPalm.AgronomyService.Interfaces.REST.Controllers;

[Authorize, ApiController, Route("api/v1"), SwaggerTag("Agronomic recommendation endpoints")]
public class RecommendationsController(IRecommendationCommandService commands, IRecommendationQueryService queries) : ControllerBase
{
    [HttpGet("sectors/{sectorId:long}/recommendations")]
    public async Task<IActionResult> ListSectorRecommendations(long sectorId, [FromQuery] string? status, [FromQuery] long? agronomistId, CancellationToken token)
    { var result = await queries.HandleAsync(new GetSectorRecommendationsQuery(sectorId, AgronomyAssemblers.ParseRecommendationStatus(status), agronomistId), token); return Ok(result.Select(AgronomyAssemblers.ToResource)); }

    [HttpPost("sectors/{sectorId:long}/recommendations")]
    public async Task<IActionResult> CreateSectorRecommendation(long sectorId, [FromBody] CreateRecommendationResource resource, CancellationToken token)
    { var value = await commands.HandleAsync(AgronomyAssemblers.ToCommand(sectorId, resource), token); return Created($"/api/v1/recommendations/{value.Id}", AgronomyAssemblers.ToResource(value)); }

    [HttpGet("recommendations/general")]
    public async Task<IActionResult> ListGeneralRecommendations([FromQuery] string? status, [FromQuery] long? agronomistId, CancellationToken token)
    { var result = await queries.HandleAsync(new GetGeneralRecommendationsQuery(AgronomyAssemblers.ParseRecommendationStatus(status), agronomistId), token); return Ok(result.Select(AgronomyAssemblers.ToResource)); }

    [HttpPost("recommendations/general")]
    public async Task<IActionResult> CreateGeneralRecommendation([FromBody] CreateRecommendationResource resource, CancellationToken token)
    { var value = await commands.HandleAsync(AgronomyAssemblers.ToCommand(null, resource), token); return Created($"/api/v1/recommendations/{value.Id}", AgronomyAssemblers.ToResource(value)); }

    [HttpGet("reports/{reportId:long}/recommendations")]
    public async Task<IActionResult> ListReportRecommendations(long reportId, CancellationToken token)
    { var result = await queries.HandleAsync(new GetRecommendationsByReportIdQuery(reportId), token); return Ok(result.Select(AgronomyAssemblers.ToResource)); }

    [HttpGet("recommendations/{recommendationId:long}")]
    public async Task<IActionResult> GetRecommendationById(long recommendationId, CancellationToken token)
    { var value = await queries.HandleAsync(new GetRecommendationByIdQuery(recommendationId), token); return value is null ? NotFound(new { message = "Recommendation not found." }) : Ok(AgronomyAssemblers.ToResource(value)); }

    [HttpPatch("recommendations/{recommendationId:long}")]
    public async Task<IActionResult> UpdateRecommendationContent(long recommendationId, [FromBody] UpdateRecommendationContentResource resource, CancellationToken token)
    { var value = await commands.HandleAsync(new UpdateRecommendationContentCommand(recommendationId, resource.content), token); return Ok(AgronomyAssemblers.ToResource(value)); }

    [HttpPatch("recommendations/{recommendationId:long}/approval"), Authorize(Roles = "Administrator,Agronomist")]
    public async Task<IActionResult> ApproveRecommendation(long recommendationId, CancellationToken token)
    { var value = await commands.HandleAsync(new ApproveRecommendationCommand(recommendationId), token); return Ok(AgronomyAssemblers.ToResource(value)); }

    [HttpPatch("recommendations/{recommendationId:long}/publication"), Authorize(Roles = "Administrator,Agronomist")]
    public async Task<IActionResult> PublishRecommendation(long recommendationId, CancellationToken token)
    { var value = await commands.HandleAsync(new PublishRecommendationCommand(recommendationId), token); return Ok(AgronomyAssemblers.ToResource(value)); }
}
