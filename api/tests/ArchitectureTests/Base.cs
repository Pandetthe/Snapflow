using Snapflow.Api;
using Snapflow.Domain.Users;
using System.Reflection;

namespace Snapflow.ArchitectureTests;

public abstract class Base
{
    protected static readonly Assembly DomainAssembly = typeof(User).Assembly;
    protected static readonly Assembly ApplicationAssembly = typeof(DependencyInjection).Assembly;
    protected static readonly Assembly InfrastructureAssembly = typeof(Infrastructure.DependencyInjection).Assembly;
    protected static readonly Assembly ApiAssembly = typeof(Program).Assembly;
}
