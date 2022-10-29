using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Code.Logic.EvaluationFunction
{
    public class EndgameEvaluator : EvaluatorBase
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
                        value -= (int) Mathf.Abs(myPawn.Position.x - enemyPawn.Position.x);
                        value -= (int) Mathf.Abs(myPawn.Position.y - enemyPawn.Position.y);
                    }
                    else
                    {
                        value += (int) Mathf.Abs(myPawn.Position.x - enemyPawn.Position.x);
                        value += (int) Mathf.Abs(myPawn.Position.y - enemyPawn.Position.y);
                    }
                }
            }

            return value;
        }
    }
}