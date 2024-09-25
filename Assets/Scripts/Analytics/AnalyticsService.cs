using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        [JsonProperty("data")] public readonly string Data;

        public AnalyticsServiceEventData(string eventName, string data)
        {
            EventName = eventName;
            Data = data;
        }
    }

    public interface IAnalyticsService
    {
        void TrackEvent(IAnalyticsEvent @event);
    }
    
    public class AnalyticsService : MonoBehaviour, IAnalyticsService
    {
        [SerializeField] private float _sendPeriodSeconds = 3f;
        [SerializeField] private string _serverUrl; 
        // по хорошему должен быть из внешнего конфига, но т.к вы написали,
        // что не стоит делать никаких бутстрапов,
        // то решил сделать конфигурацию просто через сериализованное поле

        private readonly BatchStorage<AnalyticsServiceEventData> _batchStorage = new();
        private float _lastSendTime = 0f;

        private void Update()
        {
            if (_lastSendTime + _sendPeriodSeconds > Time.time) return;

            _lastSendTime = Time.time;
            
            if (_batchStorage.CurrentSize > 0) StartCoroutine(TrySendBatch());
        }

        public void TrackEvent(IAnalyticsEvent @event)
        {
            _batchStorage.Store(new AnalyticsServiceEventData(@event.GetEventName(), JsonConvert.SerializeObject(@event)));
        }

        private IEnumerator TrySendBatch()
        {
            var (transactionId, events) = _batchStorage.Consume();
            
            var requestData = new AnalyticServiceEventsBatch(events);
            
            yield return TrySendBatch(requestData, 
                onSuccess: () => _batchStorage.CommitTransaction(transactionId), 
                onError: () => _batchStorage.RollbackTransaction(transactionId));
        }

        private IEnumerator TrySendBatch(AnalyticServiceEventsBatch requestData, Action onSuccess, Action onError)
        {
            using var request = UnityWebRequest.Post(_serverUrl, JsonConvert.SerializeObject(requestData));
            yield return request.SendWebRequest();
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                onSuccess?.Invoke();
            }
            else
            {
                onError?.Invoke();
            }
        }
    }
}