using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Code.AI
{
    public class AlphaBetaPruning : AIBase
    {
        public AlphaBetaPruning(int boardSize, PlayerData data) : base(boardSize, data)
        {
        }

        public override async UniTask<Move> Search(List<Pawn> state, bool isWhiteTurn, PlayerData data)
        {
            _isWhitePlayer = isWhiteTurn;
            _isWhiteTurn = isWhiteTurn;

            var actions = Actions(state, isWhiteTurn);
            if (actions.Count == 1)
            {
                return actions[0];
            }

            var (value, move) = MaxValue(state, _isWhiteTurn, data.searchDepth, float.MinValue, float.MaxValue);
            // var playerName = isWhiteTurn ? "white" : "black";
            //Debug.Log($"best move value for {playerName} is {value}");
            return move;
        }

        private (int, Move) MaxValue(List<Pawn> state, bool isWhiteTurn, int depth, float alpha, float beta)
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
                var (value2, _) = MinValue(Result(state, action), !isWhiteTurn, depth - 1, alpha, beta);
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

        private (int, Move) MinValue(List<Pawn> state, bool isWhiteTurn, int depth, float alpha, float beta)
        {
            if (depth == 0 || IsGameFinished(state))
            {
                return (GetStateValue(state), null);
            }

            var value = int.MaxValue;
            Move move = null;
            var actions = Actions(state, isWhiteTurn);
            foreach (var action in actions)
            {
                var (value2, _) = MaxValue(Result(state, action), !isWhiteTurn, depth - 1, alpha, beta);
                if (value2 < value || (value2 == value && Random.Range(0.0f, 1.0f) > 0.5f))
                {
                    value = value2;
                    move = action;
                    beta = Mathf.Min(beta, value);
                }

                if (value <= alpha)
                {
                    return (value, move);
                }
            }

            return (value, move);
        }
    }
}