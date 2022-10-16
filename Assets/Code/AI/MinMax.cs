using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Code.AI
{
    public class MinMax : AIBase
    {
        public MinMax(int boardSize, PlayerData data) : base(boardSize, data)
        {
        }

        public override async UniTask<Move> Search(List<Pawn> state, bool isWhiteTurn, PlayerData data)
        {
            var playerName = isWhiteTurn ? "white" : "black";
            _isWhitePlayer = isWhiteTurn;
            _isWhiteTurn = isWhiteTurn;
            var (value, move) = MaxValue(state, _isWhiteTurn, data.searchDepth);
            Debug.Log($"best move value for {playerName} is {value}");
            return move;
        }

        private (int, Move) MaxValue(List<Pawn> state, bool isWhiteTurn, int depth)
        {
            if (depth == 0 || IsGameFinished(state))
            {
                // Debug.Log($"Ending branch depth: {depth}");
                return (GetStateValue(state), null);
            }

            var value = int.MinValue;
            Move move = null;
            var actions = Actions(state, isWhiteTurn);
            // var playerName = isWhiteTurn ? "white" : "black";
            // Debug.Log($"possible moves {actions.Count} on depth {depth} during {playerName} turn");
            foreach (var action in actions)
            {
                var (value2, _) = MinValue(Result(state, action), !isWhiteTurn, depth - 1);
                if (value2 > value || (value2 == value && Random.Range(0.0f, 1.0f) > 0.5f))
                {
                    value = value2;
                    move = action;
                }
            }

            return (value, move);
        }

        private (int, Move) MinValue(List<Pawn> state, bool isWhiteTurn, int depth)
        {
            if (depth == 0 || IsGameFinished(state))
            {
                return (GetStateValue(state), null);
            }

            var value = int.MaxValue;
            Move move = null;
            var actions = Actions(state, isWhiteTurn);
            // var playerName = isWhiteTurn ? "white" : "black";
            // Debug.Log($"possible moves {actions.Count} on depth {depth} during {playerName} turn");
            foreach (var action in actions)
            {
                var (value2, _) = MaxValue(Result(state, action), !isWhiteTurn, depth - 1);
                if (value2 < value || (value2 == value && Random.Range(0.0f, 1.0f) > 0.5f))
                {
                    value = value2;
                    move = action;
                }
            }

            return (value, move);
        }
    }
}