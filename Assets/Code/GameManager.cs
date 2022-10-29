using System;
using System.Collections.Generic;
using Code.Logic;
using Code.Utils;
using Code.View;
using UnityEngine;

namespace Code
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private int boardSize;
        [SerializeField] private int pawnRows;

        [SerializeField] private BoxCollider boardObject;
        [SerializeField] private Transform cameraPos;
        [SerializeField] private GameObject pawnsObject;

        [SerializeField] private PawnView whitePawn;
        [SerializeField] private PawnView blackPawn;
        [SerializeField] private BoardCell whiteSquare;
        [SerializeField] private BoardCell blackSquare;

        [SerializeField] private PlayerData whitePlayerData;
        [SerializeField] private PlayerData blackPlayerData;

        private BoardCell[,] _board;
        private List<Pawn> _pawns;

        private bool _turnInProgress;
        private bool _gameOver;

        private PlayerManager _playerManager;

        private void Start()
        {
            Application.runInBackground = true;

            _board = new BoardCell[boardSize, boardSize];
            _pawns = new List<Pawn>();

            boardObject.size = new Vector3(boardSize, boardSize, 1);
            boardObject.center = new Vector3((boardSize - 1) / 2.0f, (boardSize - 1) / 2.0f, 0);
            cameraPos.position = new Vector3((boardSize - 1) / 2.0f, (boardSize - 1) / 2.0f, -10);

            _playerManager = new PlayerManager(whitePlayerData, blackPlayerData, boardSize);

            GenerateBoard();
        }

        private async void Update()
        {
            if (_turnInProgress)
            {
                return;
            }

            _turnInProgress = true;
            await _playerManager.MakeTurn(_pawns);
            _turnInProgress = false;
        }

        private void GenerateBoard()
        {
            for (int y = 0; y < boardSize; y++)
            {
                for (int x = 0; x < boardSize; x++)
                {
                    var squarePrefab = (y + x) % 2 == 0 ? blackSquare : whiteSquare;

                    var square = Instantiate(squarePrefab, new Vector3(x, y, 0), Quaternion.identity,
                        boardObject.transform);
                    square.name = $"S{y}-{x}";
                    _board[y, x] = square;

                    if ((y + x) % 2 == 0 && (y < pawnRows || y >= boardSize - pawnRows))
                    {
                        var pawnPrefab = y > boardSize / 2 ? blackPawn : whitePawn;
                        var pawnGO = Instantiate(pawnPrefab, new Vector3(x, y, 0), Quaternion.identity,
                            pawnsObject.transform);
                        var pawn = new Pawn(boardSize, new Vector2Int(x, y), y <= boardSize / 2, pawnGO);
                        _pawns.Add(pawn);
                    }
                }
            }

            _pawns.GenerateMoves();
        }

        public void AnimateMoves(Move move)
        {
            var field = _board[(int)move.endPos.y, (int)move.endPos.x];
            field.Highlight();
        }
    }
}