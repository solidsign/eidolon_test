using System;

namespace Analytics.Events
{
    public class MyAnalyticsPropertyAttribute : Attribute
    {
        public string Name { get; }

        public MyAnalyticsPropertyAttribute(string name)
        {
            Name = name;
        }
    }
}