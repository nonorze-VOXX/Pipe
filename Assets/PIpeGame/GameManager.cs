using System;
using System.Collections.Generic;
using GameSetting;
using General;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Pipe
{
    public enum GameFlow
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
            pipeData.GameWin = false;
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
            _camera.transform.position = new Vector3(pipeData.pipeSize.x * pipeData.mapSize.x,
                pipeData.pipeSize.y * pipeData.mapSize.y, -20) / 2;

            _pipeGameObjects = new List<List<UnitPipeGameObject>>();
            _pipe2D = ListFunction.Generate2DArrByVector2<UnitPipe>(pipeData.mapSize);

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
                    var orthographicSize = _camera.orthographicSize * 2;
                    var cameraSize = new Vector2(_camera.aspect, 1) * orthographicSize;
                    _camera.orthographicSize = GetCameraSize(pipeData.mapSize, cameraSize, _camera.aspect);
                    if (pipeData.GameWin)
                        _gameFlow = GameFlow.WIN;
                    break;
                case GameFlow.WIN:

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public GameFlow GetGameFlow()
        {
            return _gameFlow;
        }

        private float GetCameraSize(Vector2 mapSize, Vector2 cameraSize, float cameraAspect)
        {
            if (cameraSize.x > cameraSize.y)
                return mapSize.y / 2;
            return mapSize.x / cameraAspect / 2;
        }

        private Vector3 GetPipePositon(Vector3 pipeDataBoardLeftDown, Vector3 pipeSize, Vector3 index)
        {
            return new Vector3(
                pipeDataBoardLeftDown.x + index.x * pipeSize.x + pipeSize.x / 2,
                pipeDataBoardLeftDown.y + index.y * pipeSize.y + pipeSize.y / 2,
                pipeDataBoardLeftDown.z + index.z * pipeSize.z + pipeSize.z / 2
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
                        ListFunction.Get2DArrByVector2(pipe2D, next + dir).GetNumOfConnection() < PuzzleType - 1)
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
            ListFunction.Get2DArrByVector2(pipe2D, next).connections[V2ToIndex[dir]] = true;
            ListFunction.Get2DArrByVector2(pipe2D, next + dir)
                .connections[(puzzleType / 2 + V2ToIndex[dir]) % puzzleType] = true;
        }

        private bool InMap(Vector2 now, Vector2 mapSize)
        {
            return now.x < mapSize.x &&
                   now.x >= 0 &&
                   now.y < mapSize.y &&
                   now.y >= 0;
        }


        public void UpdatePipe()
        {
            var waterPipe = GetWaterPipe(_pipe2D, _waterSource, pipeData.puzzleType);
            var connected = 0;
            for (var y = 0; y < waterPipe.Count; y++)
            for (var x = 0; x < waterPipe[0].Count; x++)
            {
                if (waterPipe[y][x]) connected++;
                ListFunction.Get2DArrByVector2(_pipeGameObjects, new Vector2(x, y))
                    .SetConnectWaterSource(waterPipe[y][x]);
            }

            if (connected == (int)pipeData.mapSize.y * (int)pipeData.mapSize.x) pipeData.GameWin = true;
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
                var linkState = ListFunction.Get2DArrByVector2(pipe2D, now).connections;
                for (var dir = 0; dir < linkState.Count; dir++)
                {
                    var next = now + iToV2[dir];
                    if (!InMap(next, new Vector2(pipe2D[0].Count, pipe2D.Count))
                        || visted.Contains(next))
                        continue;
                    if (linkState[dir] && ListFunction.Get2DArrByVector2(pipe2D, now + iToV2[dir])
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

            foreach (var vector2 in visted) ListFunction.Set2DArrByVector2(waterPipe, vector2, true);

            return waterPipe;
        }
    }
}