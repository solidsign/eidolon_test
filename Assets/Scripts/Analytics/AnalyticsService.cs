using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Analytics
{
    public readonly struct AnalyticServiceEventsBatch
    {
        [JsonProperty("events")] public readonly List<AnalyticsServiceEventData> Events;

        public AnalyticServiceEventsBatch(IEnumerable<AnalyticsServiceEventData> events)
        {
            Events = new List<AnalyticsServiceEventData>(events);
        }
    }
    
    public readonly struct AnalyticsServiceEventData
    {
        [JsonProperty("eventName")] public readonly string EventName;
        [JsonProperty("data")] public readonly IAnalyticsEvent Data;

        public AnalyticsServiceEventData(string eventName, IAnalyticsEvent data)
        {
            EventName = eventName;
            Data = data;
        }
    }
    
    public class AnalyticsService : MonoBehaviour
    {
        [SerializeField] private string _serverUrl; 
        // по хорошему должен быть из внешнего конфига, но т.к вы написали,
        // что не стоит делать никаких бутстрапов,
        // то решил сделать конфигурацию просто через сериализованное поле

        public void TrackEvent(IAnalyticsEvent @event)
        {
            var eventName = @event.GetEventName();
            var requestData = new AnalyticsServiceEventData(eventName, @event);

            StartCoroutine(TrySendBatch(new AnalyticServiceEventsBatch(new[] { requestData })));
        }

        private IEnumerator TrySendBatch(AnalyticServiceEventsBatch requestData)
        {
            using var request = UnityWebRequest.Post(_serverUrl, JsonConvert.SerializeObject(requestData));
            yield return request.SendWebRequest();
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(request.error);
            }
            else
            {
                Debug.Log("Form upload complete!");
            }
        }
    }
}