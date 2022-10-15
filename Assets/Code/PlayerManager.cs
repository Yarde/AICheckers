using System;
using System.Collections.Generic;
using System.Linq;
using Code.AI;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Code
{
    public class PlayerManager
    {
        private AIBase _aiWhite;
        private AIBase _aiBlack;
        private AIBase _aiRandom;
        private PlayerData _whitePlayerData;
        private PlayerData _blackPlayerData;

        private int _lastAttackTurn;
        private int _turnCounter;
        private int _boardSize;
        private float _gameStart;
        private bool _isWhiteTurn;
        private bool _gameOver;
        private Pawn _selected;

        private float _whiteTime;
        private float _blackTime;

        public PlayerManager(PlayerData whitePlayerData, PlayerData blackPlayerData, int boardSize)
        {
            _gameStart = Time.realtimeSinceStartup;
            _isWhiteTurn = Random.Range(0.0f, 1.0f) > 0.5f;
            _boardSize = boardSize;

            _whitePlayerData = whitePlayerData;
            _blackPlayerData = blackPlayerData;

            _aiWhite = CreateAI(whitePlayerData.algorithmType);
            _aiBlack = CreateAI(blackPlayerData.algorithmType);
            _aiRandom = CreateAI(AlgorithmType.Random);
        }

        private AIBase CreateAI(AlgorithmType aiType)
        {
            AIBase ai = aiType switch
            {
                AlgorithmType.HumanPlayer => new HumanPlayer(),
                AlgorithmType.MinMax => new MinMax(),
                AlgorithmType.AlphaBetaPruning => new AlphaBetaPruning(),
                AlgorithmType.Random => new RandomSearch(),
                _ => throw new ArgumentOutOfRangeException(nameof(aiType), aiType, null)
            };

            ai.Setup(_boardSize);
            return ai;
        }

        public async UniTask MakeTurn(List<Pawn> pawns)
        {
            var playerData = _isWhiteTurn ? _whitePlayerData : _blackPlayerData;

            var player = _turnCounter / 2 < playerData.randomStartMoves
                ? _aiRandom
                : _isWhiteTurn
                    ? _aiWhite
                    : _aiBlack;
            var move = player.Search(pawns, _isWhiteTurn, playerData);
            var result = MakeAiTurn(await move, pawns);

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

        private GameResult MakeAiTurn(Move move, List<Pawn> pawns)
        {
            if (move != null)
            {
                _turnCounter++;
                _isWhiteTurn = !_isWhiteTurn;
                _selected = move.pawn;
                MovePawn(move);
                AttackPawn(move, pawns);
                GenerateMoves(pawns);
                _selected = null;
                return CheckWin(pawns);
            }

            return GameResult.Pat;
        }

        private void GenerateMoves(List<Pawn> pawns)
        {
            foreach (var pawn in pawns)
            {
                pawn.moves = pawn.PossibleMoves(pawns);
            }
        }

        private void MovePawn(Move move)
        {
            _selected.GO.transform.position = new Vector3(move.endPos.x, move.endPos.y, 0);
            _selected.position = new Vector2(move.endPos.x, move.endPos.y);

            if (_selected.IsWhite && (int)move.endPos.y == _boardSize - 1 ||
                !_selected.IsWhite && (int)move.endPos.y == 0)
            {
                _selected.IsQueen = true;
            }
        }

        private void AttackPawn(Move move, List<Pawn> pawns)
        {
            foreach (var hit in move.hits)
            {
                _lastAttackTurn = _turnCounter;
                pawns.Remove(hit);
                Object.Destroy(hit.GO.gameObject);
            }
        }

        private GameResult CheckWin(List<Pawn> pawns)
        {
            if (!pawns.Any(p => p.IsWhite))
            {
                return GameResult.BlackWin;
            }

            if (pawns.All(p => p.IsWhite))
            {
                return GameResult.WhiteWin;
            }

            if (_turnCounter - _lastAttackTurn > 50)
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
        Pat,
        InProgress
    }
}