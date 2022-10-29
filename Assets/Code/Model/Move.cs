using System.Collections.Generic;
using UnityEngine;

namespace Code.Logic
{
    public class Move
    {
        public Pawn pawn;
        private Vector2 startPos;
        public Vector2 endPos;
        public List<Pawn> hits;

        public bool isAttack => hits.Count > 0;

        public Move(Pawn movingPawn, Vector2 from, Vector2 to)
        {
            pawn = movingPawn;
            startPos = from;
            endPos = to;
            hits = new List<Pawn>();
        }

        public bool Equals(int x1, int y1, int x2, int y2)
        {
            return x1 == (int) startPos.x && y1 == (int) startPos.y && x2 == (int) endPos.x && y2 == (int) endPos.y;
        }
    }
}