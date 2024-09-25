namespace Analytics.Events
{
    [MyAnalyticsEvent("levelStart")]
    public class LevelStartedEvent : IAnalyticsEvent
    {
        [MyAnalyticsProperty("level")]
        public int Level { get; private set; }

        public LevelStartedEvent(int level)
        {
            Level = level;
        }

        public override string ToString()
        {
            return $"level:{Level}";
        }
    }
}