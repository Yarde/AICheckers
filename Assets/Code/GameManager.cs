using Code.Logic;
using Code.Model;
using UnityEngine;

namespace Code
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private BoxCollider boardObject;
        [SerializeField] private Transform cameraPos;
        [SerializeField] private Transform pawnsObject;

        [SerializeField] private BoardData boardData;
        [SerializeField] private PlayerData whitePlayerData;
        [SerializeField] private PlayerData blackPlayerData;

        private bool _turnInProgress;
        private bool _gameOver;

        private PlayerManager _playerManager;
        private BoardManager _boardManager;

        private void Start()
        {
            Application.runInBackground = true;

            boardObject.size = new Vector3(boardData.boardSize, boardData.boardSize, 1);
            boardObject.center = new Vector3((boardData.boardSize - 1) / 2.0f, (boardData.boardSize - 1) / 2.0f, 0);
            cameraPos.position = new Vector3((boardData.boardSize - 1) / 2.0f, (boardData.boardSize - 1) / 2.0f, -10);

            _boardManager = new BoardManager(boardData, boardObject.transform, pawnsObject);
            _playerManager = new PlayerManager(whitePlayerData, blackPlayerData, boardData.boardSize, _boardManager);
        }

        private async void Update()
        {
            if (_turnInProgress)
            {
                return;
            }

            _turnInProgress = true;
            await _playerManager.MakeTurn();
            _turnInProgress = false;
        }
    }
}