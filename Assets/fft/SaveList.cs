using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace fft
{
    public class SaveList : MonoBehaviour
    {
        public static void Save(List<List<int>> dir2d)
        {
            var list = new List<string>();
            foreach (var dir1d in dir2d)
            {
                var joinedNames = string.Join(", ", dir1d);
                list.Add(joinedNames);
            }

            File.WriteAllText("pipeState", string.Join("\n", list));
        }
        //load

        public static List<List<int>> Load()
        {
            var path = "pipeState";
            var saveJson = "";
            try
            {
                saveJson = File.ReadAllText(path);
                var list = saveJson.Split('\n').ToList();
                var dir2d = new List<List<int>>();
                foreach (var s in list)
                {
                    var dir1d = s.Split(", ").Select(int.Parse).ToList();
                    dir2d.Add(dir1d);
                }

                Debug.Log(dir2d);
                Debug.Log(dir2d.Count);
                return dir2d;
            }
            catch (FileNotFoundException)
            {
                Debug.Log("file not found");
                var f = File.Open(path, FileMode.Create);
                f.Close();
                return new List<List<int>>();
            }
        }
    }
}