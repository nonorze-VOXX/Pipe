using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace fft
{
    public class Recorder : MonoBehaviour
    {
        public GameObject cubePrefab;
        public RecorderSetting recorderSetting;
        private readonly List<GameObject> cubes = new();
        private int nowIndex;

        public GameObject pastCube;
        private void Start()
        {
            nowIndex = 0;
        }

        private void Update()
        {
            transform.position = recorderSetting.position;
        }

        public void Record(bool b)
        {
            if (nowIndex >= cubes.Count)
            {
                if (cubes.Count < recorderSetting.maxPointNumber)
                {
                    var newCube = Instantiate(cubePrefab);
                    cubes.Add(newCube);
                }
                else
                {
                    nowIndex = 0;
                }
            }

            if (b)
                cubes[nowIndex].transform.position =
                    (Vector2)transform.position + new Vector2(nowIndex, recorderSetting.upsideheight);
            else
                cubes[nowIndex].transform.position = (Vector2)transform.position +
                                                     new Vector2(nowIndex, recorderSetting.downsideheight);
            pastCube = cubes[nowIndex];
            nowIndex++;
        }
    }
}