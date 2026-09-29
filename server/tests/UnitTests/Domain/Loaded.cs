using System.Reflection;

namespace Snapflow.UnitTests.Domain;

internal static class Loaded
{
    public static T WithId<T>(T entity, int id)
    {
        typeof(T).GetProperty("Id")!.SetValue(entity, id);
        return entity;
    }
}
