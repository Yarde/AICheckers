using System;
using System.Collections.Generic;
using System.Linq;
using Code.Logic.EvaluationFunction;
using Code.Model;
using Code.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Code.Logic.AI
{
    public abstract class PlayerBase
    {
        protected bool _isWhiteTurn;
        protected bool _isWhitePlayer;
        private readonly bool _endgame;
        private EvaluatorBase _evaluator;

        protected PlayerBase(int boardSize, PlayerData data)
        {
            _endgame = data.useEndgameHeuristic;
            _evaluator = data.functionType switch
            {
                EvaluationFunctionType.PawnValue => new PawnValueEvaluator(),
                EvaluationFunctionType.PawnBoardValue => new PawnBoardValueEvaluator(boardSize),
                EvaluationFunctionType.Complex => new ComplexEvaluator(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public abstract UniTask<Move> Search(IReadOnlyList<Pawn> pawns, bool isWhiteTurn, PlayerData whitePlayerData);

        protected static bool IsGameFinished(IReadOnlyList<Pawn> state)
        {
            var isWin = state.All(p => p.IsWhite) || state.All(p => !p.IsWhite);
            var isPat = state.All(p => p.IsWhite && p.Moves.Count == 0)
                        || state.All(p => !p.IsWhite && p.Moves.Count == 0);
            return isWin || isPat;
        }

        protected int GetStateValue(IReadOnlyList<Pawn> state)
        {
            var value = 0;

            if (state.All(p => p.IsMine(_isWhitePlayer)))
            {
                return int.MaxValue;
            }

            if (state.All(p => !p.IsMine(_isWhitePlayer)))
            {
                return int.MinValue;
            }

            if (_endgame && _evaluator.GetType() != typeof(EndgameEvaluator) && state.Where(p => p.IsMine(_isWhitePlayer)).All(p => p.IsQueen))
            {
                _evaluator = new EndgameEvaluator();
            }

            value = _evaluator.Evaluate(state, _isWhitePlayer, value);
            return value;
        }

        protected static IReadOnlyList<Move> Actions(IReadOnlyList<Pawn> state, bool isWhiteTurn)
        {
            var moves = new List<Move>();
            foreach (var pawn in state)
            {
                if (isWhiteTurn != pawn.IsWhite) continue;
                foreach (var move in pawn.Moves)
                {
                    if (IsMoveValid(move, state, isWhiteTurn))
                    {
                        moves.Add(move);
                    }
                }
            }

            return moves;
        }

        protected static List<Pawn> ResultState(IEnumerable<Pawn> state, Move move)
        {
            var newState = new List<Pawn>();
            foreach (var pawn in state)
            {
                if (move.HasHit(pawn))
                {
                    continue;
                }

                var newPawn = pawn.Copy();
                if (pawn == move.Pawn)
                {
                    newPawn.Move(move);
                }

                newState.Add(newPawn);
            }

            newState.GenerateMoves();

            return newState;
        }

        private static bool IsMoveValid(Move move, IEnumerable<Pawn> state, bool isWhiteTurn)
        {
            if (move.IsAttack)
            {
                return true;
            }

            return !HasHit(state, isWhiteTurn);
        }

        private static bool HasHit(IEnumerable<Pawn> state, bool isWhiteTurn)
        {
            foreach (var pawn in state)
            {
                foreach (var p in pawn.Moves)
                {
                    if (p.IsAttack && pawn.IsMine(isWhiteTurn)) return true;
                }
            }

            return false;
        }

        public void LogState(List<Pawn> state, Move move)
        {
            Debug.Log($"Current state value for player {_isWhitePlayer}: {GetStateValue(state)}, after move: {GetStateValue(ResultState(state, move))}");
        }
    }
}