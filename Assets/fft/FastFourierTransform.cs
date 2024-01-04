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
        public Recorder recorder;
        private readonly List<GameObject> cubes = new();
        private readonly List<SpriteRenderer> leds = new();
        private readonly float[] samples = new float[512];
        private List<float> _cdTimer;
        private List<Color> _colors;

        private List<List<UnitPipeGameObject>> _pipeGameObjects;
        private List<bool> _trigger;
        private AudioSource audioSource;

        private SpriteRenderer cubeSprite;

        private void Start()
        {
            cubeShow = true;
            _colors = new List<Color>();
            _colors.Add(Color.red);
            _colors.Add(Color.green);
            _colors.Add(Color.blue);
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
            _trigger = new List<bool>();
            _cdTimer = new List<float>();
            foreach (var filter in fFtConfig.filterConfigs)
            {
                _trigger.Add(false);
                _cdTimer.Add(0);
            }

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
            for (var i = 0; i < _cdTimer.Count; i++) _cdTimer[i] += Time.deltaTime;

            GetAudio();
            if (cubeShow)
                for (var i = 0; i < samples.Length; i++)
                {
                    var tmp = cubes[i].transform.position;
                    tmp.y = samples[i] * 100;
                    cubes[i].transform.position = tmp;
                }

            var index = 0;
            for (var i = 0; i < fFtConfig.filterConfigs.Count; i++)
            {
                var filter = fFtConfig.filterConfigs[i];
                if (filter.active)
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


                    var trigger = IsTrigger(i, result, filter, _cdTimer[i]);
                    recorder.Record(trigger);
                    if (trigger) _cdTimer[i] = 0;
                    if (cubeShow)
                        if (trigger)
                            leds[index].color = Color.red;
                        else
                            leds[index].color = Color.white;
                    if (_pipeGameObjects != null)
                        if (trigger)
                            SpinPipe(_pipeGameObjects, _colors[index]);
                }

                index++;
            }
        }

        private bool IsTrigger(int i, float result, FilterConfig filter, float timer)
        {
            var nowVolumn = result * 1000;
            if (_trigger[i])
            {
                //var leftTrigger = nowVolumn < filter.threshold * (1 + filter.tolerance / 100);
                var rightTrigger = nowVolumn > filter.threshold * (1 - filter.tolerance / 100);
                _trigger[i] = rightTrigger;
                return _trigger[i];
            }

            if (timer < filter.cd)
                _trigger[i] = false;
            else
                _trigger[i] = nowVolumn > filter.threshold;
            return _trigger[i];
        }


        private void SpinPipe(List<List<UnitPipeGameObject>> pipeGameObjects, Color color)
        {
            foreach (var pipe1d in pipeGameObjects)
            foreach (var pipe in pipe1d)
            {
                if (pipe.IsTrigger()) continue;
                pipe.Trigger(color);
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