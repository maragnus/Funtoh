using Funtoh.Data;

namespace Funtoh.Web;

public static class Extensions
{
    public static bool IsEmpty<T>(this T[]? array) => array == null || array.Length == 0;
    public static bool IsNotEmpty<T>(this T[]? array) => array != null && array.Length > 0;
    
    public static bool IsEmpty<T>(this List<T>? array) => array == null || array.Count == 0;
    public static bool IsNotEmpty<T>(this List<T>? array) => array != null && array.Count > 0;
    
    public static IEnumerable<(int Index, T Item)> ToIndexedList<T>(this IEnumerable<T> list) => list.Select((item, index) => (index, item));

    public static string ToPhrase(this DeliverableSchedule schedule, int? iterations) => schedule switch
    {
        DeliverableSchedule.Single => "Once",
        DeliverableSchedule.Daily => iterations == 1 ? "Every day" : $"Every {iterations} days",
        DeliverableSchedule.Weekly => iterations == 1 ? "Every week" : $"Every {iterations} weeks",
        DeliverableSchedule.Monthly => iterations == 1 ? "Every month" : $"Every {iterations} months",
        _ => throw new ArgumentOutOfRangeException(nameof(schedule), schedule, null)
    };
}