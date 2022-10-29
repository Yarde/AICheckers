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

        public bool IsSafe => Position.y == 0 || Position.y == _boardSize - 1;
        public int DistanceToPromotion => IsQueen ? 0 : IsWhite ? _boardSize - 1 - Position.x : Position.x;

        public Pawn(int boardSize, Vector2Int position, bool isWhite, PawnView pawnView)
        {
            _boardSize = boardSize;
            Position = position;
            IsWhite = isWhite;
            _view = pawnView;
        }

        private Pawn(bool pawnIsWhite, bool pawnIsQueen, Vector2Int pawnPosition, int pawnBoardSize)
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

        private void FindInDirection(IReadOnlyCollection<Pawn> pawns, Vector2Int direction)
        {
            if (Position.x + direction.x > _boardSize - 1 || Position.x + direction.x < 0 ||
                Position.y + direction.y > _boardSize - 1 || Position.y + direction.y < 0)
            {
                return;
            }

            var pawn = pawns.FirstOrDefault(p =>
                p.Position.x == Position.x + direction.x &&
                p.Position.y == Position.y + direction.y);
            if (pawn == null)
            {
                Moves.Add(new Move(this, Position, new Vector2Int(Position.x + direction.x, Position.y + direction.y)));
            }
            else
            {
                FindAllAttacks(pawns, direction, Position);
            }
        }

        private void FindAllAttacks(IReadOnlyCollection<Pawn> pawns, Vector2Int direction, Vector2Int from,
            Move previousMove = null)
        {
            var to = new Vector2Int(from.x + direction.x * 2, from.y + direction.y * 2);
            if (IsOutsideBoard(to)) return;
            if (TargetCellNotEmpty(pawns, to)) return;

            var pawn = pawns.FirstOrDefault(p =>
                p.Position.x == from.x + direction.x && p.Position.y == from.y + direction.y);
            if (pawn == null || pawn.IsWhite == IsWhite) return;

            var move = GetMove(previousMove, to, pawn);

            // get multiple attacks 
            FindAllAttacks(pawns, direction, to, move);
            var newDirection = new Vector2Int(direction.x * -1, direction.y);
            FindAllAttacks(pawns, newDirection, to, move);
        }

        private Move GetMove(Move previousMove, Vector2Int to, Pawn pawn)
        {
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

            return move;
        }

        private bool IsOutsideBoard(Vector2Int to)
        {
            return to.x < 0 || to.x > _boardSize - 1 ||
                   to.y < 0 || to.y > _boardSize - 1;
        }

        private static bool TargetCellNotEmpty(IReadOnlyCollection<Pawn> pawns, Vector2Int to)
        {
            var target = pawns.FirstOrDefault(p =>
                p.Position.x == to.x &&
                p.Position.y == to.y);
            return target != null;
        }

        public void Move(Move move)
        {
            if (_view)
            {
                _view.transform.position = new Vector3(move.endPos.x, move.endPos.y, 0);
            }

            Position = new Vector2Int(move.endPos.x, move.endPos.y);

            if (IsWhite && move.endPos.y == _boardSize - 1 ||
                !IsWhite && move.endPos.y == 0)
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

        public Pawn Copy()
        {
            return new Pawn(IsWhite, _isQueen, Position, _boardSize);
        }
    }
}