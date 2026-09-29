using Snapflow.Domain.Users;
using System.Reflection;

namespace Snapflow.ArchitectureTests;

public abstract class Base
{
    protected static readonly Assembly DomainAssembly = typeof(IUser).Assembly;
    protected static readonly Assembly ApplicationAssembly = typeof(Application.DependencyInjection).Assembly;
    protected static readonly Assembly InfrastructureAssembly = typeof(Infrastructure.DependencyInjection).Assembly;
    protected static readonly Assembly PresentationAssembly = typeof(Presentation.DependencyInjection).Assembly;

    protected static readonly string ApplicationNamespace = typeof(Application.DependencyInjection).Namespace!;
    protected static readonly string InfrastructureNamespace = typeof(Infrastructure.DependencyInjection).Namespace!;
    protected static readonly string PresentationNamespace = typeof(Presentation.DependencyInjection).Namespace!;
}
