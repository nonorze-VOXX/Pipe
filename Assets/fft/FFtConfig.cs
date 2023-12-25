using System;
using System.Collections.Generic;
using UnityEngine;

namespace fft
{
    public enum FilterType
    {
        sum,
        max
    }

    [Serializable]
    public struct filterConfig
    {
        public int endIndex;
        public string name;
        public int startIndex;
        public float threshold;
        public FilterType type;
    }

    [CreateAssetMenu(fileName = "fftConfig", menuName = "fftConfig", order = 0)]
    public class FFtConfig : ScriptableObject
    {
        public List<filterConfig> filterConfigs;
    }
}