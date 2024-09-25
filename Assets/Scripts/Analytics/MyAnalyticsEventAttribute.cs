using System;

namespace Analytics.Events
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MyAnalyticsEventAttribute : Attribute
    {
        public string Name { get; }

        public MyAnalyticsEventAttribute(string name)
        {
            Name = name;
        }
    }
}