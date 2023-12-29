using System;
using System.Collections.Generic;
using System.Linq;
using Pipe;
using UnityEngine;

namespace fft
{
    [RequireComponent(typeof(AudioSource))]
    public class FastFourierTransform : MonoBehaviour
    {
        public GameObject cube;
        public FFtConfig fFtConfig;
        public bool cubeShow;
        private readonly List<GameObject> cubes = new();
        private readonly List<SpriteRenderer> leds = new();
        private readonly float[] samples = new float[512];

        private List<List<UnitPipeGameObject>> _pipeGameObjects;
        private AudioSource audioSource;

        private SpriteRenderer cubeSprite;

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
            cubeSprite = cube.GetComponent<SpriteRenderer>();
            if (cubeShow)
                for (var i = 0; i < samples.Length; i++)
                {
                    var nCube = Instantiate(cube);
                    nCube.name = i.ToString();
                    nCube.transform.position = new Vector3(i, 0, 0);
                    cubes.Add(nCube);
                }

            var x = 0;
            var y = -2;
            if (cubeShow)
                foreach (var filter in fFtConfig.filterConfigs)
                {
                    var nCube = Instantiate(cube);
                    nCube.name = filter.name;
                    nCube.transform.position = new Vector3(x, y, 0);
                    x += 2;
                    leds.Add(nCube.GetComponent<SpriteRenderer>());
                }
        }

        private void Update()
        {
            GetAudio();
            if (cubeShow)
                for (var i = 0; i < samples.Length; i++)
                {
                    var tmp = cubes[i].transform.position;
                    tmp.y = samples[i] * 100;
                    cubes[i].transform.position = tmp;
                }

            var index = 0;
            foreach (var filter in fFtConfig.filterConfigs)
            {
                var targetList = samples.Skip(filter.startIndex).Take(filter.endIndex);
                float result;
                switch (filter.type)
                {
                    case FilterType.sum:
                        result = targetList.Sum();
                        break;
                    case FilterType.max:
                        result = targetList.Max();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }


                if (cubeShow)
                    if (result * 1000 > filter.threshold)
                        leds[index].color = Color.red;
                    else
                        leds[index].color = Color.white;
                if (_pipeGameObjects != null)
                    if (result * 1000 > filter.threshold)
                        SpinPipe(_pipeGameObjects);
                index++;
            }
        }

        private void SpinPipe(List<List<UnitPipeGameObject>> pipeGameObjects)
        {
            foreach (var pipe1d in pipeGameObjects)
            foreach (var pipe in pipe1d)
            {
                if (pipe.IsTrigger()) continue;
                pipe.Trigger();
                return;
            }
        }

        private void GetAudio()
        {
            audioSource.GetSpectrumData(samples, 0, FFTWindow.BlackmanHarris);
        }

        public void SetPipeGameObjects(List<List<UnitPipeGameObject>> pipeGameObjects)
        {
            _pipeGameObjects = pipeGameObjects;
        }
    }
}