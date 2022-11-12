using System.Collections.Generic;
using Code.Model;
using Code.Utils;
using Code.View;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Code.Logic
{
    public class BoardManager
    {
        public List<Pawn> Pawns { get; }

        private readonly BoardCell[,] _board;

        public BoardManager(BoardData boardData, Transform boardTransform, Transform pawnsTransform)
        {
            _board = new BoardCell[boardData.boardSize, boardData.boardSize];
            Pawns = new List<Pawn>();

            GenerateBoard(boardData, boardTransform, pawnsTransform);
        }
        
        private void GenerateBoard(BoardData boardData, Transform boardTransform, Transform pawnsTransform)
        {
            for (var x = 0; x < boardData.boardSize; x++)
            {
                for (var y = 0; y < boardData.boardSize; y++)
                {
                    var squarePrefab = (y + x) % 2 == 0 ? boardData.blackSquare : boardData.whiteSquare;

                    var square = Object.Instantiate(squarePrefab, new Vector3(x, y, 0), Quaternion.identity, boardTransform);
                    square.name = $"S{x}-{y}";
                    _board[x, y] = square;

                    if ((y + x) % 2 != 0 || (y >= boardData.pawnRows && y < boardData.boardSize - boardData.pawnRows)) continue;
                    
                    var pawnPrefab = y > boardData.boardSize / 2 ? boardData.blackPawn : boardData.whitePawn;
                    var pawnView = Object.Instantiate(pawnPrefab, new Vector3(x, y, 0), Quaternion.identity, pawnsTransform);
                    var pawn = new Pawn(boardData.boardSize, new Vector2Int(x, y), y <= boardData.boardSize / 2, pawnView);
                    Pawns.Add(pawn);
                }
            }

            Pawns.GenerateMoves();
        }

        public void AnimateMoves(Move move)
        {
            var target = _board[move.EndPos.x, move.EndPos.y];
            var source = _board[move.StartPos.x, move.StartPos.y];
            target.Highlight(move.Hits.Count > 0).Forget();
            source.Highlight(false).Forget();
        }
    }
}