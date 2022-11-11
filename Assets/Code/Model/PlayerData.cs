using System;
using Code.Logic.AI;
using Code.Logic.EvaluationFunction;

namespace Code.Model
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