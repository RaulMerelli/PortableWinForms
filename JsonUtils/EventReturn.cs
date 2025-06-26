using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;

namespace JsonUtils
{
    public static class ExpandoJsonConverter
    {
        public static dynamic DeserializeToExpando(string json)
        {
            var jToken = JToken.Parse(json);
            return ConvertTokenToExpando(jToken);
        }

        private static object ConvertTokenToExpando(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    var expando = new ExpandoObject();
                    var dict = (IDictionary<string, object>)expando;
                    foreach (var prop in token.Children<JProperty>())
                    {
                        dict[prop.Name] = ConvertTokenToExpando(prop.Value);
                    }
                    return expando;

                case JTokenType.Array:
                    var list = new List<object>();
                    foreach (var item in token.Children())
                    {
                        list.Add(ConvertTokenToExpando(item));
                    }
                    return list;

                case JTokenType.Integer:
                    return token.Value<int>();

                case JTokenType.Float:
                    return token.Value<double>();

                case JTokenType.String:
                    return token.Value<string>();

                case JTokenType.Boolean:
                    return token.Value<bool>();

                case JTokenType.Null:
                    return null;

                default:
                    // We can manage other types like Date here if we need
                    return token.ToString();
            }
        }
    }
}
