using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace Code
{
    public class RandomSearch : AI
    {
        public override Move Search(List<Pawn> state, bool isWhiteTurn, int depth,
            GameManager.EvaluationFunction evaluationFunction, bool endgame)
        {
            var moves = Actions(state, isWhiteTurn);
            return moves[Random.Range(0, moves.Count - 1)];
        }
    }
}