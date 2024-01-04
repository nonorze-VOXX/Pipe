using UnityEngine;

namespace fft
{
    [CreateAssetMenu(fileName = "recorderSetting", menuName = "recorderSetting", order = 0)]
    public class RecorderSetting : ScriptableObject
    {
        public Vector3 position;
        public int maxPointNumber;
        public float upsideheight;
        public float downsideheight;
    }
}