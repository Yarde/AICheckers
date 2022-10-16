using System.Collections.Generic;
using System.Linq;
using Code.View;
using UnityEngine;
using UnityEngine.UI;

namespace Code
{
    public class Pawn
    {
        public PawnView view;
        public int boardSize;
        public Vector2 position;
        public List<Move> moves = new List<Move>();
        public bool IsWhite { get; set; }
        private bool isQueen = false;
        public bool IsQueen
        {
            get => isQueen;
            set
            {
                isQueen = value;
                if (view)
                {
                    view.queenImage.gameObject.SetActive(isQueen); 
                }
            }
        }
        
        public bool IsSafe => position.x == 0 || (int) position.x == boardSize - 1;
        public int DistanceToPromotion => IsQueen ? 0 : IsWhite ? boardSize - 1 - (int) position.y : (int) position.y;

        public List<Move> PossibleMoves(List<Pawn> pawns)
        {
            moves = new List<Move>();

            if (IsWhite || isQueen)
            {
                FindInDirection(pawns, new Vector2(1, 1));
                FindInDirection(pawns, new Vector2(-1, 1));
            }
            
            if (!IsWhite || isQueen)
            {
                FindInDirection(pawns, new Vector2(1, -1));
                FindInDirection(pawns, new Vector2(-1, -1));
            }
            
            ValidMovesOnly();

            return moves;
        }
        
        private void ValidMovesOnly()
        {
            var hasHit = moves.Any(x => x.isAttack);

            if (hasHit)
            {
                moves.RemoveAll(x => !x.isAttack);
            }
        }

        private void FindInDirection(List<Pawn> pawns, Vector2 direction)
        {
            if (position.x + direction.x > boardSize - 1 || position.x + direction.x < 0 ||
                position.y + direction.y > boardSize - 1 || position.y + direction.y < 0)
            {
                return;
            }
            var pawn = pawns.FirstOrDefault(p => (int) p.position.x == (int) (position.x + direction.x) && (int) p.position.y == (int)  (position.y + direction.y));
            if (pawn == null)
            {
                moves.Add(new Move(this, position, new Vector2(position.x + direction.x, position.y + direction.y)));
            }
            else
            {
                FindAllAttacks(pawns, direction, position);
            }
        }

        private void FindAllAttacks(List<Pawn> pawns, Vector2 direction, Vector2 from, Move previousMove = null)
        {
            if ((int) from.x + (int) direction.x < 1 || (int) from.x + (int) direction.x > boardSize - 2 ||
                (int) from.y + (int) direction.y < 1 || (int) from.y + (int) direction.y > boardSize - 2)
            {
                return;
            }
            var pawn = pawns.FirstOrDefault(p => (int) p.position.x == (int) (from.x + direction.x) && (int) p.position.y == (int)  (from.y + direction.y));
            var target = pawns.FirstOrDefault(p => (int) p.position.x == (int) (from.x + direction.x*2) && (int) p.position.y == (int)  (from.y + direction.y*2));
            if (pawn != null && pawn.IsWhite != IsWhite && target == null)
            {
                var to = new Vector2(from.x + direction.x * 2, from.y + direction.y * 2);
                Move move;
                if (previousMove == null)
                {
                    move = new Move(this, position, to);
                    moves.Add(move);
                    move.hits.Add(pawn);
                }
                else
                {
                    previousMove.hits.Add(pawn);
                    previousMove.endPos = to;
                    move = previousMove;
                }

                FindAllAttacks(pawns, direction, to, move);
                
                var newDirection = new Vector2(direction.x * -1, direction.y);
                FindAllAttacks(pawns, newDirection, to, move);
            }
        }
    }
}