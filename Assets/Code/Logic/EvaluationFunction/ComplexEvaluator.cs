using System.Collections.Generic;

namespace Code.Logic.EvaluationFunction
{
    public class ComplexEvaluator : EvaluatorBase
    {
        public override int Evaluate(IReadOnlyList<Pawn> state, bool isWhitePlayer, int value)
        {
            foreach (var pawn in state)
            {
                var pawnValue = pawn.IsQueen ? 50 : 25;
                pawnValue += pawn.Moves.Count * 10;
                pawnValue += pawn.IsSafe ? 20 : 0;
                pawnValue -= pawn.DistanceToPromotion * 5;

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