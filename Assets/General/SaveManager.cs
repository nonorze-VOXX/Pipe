using System.IO;
using UnityEngine;

namespace General
{
    public class SaveManager
    {
        public static void Save<T>(T saveData, string path)
        {
            var saveJson = JsonUtility.ToJson(saveData);
            File.WriteAllText(path, saveJson);
        }

        public static T Load<T>(string path) where T : new()
        {
            var saveJson = "";
            try
            {
                saveJson = File.ReadAllText(path);
            }
            catch (FileNotFoundException)
            {
                var f = File.Open(path, FileMode.Create);
                f.Close();
                var t = new T();
                Save(t, path);
                return t;
            }

            return JsonUtility.FromJson<T>(saveJson);
        }
    }
}