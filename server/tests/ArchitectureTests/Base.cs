using Snapflow.Domain.Users;
using Snapflow.Presentation;
using System.Reflection;

namespace Snapflow.ArchitectureTests;

public abstract class Base
{
    protected static readonly Assembly DomainAssembly = typeof(IUser).Assembly;
    protected static readonly Assembly ApplicationAssembly = typeof(DependencyInjection).Assembly;
    protected static readonly Assembly InfrastructureAssembly = typeof(Infrastructure.DependencyInjection).Assembly;
    protected static readonly Assembly PresentationAssembly = typeof(Program).Assembly;
}
