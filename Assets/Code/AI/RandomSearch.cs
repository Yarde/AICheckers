using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace Code.AI
{
    public class RandomSearch : AIBase
    {
        public override Move Search(List<Pawn> state, bool isWhiteTurn, PlayerData data)
        {
            var moves = Actions(state, isWhiteTurn);
            return moves[Random.Range(0, moves.Count - 1)];
        }
    }
}