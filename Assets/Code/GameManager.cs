using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Code
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private BoxCollider boardObject;
        [SerializeField] private Transform cameraPos;
        [SerializeField] private GameObject pawnsObject;
        [SerializeField] private GameObject aiObject;

        [SerializeField] private PawnGO whitePawn;
        [SerializeField] private PawnGO blackPawn;
        [SerializeField] private GameObject whiteSquare;
        [SerializeField] private GameObject blackSquare;

        [SerializeField] private Board board;
        [SerializeField] private GameSettings settings;

        public GameObject[,] _board;
        public List<Pawn> pawns;
        public Pawn _selected;
        public bool _isWhiteTurn;

        private bool _turnInProgress;
        private bool _gameOver;

        private AI _aiWhite;
        private AI _aiBlack;
        private AI _aiRandom;

        private PlayerMover playerMover;

        public int _turnCounter;
        private int _lastAttackTurn;

        private float gameStart;
        private float whiteTime;
        private float blackTime;

        void Start()
        {
            Application.runInBackground = true;
            gameStart = Time.realtimeSinceStartup;

            _board = new GameObject[settings.boardSize, settings.boardSize];
            pawns = new List<Pawn>();

            boardObject.size = new Vector3(settings.boardSize, settings.boardSize, 1);
            boardObject.center = new Vector3((settings.boardSize - 1) / 2.0f, (settings.boardSize - 1) / 2.0f, 0);
            cameraPos.position = new Vector3((settings.boardSize - 1) / 2.0f, (settings.boardSize - 1) / 2.0f, -10);

            if (Random.Range(0.0f, 1.0f) > 0.5f)
            {
                _isWhiteTurn = true;
                //Debug.Log("White starts");
            }
            else
            {
                _isWhiteTurn = false;
                //Debug.Log("Black starts");
            }

            if (settings.IsWhiteAI || settings.IsBlackAI)
            {
                SetupAI();
            }

            if (!settings.IsWhiteAI || !settings.IsBlackAI)
            {
                playerMover = new PlayerMover(this);
            }

            GenerateBoard();
        }

        void Update()
        {
            MakeTurn();

            if (_gameOver)
            {
                whiteTime *= 1_000;
                blackTime *= 1_000;
                Debug.Log($"Game took {Time.realtimeSinceStartup - gameStart} seconds");
                Debug.Log($"White used {whiteTime} ms avg move time {whiteTime / _turnCounter / 2} ms");
                Debug.Log($"Black used {blackTime} ms avg move time {blackTime / _turnCounter / 2} ms");
                SceneManager.LoadScene(0);
            }
        }

        private void MakeTurn()
        {
            if (_turnInProgress || _gameOver)
            {
                return;
            }

            _turnInProgress = true;

            if (_isWhiteTurn && !settings.IsWhiteAI || !_isWhiteTurn && !settings.IsBlackAI)
            {
                playerMover.PlayerMove();
            }
            else
            {
                var timeStart = Time.realtimeSinceStartup;
                if (_isWhiteTurn)
                {
                    //Debug.Log("White AI move!");
                    var move = _turnCounter < 4
                        ? _aiRandom.Search(pawns, _isWhiteTurn, 0, settings._evWhite, false)
                        : _aiWhite.Search(pawns, _isWhiteTurn, settings._aiWhiteDepth, settings._evWhite, settings._aiWhiteEndgame);

                    MakeAiTurn(move);
                    whiteTime += Time.realtimeSinceStartup - timeStart;
                }
                else
                {
                    //Debug.Log("Black AI move!");
                    var move = _turnCounter < 4
                        ? _aiRandom.Search(pawns, _isWhiteTurn, 0, settings._evBlack, false)
                        : _aiBlack.Search(pawns, _isWhiteTurn, settings._aiBlackDepth, settings._evBlack, settings._aiBlackEndgame);
                    MakeAiTurn(move);
                    blackTime += Time.realtimeSinceStartup - timeStart;
                }
            }

            _turnInProgress = false;
        }

        private void MakeAiTurn(Move move)
        {
            if (move != null)
            {
                _turnCounter++;
                _isWhiteTurn = !_isWhiteTurn;
                _selected = move.pawn;
                MovePawn(move);
                AttackPawn(move);
                GenerateMoves();
                CheckWin();
            }
            else
            {
                Debug.LogError("Draw");
                Time.timeScale = 0;
                _gameOver = true;
            }

            _selected = null;
        }

        public void GenerateMoves()
        {
            foreach (var pawn in pawns)
            {
                pawn.moves = pawn.PossibleMoves(pawns);
            }
        }

        public void MovePawn(Move move)
        {
            _selected.GO.transform.position = new Vector3(move.endPos.x, move.endPos.y, 0);
            _selected.position = new Vector2(move.endPos.x, move.endPos.y);

            if (_selected.IsWhite && (int)move.endPos.y == settings.boardSize - 1 ||
                !_selected.IsWhite && (int)move.endPos.y == 0)
            {
                _selected.IsQueen = true;
            }
        }

        public void AttackPawn(Move move)
        {
            foreach (var hit in move.hits)
            {
                _lastAttackTurn = _turnCounter;
                pawns.Remove(hit);
                Destroy(hit.GO.gameObject);
            }
        }

        private void SetupAI()
        {
            if (settings.IsWhiteAI)
            {
                if (settings._aiWhiteType == Algorithm.Minmax)
                {
                    _aiWhite = aiObject.AddComponent<MinMax>();
                }
                else
                {
                    _aiWhite = aiObject.AddComponent<AlphaBetaPruning>();
                }
                _aiWhite.Setup(settings.boardSize);
            }

            if (settings.IsBlackAI)
            {
                if (settings._aiBlackType == Algorithm.Minmax)
                {
                    _aiBlack = aiObject.AddComponent<MinMax>();
                }
                else
                {
                    _aiBlack = aiObject.AddComponent<AlphaBetaPruning>();
                }
                _aiBlack.Setup(settings.boardSize);
            }

            _aiRandom = aiObject.AddComponent<RandomSearch>();
            _aiRandom.Setup(settings.boardSize);
        }

        private void GenerateBoard()
        {
            for (int y = 0; y < settings.boardSize; y++)
            {
                for (int x = 0; x < settings.boardSize; x++)
                {
                    var squarePrefab = (y + x) % 2 == 0 ? blackSquare : whiteSquare;

                    var square = Instantiate(squarePrefab, new Vector3(x, y, 0), Quaternion.identity,
                        boardObject.transform);
                    square.name = $"S{y}-{x}";
                    _board[y, x] = square;

                    if ((y + x) % 2 == 0 && (y < settings.pawnRows || y >= settings.boardSize - settings.pawnRows))
                    {
                        var pawnPrefab = y > settings.boardSize / 2 ? blackPawn : whitePawn;
                        var pawnGO = Instantiate(pawnPrefab, new Vector3(x, y, 0), Quaternion.identity,
                            pawnsObject.transform);
                        var pawn = new Pawn
                        {
                            boardSize = settings.boardSize,
                            position = new Vector2(x, y),
                            IsWhite = y <= settings.boardSize / 2,
                            GO = pawnGO
                        };
                        //Instantiate(pawnPrefab, new Vector3(x, y, 0), Quaternion.identity, pawnsObject.transform);
                        pawns.Add(pawn);
                    }
                }
            }

            GenerateMoves();
        }

        public void CheckWin()
        {
            if (!pawns.Any(p => p.IsWhite))
            {
                Debug.LogError("Black won");
                Time.timeScale = 0;
                _gameOver = true;
            }

            if (pawns.All(p => p.IsWhite))
            {
                Debug.LogError("White won");
                Time.timeScale = 0;
                _gameOver = true;
            }

            if (_turnCounter - _lastAttackTurn > 50)
            {
                Debug.LogError("Draw");
                Time.timeScale = 0;
                _gameOver = true;
            }
        }
    }
}