using System.Collections.Generic;
using GameSetting;
using UnityEngine;

namespace Pipe
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private PipeData pipeData;
        private List<List<UnitPipe>> _pipe2D;

        private void Awake()
        {
        }

        private void Start()
        {
            _pipe2D = new List<List<UnitPipe>>();
            for (var y = 0; y < pipeData.mapSize.x; y++)
            {
                var pipe1D = new List<UnitPipe>();
                for (var x = 0; x < pipeData.mapSize.x; x++) pipe1D.Add(new UnitPipe());
                _pipe2D.Add(pipe1D);
            }

            var init = new Vector2((int)Random.Range(1, pipeData.mapSize.x) - 1,
                (int)Random.Range(1, pipeData.mapSize.x) - 1);
            Debug.Log(init);
            Debug.Log(pipeData.puzzleType);
            Debug.Log(_pipe2D);
            _pipe2D = GenerateMap(_pipe2D, init, pipeData.puzzleType);
        }

        private List<List<UnitPipe>> GenerateMap(List<List<UnitPipe>> pipe2D, Vector2 init, int PuzzleType)
        {
            List<Vector2> iToV2 = new();
            iToV2.Add(Vector2.right);
            iToV2.Add(Vector2.up);
            iToV2.Add(Vector2.down);
            iToV2.Add(Vector2.left);
            Queue<Vector2> processing = new();
            processing.Enqueue(init);
            while (processing.Count != 0)
            {
                var now = processing.Dequeue();
                if (Get2DArrByVector2(pipe2D, now).generated) continue;

                var ran = Random.Range(0, (int)Mathf.Pow(2, PuzzleType));
                //0 right
                //1 up
                //2 down
                //3 left
                for (var i = 0; i < 4; i++)
                {
                    if (ran % 2 == 1 && Get2DArrByVector2(pipe2D, now).generated == false)
                    {
                        Get2DArrByVector2(pipe2D, now).connections[i] = true;
                        Get2DArrByVector2(pipe2D, now + iToV2[i]).connections[PuzzleType - 1 - i] = true;
                        processing.Enqueue(now + iToV2[i]);
                    }
                    else
                    {
                        Get2DArrByVector2(pipe2D, now).connections[i] = false;
                    }

                    ran = ran >> 1;
                }

                Get2DArrByVector2(pipe2D, now).generated = true;
            }

            return pipe2D;
        }


        private T Get2DArrByVector2<T>(List<List<T>> pipe2D, Vector2 now)
        {
            return pipe2D[(int)now.y][(int)now.x];
        }
    }
}