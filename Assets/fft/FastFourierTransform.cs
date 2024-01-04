using System;
using System.Collections.Generic;
using System.Linq;
using Pipe;
using UnityEngine;
using Random = UnityEngine.Random;

namespace fft
{
    internal struct LastState
    {
        public float time;
        public Vector2 position;
    }

    [RequireComponent(typeof(AudioSource))]
    public class FastFourierTransform : MonoBehaviour
    {
        public GameObject cube;
        public FFtConfig fFtConfig;
        public bool cubeShow;
        public Recorder recorder;
        private readonly List<UnitPipeGameObject> _unusedPipes = new();
        private readonly List<UnitPipeGameObject> _usedPipes = new();
        private readonly List<GameObject> cubes = new();
        private readonly List<SpriteRenderer> leds = new();
        private readonly float[] samples = new float[512];
        private List<float> _cdTimer;
        private List<Color> _colors;
        private List<LastState> _filterLastStates;

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
            _filterLastStates = new List<LastState>();
            foreach (var filter in fFtConfig.filterConfigs)
            {
                _trigger.Add(false);
                _cdTimer.Add(0);
                var lastState = new LastState();
                lastState.time = Time.time;
                lastState.position = Vector2.zero;
                _filterLastStates.Add(lastState);
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

            audioSource.time = fFtConfig.startTime; // assuming that you already have reference to your AudioSource
            audioSource.Play();
        }

        private void Update()
        {
            for (var i = 0; i < _cdTimer.Count; i++) _cdTimer[i] += Time.deltaTime;
            var willRemove = new List<UnitPipeGameObject>();
            foreach (var pipe in _usedPipes)
                if (!pipe.IsTrigger())
                    willRemove.Add(pipe);

            foreach (var pipe in willRemove)
            {
                _usedPipes.Remove(pipe);
                _unusedPipes.Add(pipe);
            }

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
                            SpinPipe(_pipeGameObjects, _filterLastStates, _colors, index);
                }

                index++;
            }
        }

        private void SpinPipe(List<List<UnitPipeGameObject>> pipeGameObjects, List<LastState> filterLastTime,
            List<Color> color, int index)
        {
            var lastState = filterLastTime[index];
            var filterDeltaTime = Time.time - lastState.time;
            lastState.time = Time.time;

            UnitPipeGameObject nextPipe;
            if (filterDeltaTime < 0.3f && _unusedPipes.Count > 0)
            {
                nextPipe = _unusedPipes[0];
                foreach (var pipeGameObject in _unusedPipes)
                    if (
                        Vector2.Distance(lastState.position, pipeGameObject.transform.position)
                        < Vector2.Distance(nextPipe.transform.position, lastState.position)
                    )
                        nextPipe = pipeGameObject;
            }
            else
            {
                var random = Random.Range(0, _unusedPipes.Count);
                nextPipe = _unusedPipes[random];
            }

            _unusedPipes.Remove(nextPipe);
            _usedPipes.Add(nextPipe);
            nextPipe.Trigger(color[index]);
            lastState.position = nextPipe.transform.position;


            filterLastTime[index] = lastState;
        }

        private bool IsTrigger(int i, float result, FilterConfig filter, float timer)
        {
            var nowVolumn = result * 1000;
            if (_trigger[i])
            {
                //var leftTrigger = nowVolumn < filter.threshold * (1 + filter.tolerance / 100);
                var rightTrigger = nowVolumn > filter.threshold * (1 - filter.tolerance / 100);
                _trigger[i] = rightTrigger;
                return false;
            }

            if (timer < filter.cd)
                _trigger[i] = false;
            else
                _trigger[i] = nowVolumn > filter.threshold;
            return _trigger[i];
        }


        private void GetAudio()
        {
            audioSource.GetSpectrumData(samples, 0, FFTWindow.BlackmanHarris);
        }

        public void SetPipeGameObjects(List<List<UnitPipeGameObject>> pipeGameObjects)
        {
            _pipeGameObjects = pipeGameObjects;
            foreach (var pipe1d in pipeGameObjects)
            foreach (var pipe in pipe1d)
                _unusedPipes.Add(pipe);
        }
    }
}