using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Analytics
{
    public class BatchStorage<T>
    {
        private readonly struct BatchEntry : IComparable<BatchEntry>
        {
            public readonly int Id;
            public readonly T Entry;

            public BatchEntry(int id , T entry)
            {
                Id = id;
                Entry = entry;
            }
            
            public int CompareTo(BatchEntry other)
            {
                return Id.CompareTo(other.Id);
            }
        }
        
        private const string UncommitedEventsKey = "Analytics.UncommitedEvents";
        
        private SortedList<int, BatchEntry> _uncommitedEvents;
        private readonly Dictionary<int /* transactionId */, SortedList<int, BatchEntry>> _consumedEvents = new();
        
        private int _lastEventId = 0;
        private int _lastTransactionId = 0;

        public int CurrentSize => _uncommitedEvents.Count;

        public BatchStorage()
        {
            var uncommitedEventsJson = PlayerPrefs.GetString(UncommitedEventsKey, "");
            _uncommitedEvents = new SortedList<int, BatchEntry>();

            if (string.IsNullOrEmpty(uncommitedEventsJson) is false)
            {
                var entries = JsonConvert.DeserializeObject<List<BatchEntry>>(uncommitedEventsJson);
                foreach (var entry in entries)
                {
                    _uncommitedEvents.Add(entry.Id, entry);
                }
                
                _lastEventId = entries.Max(x => x.Id);
            }
        }
        
        public void RollbackTransaction(int transactionId)
        {
            if (_consumedEvents.TryGetValue(transactionId, out var events) is false) return;
            
            foreach (var (key, value) in events)
            {
                _uncommitedEvents.Add(key, value);
            }

            _consumedEvents.Remove(transactionId);
        }
        
        public (int transactionId, IEnumerable<T> events) Consume()
        {
            var transactionId = _lastTransactionId++;
            _consumedEvents[transactionId] = _uncommitedEvents;
            _uncommitedEvents = new();

            return (transactionId, _consumedEvents[transactionId].Values.Select(x => x.Entry));
        }
        
        public void CommitTransaction(int transactionId)
        {
            _consumedEvents.Remove(transactionId);
        }

        public void Store(T @event)
        {
            _lastEventId++;
            _uncommitedEvents.Add(_lastEventId, new BatchEntry(_lastEventId, @event));
            SaveUncommitedEvents();
        }
        
        private void SaveUncommitedEvents()
        {
            var allUncommitedEntries = _consumedEvents.Values.SelectMany(x => x.Values)
                .Concat(_uncommitedEvents.Values)
                .ToList();
            
            PlayerPrefs.SetString(UncommitedEventsKey, JsonConvert.SerializeObject(allUncommitedEntries));
        }
    }
}