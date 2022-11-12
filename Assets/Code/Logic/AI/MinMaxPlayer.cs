using System.Collections.Generic;
using Code.Model;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Code.Logic.AI
{
    public class MinMaxPlayer : PlayerBase
    {
        public MinMaxPlayer(int boardSize, PlayerData data) : base(boardSize, data)
        {
        }

        public override async UniTask<Move> Search(IReadOnlyList<Pawn> state, bool isWhiteTurn, PlayerData data)
        {
            var playerName = isWhiteTurn ? "white" : "black";
            _isWhitePlayer = isWhiteTurn;
            _isWhiteTurn = isWhiteTurn;
            var (value, move) = MaxValue(state, _isWhiteTurn, data.searchDepth);
            Debug.Log($"best move value for {playerName} is {value}");
            await UniTask.CompletedTask;
            return move;
        }

        private (int, Move) MaxValue(IReadOnlyList<Pawn> state, bool isWhiteTurn, int depth)
        {
            if (depth == 0 || IsGameFinished(state))
            {
                return (GetStateValue(state), null);
            }

            var value = int.MinValue;
            Move move = null;
            var actions = Actions(state, isWhiteTurn);
            foreach (var action in actions)
            {
                var value2 = MinValue(Result(state, action), !isWhiteTurn, depth - 1);
                if (value2 > value || (value2 == value && Random.Range(0.0f, 1.0f) > 0.5f))
                {
                    value = value2;
                    move = action;
                }
            }

            return (value, move);
        }

        private int MinValue(List<Pawn> state, bool isWhiteTurn, int depth)
        {
            if (depth == 0 || IsGameFinished(state))
            {
                return GetStateValue(state);
            }

            var value = int.MaxValue;
            var actions = Actions(state, isWhiteTurn);
            foreach (var action in actions)
            {
                var (value2, _) = MaxValue(Result(state, action), !isWhiteTurn, depth - 1);
                if (value2 < value || (value2 == value && Random.Range(0.0f, 1.0f) > 0.5f))
                {
                    value = value2;
                }
            }

            return value;
        }
    }
}