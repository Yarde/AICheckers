// using System;
// using System.Collections.Generic;
// using System.Linq;
// using UnityEngine;
//
// namespace Code.AI
// {
//     public class HumanPlayer : AIBase
//     {
//         private Vector2 _mousePosition;
//         //private GameManager _gm;
//
//         public HumanPlayer(GameManager gameManager)
//         {
//             _gm = gameManager;
//         }
//
//         public override Move Search(List<Pawn> state, bool isWhiteTurn, PlayerData data)
//         {
//             Move move = null;
//             while (move == null)
//             {
//                 move = GetMove();
//             }
//             return move;
//         }
//
//         private void SelectPawn(int x, int y)
//         {
//             var pawn = _gm.pawns.FirstOrDefault(p => (int)p.position.x == x && (int)p.position.y == y);
//             if (pawn != null)
//             {
//                 if (pawn.IsWhite == _gm._isWhiteTurn)
//                 {
//                     _gm._selected = pawn;
//                     var hasHit = HasHit();
//                     foreach (var move in _gm._selected.moves)
//                     {
//                         if (!hasHit || move.isAttack)
//                         {
//                             var field = _gm.Board[(int)move.endPos.y, (int)move.endPos.x];
//                             field.GetComponent<Animation>().Play();
//                         }
//                     }
//                 }
//             }
//         }
//
//         private Move ValidMove(int x1, int y1, int x2, int y2)
//         {
//             foreach (var move in _gm._selected.moves)
//             {
//                 if (move.Equals(x1, y1, x2, y2))
//                 {
//                     if (move.isAttack)
//                     {
//                         return move;
//                     }
//
//                     return HasHit() ? null : move;
//                 }
//             }
//
//             return null;
//         }
//
//
//         private bool HasHit()
//         {
//             foreach (var pawn in _gm.pawns)
//             {
//                 if (pawn != null && _gm._isWhiteTurn == pawn.IsWhite)
//                 {
//                     if (pawn.moves.Any(p => p.isAttack))
//                     {
//                         Debug.Log("You have hit!");
//                         return true;
//                     }
//                 }
//             }
//
//             return false;
//         }
//
//         private void UpdateMouse()
//         {
//             RaycastHit hit;
//             if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f,
//                     LayerMask.GetMask("Board")))
//             {
//                 _mousePosition = new Vector2((int)(hit.point.x + 0.5f), (int)(hit.point.y + 0.5f));
//             }
//             else
//             {
//                 _mousePosition = new Vector2(-1, -1);
//             }
//         }
//
//         private Move GetMove()
//         {
//             UpdateMouse();
//
//             int x = (int)_mousePosition.x;
//             int y = (int)_mousePosition.y;
//
//             if (Input.GetMouseButtonDown(0) && x > -1 && y > -1 && _gm._selected == null)
//             {
//                 SelectPawn(x, y);
//             }
//             else if (Input.GetMouseButtonUp(0) && x > -1 && y > -1 && _gm._selected != null)
//             {
//                 var move = ValidMove((int)_gm._selected.position.x, (int)_gm._selected.position.y, x, y);
//                 _gm._selected = null;
//                 if (move != null)
//                 {
//                     return move;
//                     // _gm._turnCounter++;
//                     // _gm.MovePawn(move);
//                     // _gm.AttackPawn(move);
//                     // _gm.CheckWin();
//                     // _gm.GenerateMoves();
//                     // _gm._isWhiteTurn = !_gm._isWhiteTurn;
//                 }
//
//                 Debug.Log($"Invalid move!");
//             }
//
//             return null;
//         }
//     }
// }