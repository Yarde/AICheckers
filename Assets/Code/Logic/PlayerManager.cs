using System;
using System.Collections.Generic;
using System.Linq;
using Code.Logic.AI;
using Code.Model;
using Code.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace Code.Logic
{
    public class PlayerManager
    {
        private readonly PlayerBase _playerWhite;
        private readonly PlayerBase _playerBlack;
        private readonly PlayerBase _playerRandom;
        private readonly PlayerData _whitePlayerData;
        private readonly PlayerData _blackPlayerData;

        private readonly int _boardSize;
        private readonly float _gameStart;
        private readonly BoardManager _boardManager;

        private int _lastAttackTurn;
        private int _turnCounter;

        private bool _isWhiteTurn;
        private bool _gameOver;
        private Pawn _selected;

        private float _whiteTime;
        private float _blackTime;

        public PlayerManager(PlayerData whitePlayerData, PlayerData blackPlayerData, int boardSize, BoardManager boardManager)
        {
            _gameStart = Time.realtimeSinceStartup;
            _isWhiteTurn = Random.Range(0.0f, 1.0f) > 0.5f;
            _boardSize = boardSize;
            _boardManager = boardManager;

            _whitePlayerData = whitePlayerData;
            _blackPlayerData = blackPlayerData;

            _playerWhite = CreateAI(whitePlayerData);
            _playerBlack = CreateAI(blackPlayerData);
            _playerRandom = CreateAI(new PlayerData { algorithmType = AlgorithmType.Random });
        }

        private PlayerBase CreateAI(PlayerData data)
        {
            PlayerBase player = data.algorithmType switch
            {
                AlgorithmType.HumanPlayer => new HumanPlayer(_boardSize, data, _boardManager),
                AlgorithmType.MinMax => new MinMaxPlayer(_boardSize, data),
                AlgorithmType.AlphaBetaPruning => new AlphaBetaPruningPlayer(_boardSize, data),
                AlgorithmType.Random => new RandomSearchPlayer(_boardSize, data),
                _ => throw new ArgumentOutOfRangeException()
            };
            return player;
        }

        public async UniTask MakeTurn()
        {
            var turnStartTime = Time.realtimeSinceStartup;
            var playerData = _isWhiteTurn ? _whitePlayerData : _blackPlayerData;

            var player = _turnCounter / 2 < playerData.randomStartMoves
                ? _playerRandom
                : _isWhiteTurn
                    ? _playerWhite
                    : _playerBlack;
            var move = await player.Search(_boardManager.Pawns, _isWhiteTurn, playerData);
            player.LogState(_boardManager.Pawns, move);
            var result = MakeMove(move, _boardManager.Pawns);

            if (_isWhiteTurn)
            {
                _whiteTime += Time.realtimeSinceStartup - turnStartTime;
            }
            else
            {
                _blackTime += Time.realtimeSinceStartup - turnStartTime;
            }

            if (result != GameResult.InProgress)
            {
                _whiteTime *= 1_000;
                _blackTime *= 1_000;
                Debug.LogError(result);
                Debug.Log($"Game took {Time.realtimeSinceStartup - _gameStart} seconds");
                Debug.Log($"White used {_whiteTime} ms avg move time {_whiteTime / _turnCounter / 2} ms");
                Debug.Log($"Black used {_blackTime} ms avg move time {_blackTime / _turnCounter / 2} ms");

                // todo not reload
                SceneManager.LoadScene(0);
            }
        }

        private GameResult MakeMove(Move move, List<Pawn> pawns)
        {
            if (move != null)
            {
                _turnCounter++;
                _isWhiteTurn = !_isWhiteTurn;
                _selected = move.Pawn;
                _selected.Move(move);
                AttackPawn(move, pawns);
                pawns.GenerateMoves();
                _selected = null;
                return CheckWin(pawns);
            }

            return _isWhiteTurn ? GameResult.BlackWin : GameResult.WhiteWin;
        }

        private void AttackPawn(Move move, ICollection<Pawn> pawns)
        {
            foreach (var hit in move.Hits)
            {
                _lastAttackTurn = _turnCounter;
                pawns.Remove(hit);
                hit.Destroy();
            }
        }

        private GameResult CheckWin(IReadOnlyCollection<Pawn> pawns)
        {
            if (!pawns.Any(p => p.IsWhite))
            {
                return GameResult.BlackWin;
            }

            if (pawns.All(p => p.IsWhite))
            {
                return GameResult.WhiteWin;
            }

            if (_turnCounter - _lastAttackTurn > 100)
            {
                return GameResult.Draw;
            }

            return GameResult.InProgress;
        }
    }

    public enum GameResult
    {
        WhiteWin,
        BlackWin,
        Draw,
        InProgress
    }
}