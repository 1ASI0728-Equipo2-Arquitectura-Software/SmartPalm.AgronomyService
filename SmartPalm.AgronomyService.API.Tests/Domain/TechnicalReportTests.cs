using SmartPalm.AgronomyService.Domain.Model.Aggregates;
using SmartPalm.AgronomyService.Domain.Model.ValueObjects;
namespace SmartPalm.AgronomyService.API.Tests.Domain;
public class TechnicalReportTests
{
    [Fact]
    public void Report_CanBeEditedUntilPublished()
    {
        var value = new TechnicalReport(4, 8, "Initial", "Body", null, null); value.UpdateContent("Updated", "New body", "Finding");
        Assert.Equal(2, value.Version); value.Publish(); Assert.Equal(ReportStatus.Published, value.Status);
        Assert.Throws<InvalidOperationException>(() => value.UpdateContent("Again", "Body", null));
    }
}
