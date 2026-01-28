using System.Text;
using UnityEngine;


namespace GameClient
{
    public interface ICustomSerializer
    {
        public byte[] ToByteArray(object serializeObject);
        public string ToJson(object serializeObject);
        public T FromJson<T>(string json);
        public T FromByteArray<T>(byte[] data);
    }

    public class BaseUnitySerializer : ICustomSerializer
    {
        public T FromByteArray<T>(byte[] data)
        {
            string json = Encoding.UTF8.GetString(data);
            return FromJson<T>(json);
        }

        public T FromJson<T>(string json)
        {
            return JsonUtility.FromJson<T>(json);
        }

        public byte[] ToByteArray(object serializeObject)
        {
             return Encoding.UTF8.GetBytes(ToJson(serializeObject));
        }

        public string ToJson(object serializeObject)
        {
            string json = JsonUtility.ToJson(serializeObject);
            return json;
        }
    }
}
