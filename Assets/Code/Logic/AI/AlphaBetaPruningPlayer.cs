using System.Collections.Generic;
using Code.Model;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Code.Logic.AI
{
    public class AlphaBetaPruningPlayer : PlayerBase
    {
        public AlphaBetaPruningPlayer(int boardSize, PlayerData data) : base(boardSize, data)
        {
        }

        public override async UniTask<Move> Search(IReadOnlyList<Pawn> state, bool isWhiteTurn, PlayerData data)
        {
            _isWhitePlayer = isWhiteTurn;
            _isWhiteTurn = isWhiteTurn;

            var actions = Actions(state, isWhiteTurn);
            if (actions.Count == 1)
            {
                return actions[0];
            }

            var (_, move) = MaxValue(state, _isWhiteTurn, data.searchDepth, float.MinValue, float.MaxValue);
            await UniTask.CompletedTask;
            return move;
        }

        private (int, Move) MaxValue(IReadOnlyList<Pawn> state, bool isWhiteTurn, int depth, float alpha, float beta)
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
                var value2 = MinValue(ResultState(state, action), !isWhiteTurn, depth - 1, alpha, beta);
                if (value2 > value || (value2 == value && Random.Range(0.0f, 1.0f) > 0.5f))
                {
                    value = value2;
                    move = action;
                    alpha = Mathf.Max(alpha, value);
                }

                if (value >= beta)
                {
                    return (value, move);
                }
            }

            return (value, move);
        }

        private int MinValue(IReadOnlyList<Pawn> state, bool isWhiteTurn, int depth, float alpha, float beta)
        {
            if (depth == 0 || IsGameFinished(state))
            {
                return GetStateValue(state);
            }

            var value = int.MaxValue;
            var actions = Actions(state, isWhiteTurn);
            foreach (var action in actions)
            {
                var (value2, _) = MaxValue(ResultState(state, action), !isWhiteTurn, depth - 1, alpha, beta);
                if (value2 < value || (value2 == value && Random.Range(0.0f, 1.0f) > 0.5f))
                {
                    value = value2;
                    beta = Mathf.Min(beta, value);
                }

                if (value <= alpha)
                {
                    return value;
                }
            }

            return value;
        }
    }
}