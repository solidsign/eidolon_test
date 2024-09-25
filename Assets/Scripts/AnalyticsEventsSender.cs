using System;
using Analytics;
using Analytics.Events;
using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public class AnalyticsEventsSender : MonoBehaviour
    {
        [SerializeField] private AnalyticsService _analytics; // вместо этого должен быть любой DI
        [SerializeField] private Button _sendLevelStartedEventButton;

        private int _lastLevel = 0;
        
        private void Awake()
        {
            _sendLevelStartedEventButton.onClick.AddListener(() => _analytics.TrackEvent(new LevelStartedEvent(_lastLevel++)));
        }
    }
}