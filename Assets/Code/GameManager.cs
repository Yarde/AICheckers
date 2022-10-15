using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Code
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private int boardSize;
        [SerializeField] private int pawnRows;

        [SerializeField] private BoxCollider boardObject;
        [SerializeField] private Transform cameraPos;
        [SerializeField] private GameObject pawnsObject;

        [SerializeField] private PawnGO whitePawn;
        [SerializeField] private PawnGO blackPawn;
        [SerializeField] private GameObject whiteSquare;
        [SerializeField] private GameObject blackSquare;

        [SerializeField] private PlayerData whitePlayerData;
        [SerializeField] private PlayerData blackPlayerData;

        public GameObject[,] Board;
        public List<Pawn> pawns;

        private bool _turnInProgress;
        private bool _gameOver;

        private PlayerManager _playerManager;

        private void Start()
        {
            Application.runInBackground = true;

            Board = new GameObject[boardSize, boardSize];
            pawns = new List<Pawn>();

            boardObject.size = new Vector3(boardSize, boardSize, 1);
            boardObject.center = new Vector3((boardSize - 1) / 2.0f, (boardSize - 1) / 2.0f, 0);
            cameraPos.position = new Vector3((boardSize - 1) / 2.0f, (boardSize - 1) / 2.0f, -10);

            _playerManager = new PlayerManager(whitePlayerData, blackPlayerData, boardSize);

            GenerateBoard();
        }

        private void Update()
        {
            if (_turnInProgress)
            {
                return;
            }

            _turnInProgress = true;
            _playerManager.MakeTurn(pawns);
            _turnInProgress = false;
        }

        private void GenerateMoves()
        {
            foreach (var pawn in pawns)
            {
                pawn.moves = pawn.PossibleMoves(pawns);
            }
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
                    Board[y, x] = square;

                    if ((y + x) % 2 == 0 && (y < pawnRows || y >= boardSize - pawnRows))
                    {
                        var pawnPrefab = y > boardSize / 2 ? blackPawn : whitePawn;
                        var pawnGO = Instantiate(pawnPrefab, new Vector3(x, y, 0), Quaternion.identity,
                            pawnsObject.transform);
                        var pawn = new Pawn
                        {
                            boardSize = boardSize,
                            position = new Vector2(x, y),
                            IsWhite = y <= boardSize / 2,
                            GO = pawnGO
                        };
                        //Instantiate(pawnPrefab, new Vector3(x, y, 0), Quaternion.identity, pawnsObject.transform);
                        pawns.Add(pawn);
                    }
                }
            }

            GenerateMoves();
        }
    }
}