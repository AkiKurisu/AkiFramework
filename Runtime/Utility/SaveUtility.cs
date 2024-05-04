using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
namespace Kurisu.Framework
{
    public class SaveUtility
    {
        private static readonly string SavePath = Path.Combine(Application.persistentDataPath, "Saving");
        private static readonly BinaryFormatter formatter = new();
        /// <summary>
        /// Save object data to saving
        /// </summary>
        /// <param name="data"></param>
        /// <param name="key"></param>
        public static void Save(string key, object data)
        {
            var jsonData = JsonUtility.ToJson(data);
            if (!Directory.Exists(SavePath)) Directory.CreateDirectory(SavePath);
            FileStream file = File.Create($"{SavePath}/{key}.bin");
            formatter.Serialize(file, jsonData);
            file.Close();
        }
        public static void Save<T>(T data)
        {
            Save(typeof(T).Name, data);
        }
        /// <summary>
        /// Delate saving
        /// </summary>
        /// <param name="key"></param>
        public static void Delate(string key)
        {
            if (!Directory.Exists(SavePath)) return;
            string path = $"{SavePath}/{key}.bin";
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        /// <summary>
        /// Delate saving
        /// </summary>
        public static void DelateAll()
        {
            if (Directory.Exists(SavePath)) Directory.Delete(SavePath, true);
        }

        /// <summary>
        /// Save json to saving
        /// </summary>
        /// <param name="key"></param>
        /// <param name="jsonData"></param>
        public static void SaveJson(string key, string jsonData)
        {
            if (!Directory.Exists(SavePath)) Directory.CreateDirectory(SavePath);
            FileStream file = File.Create($"{SavePath}/{key}.bin");
            formatter.Serialize(file, jsonData);
            file.Close();
        }
        public static bool SavingExists(string key)
        {
            return File.Exists($"{SavePath}/{key}.bin");
        }
        /// <summary>
        /// Load json from saving
        /// </summary>
        /// <param name="key"></param>
        public static bool TryLoad(string key, out string jsonData)
        {
            string path = $"{SavePath}/{key}.bin";
            if (File.Exists(path))
            {
                FileStream file = File.Open(path, FileMode.Open);
                jsonData = (string)formatter.Deserialize(file);
                file.Close();
                return true;
            }
            jsonData = null;
            return false;
        }
        public static bool TryOverwrite(string key, object data)
        {
            string path = $"{SavePath}/{key}.bin";
            if (File.Exists(path))
            {
                FileStream file = File.Open(path, FileMode.Open);
                JsonUtility.FromJsonOverwrite((string)formatter.Deserialize(file), data);
                file.Close();
                return true;
            }
            return false;
        }
        /// <summary>
        /// Load json from saving and overwrite object
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        public static void Overwrite(string key, object data)
        {
            string path = $"{SavePath}/{key}.bin";
            if (File.Exists(path))
            {
                FileStream file = File.Open(path, FileMode.Open);
                JsonUtility.FromJsonOverwrite((string)formatter.Deserialize(file), data);
                file.Close();
            }
        }
        /// <summary>
        /// Load json from saving and parse to <see cref="T"/> object, if has no saving allocate new one
        /// </summary>
        /// <param name="key"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T LoadOrNew<T>(string key) where T : new()
        {
            string path = $"{SavePath}/{key}.bin";
            if (File.Exists(path))
            {
                FileStream file = File.Open(path, FileMode.Open);
                var data = JsonUtility.FromJson<T>((string)formatter.Deserialize(file));
                file.Close();
                return data;
            }
            return new T();
        }
        public static T LoadOrNew<T>() where T : new()
        {
            return LoadOrNew<T>(typeof(T).Name);
        }
    }
}