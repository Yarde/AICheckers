using System;
using System.Collections.Generic;
using System.Linq;
using Code.Logic.EvaluationFunction;
using Code.Utils;
using Cysharp.Threading.Tasks;

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

        public abstract UniTask<Move> Search(List<Pawn> pawns, bool isWhiteTurn, PlayerData whitePlayerData);

        protected static bool IsGameFinished(List<Pawn> state)
        {
            var isWin = state.All(p => p.IsWhite) || state.All(p => !p.IsWhite);
            var isPat = state.All(p => p.IsWhite && p.Moves.Count == 0)
                        || state.All(p => !p.IsWhite && p.Moves.Count == 0);
            return isWin || isPat;
        }

        protected int GetStateValue(List<Pawn> state)
        {
            var value = 0;

            if (state.All(p => p.IsWhite == _isWhitePlayer))
            {
                value = 1000;
            }

            if (state.All(p => p.IsWhite != _isWhitePlayer))
            {
                value = -1000;
            }

            if (_endgame && state.Where(p => p.IsWhite == _isWhitePlayer).All(p => p.IsQueen))
            {
                _evaluator = new EndgameEvaluator();
            }

            value = _evaluator.Evaluate(state, _isWhitePlayer, value);
            return value;
        }

        protected List<Move> Actions(List<Pawn> state, bool isWhiteTurn)
        {
            var moves = new List<Move>();
            foreach (var pawn in state)
            {
                if (isWhiteTurn != pawn.IsWhite) continue;

                foreach (var move in pawn.Moves)
                {
                    var isValid = IsMoveValid(move, state, isWhiteTurn);
                    if (isValid)
                    {
                        moves.Add(move);
                    }
                }
            }

            return moves;
        }

        protected List<Pawn> Result(List<Pawn> state, Move move)
        {
            var newState = new List<Pawn>();
            foreach (var pawn in state)
            {
                if (move.hits.Contains(pawn))
                {
                    continue;
                }

                var newPawn = pawn.Copy();
                if (pawn == move.pawn)
                {
                    newPawn.Move(move);
                }

                newState.Add(newPawn);
            }

            newState.GenerateMoves();

            return newState;
        }

        private bool IsMoveValid(Move move, IEnumerable<Pawn> state, bool isWhiteTurn)
        {
            if (move.isAttack)
            {
                return true;
            }

            return !HasHit(state, isWhiteTurn);
        }

        private static bool HasHit(IEnumerable<Pawn> state, bool isWhiteTurn)
        {
            return state.Any(pawn => pawn.Moves.Any(p => p.isAttack && pawn.IsMine(isWhiteTurn)));
        }
    }
}