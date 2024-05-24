using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
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
            string jsonData;
            if (data.GetType().GetCustomAttribute<PreferJsonConvertAttribute>() == null)
                jsonData = JsonUtility.ToJson(data);
            else
                jsonData = JsonConvert.SerializeObject(data);
            if (!Directory.Exists(SavePath)) Directory.CreateDirectory(SavePath);
            using FileStream file = File.Create($"{SavePath}/{key}.bin");
            formatter.Serialize(file, jsonData);
        }
        /// <summary>
        /// Save data to saving
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <typeparam name="T"></typeparam>
        public static void Save<T>(string key, T data)
        {
            string jsonData;
            if (typeof(T).GetCustomAttribute<PreferJsonConvertAttribute>() == null)
                jsonData = JsonUtility.ToJson(data);
            else
                jsonData = JsonConvert.SerializeObject(data);
            if (!Directory.Exists(SavePath)) Directory.CreateDirectory(SavePath);
            using FileStream file = File.Create($"{SavePath}/{key}.bin");
            formatter.Serialize(file, jsonData);
        }
        /// <summary>
        /// Save data to saving
        /// </summary>
        /// <param name="data"></param>
        /// <typeparam name="T"></typeparam>
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
        public static bool TryLoadJson(string key, out string jsonData)
        {
            string path = $"{SavePath}/{key}.bin";
            if (File.Exists(path))
            {
                using FileStream file = File.Open(path, FileMode.Open);
                jsonData = (string)formatter.Deserialize(file);
                return true;
            }
            jsonData = null;
            return false;
        }
        /// <summary>
        /// Load json from saving and overwrite object
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        public static bool Overwrite(string key, object data)
        {
            string path = $"{SavePath}/{key}.bin";
            if (File.Exists(path))
            {
                using FileStream file = File.Open(path, FileMode.Open);
                if (data.GetType().GetCustomAttribute<PreferJsonConvertAttribute>() == null)
                    JsonUtility.FromJsonOverwrite((string)formatter.Deserialize(file), data);
                else
                    JsonConvert.PopulateObject((string)formatter.Deserialize(file), data);
                return true;
            }
            return false;
        }
        /// <summary>
        /// Load json from saving and overwrite object
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool Overwrite<T>(string key, T data)
        {
            string path = $"{SavePath}/{key}.bin";
            if (File.Exists(path))
            {
                using FileStream file = File.Open(path, FileMode.Open);
                if (typeof(T).GetCustomAttribute<PreferJsonConvertAttribute>() == null)
                    JsonUtility.FromJsonOverwrite((string)formatter.Deserialize(file), data);
                else
                    JsonConvert.PopulateObject((string)formatter.Deserialize(file), data);
                return true;
            }
            return false;
        }
        /// <summary>
        /// Load json from saving and overwrite object
        /// </summary>
        /// <param name="data"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool Overwrite<T>(T data)
        {
            return Overwrite(typeof(T).Name, data);
        }
        /// <summary>
        /// Load json from saving and parse to <see cref="T"/> object, if has no saving allocate new one
        /// </summary>
        /// <param name="key"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T LoadOrNew<T>(string key) where T : class, new()
        {
            T data = null;
            string path = $"{SavePath}/{key}.bin";
            if (File.Exists(path))
            {
                using FileStream file = File.Open(path, FileMode.Open);
                if (typeof(T).GetCustomAttribute<PreferJsonConvertAttribute>() == null)
                    data = JsonUtility.FromJson<T>((string)formatter.Deserialize(file));
                else
                    data = JsonConvert.DeserializeObject<T>((string)formatter.Deserialize(file));
            }
            data ??= new T();
            return data;
        }
        /// <summary>
        /// Load json from saving and parse to <see cref="T"/> object, if has no saving allocate new one
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static T LoadOrNew<T>() where T : class, new()
        {
            return LoadOrNew<T>(typeof(T).Name);
        }
    }
}