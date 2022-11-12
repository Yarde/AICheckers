using System.Collections.Generic;
using System.Linq;
using Code.Model;
using Code.Utils;
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

        public bool IsSafe => Position.y == 0 || Position.x == 0 || Position.y == _boardSize - 1 || Position.x == _boardSize - 1;
        public int DistanceToPromotion => IsQueen ? 0 : IsWhite ? _boardSize - 1 - Position.y : Position.y;

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

        private void FindAttacks(IReadOnlyCollection<Pawn> pawns, Vector2Int direction, Vector2Int from, Move previousMove = null)
        {
            var to = new Vector2Int(from.x + direction.x * 2, from.y + direction.y * 2);
            if (!HasAttack(pawns, direction, from, to, out var pawn)) return;

            if (previousMove != null && previousMove.HasHit(pawn))
                return;
            
            var move = GetNewAttackMove(previousMove, to, pawn);

            FindAttacks(pawns, new Vector2Int(direction.x, direction.y), to, move);
            FindAttacks(pawns, new Vector2Int(direction.x * -1, direction.y), to, move);

            if (pawn.IsQueen)
            {
                FindAttacks(pawns, new Vector2Int(direction.x, direction.y * -1), to, move);
                FindAttacks(pawns, new Vector2Int(direction.x * -1, direction.y * -1), to, move);
            }
        }

        private Move GetNewAttackMove(Move previousMove, Vector2Int to, Pawn pawn)
        {
            Move move;
            if (previousMove != null)
            {
                move = new Move(this, previousMove.Pawn.Position, to);
                foreach (var hit in previousMove.Hits)
                {
                    move.AddHit(hit);
                }
            }
            else
            {
                move = new Move(this, Position, to);
            }

            Moves.Add(move);
            move.AddHit(pawn);
            return move;
        }

        private bool HasAttack(IReadOnlyCollection<Pawn> pawns, Vector2Int direction, Vector2Int from, Vector2Int to,
            out Pawn pawn)
        {
            pawn = null;
            if (IsOutsideBoard(to)) return false;
            if (TargetCellNotEmpty(pawns, to)) return false;

            pawn = pawns.FirstOrDefault(p =>
                p.Position.x == from.x + direction.x && p.Position.y == from.y + direction.y);
            return pawn != null && !pawn.IsMine(IsWhite);
        }

        private bool IsOutsideBoard(Vector2Int to)
        {
            return to.x < 0 || to.x > _boardSize - 1 ||
                   to.y < 0 || to.y > _boardSize - 1;
        }

        private static bool TargetCellNotEmpty(IEnumerable<Pawn> pawns, Vector2Int to)
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
                _view.transform.position = new Vector3(move.EndPos.x, move.EndPos.y, 0);
            }

            Position = new Vector2Int(move.EndPos.x, move.EndPos.y);

            if (IsWhite && move.EndPos.y == _boardSize - 1 || !IsWhite && move.EndPos.y == 0)
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