using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Code.AI
{
    public class HumanPlayer : AIBase
    {
        private Vector2 _mousePosition;
        private readonly Camera _camera;
        private readonly GameManager _gameManager;
        private Pawn _selected;

        public HumanPlayer(int boardSize, PlayerData data) : base(boardSize, data)
        {
            _camera = Camera.main;
            _gameManager = Object.FindObjectOfType<GameManager>();
        }

        public override async UniTask<Move> Search(List<Pawn> pawns, bool isWhiteTurn, PlayerData data)
        {
            Move move = null;
            await UniTask.WaitUntil(() =>
            {
                move = GetMove(pawns, isWhiteTurn);
                return move != null;
            });

            return move;
        }

        private Move GetMove(List<Pawn> pawns, bool isWhiteTurn)
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
                var move = ValidMove((int)_selected.position.x, (int)_selected.position.y, x, y, pawns, isWhiteTurn);
                _selected = null;
                if (move != null)
                {
                    return move;
                }

                Debug.Log("Invalid move!");
            }

            return null;
        }

        private void SelectPawn(int x, int y, List<Pawn> pawns, bool isWhiteTurn)
        {
            var pawn = pawns.FirstOrDefault(p => (int)p.position.x == x && (int)p.position.y == y);
            if (pawn == null) return;
            if (pawn.IsWhite != isWhiteTurn) return;

            _selected = pawn;
            var hasHit = HasHit(pawns, isWhiteTurn);
            foreach (var move in _selected.moves)
            {
                if (!hasHit || move.isAttack)
                {
                    _gameManager.AnimateMoves(move);
                }
            }
        }

        private Move ValidMove(int x1, int y1, int x2, int y2, List<Pawn> pawns, bool isWhiteTurn)
        {
            foreach (var move in _selected.moves)
            {
                if (move.Equals(x1, y1, x2, y2))
                {
                    if (move.isAttack)
                    {
                        return move;
                    }

                    return HasHit(pawns, isWhiteTurn) ? null : move;
                }
            }

            return null;
        }


        private bool HasHit(List<Pawn> pawns, bool isWhiteTurn)
        {
            foreach (var pawn in pawns)
            {
                if (pawn != null && isWhiteTurn == pawn.IsWhite)
                {
                    if (pawn.moves.Any(p => p.isAttack))
                    {
                        Debug.Log("You have hit!");
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