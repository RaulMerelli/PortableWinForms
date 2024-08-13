using System;
using System.Linq;
using System.Reflection;
using Windows.Data.Json;

namespace App1
{
    public static class JsonHelper
    {
        public static T Deserialize<T>(string jsonString)
        {
            JsonObject jsonObject = JsonObject.Parse(jsonString);
            return JsonObjectToCustomObject<T>(jsonObject);
        }

        private static T JsonObjectToCustomObject<T>(JsonObject jsonObject)
        {
            T customObject = Activator.CreateInstance<T>();

            foreach (var property in typeof(T).GetProperties())
            {
                if (jsonObject.TryGetValue(property.Name, out var jsonValue))
                {
                    object value = null;
                    switch (jsonValue.ValueType)
                    {
                        case JsonValueType.Boolean:
                            value = jsonValue.GetBoolean();
                            break;
                        case JsonValueType.String:
                            value = jsonValue.GetString();
                            break;
                        case JsonValueType.Number:
                            value = Convert.ChangeType(jsonValue.GetNumber(), property.PropertyType);
                            break;
                        case JsonValueType.Array:
                            value = jsonValue.GetArray().Select(jv => jv.GetString()).ToArray();
                            break;
                        case JsonValueType.Object:
                            value = jsonValue.GetObject();
                            break;
                    }

                    property.SetValue(customObject, value);
                }
            }

            return customObject;
        }


        public static string Serialize<T>(T customObject)
        {
            JsonObject jsonObject = CustomObjectToJsonObject(customObject);
            return jsonObject.Stringify();
        }

        private static JsonObject CustomObjectToJsonObject<T>(T customObject)
        {
            JsonObject jsonObject = new JsonObject();
            
            var properties = customObject.GetType().GetProperties();
            if (properties.Length == 0)
            {
                properties = customObject.GetType().GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
            }
            var fields = customObject.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            
            foreach (var property in properties)
            {
                object value = property.GetValue(customObject);

                if (value != null)
                {
                    if (value is bool booleanValue)
                    {
                        jsonObject[property.Name] = JsonValue.CreateBooleanValue(booleanValue);
                    }
                    else if (value is string stringValue)
                    {
                        jsonObject[property.Name] = JsonValue.CreateStringValue(stringValue);
                    }
                    else if (value is double doubleValue)
                    {
                        jsonObject[property.Name] = JsonValue.CreateNumberValue(doubleValue);
                    }
                    else if (value is Array arrayValue)
                    {
                        JsonArray jsonArray = new JsonArray();
                        foreach (var item in arrayValue)
                        {
                            jsonArray.Add(JsonValue.CreateStringValue(item.ToString()));
                        }
                        jsonObject[property.Name] = jsonArray;
                    }
                    else if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
                    {
                        jsonObject[property.Name] = CustomObjectToJsonObject(value);
                    }
                    else
                    {
                        jsonObject[property.Name] = JsonValue.CreateStringValue(value.ToString());
                    }
                }
            }

            foreach (var filed in fields)
            {
                object value = filed.GetValue(customObject);

                if (value != null)
                {
                    if (value is bool booleanValue)
                    {
                        jsonObject[filed.Name] = JsonValue.CreateBooleanValue(booleanValue);
                    }
                    else if (value is string stringValue)
                    {
                        jsonObject[filed.Name] = JsonValue.CreateStringValue(stringValue);
                    }
                    else if (value is double doubleValue)
                    {
                        jsonObject[filed.Name] = JsonValue.CreateNumberValue(doubleValue);
                    }
                    else if (value is Array arrayValue)
                    {
                        JsonArray jsonArray = new JsonArray();
                        foreach (var item in arrayValue)
                        {
                            jsonArray.Add(JsonValue.CreateStringValue(item.ToString()));
                        }
                        jsonObject[filed.Name] = jsonArray;
                    }
                    else if (filed.FieldType.IsClass && filed.FieldType != typeof(string))
                    {
                        jsonObject[filed.Name] = JsonValue.CreateStringValue(CustomObjectToJsonObject(value).Stringify());
                    }
                    else
                    {
                        jsonObject[filed.Name] = JsonValue.CreateStringValue(value.ToString());
                    }
                }
            }

            return jsonObject;
        }
    }
}
