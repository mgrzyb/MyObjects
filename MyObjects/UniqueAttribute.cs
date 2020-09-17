using System;

namespace MyObjects
{
    [AttributeUsage(AttributeTargets.Property)]
    public class UniqueAttribute : Attribute
    {
        public UniqueAttribute()
        {
        }

        public UniqueAttribute(string key)
        {
            this.Key = key;
        }

        public string Key { get; set; }
    }
}