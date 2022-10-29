using System.Collections.Generic;
using Code.Logic;

namespace Code.Utils
{
    public static class Extensions
    {
        public static bool IsMine(this Pawn pawn, bool isWhiteTurn)
        {
            return pawn.IsWhite == isWhiteTurn;
        }

        public static void GenerateMoves(this List<Pawn> state)
        {
            foreach (var pawn in state)
            {
                pawn.GeneratePossibleMoves(state);
            }
        }
    }
}