using System.Collections.Generic;

namespace Pipe
{
    public class UnitPipe
    {
        public bool choosed;
        public List<bool> connections;

        public bool generated;
        // public List<Vector2> neighbors;

        public UnitPipe()
        {
            connections = new List<bool>();
            connections.Add(false);
            connections.Add(false);
            connections.Add(false);
            connections.Add(false);
            generated = false;
            choosed = false;
            // neighbors = new List<Vector2>();
        }
    }
}