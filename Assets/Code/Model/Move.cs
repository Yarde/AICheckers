using System.Collections.Generic;
using UnityEngine;

namespace Code.Logic
{
    public class Move
    {
        public Pawn pawn;
        private Vector2Int startPos;
        public Vector2Int endPos;
        public List<Pawn> hits;

        public bool isAttack => hits.Count > 0;

        public Move(Pawn movingPawn, Vector2Int from, Vector2Int to)
        {
            pawn = movingPawn;
            startPos = from;
            endPos = to;
            hits = new List<Pawn>();
        }

        public bool Equals(int x1, int y1, int x2, int y2)
        {
            return x1 == startPos.x && y1 == startPos.y && x2 == endPos.x && y2 == endPos.y;
        }
    }
}