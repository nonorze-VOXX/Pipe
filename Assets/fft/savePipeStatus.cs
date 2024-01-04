using System.Collections.Generic;
using UnityEngine;

namespace fft
{
    [CreateAssetMenu(fileName = "pipeStatus", menuName = "pipeStatus", order = 0)]
    public class savePipeStatus : ScriptableObject
    {
        public bool recorded;
        public List<List<int>> pipeStatus;
    }
}