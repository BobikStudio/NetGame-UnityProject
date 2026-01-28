using System.IO;
using UnityEngine;

namespace GameClient
{
    public class LocalDatastore
    {
        private readonly ICustomSerializer _serializer;

        const string EditorDataPath = "Project/Data";
        const string BuildDataPath = "Datastore";

        private string BasePath
        {
            get
            {
                #if UNITY_EDITOR
                return Path.Combine(Application.dataPath, EditorDataPath);
                #else
                return Path.Combine(Application.persistentDataPath, BuildDataPath);
                #endif
            }
        }

        public LocalDatastore(ICustomSerializer customSerializer)
        {
            _serializer = customSerializer;

            if (!Directory.Exists(BasePath))
            {
                Directory.CreateDirectory(BasePath);
            }
        }

        public void SaveData<T>(string path, object data)
        {
             byte[] dataBytes = _serializer.ToByteArray(data);
             File.WriteAllBytes(Path.Combine(BasePath, path), dataBytes);
        }

        public T LoadData<T>(string path)
        {
            string fullPath = Path.Combine(BasePath, path);

            if (!DataExist(path))
            {
                return default;
            }    

            byte[] dataBytes = File.ReadAllBytes(fullPath);
            return _serializer.FromByteArray<T>(dataBytes);
        }

        public void DeleteData(string path)
        {
            string fullPath = Path.Combine(BasePath, path);

            if (!DataExist(path))
            {
                File.Delete(path);
            }
        }
        public bool DataExist(string path)
        {
            string fullPath = Path.Combine(BasePath, path);
            return File.Exists(fullPath);
        }

    }
}
