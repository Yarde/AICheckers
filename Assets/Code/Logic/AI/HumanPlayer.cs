using System.Collections.Generic;
using System.Linq;
using Code.Model;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Code.Logic.AI
{
    public class HumanPlayer : PlayerBase
    {
        private Vector2 _mousePosition;
        private readonly Camera _camera;
        private Pawn _selected;
        private readonly BoardManager _boardManager;

        public HumanPlayer(int boardSize, PlayerData data, BoardManager boardManager) : base(boardSize, data)
        {
            _camera = Camera.main;
            _boardManager = boardManager;
        }

        public override async UniTask<Move> Search(IReadOnlyList<Pawn> pawns, bool isWhiteTurn, PlayerData data)
        {
            Move move = null;
            await UniTask.WaitUntil(() =>
            {
                move = GetMove(pawns, isWhiteTurn);
                return move != null;
            });

            return move;
        }

        private Move GetMove(IReadOnlyCollection<Pawn> pawns, bool isWhiteTurn)
        {
            if (!Input.GetMouseButtonDown(0) && !Input.GetMouseButtonUp(0))
            {
                return null;
            }

            UpdateMouse();

            var x = (int)_mousePosition.x;
            var y = (int)_mousePosition.y;

            if (Input.GetMouseButtonDown(0) && x > -1 && y > -1 && _selected == null)
            {
                SelectPawn(x, y, pawns, isWhiteTurn);
            }
            else if (Input.GetMouseButtonUp(0) && x > -1 && y > -1 && _selected != null)
            {
                var move = ValidMove(_selected.Position.x, _selected.Position.y, x, y, pawns, isWhiteTurn);
                _selected = null;
                if (move != null)
                {
                    return move;
                }

                Debug.Log("Invalid move!");
            }

            return null;
        }

        private void SelectPawn(int x, int y, IReadOnlyCollection<Pawn> pawns, bool isWhiteTurn)
        {
            var pawn = pawns.FirstOrDefault(p => p.Position.x == x && p.Position.y == y);
            if (pawn == null) return;
            if (pawn.IsWhite != isWhiteTurn) return;

            _selected = pawn;
            var hasHit = HasHit(pawns, isWhiteTurn);
            foreach (var move in _selected.Moves)
            {
                if (!hasHit || move.IsAttack)
                {
                    _boardManager.AnimateMoves(move);
                }
            }
        }

        private Move ValidMove(int x1, int y1, int x2, int y2, IEnumerable<Pawn> pawns, bool isWhiteTurn)
        {
            foreach (var move in _selected.Moves)
            {
                if (move.Equals(x1, y1, x2, y2))
                {
                    if (move.IsAttack)
                    {
                        return move;
                    }

                    return HasHit(pawns, isWhiteTurn) ? null : move;
                }
            }

            return null;
        }


        private bool HasHit(IEnumerable<Pawn> pawns, bool isWhiteTurn)
        {
            foreach (var pawn in pawns)
            {
                if (pawn != null && isWhiteTurn == pawn.IsWhite)
                {
                    var attacks = pawn.Moves.Where(p => p.IsAttack).ToList();
                    foreach (var attack in attacks)
                    {
                        _boardManager.AnimateMoves(attack);
                    }

                    if (attacks.Count > 0)
                    {
                        Debug.Log($"You have hit with: {string.Join(", ", attacks.Select(a => a.Pawn.Position))}!");
                        return true;
                    }
                }
            }

            return false;
        }

        private void UpdateMouse()
        {
            _mousePosition = Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out var hit, 25.0f,
                LayerMask.GetMask("Board"))
                ? new Vector2((int)(hit.point.x + 0.5f), (int)(hit.point.y + 0.5f))
                : new Vector2(-1, -1);
        }
    }
}