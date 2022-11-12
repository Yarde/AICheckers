using System.Collections.Generic;
using Code.Logic;
using UnityEngine;

namespace Code.Model
{
    public class Move
    {
        public Pawn Pawn { get; }
        public Vector2Int StartPos { get; }
        public Vector2Int EndPos { get; }
        public List<Pawn> Hits { get; }

        public bool IsAttack => Hits.Count > 0;

        public Move(Pawn movingPawn, Vector2Int from, Vector2Int to)
        {
            Pawn = movingPawn;
            StartPos = from;
            EndPos = to;
            Hits = new List<Pawn>();
        }

        public void AddHit(Pawn pawnToHit)
        {
            Hits.Add(pawnToHit);
        }

        public bool HasHit(Pawn pawn)
        {
            return Hits.Contains(pawn);
        }

        public bool Equals(int x1, int y1, int x2, int y2)
        {
            return x1 == StartPos.x && y1 == StartPos.y && x2 == EndPos.x && y2 == EndPos.y;
        }
    }
}