using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace NotifiAlert.Cloud.Util
{
    public class QueryStringFormatter<T> : IFormatter where T : class
    {
        public static string Serialize(T value)
        {
            var stream = new MemoryStream();
            var serializer = new QueryStringFormatter<T>();
            serializer.Serialize(stream, value);
            return Encoding.UTF8.GetString(stream.ToArray());
        }

        public object Deserialize(Stream serializationStream)
        {
            throw new NotImplementedException();
        }

        public void Serialize(Stream serializationStream, object graph)
        {
            if (serializationStream == null)
            {
                throw new ArgumentNullException(nameof(serializationStream));
            }

            if(graph == null) {
                return;
            }

            using (var stream = new StreamWriter(serializationStream))
            {
                if (graph is IDictionary<string, dynamic> graphDictionary)
                {
                    // Tracks if this is the first member or not
                    bool first = true;

                    // For each entry
                    foreach (KeyValuePair<string, dynamic> entry in graphDictionary)
                    {
                        // If it has a value
                        if (entry.Value != null)
                        {
                            stream.Write(first ? '?' : '&');
                            stream.Write(Uri.EscapeDataString(entry.Key));
                            stream.Write("=");
                            stream.Write(Uri.EscapeDataString(StringConverter.ToStringDynamic(entry.Value)));
                            first = false;
                        }
                    }

                    stream.Flush();
                }
                else
                {
                    // Tracks if this is the first member or not
                    bool first = true;

                    // Get the public properties
                    PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

                    // For each property
                    foreach (PropertyInfo prop in properties)
                    {
                        // Get the property info
                        QueryPropertyNameAttribute keyAttribute = prop.GetCustomAttribute<QueryPropertyNameAttribute>(true);
                        string queryStringKey = keyAttribute != null ? keyAttribute.Name : prop.Name;
                        dynamic value = prop.GetValue(graph);

                        // If it has a value
                        if (value != null)
                        {
                            stream.Write(first ? '?' : '&');
                            stream.Write(Uri.EscapeDataString(queryStringKey));
                            stream.Write("=");
                            stream.Write(Uri.EscapeDataString(StringConverter.ToStringDynamic(value)));
                            first = false;
                        }
                    }
                    stream.Flush();
                }
            }
        }

        public ISurrogateSelector SurrogateSelector { get; set; }

        public SerializationBinder Binder { get; set; }

        public StreamingContext Context { get; set; }
    }
}