using System;
using System.Collections.Generic;
using GameSetting;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Pipe
{
    internal enum GameFlow
    {
        START,
        PLAYING,
        WIN
    }

    public class GameManager : MonoBehaviour
    {
        [SerializeField] private PipeData pipeData;
        [SerializeField] private GameObject pipePrefab;
        private readonly List<Vector2> iToV2 = new();
        private readonly Dictionary<Vector2, int> V2ToIndex = new();
        private Camera _camera;
        private GameFlow _gameFlow;
        private List<List<UnitPipe>> _pipe2D;
        private List<List<UnitPipeGameObject>> _pipeGameObjects;
        private Vector2 _waterSource;

        private void Awake()
        {
            iToV2.Add(Vector2.right);
            iToV2.Add(Vector2.up);
            iToV2.Add(Vector2.left);
            iToV2.Add(Vector2.down);
            V2ToIndex.Add(Vector2.right, 0);
            V2ToIndex.Add(Vector2.up, 1);
            V2ToIndex.Add(Vector2.left, 2);
            V2ToIndex.Add(Vector2.down, 3);
        }

        private void Start()
        {
            _camera = Camera.main;
            // _camera.
            _pipeGameObjects = new List<List<UnitPipeGameObject>>();
            _pipe2D = new List<List<UnitPipe>>();
            for (var y = 0; y < pipeData.mapSize.y; y++)
            {
                var pipe1D = new List<UnitPipe>();
                for (var x = 0; x < pipeData.mapSize.x; x++) pipe1D.Add(new UnitPipe());
                _pipe2D.Add(pipe1D);
            }

            var init = new Vector2(Random.Range(0, (int)pipeData.mapSize.x - 1),
                Random.Range(0, (int)pipeData.mapSize.y - 1));
            _pipe2D = GenerateMap(_pipe2D, init, pipeData.puzzleType);
            _waterSource = init;

            for (var y = 0; y < _pipe2D.Count; y++)
            {
                var pipe1D = _pipe2D[y];
                var list = new List<UnitPipeGameObject>();
                for (var x = 0; x < pipe1D.Count; x++)
                {
                    var p = pipe1D[x];
                    var np = Instantiate(pipePrefab, transform);
                    np.transform.name = "pipe" + y + "-" + x;
                    np.transform.position =
                        GetPipePositon(pipeData.boardLeftDown, pipeData.pipeSize, new Vector3(x, y, 0));
                    var unitPipeGameObject = np.GetComponent<UnitPipeGameObject>();
                    unitPipeGameObject.SetGameManager(this);
                    unitPipeGameObject.SetPuzzleType(pipeData.puzzleType);
                    unitPipeGameObject.SetUnitPipe(p);
                    var ran = Random.Range(0, pipeData.puzzleType);
                    for (var i = 0; i < ran; i++) unitPipeGameObject.RotateOverClock(true);
                    list.Add(unitPipeGameObject);
                }

                _pipeGameObjects.Add(list);
            }

            _gameFlow = GameFlow.START;
        }

        private void Update()
        {
            switch (_gameFlow)
            {
                case GameFlow.START:
                    UpdatePipe();
                    _gameFlow = GameFlow.PLAYING;
                    break;
                case GameFlow.PLAYING:
                    break;
                case GameFlow.WIN:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private Vector3 GetPipePositon(Vector3 pipeDataBoardLeftDown, Vector3 pipeSize, Vector3 index)
        {
            return new Vector3(
                pipeDataBoardLeftDown.x + index.x * pipeSize.x,
                pipeDataBoardLeftDown.y + index.y * pipeSize.y,
                pipeDataBoardLeftDown.z + index.z * pipeSize.z
            );
        }


        private List<List<UnitPipe>> GenerateMap(List<List<UnitPipe>> pipe2D, Vector2 init, int PuzzleType)
        {
            var visted = new List<Vector2>();
            var candidate = new List<Vector2>();
            candidate.Add(init);
            var times = 0;
            visted.Add(init);
            while (candidate.Count != 0)
            {
                times++; //for save
                if (times > pipe2D[0].Count * pipe2D.Count * 3) break;
                var ran = Random.Range(0, candidate.Count);
                var next = candidate[ran];
                candidate.RemoveRange(ran, 1);
                List<Vector2> connectCandidate = new();
                // foreach (var c in visted) Debug.Log(c);
                foreach (var dir in iToV2)
                {
                    if (!InMap(next + dir, new Vector2(pipe2D[0].Count, pipe2D.Count))) continue;

                    if (visted.Contains(next + dir) &&
                        Get2DArrByVector2(pipe2D, next + dir).GetNumOfConnection() < PuzzleType - 1)
                    {
                        connectCandidate.Add(dir);
                    }
                    else
                    {
                        if (!candidate.Contains(next + dir) && !visted.Contains(next + dir)) candidate.Add(next + dir);
                    }
                }

                if (connectCandidate.Count != 0)
                {
                    var ranConnection = Random.Range(0, connectCandidate.Count);
                    ConnectPipeByDirection(pipe2D, next, connectCandidate[ranConnection], PuzzleType);
                    visted.Add(next);
                }
            }


            return pipe2D;
        }

        private void ConnectPipeByDirection(List<List<UnitPipe>> pipe2D, Vector2 next, Vector2 dir, int puzzleType)
        {
            Get2DArrByVector2(pipe2D, next).connections[V2ToIndex[dir]] = true;
            Get2DArrByVector2(pipe2D, next + dir).connections[(puzzleType / 2 + V2ToIndex[dir]) % puzzleType] = true;
        }

        private bool InMap(Vector2 now, Vector2 mapSize)
        {
            return now.x < mapSize.x &&
                   now.x >= 0 &&
                   now.y < mapSize.y &&
                   now.y >= 0;
        }


        private T Get2DArrByVector2<T>(List<List<T>> pipe2D, Vector2 now)
        {
            return pipe2D[(int)now.y][(int)now.x];
        }

        public void UpdatePipe()
        {
            var waterPipe = GetWaterPipe(_pipe2D, _waterSource, pipeData.puzzleType);
            for (var y = 0; y < waterPipe.Count; y++)
            for (var x = 0; x < waterPipe[0].Count; x++)
                Get2DArrByVector2(_pipeGameObjects, new Vector2(x, y)).SetConnectWaterSource(waterPipe[y][x]);
        }

        private List<List<bool>> GetWaterPipe(List<List<UnitPipe>> pipe2D, Vector2 waterSource, int puzzleType)
        {
            var visted = new List<Vector2>();
            var candidate = new Queue<Vector2>();
            candidate.Enqueue(waterSource);
            while (candidate.Count != 0)
            {
                var now = candidate.Dequeue();
                visted.Add(now);
                var linkState = Get2DArrByVector2(pipe2D, now).connections;
                for (var dir = 0; dir < linkState.Count; dir++)
                {
                    var next = now + iToV2[dir];
                    if (!InMap(next, new Vector2(pipe2D[0].Count, pipe2D.Count))
                        || visted.Contains(next))
                        continue;
                    if (linkState[dir] && Get2DArrByVector2(pipe2D, now + iToV2[dir])
                            .connections[(dir + puzzleType / 2) % puzzleType]) candidate.Enqueue(next);
                }
            }

            List<List<bool>> waterPipe = new();
            for (var y = 0; y < pipe2D[0].Count; y++)
            {
                var pipe1D = new List<bool>();
                for (var x = 0; x < pipe2D.Count; x++) pipe1D.Add(false);
                waterPipe.Add(pipe1D);
            }

            foreach (var vector2 in visted) Set2DArrByVector2(waterPipe, vector2, true);

            return waterPipe;
        }

        private void Set2DArrByVector2<T>(List<List<T>> arr2, Vector2 vector2, T t)
        {
            arr2[(int)vector2.y][(int)vector2.x] = t;
        }
    }
}