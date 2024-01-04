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
    public struct FilterConfig
    {
        public int endIndex;
        public string name;
        public int startIndex;
        public float threshold;
        public FilterType type;
        public float tolerance;
        public float cd;
    }

    [CreateAssetMenu(fileName = "fftConfig", menuName = "fftConfig", order = 0)]
    public class FFtConfig : ScriptableObject
    {
        public List<FilterConfig> filterConfigs;
    }
}