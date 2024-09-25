using System.Collections.Generic;
using System.Linq;

namespace Analytics
{
    public class AnalyticEventsBatch
    {
        private SortedDictionary<int /* порядковый номер */, IAnalyticsEvent> _uncommitedEvents = new();
        private readonly Dictionary<int /* номер транзакции */, SortedDictionary<int /* порядковый номер */, IAnalyticsEvent>> _consumedEvents = new();
        
        private int _lastEventId = 0;
        private int _lastTransactionId = 0;

        public void RollbackTransaction(int transactionId)
        {
            if (_consumedEvents.TryGetValue(transactionId, out var events) is false) return;
            
            foreach (var (key, value) in events)
            {
                _uncommitedEvents.Add(key, value);
            }

            _consumedEvents.Remove(transactionId);
        }
        
        public (int transactionId, IEnumerable<IAnalyticsEvent> events) Consume()
        {
            var transactionId = _lastTransactionId++;
            _consumedEvents[transactionId] = _uncommitedEvents;
            _uncommitedEvents = new();

            return (transactionId, _consumedEvents[transactionId].Values);
        }
        
        public void CommitTransaction(int transactionId)
        {
            _consumedEvents.Remove(transactionId);
        }

        public void Store(IAnalyticsEvent @event)
        {
            _lastEventId++;
            _uncommitedEvents.Add(_lastEventId, @event);
        }
    }
}