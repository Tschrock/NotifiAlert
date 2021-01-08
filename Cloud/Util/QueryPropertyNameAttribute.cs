using System;

namespace NotifiAlert.Cloud.Util
{

    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    sealed class QueryPropertyNameAttribute : Attribute
    {
        readonly string name;

        public string Name
        {
            get { return name; }
        }

        // This is a positional argument
        public QueryPropertyNameAttribute(string name)
        {
            this.name = name;
        }

    }

}