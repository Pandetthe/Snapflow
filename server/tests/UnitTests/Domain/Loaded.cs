using System.Reflection;

namespace Snapflow.UnitTests.Domain;

internal static class Loaded
{
    public static void Into<TItem>(object owner, string field, params TItem[] items)
    {
        var collection = (List<TItem>)owner.GetType()
            .GetField(field, BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(owner)!;
        collection.AddRange(items);
    }

    public static T WithId<T>(T entity, int id)
    {
        typeof(T).GetProperty("Id")!.SetValue(entity, id);
        return entity;
    }
}
