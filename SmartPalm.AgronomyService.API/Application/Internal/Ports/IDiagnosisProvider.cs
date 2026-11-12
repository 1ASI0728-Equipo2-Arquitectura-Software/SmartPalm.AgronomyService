using SmartPalm.AgronomyService.Domain.Services;

namespace SmartPalm.AgronomyService.Application.Ports;

public interface IDiagnosisProvider
{
    Task<string> GenerateAsync(DiagnosisSummary summary, CancellationToken cancellationToken);
}
