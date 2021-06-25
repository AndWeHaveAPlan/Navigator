using System;
using System.IO;
using Newtonsoft.Json;

namespace Navigator.Serialization
{
    public static class JsonSerializerWrapper
    {
        private static readonly JsonSerializer JsonSerializer;

        static JsonSerializerWrapper()
        {
            JsonSerializer = JsonSerializer.Create(new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string Serialize(object obj)
        {
            using (StringWriter writer = new StringWriter())
            {
                JsonSerializer.Serialize(writer, obj);
                writer.Flush();

                string bodyString = writer.GetStringBuilder().ToString();
                return bodyString;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        public static T Deserialize<T>(string jsonString)
        {
            using (StringReader reader = new StringReader(jsonString))
            {
                return (T)JsonSerializer.Deserialize(reader, typeof(T));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jsonString"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object Deserialize(string jsonString, Type type)
        {
            using (StringReader reader = new StringReader(jsonString))
            {
                return JsonSerializer.Deserialize(reader, type);
            }
        }
    }
}
