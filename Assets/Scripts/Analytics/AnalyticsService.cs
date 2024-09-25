using UnityEngine;

namespace Analytics
{
    public class AnalyticsService : MonoBehaviour
    {
        [SerializeField] private string _serverUrl; 
        // по хорошему должен быть из внешнего конфига, но т.к вы написали,
        // что не стоит делать никаких бутстрапов,
        // то решил сделать конфигурацию просто через сериализованное поле

        public void TrackEvent(IAnalyticsEvent @event)
        {
            var eventName = @event.GetEventName();
            var data = Newtonsoft.Json.JsonConvert.SerializeObject(@event);
            
            
        }
    }
}