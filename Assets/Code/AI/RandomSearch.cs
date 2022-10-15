using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Random = UnityEngine.Random;

namespace Code.AI
{
    public class RandomSearch : AIBase
    {
        public override async UniTask<Move> Search(List<Pawn> state, bool isWhiteTurn, PlayerData data)
        {
            var moves = Actions(state, isWhiteTurn);
            return moves.Count == 0 ? null : moves[Random.Range(0, moves.Count - 1)];
        }
    }
}