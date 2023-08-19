using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Pipe
{
    public class UnitPipeGameObject : MonoBehaviour
    {
        private List<GameObject> _pipeLine;
        private int _puzzleType;
        private UnitPipe _unitPipe;

        private void Awake()
        {
            _pipeLine = new List<GameObject>();
            _pipeLine.Add(transform.GetChild(0).GameObject());
        }

        private void Start()
        {
            var angle = 360 / _puzzleType;
            for (var i = 1; i < _puzzleType; i++)
            {
                var line = Instantiate(transform.GetChild(0).GameObject(), transform);
                line.transform.Rotate(new Vector3(0, 0, angle * i));
                _pipeLine.Add(line);
            }

            var index = 0;
            foreach (var up in _unitPipe.connections) transform.GetChild(index++).GameObject().SetActive(up);
        }

        public void SetPuzzleType(int line)
        {
            _puzzleType = line;
        }

        public void SetUnitPipe(UnitPipe up)
        {
            _unitPipe = up;
        }
    }
}