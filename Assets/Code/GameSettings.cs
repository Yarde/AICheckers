using UnityEngine;

namespace Code
{
    [CreateAssetMenu(fileName = "Game", menuName = "Settings")]
    public class GameSettings : ScriptableObject
    {
        public int boardSize;
        public int pawnRows;
        
        public Algorithm _aiWhiteType;
        public Algorithm _aiBlackType;

        public EvaluationFunction _evWhite;
        public EvaluationFunction _evBlack;

        public int _aiWhiteDepth;
        public int _aiBlackDepth;

        public bool _aiWhiteEndgame;
        public bool _aiBlackEndgame;

        public bool IsWhiteAI = true;
        public bool IsBlackAI = true;
    }
}