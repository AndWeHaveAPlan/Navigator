using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace Navigator.Core.Pipeline.Middleware
{
    public interface INavigatorSerializer
    {
        string ContentType { get; }

        IRequestBodyModel[] ProcessBody(byte[] messageBody);

        byte[] CreateBody(object bodyObject);
    }

    public interface IRequestBodyModel
    {
        object GetObject(Type targetType);
    }

    public class JsonNavigatorSerializer : INavigatorSerializer
    {
        public string ContentType { get; private set; } = "application/json";

        public IRequestBodyModel[] ProcessBody(byte[] messageBody)
        {
            //TODO: double parsing?
            var objects = JsonSerializer.Deserialize<JsonElement[]>(messageBody);

            return objects.Select(o => new JsonRequestBodyModel(o) as IRequestBodyModel).ToArray();
        }

        public byte[] CreateBody(object bodyObject)
        {
            var jsonString = JsonSerializer.Serialize(bodyObject);
            return Encoding.UTF8.GetBytes(jsonString);
            //return JsonSerializer.SerializeToUtf8Bytes(bodyObject);
        }
    }

    public class JsonRequestBodyModel : IRequestBodyModel
    {
        private readonly JsonElement _element;

        public JsonRequestBodyModel(JsonElement element)
        {
            _element = element;
        }

        public object GetObject(Type targetType)
        {
            return JsonSerializer.Deserialize(_element.GetRawText(), targetType);
        }
    }
}
