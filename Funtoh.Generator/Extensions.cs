namespace Funtoh.Generator;

public static class Extensions
{
    public static string ToCommaDelimited(this IEnumerable<string> items) =>
        string.Join(", ", items);
    
    public static string ToCommaDelimited<TItem>(this IEnumerable<TItem> items, Func<TItem, string> predicate) =>
        string.Join(", ", items.Select(predicate));
    
    public static string ToLineDelimited(this IEnumerable<string> items) =>
        string.Join(Environment.NewLine, items);
    
    public static string ToLineDelimited<TItem>(this IEnumerable<TItem> items, Func<TItem, string> predicate) =>
        string.Join(Environment.NewLine, items.Select(predicate));
    
    public static IEnumerable<(int Index, T Item)> ToIndexedList<T>(this IEnumerable<T> list) => list.Select((item, index) => (index, item));
}