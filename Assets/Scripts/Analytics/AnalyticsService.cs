using System.Collections;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Analytics
{
    public readonly struct AnalyticsServiceRequest
    {
        [JsonProperty("eventName")] public readonly string EventName;
        [JsonProperty("data")] public readonly IAnalyticsEvent Data;

        public AnalyticsServiceRequest(string eventName, IAnalyticsEvent data)
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
            var requestData = new AnalyticsServiceRequest(eventName, @event);

            StartCoroutine(TrySendEvent(requestData));
        }

        private IEnumerator TrySendEvent(AnalyticsServiceRequest requestData)
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