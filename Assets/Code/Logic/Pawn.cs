using System.Collections.Generic;
using System.Linq;
using Code.Model;
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

        public void GeneratePossibleMoves(IReadOnlyList<Pawn> pawns)
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
            var hasHit = Moves.Any(x => x.IsAttack);

            if (hasHit)
            {
                Moves.RemoveAll(x => !x.IsAttack);
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
                FindAttacks(pawns, direction, Position);
            }
        }

        private void FindAttacks(IReadOnlyCollection<Pawn> pawns, Vector2Int direction, Vector2Int from)
        {
            if (CheckAttack(pawns, direction, from, out var to, out var pawn)) return;

            var move = new Move(this, Position, to);
            Moves.Add(move);
            move.hits.Add(pawn);

            FindConsecutiveAttacks(pawns, new Vector2Int(direction.x, direction.y), to, move);
            FindConsecutiveAttacks(pawns, new Vector2Int(direction.x, direction.y * -1), to, move);
        }

        private bool CheckAttack(IReadOnlyCollection<Pawn> pawns, Vector2Int direction, Vector2Int from,
            out Vector2Int to,
            out Pawn pawn)
        {
            pawn = null;
            to = new Vector2Int(from.x + direction.x * 2, from.y + direction.y * 2);
            if (IsOutsideBoard(to)) return true;
            if (TargetCellNotEmpty(pawns, to)) return true;

            pawn = pawns.FirstOrDefault(p =>
                p.Position.x == from.x + direction.x && p.Position.y == from.y + direction.y);
            if (pawn == null || pawn.IsWhite == IsWhite) return true;
            return false;
        }

        private void FindConsecutiveAttacks(IReadOnlyCollection<Pawn> pawns, Vector2Int direction, Vector2Int from,
            Move previousMove)
        {
            if (CheckAttack(pawns, direction, from, out var to, out var pawn)) return;

            previousMove.hits.Add(pawn);
            previousMove.endPos = to;

            // get multiple attacks 
            FindConsecutiveAttacks(pawns, new Vector2Int(direction.x, direction.y), to, previousMove);
            FindConsecutiveAttacks(pawns, new Vector2Int(direction.x, direction.y * -1), to, previousMove);
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