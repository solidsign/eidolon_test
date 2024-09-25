using System.Reflection;
using Analytics.Events;
using Newtonsoft.Json;

namespace Analytics
{
    public static class AnalyticsExtensions
    {
        public static string GetEventName(this IAnalyticsEvent @event)
        {
            var attribute = @event.GetType().GetCustomAttribute<MyAnalyticsEventAttribute>();
            return attribute.Name;
        }
        
        public static string Serialize(this IAnalyticsEvent @event)
        {
            // Чтобы не дублировать везде реализацию вынес сюда.
            // При наличии кучи разных сервисов аналитики можно иметь разные методы сериализации.
            
            // Как я понял формат должен быть: <property>:<value>
            // По хорошему стоило бы здесь реализовать сериализацию данных через рефлексию,
            // (для этого наметил MyAnalyticsPropertyAttribute как название конкретно для этого сервиса из возможных многих)
            // но думаю в тестовом можно не усложнять этот момент, поэтому вставил форматирование просто в ToString
            
            return @event.ToString();
        }
    }
}