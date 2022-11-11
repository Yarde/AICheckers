using System.Collections.Generic;
using Code.Model;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Code.Logic.AI
{
    public class RandomSearchPlayer : PlayerBase
    {
        public RandomSearchPlayer(int boardSize, PlayerData data) : base(boardSize, data)
        {
        }

        public override async UniTask<Move> Search(IReadOnlyList<Pawn> state, bool isWhiteTurn, PlayerData data)
        {
            var moves = Actions(state, isWhiteTurn);
            await UniTask.CompletedTask;
            return moves.Count == 0 ? null : moves[Random.Range(0, moves.Count - 1)];
        }
    }
}