using System;
using System.Collections.Generic;
using System.Linq;
using GameSetting;
using Pipe;
using UnityEngine;
using Random = UnityEngine.Random;

namespace fft
{
    internal struct LastState
    {
        public float time;
        public Vector2 position;
        public Vector2 direction;
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
        private UnitPipeGameObject _AOECenter;
        private List<float> _cdTimer;
        private List<LastState> _filterLastStates;
        private List<Vector2> _neighbor;

        private List<List<UnitPipeGameObject>> _pipeGameObjects;
        private List<bool> _trigger;
        private Color AOEColor;
        private float AOETimer;
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
            UpdateAOE();
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
                            SpinPipe(_pipeGameObjects, _filterLastStates, index, filter);
                }

                index++;
            }
        }

        private void UpdateAOE()
        {
            if (_AOECenter != null)
            {
                AOETimer += 1;
                var list = FindPipeByDistance(_AOECenter, AOETimer);
                //foreach (var pipe in list) pipe.Trigger(AOEColor);
                foreach (var pipe in list) pipe.ChangeOneBgColor(AOEColor);
                list = FindPipeByDistance(_AOECenter, AOETimer - 1);
                foreach (var pipe in list) pipe.ChangeOneBgColor(Color.white);
                if (AOETimer >= _pipeGameObjects.Count * 2) _AOECenter = null;
                ;
            }
        }

        private List<UnitPipeGameObject> FindPipeByDistance(UnitPipeGameObject center,
            float distance)
        {
            var ans = new List<UnitPipeGameObject>();
            foreach (var pipes in _pipeGameObjects)
            foreach (var pipe in pipes)
                if (
                    Vector2.Distance(pipe.transform.position, center.transform.position) <
                    distance + 0.5f &&
                    Vector2.Distance(pipe.transform.position, center.transform.position) >
                    distance - 0.5f
                )
                    ans.Add(pipe);
            return ans;
        }

        public void SetPuzzleType(PuzzleType puzzleType)
        {
            switch (puzzleType)
            {
                case PuzzleType.FOUR:
                    _neighbor = new List<Vector2>
                    {
                        Vector2.right,
                        Vector2.up,
                        Vector2.left,
                        Vector2.down
                    };
                    break;
                case PuzzleType.SIX:
                    _neighbor = new List<Vector2>
                    {
                        Vector2.right,
                        new(1, 1),
                        Vector2.up,
                        new(-1, 1),
                        Vector2.left,
                        new(-1, -1),
                        Vector2.down,
                        new(1, -1)
                    };
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(puzzleType), puzzleType, null);
            }
        }

        private void SpinPipe(List<List<UnitPipeGameObject>> pipeGameObjects, List<LastState> filterLastTime
            , int index, FilterConfig filterConfig)
        {
            var lastState = filterLastTime[index];
            var filterDeltaTime = Time.time - lastState.time;
            lastState.time = Time.time;
            if (_unusedPipes.Count == 0) return;
            UnitPipeGameObject nextPipe;
            switch (filterConfig.showType)
            {
                case ShowType.Note:
                    if (filterDeltaTime < 0.3f)
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

                    break;
                case ShowType.slide:
                    if (filterDeltaTime > 1f)
                    {
                        var random = Random.Range(0, _unusedPipes.Count);
                        nextPipe = _unusedPipes[random];
                        random = Random.Range(0, _unusedPipes.Count);
                        lastState.direction = _unusedPipes[random].transform.position - nextPipe.transform.position;
                    }
                    else
                    {
                        var triggered = false;
                        var minPipe = _unusedPipes[0];
                        var maxPipe = _unusedPipes[0];
                        foreach (var pipeGameObject in _unusedPipes)
                        {
                            if (
                                Vector2.Distance(lastState.position + lastState.direction,
                                    pipeGameObject.transform.position)
                                <= Vector2.Distance(minPipe.transform.position,
                                    lastState.position + lastState.direction)
                            )
                            {
                                minPipe = pipeGameObject;
                                triggered = true;
                            }

                            if (
                                Vector2.Distance(lastState.position + lastState.direction,
                                    pipeGameObject.transform.position)
                                > Vector2.Distance(maxPipe.transform.position,
                                    lastState.position + lastState.direction)
                            )
                                maxPipe = pipeGameObject;
                        }

                        if (triggered)
                            nextPipe = minPipe;
                        else
                            nextPipe = maxPipe;
                    }

                    break;
                case ShowType.AOE:
                    if (_AOECenter != null) return;
                    var ran = Random.Range(0, _unusedPipes.Count);
                    var pipe = _unusedPipes[ran];
                    AOEColor = filterConfig.color;
                    pipe.ChangeOneBgColor(filterConfig.color);
                    _AOECenter = pipe;
                    AOETimer = 0;
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }


            _unusedPipes.Remove(nextPipe);
            _usedPipes.Add(nextPipe);
            nextPipe.Trigger(filterConfig.color);
            lastState.position = nextPipe.transform.position;


            filterLastTime[index] = lastState;
        }

        private bool IsTrigger(int i, float result, FilterConfig filter, float timer)
        {
            switch (filter.showType)
            {
                case ShowType.Note:
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

                case ShowType.AOE:
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
                case ShowType.slide:
                {
                    var nowVolumn = result * 1000;
                    if (_trigger[i])
                    {
                        var rightTrigger = nowVolumn > filter.threshold * (1 - filter.tolerance / 100);
                        _trigger[i] = rightTrigger;
                    }

                    if (timer < filter.cd)
                        _trigger[i] = false;
                    else
                        _trigger[i] = nowVolumn > filter.threshold;

                    return _trigger[i];
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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

        public void ButtonClick(int i)
        {
            var tmp = fFtConfig.filterConfigs[i];
            tmp.active = !fFtConfig.filterConfigs[i].active;
            fFtConfig.filterConfigs[i] = tmp;
        }
    }
}