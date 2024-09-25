using System.Reflection;
using Analytics.Events;

namespace Analytics
{
    public interface IAnalyticsEventsFormatter
    {
        string GetEventName(IAnalyticsEvent @event);
        string Serialize(IAnalyticsEvent @event);
    }
    
    public class MyAnalyticsEventsFormatter : IAnalyticsEventsFormatter
    {
        public string GetEventName(IAnalyticsEvent @event)
        {
            var attribute = @event.GetType().GetCustomAttribute<MyAnalyticsEventAttribute>();
            return attribute.Name;
        }

        public string Serialize(IAnalyticsEvent @event)
        {
            // Как я понял формат должен быть: <property>:<value>
            // По хорошему стоило бы здесь реализовать сериализацию данных через рефлексию,
            // (для этого наметил MyAnalyticsPropertyAttribute как название конкретно для этого сервиса из возможных многих)
            // но думаю в тестовом можно не усложнять этот момент, поэтому вставил форматирование просто в ToString
            
            return @event.ToString();
        }
    }
}