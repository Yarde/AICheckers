using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Code.AI
{
    public class RandomSearchPlayer : PlayerBase
    {
        public RandomSearchPlayer(int boardSize, PlayerData data) : base(boardSize, data)
        {
        }

        public override async UniTask<Move> Search(List<Pawn> state, bool isWhiteTurn, PlayerData data)
        {
            var moves = Actions(state, isWhiteTurn);
            return moves.Count == 0 ? null : moves[Random.Range(0, moves.Count - 1)];
        }
    }
}