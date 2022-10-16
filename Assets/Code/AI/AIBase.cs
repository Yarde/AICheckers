using System.Collections.Generic;
using System.Linq;
using Code.EvaluationFunction;
using Code.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Code.AI
{
    public abstract class AIBase
    {
        protected bool _isWhiteTurn;
        protected bool _isWhitePlayer;
        private readonly int _boardSize;
        private readonly bool _endgame;
        private Evaluator _evaluator;

        protected AIBase(int boardSize, PlayerData data)
        {
            _boardSize = boardSize;
            _endgame = data.useEndgameHeuristic;
            _evaluator = data.functionType switch
            {
                EvaluationFunctionType.PawnValue => new PawnValueEvaluator(),
                EvaluationFunctionType.PawnBoardValue => new PawnBoardValueEvaluator(_boardSize),
                EvaluationFunctionType.Complex => new ComplexEvaluator()
            };
        }

        public abstract UniTask<Move> Search(List<Pawn> pawns, bool isWhiteTurn, PlayerData whitePlayerData);

        protected bool IsGameFinished(List<Pawn> state)
        {
            // todo add draw and pat conditions here
            return state.All(p => p.IsWhite) || state.All(p => !p.IsWhite);
        }

        protected int GetStateValue(List<Pawn> state)
        {
            var value = 0;

            if (state.All(p => p.IsWhite == _isWhitePlayer))
            {
                value = 1000;
            }

            if (state.All(p => p.IsWhite == !_isWhitePlayer))
            {
                value = -1000;
            }

            // todo change that condition
            if (_endgame && state.All(p => p.IsQueen))
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
                if (isWhiteTurn == pawn.IsWhite)
                {
                    foreach (var move in pawn.moves)
                    {
                        var isValid = IsMoveValid(move, state, isWhiteTurn);
                        if (isValid)
                        {
                            moves.Add(move);
                        }
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

                var newPawn = new Pawn
                {
                    IsWhite = pawn.IsWhite,
                    IsQueen = pawn.IsQueen,
                    position = pawn.position,
                    boardSize = _boardSize
                };
                if (pawn == move.pawn)
                {
                    newPawn.position = new Vector2(move.endPos.x, move.endPos.y);

                    if (newPawn.IsWhite && (int)move.endPos.y == _boardSize - 1 ||
                        !newPawn.IsWhite && (int)move.endPos.y == 0)
                    {
                        newPawn.IsQueen = true;
                    }
                }

                newState.Add(newPawn);
            }

            foreach (var pawn in newState)
            {
                pawn.moves = pawn.PossibleMoves(newState);
            }

            return newState;
        }

        private bool IsMoveValid(Move move, List<Pawn> state, bool isWhiteTurn)
        {
            if (move.isAttack)
            {
                return true;
            }

            return !HasHit(state, isWhiteTurn);
        }

        private bool HasHit(IEnumerable<Pawn> state, bool isWhiteTurn)
        {
            return state.Any(pawn => pawn.moves.Any(p => p.isAttack && pawn.IsMine(isWhiteTurn)));
        }
    }
}