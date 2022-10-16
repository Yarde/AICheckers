namespace Code.Utils
{
    public static class Extensions
    {
        public static bool IsMine(this Pawn pawn, bool isWhiteTurn)
        {
            return pawn.IsWhite == isWhiteTurn;
        }
    }
}