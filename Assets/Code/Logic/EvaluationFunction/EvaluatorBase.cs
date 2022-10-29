using System.Collections.Generic;

namespace Code.EvaluationFunction
{
    public abstract class EvaluatorBase
    {
        public abstract int Evaluate(IEnumerable<Pawn> state, bool isWhitePlayer, int value);
    }
}