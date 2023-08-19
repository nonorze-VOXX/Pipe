using System.Collections.Generic;
using GameSetting;
using UnityEngine;

namespace Pipe
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private PipeData pipeData;
        [SerializeField] private GameObject pipePrefab;
        private List<List<UnitPipe>> _pipe2D;

        private void Awake()
        {
            _pipe2D = new List<List<UnitPipe>>();
            for (var y = 0; y < pipeData.mapSize.x; y++)
            {
                var pipe1D = new List<UnitPipe>();
                for (var x = 0; x < pipeData.mapSize.x; x++) pipe1D.Add(new UnitPipe());
                _pipe2D.Add(pipe1D);
            }
        }

        private void Start()
        {
            var init = new Vector2(Random.Range(0, (int)pipeData.mapSize.x - 1),
                Random.Range(0, (int)pipeData.mapSize.x - 1));
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
            List<Vector2> iToV2 = new();
            iToV2.Add(Vector2.right);
            iToV2.Add(Vector2.up);
            iToV2.Add(Vector2.left);
            iToV2.Add(Vector2.down);
            Queue<Vector2> processing = new();
            processing.Enqueue(init);
            while (processing.Count != 0)
            {
                var now = processing.Dequeue();
                Debug.Log(now);
                if (Get2DArrByVector2(pipe2D, now).generated) continue;

                var ran = Random.Range(2, (int)Mathf.Pow(2, PuzzleType + 1));
                //0 right
                //1 up
                //2 left
                //3 down
                for (var i = 0; i < 4; i++)
                {
                    ran = ran >> 1;
                    if (!InMap(now + iToV2[i], pipe2D.Count, pipe2D[0].Count)) continue;
                    if (Get2DArrByVector2(pipe2D, now + iToV2[i]).generated) continue;
                    if (Get2DArrByVector2(pipe2D, now + iToV2[i]).choosed) continue;
                    if (ran % 2 == 1 && !Get2DArrByVector2(pipe2D, now + iToV2[i]).generated)
                    {
                        Get2DArrByVector2(pipe2D, now).connections[i] = true;
                        Get2DArrByVector2(pipe2D, now + iToV2[i]).connections[(PuzzleType / 2 + i) % PuzzleType] = true;
                        Get2DArrByVector2(pipe2D, now + iToV2[i]).choosed = true;

                        processing.Enqueue(now + iToV2[i]);
                    }
                    else
                    {
                        Get2DArrByVector2(pipe2D, now).connections[i] = false;
                    }
                }

                foreach (var c in Get2DArrByVector2(pipe2D, now).connections) Debug.Log(c);
                Get2DArrByVector2(pipe2D, now).choosed = true;
                Get2DArrByVector2(pipe2D, now).generated = true;
            }

            return pipe2D;
        }

        private bool InMap(Vector2 now, int y, int x)
        {
            return now.x < x &&
                   now.x >= 0 &&
                   now.y < y &&
                   now.y >= 0;
        }


        private T Get2DArrByVector2<T>(List<List<T>> pipe2D, Vector2 now)
        {
            return pipe2D[(int)now.y][(int)now.x];
        }
    }
}