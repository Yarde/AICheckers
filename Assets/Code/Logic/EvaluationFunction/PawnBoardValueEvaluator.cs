using System.Collections.Generic;
using Code.Utils;

namespace Code.Logic.EvaluationFunction
{
    public class PawnBoardValueEvaluator : EvaluatorBase
    {
        private readonly int _boardSize;
        
        public PawnBoardValueEvaluator(int boardSize)
        {
            _boardSize = boardSize;
        }
        
        public override int Evaluate(IReadOnlyList<Pawn> state, bool isWhitePlayer, int value)
        {
            foreach (var pawn in state)
            {
                var v = (pawn.IsWhite && pawn.Position.y >= _boardSize / 2.0f) ||
                        (!pawn.IsWhite && pawn.Position.y < _boardSize / 2.0f)
                    ? 7
                    : 5;
                if (pawn.IsMine(isWhitePlayer))
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