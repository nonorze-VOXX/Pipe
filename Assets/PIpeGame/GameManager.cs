using System;
using System.Collections.Generic;
using System.Linq;
using GameSetting;
using General;
using UnityEngine;
using UnityTools.Vector2;
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
        private Camera _camera;
        private GameFlow _gameFlow;
        private List<List<UnitPipe>> _pipe2D;
        private List<List<UnitPipeGameObject>> _pipeGameObjects;
        private Vector2 _waterSource;

        private void Update()
        {
            switch (_gameFlow)
            {
                case GameFlow.START:
                    UpdatePipe();
                    _gameFlow = GameFlow.PLAYING;
                    break;
                case GameFlow.PLAYING:
                    _camera.transform.position = GetBoardCenterPosition(pipeData.boardLeftDown, pipeData.pipeSize,
                        pipeData.mapSize, pipeData.puzzleType);
                    var orthographicSize = _camera.orthographicSize * 2;
                    var cameraSize = new Vector2(_camera.aspect, 1) * orthographicSize;
                    _camera.orthographicSize = GetCameraSize(pipeData.mapSize, pipeData.pipeSize, cameraSize,
                        _camera.aspect, pipeData.puzzleType);
                    if (pipeData.GameWin)
                        _gameFlow = GameFlow.WIN;
                    break;
                case GameFlow.WIN:

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool InMap<T>(List<List<T>> list, Vector2 targetPosition)
        {
            try
            {
                var t = list[(int)targetPosition.y][(int)targetPosition.x];
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }


        public void UpdatePipe()
        {
            var waterPipe = GetWaterPipe(_pipe2D, _waterSource, pipeData.puzzleType);
            var connected = 0;
            foreach (var (pipe1d, y) in waterPipe.Select((value, i) => (value, i)))
            foreach (var (pipe, x) in pipe1d.Select((value, i) => (value, i)))
            {
                var upg = ListFunction.Get2DArrByVector2(_pipeGameObjects, new Vector2(x, y));
                if (waterPipe[y][x] == PipeStatus.Watered) connected++;
                upg.SetConnectWaterSource(waterPipe[y][x]);
            }

            if (connected == (int)pipeData.mapSize.y * (int)pipeData.mapSize.x) pipeData.GameWin = true;
        }


        #region Getter

        private List<List<PipeStatus>> GetWaterPipe(List<List<UnitPipe>> pipe2D, Vector2 waterSource,
            PuzzleType puzzleType)
        {
            var pipeContain = PipeStatus.Watered;
            var visted = new List<Vector2>();
            var candidate = new Queue<Vector2>();
            candidate.Enqueue(waterSource);
            while (candidate.Count != 0)
            {
                var now = candidate.Dequeue();
                visted.Add(now);
                var neighborVisited = 0;
                var neighbor = GetNeighbor(puzzleType, (int)now.y);

                var linkState = ListFunction.Get2DArrByVector2(pipe2D, now).connections;
                foreach (var dir in neighbor)
                {
                    var next = now + dir;
                    if (!InMap(pipe2D, next))
                        continue;
                    if (linkState[dir] && ListFunction.Get2DArrByVector2(pipe2D, now + dir)
                            .connections[dir * -1])
                    {
                        if (visted.Contains(next))
                            neighborVisited += 1;
                        else
                            candidate.Enqueue(next);
                    }
                }

                if (neighborVisited > 1) pipeContain = PipeStatus.Cycling;
            }

            List<List<PipeStatus>> waterPipe = new();
            for (var y = 0; y < pipe2D[0].Count; y++)
            {
                var pipe1D = new List<PipeStatus>();
                for (var x = 0; x < pipe2D.Count; x++) pipe1D.Add(PipeStatus.Dry);
                waterPipe.Add(pipe1D);
            }

            foreach (var vector2 in visted) ListFunction.Set2DArrByVector2(waterPipe, vector2, pipeContain);

            return waterPipe;
        }

        private List<Vector2> GetNeighbor(PuzzleType puzzleType, int x)
        {
            switch (puzzleType)
            {
                case PuzzleType.FOUR:
                    return Vector2List.FourDirection();
                case PuzzleType.SIX:
                    return Vector2List.SixDirectionIn2DArray()[x % 2];
                default:
                    throw new ArgumentOutOfRangeException(nameof(puzzleType), puzzleType, null);
            }
        }

        public GameFlow GetGameFlow()
        {
            return _gameFlow;
        }

        #region Camera

        private Vector3 GetBoardCenterPosition(Vector3 boardLeftDown, Vector3 pipeSize, Vector2 mapSize,
            PuzzleType puzzleType)
        {
            switch (puzzleType)
            {
                case PuzzleType.FOUR:
                    return new Vector3(boardLeftDown.x + pipeData.pipeSize.x * pipeData.mapSize.x,
                        boardLeftDown.y + pipeData.pipeSize.y * pipeData.mapSize.y, -20) / 2;
                case PuzzleType.SIX:
                    return boardLeftDown + GetBoardSize(pipeSize, mapSize, puzzleType) / 2;
                default:
                    throw new ArgumentOutOfRangeException(nameof(puzzleType), puzzleType, null);
            }
        }

        #region boardSize

        private Vector3 GetBoardSize(Vector3 pipeSize, Vector2 mapSize, PuzzleType puzzleType)
        {
            return new Vector3(GetBoardWidth(pipeSize, mapSize, puzzleType),
                GetBoardHeight(pipeSize, mapSize, puzzleType), -20);
        }

        private float GetBoardHeight(Vector3 pipeSize, Vector2 mapSize, PuzzleType puzzleType)
        {
            float blockHeight;
            switch (puzzleType)
            {
                case PuzzleType.FOUR:
                    blockHeight = pipeSize.y;
                    break;
                case PuzzleType.SIX:
                    blockHeight = pipeSize.x / Mathf.Cos(30.0f / 180 * Mathf.PI);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(puzzleType), puzzleType, null);
            }

            return GetPipePositon(pipeSize, new Vector3(mapSize.x - 1, mapSize.y - 1, 0), puzzleType).y -
                GetPipePositon(pipeSize, new Vector3(0, 0, 0), puzzleType).y + blockHeight;
        }

        private float GetBoardWidth(Vector3 pipeSize, Vector2 mapSize, PuzzleType puzzleType)
        {
            switch (puzzleType)
            {
                case PuzzleType.FOUR:
                    return GetPipePositon(pipeSize, new Vector3(mapSize.x, 1, 0), puzzleType).x -
                           GetPipePositon(pipeSize, new Vector3(0, 0, 0), puzzleType).x;
                case PuzzleType.SIX:
                    var width = GetPipePositon(pipeSize, new Vector3(mapSize.x, 1, 0), puzzleType).x -
                        GetPipePositon(pipeSize, new Vector3(0, 0, 0), puzzleType).x + pipeSize.x;
                    return width;
                default:
                    throw new ArgumentOutOfRangeException(nameof(puzzleType), puzzleType, null);
            }
        }

        #endregion

        private float GetCameraSize(Vector2 mapSize, Vector2 pipeSize, Vector2 cameraSize, float cameraAspect,
            PuzzleType puzzleType)
        {
            var boardHeight = GetBoardHeight(pipeSize, mapSize, puzzleType);
            var boardWidth = GetBoardWidth(pipeSize, mapSize, puzzleType);
            if (boardHeight / boardWidth < cameraSize.y / cameraSize.x)
                return GetBoardWidth(pipeSize, mapSize, puzzleType) / cameraAspect / 2;
            return GetBoardHeight(pipeSize, mapSize, puzzleType) / 2;
        }

        #endregion

        private Vector3 GetPipePositon(Vector3 pipeSize, Vector3 index,
            PuzzleType puzzleType)
        {
            return GetPipePositon(Vector3.zero, pipeSize, index, puzzleType);
        }

        private Vector3 GetPipePositon(Vector3 boardLeftDown, Vector3 pipeSize, Vector3 index,
            PuzzleType puzzleType)
        {
            switch (puzzleType)
            {
                case PuzzleType.FOUR:
                    return
                        boardLeftDown + VectorFunction.ElementWiseMultiply(index, pipeSize) + pipeSize / 2;
                case PuzzleType.SIX:
                    return boardLeftDown + VectorFunction.ElementWiseMultiply(pipeSize,
                        new Vector3(index.x + pipeSize.x - pipeSize.x / 2 * (index.y % 2),
                            pipeSize.y / 2 / Mathf.Cos(30.0f / 180 * Mathf.PI) +
                            index.y * Mathf.Cos(30.0f / 180 * Mathf.PI),
                            index.z));
                default:
                    throw new ArgumentOutOfRangeException(nameof(puzzleType), puzzleType, null);
            }
        }

        #endregion


        #region Init

        private void Awake()
        {
            pipeData.GameWin = false;
        }

        private void Start()
        {
            _camera = Camera.main;

            _pipeGameObjects = new List<List<UnitPipeGameObject>>();
            _pipe2D = ListFunction.Generate2DArrByVector2<UnitPipe>(pipeData.mapSize);
            foreach (var (pipe1D, y) in _pipe2D.Select((value, i) => (value, i)))
            foreach (var (pipe, x) in pipe1D.Select((value, i) => (value, i)))
            {
                pipe.SetPuzzleType(pipeData.puzzleType);
                pipe.SetNeighbor(GetNeighbor(pipeData.puzzleType, y));
            }

            var init = new Vector2(Random.Range(0, (int)pipeData.mapSize.x - 1),
                Random.Range(0, (int)pipeData.mapSize.y - 1));
            _pipe2D = GenerateMap(_pipe2D, init, pipeData.puzzleType);
            _waterSource = init;

            foreach (var (pipe1d, y) in _pipe2D.Select((value, i) => (value, i)))
            {
                var list = new List<UnitPipeGameObject>();
                foreach (var (pipe, x) in pipe1d.Select((value, i) => (value, i)))
                {
                    var np = Instantiate(pipePrefab, transform);
                    np.transform.name = "pipe" + y + "-" + x;
                    np.transform.position =
                        GetPipePositon(pipeData.boardLeftDown, pipeData.pipeSize, new Vector3(x, y, 0),
                            pipeData.puzzleType);
                    var unitPipeGameObject = np.GetComponent<UnitPipeGameObject>();
                    unitPipeGameObject.SetGameManager(this);
                    unitPipeGameObject.SetPuzzleType(pipeData.puzzleType);
                    unitPipeGameObject.SetUnitPipe(pipe);
                    var ran = Random.Range(0, (int)pipeData.puzzleType);
                    for (var i = 0; i < ran; i++) unitPipeGameObject.RotateOverClock(true);
                    list.Add(unitPipeGameObject);
                }

                _pipeGameObjects.Add(list);
            }

            _gameFlow = GameFlow.START;
        }

        #endregion


        #region generateMap

        private List<List<UnitPipe>> GenerateMap(List<List<UnitPipe>> pipe2D, Vector2 init, PuzzleType puzzleType)
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
                var output = "";
                foreach (var c in candidate) output += c.ToString();
                print(output);
                candidate.RemoveRange(ran, 1);
                var connectCandidate = GetConnectCandidate(next, pipe2D, visted, puzzleType);
                if (connectCandidate.Count != 0)
                {
                    var ranConnection = Random.Range(0, connectCandidate.Count);
                    ConnectPipeByDirection(pipe2D, next, connectCandidate[ranConnection]);
                    visted.Add(next);
                }

                if (connectCandidate.Count != 0 || visted.Count == 1)
                    foreach (var dir in GetNeighbor(puzzleType, (int)next.x))
                        if (InMap(pipe2D, next + dir) &&
                            !candidate.Contains(next + dir) &&
                            !visted.Contains(next + dir))
                            candidate.Add(next + dir);
            }


            return pipe2D;
        }

        private List<Vector2> GetConnectCandidate(Vector2 next, List<List<UnitPipe>> pipe2D, List<Vector2> visted,
            PuzzleType puzzleType)
        {
            var connectCandidate = new List<Vector2>();
            foreach (var dir in GetNeighbor(puzzleType, (int)next.y))
            {
                if (!InMap(pipe2D, next + dir)) continue;

                if (visted.Contains(next + dir) &&
                    ListFunction.Get2DArrByVector2(pipe2D, next + dir).GetNumOfConnection() < (int)puzzleType - 1)
                    connectCandidate.Add(dir);
            }

            return connectCandidate;
        }

        private void ConnectPipeByDirection(List<List<UnitPipe>> pipe2D, Vector2 next, Vector2 dir)
        {
            ListFunction.Get2DArrByVector2(pipe2D, next).connections[dir] = true;
            ListFunction.Get2DArrByVector2(pipe2D, next + dir)
                .connections[dir * -1] = true;
        }

        #endregion
    }
}