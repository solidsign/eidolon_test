using System.Reflection;
using Analytics.Events;

namespace Analytics
{
    public static class AnalyticsExtensions
    {
        public static string GetEventName(this IAnalyticsEvent @event)
        {
            var attribute = @event.GetType().GetCustomAttribute<MyAnalyticsEventAttribute>();
            return attribute.Name;
        }
    }
}