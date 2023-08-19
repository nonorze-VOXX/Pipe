using UnityEngine;

namespace GameSetting
{
    [CreateAssetMenu(fileName = "pipeData", menuName = "data/pipeData", order = 0)]
    public class PipeData : ScriptableObject
    {
        public Vector2 mapSize;
        public int puzzleType;
    }
}