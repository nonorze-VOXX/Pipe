using System;
using System.Collections.Generic;

namespace Pipe
{
    [Serializable]
    public class UnitPipe
    {
        public List<bool> connections;

        public UnitPipe()
        {
            connections = new List<bool>();
            connections.Add(false);
            connections.Add(false);
            connections.Add(false);
            connections.Add(false);
        }

        public int GetNumOfConnection()
        {
            var ans = 0;
            foreach (var c in connections)
                if (c)
                    ans++;

            return ans;
        }

        public void RotateOverClock(bool reverse)
        {
            List<bool> newList = new();
            var dir = 1;
            if (reverse) dir = -1;
            for (var index = 0; index < connections.Count; index++)
                newList.Add(connections[(index + connections.Count + dir) % connections.Count]);
            connections = newList;
        }
    }
}