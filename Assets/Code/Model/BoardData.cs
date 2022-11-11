using Code.View;
using UnityEngine;

namespace Code.Model
{
    [CreateAssetMenu]
    public class BoardData : ScriptableObject
    {
        public int boardSize;
        public int pawnRows;

        public PawnView whitePawn;
        public PawnView blackPawn;
        public BoardCell whiteSquare;
        public BoardCell blackSquare;
    }
}