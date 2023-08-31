using UnityEngine;

namespace GameSetting
{
    public enum PuzzleType
    {
        FOUR = 4,
        SIX = 6
    }

    [CreateAssetMenu(fileName = "pipeData", menuName = "data/pipeData", order = 0)]
    public class PipeData : ScriptableObject
    {
        public Vector2 mapSize;
        public PuzzleType puzzleType;
        public Vector3 boardLeftDown;
        public Vector3 pipeSize;
        public bool GameWin;
    }
}