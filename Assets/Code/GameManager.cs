using System.Collections.Generic;
using System.Linq;
using Code;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int boardSize;
    [SerializeField] private int pawnRows;

    [SerializeField] private BoxCollider boardObject;
    [SerializeField] private Transform cameraPos;
    [SerializeField] private GameObject pawnsObject;
    [SerializeField] private GameObject aiObject;

    [SerializeField] private PawnGO whitePawn;
    [SerializeField] private PawnGO blackPawn;
    [SerializeField] private GameObject whiteSquare;
    [SerializeField] private GameObject blackSquare;
    
    [SerializeField] private Algorithm _aiWhiteType;
    [SerializeField] private Algorithm _aiBlackType;

    [SerializeField] private EvaluationFunction _evWhite;
    [SerializeField] private EvaluationFunction _evBlack;
    
    [SerializeField] private int _aiWhiteDepth;
    [SerializeField] private int _aiBlackDepth;
    
    [SerializeField] private bool _aiWhiteEndgame;
    [SerializeField] private bool _aiBlackEndgame;
    
    [SerializeField] private bool IsWhiteAI = true;
    [SerializeField] private bool IsBlackAI = true;

    public GameObject[,] Board;
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
        
        Board = new GameObject[boardSize, boardSize];
        pawns = new List<Pawn>();
        
        boardObject.size = new Vector3(boardSize, boardSize, 1);
        boardObject.center = new Vector3((boardSize-1)/2.0f, (boardSize-1)/2.0f, 0);
        cameraPos.position = new Vector3((boardSize-1)/2.0f, (boardSize-1)/2.0f, -10);

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

        if (IsWhiteAI || IsBlackAI)
        {
            SetupAI();
        }
        if (!IsWhiteAI || !IsBlackAI)
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
            Debug.Log($"Game took {Time.realtimeSinceStartup-gameStart} seconds");
            Debug.Log($"White used {whiteTime} ms avg move time {whiteTime/_turnCounter/2} ms");
            Debug.Log($"Black used {blackTime} ms avg move time {blackTime/_turnCounter/2} ms");
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
        
        if (_isWhiteTurn && !IsWhiteAI || !_isWhiteTurn && !IsBlackAI)
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
                    ? _aiRandom.Search(pawns, _isWhiteTurn, 0, _evWhite, false) 
                    : _aiWhite.Search(pawns, _isWhiteTurn, _aiWhiteDepth, _evWhite, _aiWhiteEndgame);
                
                MakeAiTurn(move);
                whiteTime += Time.realtimeSinceStartup - timeStart;
            }
            else
            {
                //Debug.Log("Black AI move!");
                var move = _turnCounter < 4 
                    ? _aiRandom.Search(pawns, _isWhiteTurn, 0, _evBlack, false) 
                    : _aiBlack.Search(pawns, _isWhiteTurn, _aiBlackDepth, _evBlack, _aiBlackEndgame);
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

        if (_selected.IsWhite && (int) move.endPos.y == boardSize-1 || !_selected.IsWhite && (int) move.endPos.y == 0)
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
        if (IsWhiteAI)
        {
            if (_aiWhiteType == Algorithm.Minmax)
            {
                _aiWhite = aiObject.AddComponent<MinMax>();
                _aiWhite.Setup(boardSize, aiObject);
            }
            else
            {
                _aiWhite = aiObject.AddComponent<ABPruning>();
                _aiWhite.Setup(boardSize, aiObject);
            }
        }

        if (IsBlackAI)
        {
            if (_aiBlackType == Algorithm.Minmax)
            {
                _aiBlack = aiObject.AddComponent<MinMax>();
                _aiBlack.Setup(boardSize, aiObject);
            }
            else
            {
                _aiBlack = aiObject.AddComponent<ABPruning>();
                _aiBlack.Setup(boardSize, aiObject);
            }
        }

        _aiRandom = aiObject.AddComponent<RandomSearch>();
        _aiRandom.Setup(boardSize, aiObject);
    }
    
    private void GenerateBoard()
    {
        for (int y = 0; y < boardSize; y++)
        {
            for (int x = 0; x < boardSize; x++)
            {
                var squarePrefab = (y + x) % 2 == 0 ? blackSquare : whiteSquare;

                var square = Instantiate(squarePrefab, new Vector3(x, y, 0), Quaternion.identity, boardObject.transform);
                square.name = $"S{y}-{x}";
                Board[y, x] = square;

                if ((y + x) % 2 == 0 && (y < pawnRows|| y >= boardSize - pawnRows))
                {
                    var pawnPrefab = y > boardSize/2 ? blackPawn : whitePawn;
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

    public void CheckWin()
    {
        if (!pawns.Any(p => p.IsWhite))
        {
            Debug.LogError("Black won");
            Time.timeScale = 0;
            _gameOver = true;
        }

        if (!pawns.Any(p => !p.IsWhite))
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
    
    public enum Algorithm
    {
        Minmax,
        AlphaBeta
    }
    
    public enum EvaluationFunction
    {
        PawnValue,
        PawnBoardValue,
        Extra
    }
}