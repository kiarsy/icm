using System.Reflection;
using ICMarkets.Application.Queries;
using ICMarkets.Domain;
using ICMarkets.Infrastructure.Repositories;
using NetArchTest.Rules;
using FluentAssertions;

namespace ICMarkets.ArchitectureTests;

public class ArchitectureTests
{
    private const string Application = "ICMarkets.Application";
    private const string Infrastructure = "ICMarkets.Infrastructure";
    private const string Api = "ICMarkets.Api";

    private static readonly Assembly DomainAssembly = typeof(IDomainEvent).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(GetAllHistoryQuery).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(EventStoreRepository).Assembly;

    [Fact]
    public void Domain_Should_Not_Depend_On_Other_Layers()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(Application, Infrastructure, Api)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"Domain violated by: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }
    

    [Fact]
    public void Application_Should_Not_Depend_On_Infrastructure_Or_Api()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(Infrastructure, Api)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"Application violated by: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Infrastructure_Should_Not_Depend_On_Api()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .ShouldNot()
            .HaveDependencyOn(Api)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"Infrastructure violated by: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }
}