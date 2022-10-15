using System;

namespace Code
{
    [Serializable]
    public struct PlayerData
    {
        public AlgorithmType algorithmType;
        public EvaluationFunctionType functionType;
        public int searchDepth;
        public bool useEndgameHeuristic;
        public int randomStartMoves;
    }
}