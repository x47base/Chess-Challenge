using ChessChallenge.API;
using System.Data;
using static System.Math;

public class MyBot : IChessBot
{
    /* Piece Valuesin Binary:
     * Pawn    = 0b0001
     * Knight  = 0b0011
     * Bishop  = 0b0011
     * Rook    = 0b0101
     * Queen   = 0b1001
    */
    private const int PieceValues = 0b00010011001101011001;
    
    public static int Evaluate(Board board)
    {
        
        return 0;
    }

    public static int Search(int depth)
    {
        if (depth == 0)
        {
            return Evaluate();
        }

        int score = 0;

        return score;
    }

    public Move Think(Board board, Timer timer)
    {
        /*
        Move[] moves = board.GetLegalMoves();
        return moves[0];
        */

        Move[] moves = board.GetLegalMoves();


        Move bestMove = Move.NullMove;
        int bestScore = int.MinValue;

        foreach (Move move in moves)
        {
            Board tempBoard = Board.CreateBoardFromFEN(board.GetFenString());
            tempBoard.MakeMove(move);

            int score = EvaluatePosition(tempBoard);

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }

        return bestMove;
    }
}
