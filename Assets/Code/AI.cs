using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Code
{
    public abstract class AI
    {
        protected bool _isWhiteTurn;
        protected bool _isWhitePlayer;
        protected int _boardSize;
        protected bool _endgame;
        protected GameManager.EvaluationFunction _evaluation;
        protected GameObject _go;

        public void Setup(int boardSize, GameObject go)
        {
            _boardSize = boardSize;
            _go = go;
        }

        public abstract Move Search(List<Pawn> state, bool isWhiteTurn, int depth,
            GameManager.EvaluationFunction evaluationFunction, bool endgame);

        protected bool End(List<Pawn> state)
        {
            return state.All(p => p.IsWhite) || state.All(p => !p.IsWhite);
        }

        protected int Value(List<Pawn> state)
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

            if (_endgame && state.All(p =>  p.IsQueen))
            {
                value = EndgameValue(state);
            }
            else
                value = _evaluation switch
                {
                    GameManager.EvaluationFunction.PawnValue => PawnValue(state, value),
                    GameManager.EvaluationFunction.PawnBoardValue => PawnBoardValue(state, value),
                    GameManager.EvaluationFunction.Extra => ComplexValue(state, value),
                    _ => value
                };

            return value;
        }
        
        private int PawnValue(IEnumerable<Pawn> state, int value)
        {
            foreach (var pawn in state)
            {
                if (_isWhitePlayer == pawn.IsWhite || !_isWhitePlayer == !pawn.IsWhite)
                {
                    value += pawn.IsQueen ? 2 : 1;
                }
                else
                {
                    value -= pawn.IsQueen ? 2 : 1;
                }
            }

            return value;
        }

        private int PawnBoardValue(IEnumerable<Pawn> state, int value)
        {
            foreach (var pawn in state)
            {
                var v = (pawn.IsWhite && pawn.position.y >= _boardSize / 2.0f) ||
                        (!pawn.IsWhite && pawn.position.y < _boardSize / 2.0f)
                    ? 7
                    : 5;
                if (_isWhitePlayer == pawn.IsWhite || !_isWhitePlayer == !pawn.IsWhite)
                {
                    value += pawn.IsQueen ? 10 : v;
                }
                else
                {
                    value -= pawn.IsQueen ? 10 : v;
                }
            }

            return value;
        }

        private int ComplexValue(IEnumerable<Pawn> state, int value)
        {
            foreach (var pawn in state)
            {
                var pval = pawn.IsQueen ? 50 : 25;
                pval += pawn.moves.Count * 5;
                pval += pawn.IsSafe ? 3 : 0;
                pval += pawn.DistanceToPromotion;
                
                if (_isWhitePlayer == pawn.IsWhite || !_isWhitePlayer == !pawn.IsWhite)
                {
                    value += pval;
                }
                else
                {
                    value -= pval;
                }
            }

            return value;
        }

        private int EndgameValue(List<Pawn> state)
        {
            var value = 100;
            var myPawns = state.Where(p => _isWhitePlayer == p.IsWhite).ToList();
            var enemyPawns = state.Where(p => _isWhitePlayer == !p.IsWhite).ToList();
            foreach (var myPawn in myPawns)
            {
                foreach (var enemyPawn in enemyPawns)
                {
                    if (myPawns.Count >= enemyPawns.Count)
                    {
                        value -= (int) Mathf.Abs(myPawn.position.x - enemyPawn.position.x);
                        value -= (int) Mathf.Abs(myPawn.position.y - enemyPawn.position.y);
                    }
                    else
                    {
                        value += (int) Mathf.Abs(myPawn.position.x - enemyPawn.position.x);
                        value += (int) Mathf.Abs(myPawn.position.y - enemyPawn.position.y);
                    }
                }
            }

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
                        var isValid = ValidMove(move, state, isWhiteTurn);
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
                }; //Instantiate(pawn, _go.transform);
                if (pawn == move.pawn)
                {
                    newPawn.position = new Vector2(move.endPos.x, move.endPos.y);

                    if (newPawn.IsWhite && (int) move.endPos.y == _boardSize - 1 ||
                        !newPawn.IsWhite && (int) move.endPos.y == 0)
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

        private bool ValidMove(Move move, List<Pawn> state, bool isWhiteTurn)
        {
            if (move.isAttack)
            {
                return true;
            }

            return !HasHit(state, isWhiteTurn);
        }

        private bool HasHit(List<Pawn> state, bool isWhiteTurn)
        {
            return state.Any(pawn => pawn.moves.Any(p => p.isAttack && pawn.IsWhite == isWhiteTurn));
        }
    }
}