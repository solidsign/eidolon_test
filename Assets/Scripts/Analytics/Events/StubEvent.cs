using Newtonsoft.Json;

namespace Analytics.Events
{
    [MyAnalyticsEvent("StubEvent")]
    public class StubEvent : IAnalyticsEvent
    {
        [JsonProperty("count")]
        public int Count { get; private set; }

        [JsonConstructor]
        public StubEvent()
        {
        }
        
        public StubEvent(int count)
        {
            Count = count;
        }
    }
}