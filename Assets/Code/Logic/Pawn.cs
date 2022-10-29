using System.Collections.Generic;
using System.Linq;
using Code.View;
using UnityEngine;

namespace Code.Logic
{
    public class Pawn
    {
        private readonly PawnView _view;
        private readonly int _boardSize;
        public Vector2Int Position { get; private set; }
        public List<Move> Moves { get; } = new();
        public bool IsWhite { get; }
        private bool _isQueen;

        public bool IsQueen
        {
            get => _isQueen;
            private set
            {
                _isQueen = value;
                if (_view)
                {
                    _view.queenImage.gameObject.SetActive(_isQueen);
                }
            }
        }

        public bool IsSafe => Position.x == 0 || (int)Position.x == _boardSize - 1;
        public int DistanceToPromotion => IsQueen ? 0 : IsWhite ? _boardSize - 1 - (int)Position.y : (int)Position.y;

        public Pawn(int boardSize, Vector2Int vector2, bool b, PawnView pawnGo)
        {
            _boardSize = boardSize;
            Position = vector2;
            IsWhite = b;
            _view = pawnGo;
        }

        public Pawn(bool pawnIsWhite, bool pawnIsQueen, Vector2Int pawnPosition, int pawnBoardSize)
        {
            IsWhite = pawnIsWhite;
            IsQueen = pawnIsQueen;
            Position = pawnPosition;
            _boardSize = pawnBoardSize;
        }

        public void GeneratePossibleMoves(List<Pawn> pawns)
        {
            Moves.Clear();

            if (IsWhite || _isQueen)
            {
                FindInDirection(pawns, new Vector2Int(1, 1));
                FindInDirection(pawns, new Vector2Int(-1, 1));
            }

            if (!IsWhite || _isQueen)
            {
                FindInDirection(pawns, new Vector2Int(1, -1));
                FindInDirection(pawns, new Vector2Int(-1, -1));
            }

            ValidMovesOnly();
        }

        private void ValidMovesOnly()
        {
            var hasHit = Moves.Any(x => x.isAttack);

            if (hasHit)
            {
                Moves.RemoveAll(x => !x.isAttack);
            }
        }

        private void FindInDirection(List<Pawn> pawns, Vector2Int direction)
        {
            if (Position.x + direction.x > _boardSize - 1 || Position.x + direction.x < 0 ||
                Position.y + direction.y > _boardSize - 1 || Position.y + direction.y < 0)
            {
                return;
            }

            var pawn = pawns.FirstOrDefault(p =>
                (int)p.Position.x == (int)(Position.x + direction.x) &&
                (int)p.Position.y == (int)(Position.y + direction.y));
            if (pawn == null)
            {
                Moves.Add(new Move(this, Position, new Vector2Int(Position.x + direction.x, Position.y + direction.y)));
            }
            else
            {
                FindAllAttacks(pawns, direction, Position);
            }
        }

        private void FindAllAttacks(List<Pawn> pawns, Vector2Int direction, Vector2Int from, Move previousMove = null)
        {
            if (from.x + direction.x < 1 || from.x + direction.x > _boardSize - 2 ||
                from.y + direction.y < 1 || from.y + direction.y > _boardSize - 2)
            {
                return;
            }

            var pawn = pawns.FirstOrDefault(p =>
                (int)p.Position.x == from.x + direction.x && (int)p.Position.y == from.y + direction.y);
            var target = pawns.FirstOrDefault(p =>
                (int)p.Position.x == from.x + direction.x * 2 &&
                (int)p.Position.y == from.y + direction.y * 2);
            if (pawn != null && pawn.IsWhite != IsWhite && target == null)
            {
                var to = new Vector2Int(from.x + direction.x * 2, from.y + direction.y * 2);
                Move move;
                if (previousMove == null)
                {
                    move = new Move(this, Position, to);
                    Moves.Add(move);
                    move.hits.Add(pawn);
                }
                else
                {
                    previousMove.hits.Add(pawn);
                    previousMove.endPos = to;
                    move = previousMove;
                }

                FindAllAttacks(pawns, direction, to, move);

                var newDirection = new Vector2Int(direction.x * -1, direction.y);
                FindAllAttacks(pawns, newDirection, to, move);
            }
        }

        public void Move(Move move)
        {
            if (_view)
            {
                _view.transform.position = new Vector3(move.endPos.x, move.endPos.y, 0);
            }

            Position = new Vector2Int(move.endPos.x, move.endPos.y);

            if (IsWhite && (int)move.endPos.y == _boardSize - 1 ||
                !IsWhite && (int)move.endPos.y == 0)
            {
                IsQueen = true;
            }
        }

        public void Destroy()
        {
            if (_view)
            {
                Object.Destroy(_view.gameObject);
            }
        }
    }
}