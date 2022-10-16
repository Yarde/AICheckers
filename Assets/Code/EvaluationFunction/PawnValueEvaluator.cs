using System.Collections.Generic;

namespace Code.EvaluationFunction
{
    public class PawnValueEvaluator : Evaluator
    {
        public override int Evaluate(IEnumerable<Pawn> state, bool isWhitePlayer, int value)
        {
            foreach (var pawn in state)
            {
                if (isWhitePlayer == pawn.IsWhite || !isWhitePlayer == !pawn.IsWhite)
                {
                    value += pawn.IsQueen ? 2 : 1;
                }
                else
                {
                    value -= pawn.IsQueen ? 2 : 1;
                }
            }

            return value;
        }
    }
}