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
        
        private SortedList<int, BatchEntry> _uncommitedEntries;
        private readonly Dictionary<int /* transactionId */, SortedList<int, BatchEntry>> _consumedEntries = new();

        private readonly string _uncommitedEntriesKey;

        private int _lastEntryId = 0;
        private int _lastTransactionId = 0;

        public int CurrentSize => _uncommitedEntries.Count;

        public BatchStorage(string uncommitedEntriesKey)
        {
            _uncommitedEntriesKey = uncommitedEntriesKey;
            
            var uncommitedEventsJson = PlayerPrefs.GetString(_uncommitedEntriesKey, "");
            _uncommitedEntries = new SortedList<int, BatchEntry>();

            if (string.IsNullOrEmpty(uncommitedEventsJson)) return;
            
            var entries = JsonConvert.DeserializeObject<List<BatchEntry>>(uncommitedEventsJson);
            foreach (var entry in entries)
            {
                _uncommitedEntries.Add(entry.Id, entry);
            }

            if (_uncommitedEntries.Any())
                _lastEntryId = _uncommitedEntries.Last().Key;
        }


        public void RollbackTransaction(int transactionId)
        {
            if (_consumedEntries.TryGetValue(transactionId, out var events) is false) return;
            
            foreach (var (key, value) in events)
            {
                _uncommitedEntries.Add(key, value);
            }

            _consumedEntries.Remove(transactionId);
            
            Debug.Log($"Rollback transaction {transactionId}. Events: {string.Join("\n", events.Values.Select(x => x.Entry.ToString()))}");
        }
        
        public (int transactionId, IEnumerable<T> events) Consume()
        {
            var transactionId = _lastTransactionId++;
            
            Debug.Log($"Consume. Transaction:{transactionId}. Events: {string.Join("\n", _uncommitedEntries.Values.Select(x => x.Entry.ToString()))}");

            _consumedEntries[transactionId] = _uncommitedEntries;
            _uncommitedEntries = new();
            

            return (transactionId, _consumedEntries[transactionId].Values.Select(x => x.Entry));
        }
        
        public void CommitTransaction(int transactionId)
        {
            Debug.Log($"Commit transaction {transactionId}. Entries: \n{string.Join("\n", _consumedEntries[transactionId].Values.Select(x => x.Entry.ToString()))}");
            _consumedEntries.Remove(transactionId);
            SaveUncommitedEntries();
        }

        public void Store(T entry)
        {
            _lastEntryId++;
            _uncommitedEntries.Add(_lastEntryId, new BatchEntry(_lastEntryId, entry));
            Debug.Log($"Stored entry: {entry.ToString()}");
            SaveUncommitedEntries();
        }
        
        private void SaveUncommitedEntries()
        {
            var allUncommitedEntries = _consumedEntries.Values.SelectMany(x => x.Values)
                .Concat(_uncommitedEntries.Values)
                .ToList();

            var data = JsonConvert.SerializeObject(allUncommitedEntries);
            PlayerPrefs.SetString(_uncommitedEntriesKey, data);
            
            Debug.Log($"Saved uncommited to prefs: {data}");
        }
    }
}