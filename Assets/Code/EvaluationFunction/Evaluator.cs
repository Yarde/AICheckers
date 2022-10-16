using System.Collections.Generic;

namespace Code.EvaluationFunction
{
    public abstract class Evaluator
    {
        public abstract int Evaluate(IEnumerable<Pawn> state, bool isWhitePlayer, int value);
    }
}