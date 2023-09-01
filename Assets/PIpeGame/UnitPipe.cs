using System;
using System.Collections.Generic;
using GameSetting;
using UnityEngine;

namespace Pipe
{
    [Serializable]
    public class UnitPipe
    {
        private List<Vector2> _neighbor;
        public Dictionary<Vector2, bool> connections;

        public UnitPipe()
        {
            connections = new Dictionary<Vector2, bool>
            {
                { Vector2.right, false },
                { Vector2.up, false },
                { Vector2.left, false },
                { Vector2.down, false }
            };
        }

        public void SetNeighbor(List<Vector2> neighbor)
        {
            _neighbor = neighbor;
        }


        public int GetNumOfConnection()
        {
            var ans = 0;
            foreach (var c in connections)
                if (c.Value)
                    ans++;

            return ans;
        }

        public void RotateOverClock(bool reverse)
        {
            Queue<bool> queue = new();
            foreach (var dir in _neighbor) queue.Enqueue(connections[dir]);
            for (var i = 0; i < (_neighbor.Count + (reverse ? -1 : 1)) % _neighbor.Count; i++)
                queue.Enqueue(queue.Dequeue());

            foreach (var dir in _neighbor) connections[dir] = queue.Dequeue();
        }

        public void SetPuzzleType(PuzzleType puzzleType)
        {
            switch (puzzleType)
            {
                case PuzzleType.FOUR:
                    connections = new Dictionary<Vector2, bool>
                    {
                        { Vector2.right, false },
                        { Vector2.up, false },
                        { Vector2.left, false },
                        { Vector2.down, false }
                    };
                    break;
                case PuzzleType.SIX:
                    connections = new Dictionary<Vector2, bool>
                    {
                        { Vector2.right, false },
                        { new Vector2(1, 1), false },
                        { Vector2.up, false },
                        { new Vector2(-1, 1), false },
                        { Vector2.left, false },
                        { new Vector2(-1, -1), false },
                        { Vector2.down, false },
                        { new Vector2(1, -1), false }
                    };
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(puzzleType), puzzleType, null);
            }
        }

        public List<Vector2> GetNeighbor()
        {
            return _neighbor;
        }
    }
}