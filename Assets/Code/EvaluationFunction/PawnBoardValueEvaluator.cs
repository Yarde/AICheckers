using System.Collections.Generic;

namespace Code.EvaluationFunction
{
    public class PawnBoardValueEvaluator : Evaluator
    {
        private int _boardSize;
        
        public PawnBoardValueEvaluator(int boardSize)
        {
            _boardSize = boardSize;
        }
        
        public override int Evaluate(IEnumerable<Pawn> state, bool isWhitePlayer, int value)
        {
            foreach (var pawn in state)
            {
                var v = (pawn.IsWhite && pawn.position.y >= _boardSize / 2.0f) ||
                        (!pawn.IsWhite && pawn.position.y < _boardSize / 2.0f)
                    ? 7
                    : 5;
                if (isWhitePlayer == pawn.IsWhite || !isWhitePlayer == !pawn.IsWhite)
                {
                    value += pawn.IsQueen ? 10 : v;
                }
                else
                {
                    value -= pawn.IsQueen ? 10 : v;
                }
            }

            return value;
        }
    }
}