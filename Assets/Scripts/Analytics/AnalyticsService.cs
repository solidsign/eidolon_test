using System;
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
        [JsonProperty("type")] public readonly string EventName;
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
        [SerializeField] private float _cooldownBeforeSendSeconds = 3f;
        [SerializeField] private string _serverUrl; 
        // по хорошему это должно быть из внешнего конфига, но т.к вы написали,
        // что не стоит делать никаких бутстрапов,
        // то решил сделать конфигурацию просто через сериализованные поля

        private readonly BatchStorage<AnalyticsServiceEventData> _batchStorage = new();
        private Coroutine _currentSendRoutine = null;

        private void Awake()
        {
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
                onSuccess: () => _batchStorage.CommitTransaction(transactionId), 
                onError: () =>
                {
                    _batchStorage.RollbackTransaction(transactionId);
                    _currentSendRoutine = StartCoroutine(TrySendBatch(_cooldownBeforeSendSeconds)); // retry
                });

            _currentSendRoutine = null;
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