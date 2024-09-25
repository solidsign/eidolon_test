using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

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
        [JsonProperty("type")] public readonly string EventName;
        [JsonProperty("data")] public readonly string Data;

        public AnalyticsServiceEventData(string eventName, string data)
        {
            EventName = eventName;
            Data = data;
        }

        public override string ToString()
        {
            return $"EventName: {EventName}, Data: {Data}";
        }
    }

    public interface IAnalyticsService
    {
        void TrackEvent(IAnalyticsEvent @event);
    }
    
    public class AnalyticsService : MonoBehaviour, IAnalyticsService
    {
        [SerializeField] private Server.Server _server; // Засунул сюда, чтобы не мудрить с DI и было проще тестировать
        [Space]
        [SerializeField] private float _cooldownBeforeSendSeconds = 3f;
        [SerializeField] private string _serverUrl; 
        // по хорошему это должно быть из внешнего конфига, но т.к вы написали,
        // что не стоит делать никаких бутстрапов,
        // то решил сделать конфигурацию просто через сериализованные поля

        private BatchStorage<AnalyticsServiceEventData> _batchStorage;
        private Coroutine _currentSendRoutine = null;
        
        private const string UncommitedEntriesKey = "Analytics.UncommitedEntries";

        private void Awake()
        {
            _batchStorage = new(UncommitedEntriesKey);
            if (_batchStorage.CurrentSize > 0)
            {
                _currentSendRoutine = StartCoroutine(TrySendBatch(0f));
            }
        }

        public void TrackEvent(IAnalyticsEvent @event)
        {
            _batchStorage.Store(new AnalyticsServiceEventData(@event.GetEventName(), JsonConvert.SerializeObject(@event)));
            
            if (_currentSendRoutine != null) return;
            
            _currentSendRoutine = StartCoroutine(TrySendBatch(_cooldownBeforeSendSeconds));
        }

        private IEnumerator TrySendBatch(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            
            var (transactionId, events) = _batchStorage.Consume();
            
            var requestData = new AnalyticServiceEventsBatch(events);
            
            yield return TrySendBatch(requestData, 
                onSuccess: () =>
                {
                    _batchStorage.CommitTransaction(transactionId);
                    _currentSendRoutine = null;
                }, 
                onError: () =>
                {
                    _batchStorage.RollbackTransaction(transactionId);
                    _currentSendRoutine = StartCoroutine(TrySendBatch(_cooldownBeforeSendSeconds)); // retry
                });
        }

        private IEnumerator TrySendBatch(AnalyticServiceEventsBatch requestData, Action onSuccess, Action onError)
        {
            yield return _server.SendPostRequest(_serverUrl, requestData, onSuccess, onError);
        }
    }
}