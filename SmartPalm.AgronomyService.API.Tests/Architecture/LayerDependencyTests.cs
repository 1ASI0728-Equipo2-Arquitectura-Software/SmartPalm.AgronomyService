using System.Reflection;
using SmartPalm.AgronomyService.Domain.Model.Aggregates;
namespace SmartPalm.AgronomyService.API.Tests.Architecture;
public class LayerDependencyTests
{
    [Fact]
    public void DomainPublicApi_DoesNotExposeOuterLayers()
    {
        var forbidden = new[] { ".Application", ".Infrastructure", ".Interfaces", ".Api" };
        var violations = typeof(Recommendation).Assembly.GetTypes().Where(t => t.Namespace?.StartsWith("SmartPalm.AgronomyService.Domain", StringComparison.Ordinal) == true)
            .SelectMany(ExposedTypes).Where(t => forbidden.Any(s => t.Namespace?.Contains(s, StringComparison.Ordinal) == true)).Select(t => t.FullName).Distinct().ToArray();
        Assert.Empty(violations);
    }
    [Fact]
    public void CommandAndQueryTypes_UseDomainModelNamespaces()
    {
        var types = typeof(Recommendation).Assembly.GetTypes(); var commands = types.Where(t => t.Name.EndsWith("Command", StringComparison.Ordinal)).ToArray();
        var queries = types.Where(t => t.Name.EndsWith("Query", StringComparison.Ordinal)).ToArray(); Assert.NotEmpty(commands); Assert.NotEmpty(queries);
        Assert.All(commands, t => Assert.Equal("SmartPalm.AgronomyService.Domain.Model.Commands", t.Namespace));
        Assert.All(queries, t => Assert.Equal("SmartPalm.AgronomyService.Domain.Model.Queries", t.Namespace));
    }
    private static IEnumerable<Type> ExposedTypes(Type type)
    {
        foreach (var c in type.GetConstructors(BindingFlags.Instance | BindingFlags.Public)) foreach (var p in c.GetParameters()) yield return Unwrap(p.ParameterType);
        foreach (var m in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly)) { yield return Unwrap(m.ReturnType); foreach (var p in m.GetParameters()) yield return Unwrap(p.ParameterType); }
        foreach (var p in type.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly)) yield return Unwrap(p.PropertyType);
    }
    private static Type Unwrap(Type type) => type.IsArray ? type.GetElementType() ?? type : type.IsGenericType ? type.GetGenericArguments().FirstOrDefault() ?? type : type;
}
