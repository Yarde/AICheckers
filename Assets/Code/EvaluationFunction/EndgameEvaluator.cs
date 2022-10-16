using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Code.EvaluationFunction
{
    public class EndgameEvaluator : Evaluator
    {
        public override int Evaluate(IEnumerable<Pawn> state, bool isWhitePlayer, int value)
        {
            value += 100;
            var myPawns = state.Where(p => isWhitePlayer == p.IsWhite).ToList();
            var enemyPawns = state.Where(p => isWhitePlayer == !p.IsWhite).ToList();
            foreach (var myPawn in myPawns)
            {
                foreach (var enemyPawn in enemyPawns)
                {
                    if (myPawns.Count >= enemyPawns.Count)
                    {
                        value -= (int) Mathf.Abs(myPawn.position.x - enemyPawn.position.x);
                        value -= (int) Mathf.Abs(myPawn.position.y - enemyPawn.position.y);
                    }
                    else
                    {
                        value += (int) Mathf.Abs(myPawn.position.x - enemyPawn.position.x);
                        value += (int) Mathf.Abs(myPawn.position.y - enemyPawn.position.y);
                    }
                }
            }

            return value;
        }
    }
}