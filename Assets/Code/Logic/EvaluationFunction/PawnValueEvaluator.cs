using System.Collections.Generic;
using Code.Utils;

namespace Code.Logic.EvaluationFunction
{
    public class PawnValueEvaluator : EvaluatorBase
    {
        public override int Evaluate(IReadOnlyList<Pawn> state, bool isWhitePlayer, int value)
        {
            foreach (var pawn in state)
            {
                if (pawn.IsMine(isWhitePlayer))
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