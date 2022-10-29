using System.Collections.Generic;

namespace Code.EvaluationFunction
{
    public class ComplexEvaluator : EvaluatorBase
    {
        public override int Evaluate(IEnumerable<Pawn> state, bool isWhitePlayer, int value)
        {
            foreach (var pawn in state)
            {
                var pawnValue = pawn.IsQueen ? 50 : 25;
                pawnValue += pawn.moves.Count * 5;
                pawnValue += pawn.IsSafe ? 3 : 0;
                pawnValue += pawn.DistanceToPromotion;
                
                if (isWhitePlayer == pawn.IsWhite || !isWhitePlayer == !pawn.IsWhite)
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