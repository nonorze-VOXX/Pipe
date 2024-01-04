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

    public enum ShowType
    {
        Note,
        slide,
        AOE
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
        public bool active;
        public ShowType showType;
    }

    [CreateAssetMenu(fileName = "fftConfig", menuName = "fftConfig", order = 0)]
    public class FFtConfig : ScriptableObject
    {
        public List<FilterConfig> filterConfigs;
        public float startTime;
    }
}