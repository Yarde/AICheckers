using System.Collections.Generic;
using Code.Logic;
using UnityEngine;

namespace Code.Model
{
    public class Move
    {
        public readonly Pawn pawn;
        private Vector2Int _startPos;
        public Vector2Int endPos;
        public readonly List<Pawn> hits;

        public bool IsAttack => hits.Count > 0;

        public Move(Pawn movingPawn, Vector2Int from, Vector2Int to)
        {
            pawn = movingPawn;
            _startPos = from;
            endPos = to;
            hits = new List<Pawn>();
        }

        public bool Equals(int x1, int y1, int x2, int y2)
        {
            return x1 == _startPos.x && y1 == _startPos.y && x2 == endPos.x && y2 == endPos.y;
        }
    }
}