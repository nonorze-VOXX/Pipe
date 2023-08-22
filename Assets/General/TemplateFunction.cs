using System.Collections.Generic;
using UnityEngine;

namespace General
{
    public class TemplateFunction
    {
        
        public static List<List<T>> Generate2DArrByVector2<T>(Vector2 size) where T : new()
        {
            var t2 = new List<List<T>>();
            for (var y = 0; y < size.y; y++)
            {
                var t1 = new List<T>();
                for (var x = 0; x < size.x; x++) t1.Add(new T());
                t2.Add(t1);
            }

            return t2;
        }

        public static T Get2DArrByVector2<T>(List<List<T>> pipe2D, Vector2 now)
        {
            return pipe2D[(int)now.y][(int)now.x];
        }

        public static void Set2DArrByVector2<T>(List<List<T>> arr2, Vector2 vector2, T t)
        {
            arr2[(int)vector2.y][(int)vector2.x] = t;
        }
    }
}