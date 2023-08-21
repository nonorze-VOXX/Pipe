using System.Collections.Generic;

namespace Pipe
{
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
    }
}