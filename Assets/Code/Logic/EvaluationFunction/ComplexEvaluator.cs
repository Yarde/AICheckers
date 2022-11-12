using System.Collections.Generic;
using Code.Utils;

namespace Code.Logic.EvaluationFunction
{
    public class ComplexEvaluator : EvaluatorBase
    {
        public override int Evaluate(IReadOnlyList<Pawn> state, bool isWhitePlayer, int value)
        {
            foreach (var pawn in state)
            {
                var pawnValue = pawn.IsQueen ? 40 : 20;
                pawnValue += pawn.Moves.Count * 1;
                pawnValue += pawn.IsSafe ? 5 : 0;
                pawnValue -= pawn.DistanceToPromotion * 1;

                if (pawn.IsMine(isWhitePlayer))
                {
                    value += pawnValue;
                }
                else
                {
                    value -= pawnValue;
                }
            }

            return value;
        }
    }
}