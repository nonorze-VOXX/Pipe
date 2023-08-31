using System;
using System.Collections.Generic;
using UnityEngine;
using UnityTools.Vector2;

namespace Pipe
{
    [Serializable]
    public class UnitPipe
    {
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
            foreach (var dir in Vector2List.FourDirection()) queue.Enqueue(connections[dir]);
            queue.Enqueue(queue.Dequeue());
            if (reverse)
            {
                queue.Enqueue(queue.Dequeue());
                queue.Enqueue(queue.Dequeue());
            }

            foreach (var dir in Vector2List.FourDirection()) connections[dir] = queue.Dequeue();
        }
    }
}