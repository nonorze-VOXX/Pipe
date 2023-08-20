using System.Collections.Generic;
using GameSetting;
using UnityEngine;

namespace Pipe
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private PipeData pipeData;
        [SerializeField] private GameObject pipePrefab;
        private readonly List<Vector2> iToV2 = new();
        private readonly Dictionary<Vector2, int> V2ToIndex = new();
        private List<List<UnitPipe>> _pipe2D;

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
            _pipe2D = new List<List<UnitPipe>>();
            for (var y = 0; y < pipeData.mapSize.y; y++)
            {
                var pipe1D = new List<UnitPipe>();
                for (var x = 0; x < pipeData.mapSize.x; x++) pipe1D.Add(new UnitPipe());
                _pipe2D.Add(pipe1D);
            }
        }

        private void Start()
        {
            var init = new Vector2(Random.Range(0, (int)pipeData.mapSize.x - 1),
                Random.Range(0, (int)pipeData.mapSize.y - 1));
            // var init = new Vector2(1, 1);
            _pipe2D = GenerateMap(_pipe2D, init, pipeData.puzzleType);
            for (var y = 0; y < _pipe2D.Count; y++)
            {
                var pipe1D = _pipe2D[y];
                for (var x = 0; x < pipe1D.Count; x++)
                {
                    var p = pipe1D[x];
                    var np = Instantiate(pipePrefab, transform);
                    np.transform.name = "pipe" + y + x;
                    np.transform.position =
                        GetPipePositon(pipeData.boardLeftDown, pipeData.pipeSize, new Vector3(x, y, 0));
                    var unitPipeGameObject = np.GetComponent<UnitPipeGameObject>();
                    unitPipeGameObject.SetPuzzleType(pipeData.puzzleType);
                    unitPipeGameObject.SetUnitPipe(p);
                }
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
            while (candidate.Count != 0)
            {
                var ran = Random.Range(0, candidate.Count);
                var next = candidate[ran];
                candidate.RemoveRange(ran, 1);
                visted.Add(next);
                List<Vector2> connectCandidate = new();
                // foreach (var c in visted) Debug.Log(c);
                foreach (var dir in iToV2)
                {
                    if (!InMap(next + dir, new Vector2(pipe2D[0].Count, pipe2D.Count))) continue;

                    if (visted.Contains(next + dir))
                    {
                        connectCandidate.Add(dir);
                    }
                    else
                    {
                        if (!candidate.Contains(next + dir)) candidate.Add(next + dir);
                    }
                }

                if (connectCandidate.Count != 0)
                {
                    var ranConnection = Random.Range(0, connectCandidate.Count);
                    ConnectPipeByDirection(pipe2D, next, connectCandidate[ranConnection], PuzzleType);
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
    }
}