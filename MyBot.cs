using System;
using System.Collections.Generic;
using System.Linq;
using ChessChallenge.API;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static System.Math;

public class MyBot : IChessBot
{
    // Piece values: null, pawn, knight, bishop, rook, queen, king
    int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };

    // Current best move
    Move bestMove;

    int[,] pieceSquareTable =
    {
        // Pawn
        { 0, 0, 0, 0, 0, 0, 0, 0,
          50, 50, 50, 50, 50, 50, 50, 50,
          10, 10, 20, 30, 30, 20, 10, 10,
          5, 5, 10, 25, 25, 10, 5, 5,
          0, 0, 0, 20, 20, 0, 0, 0,
          5, -5, -10, 0, 0, -10, -5, 5,
          5, 10, 10, -20, -20, 10, 10, 5,
          0, 0, 0, 0, 0, 0, 0, 0 },

        // Knight
        { -50, -40, -30, -30, -30, -30, -40, -50,
          -40, -20, 0, 0, 0, 0, -20, -40,
          -30, 0, 10, 15, 15, 10, 0, -30,
          -30, 5, 15, 20, 20, 15, 5, -30,
          -30, 0, 15, 20, 20, 15, 0, -30,
          -30, 5, 10, 15, 15, 10, 5, -30,
          -40, -20, 0, 5, 5, 0, -20, -40,
          -50, -40, -30, -30, -30, -30, -40, -50 },

        // Bishop
        { -20, -10, -10, -10, -10, -10, -10, -20,
          -10, 0, 0, 0, 0, 0, 0, -10,
          -10, 0, 5, 10, 10, 5, 0, -10,
          -10, 5, 5, 10, 10, 5, 5, -10,
          -10, 0, 10, 10, 10, 10, 0, -10,
          -10, 10, 10, 10, 10, 10, 10, -10,
          -10, 5, 0, 0, 0, 0, 5, -10,
          -20, -10, -10, -10, -10, -10, -10, -20 },

        // Rook
        { 0, 0, 0, 5, 5, 0, 0, 0,
          -5, 0, 0, 0, 0, 0, 0, -5,
          -5, 0, 0, 0, 0, 0, 0, -5,
          -5, 0, 0, 0, 0, 0, 0, -5,
          -5, 0, 0, 0, 0, 0, 0, -5,
          -5, 0, 0, 0, 0, 0, 0, -5,
          5, 10, 10, 10, 10, 10, 10, 5,
          0, 0, 0, 0, 0, 0, 0, 0 },

        // Queen
        { -20, -10, -10, -5, -5, -10, -10, -20,
          -10, 0, 0, 0, 0, 0, 0, -10,
          -10, 0, 5, 5, 5, 5, 0, -10,
          -5, 0, 5, 5, 5, 5, 0, -5,
          0, 0, 5, 5, 5, 5, 0, -5,
          -10, 5, 5, 5, 5, 5, 0, -10,
          -10, 0, 5, 0, 0, 0, 0, -10,
          -20, -10, -10, -5, -5, -10, -10, -20 },

        // King
        { 20, 30, 10, 0, 0, 10, 30, 20,
          20, 20, 0, 0, 0, 0, 20, 20,
          -10, -20, -20, -20, -20, -20, -20, -10,
          -20, -30, -30, -40, -40, -30, -30, -20,
          -30, -40, -40, -50, -50, -40, -40, -30,
          -30, -40, -40, -50, -50, -40, -40, -30,
          -30, -40, -40, -50, -50, -40, -40, -30,
          -30, -40, -40, -50, -50, -40, -40, -30 }
    };
    public Move Think(Board board, Timer timer)
    {
        Move[] allMoves = board.GetLegalMoves();

        // Order moves based on static evaluation
        Array.Sort(allMoves, (move1, move2) =>
        {
            int eval1 = EvaluateMove(board, move1);
            int eval2 = EvaluateMove(board, move2);
            return eval2.CompareTo(eval1);
        });

        // Pick a random move to play if nothing better is found
        Random rng = new();
        Move moveToPlay = allMoves[rng.Next(allMoves.Length)];
        int bestEvaluation = 0;

        foreach (Move move in allMoves)
        {
            // Always play checkmate in one
            if (MoveIsCheckmate(board, move))
            {
                moveToPlay = move;
                break;
            }

            // Find highest value capture
            /*
            
            Piece capturedPiece = board.GetPiece(move.TargetSquare);
            int capturedPieceValue = pieceValues[(int)capturedPiece.PieceType];

            if (capturedPieceValue > bestEvaluation)
            {
                moveToPlay = move;
                bestEvaluation = capturedPieceValue;
            }

            int cCheck = coveredCheck(board, move) ? 1 : 0;            
            int cCapture = coveredCapture(board, move) ? 1 : 0;
            Console.WriteLine(cCheck + " ; " + cCapture);
            
             */

            // Evaluate the position using minimax with alpha-beta pruning
            int evaluation = MinimaxWithPruning(board, 2, int.MinValue, int.MaxValue, false);

            if (evaluation > bestEvaluation)
            {
                moveToPlay = move;
                bestEvaluation = evaluation;
            }
        }

        return moveToPlay;
    }

    bool MoveIsCheckmate(Board board, Move move)
    {
        board.MakeMove(move);
        bool isMate = board.IsInCheckmate();
        board.UndoMove(move);
        return isMate;
    }

    int EvaluatePosition(Board board)
    {
        int totalEvaluation = 0;

        for (int file = 0; ++file < 8;)
        {
            for (int rank = 0; ++rank < 8;)
            {
                Square square = new Square(file, rank);
                Piece piece = board.GetPiece(square);

                if (!piece.IsNull)
                {
                    int pieceValue = pieceValues[(int)piece.PieceType];
                    int pieceSquareValue = pieceSquareTable[((int)piece.PieceType - 1), square.Index];

                    totalEvaluation += pieceValue + pieceSquareValue;
                }
            }
        }

        return totalEvaluation;
    }

    int EvaluateMove(Board board, Move move)
    {
        board.MakeMove(move);
        int eval = EvaluatePosition(board);
        board.UndoMove(move);
        return eval;
    }

    int MinimaxWithPruning(Board board, int depth, int alpha, int beta, bool maximizingPlayer)
    {
        if (depth == 0 || board.IsInCheckmate() || board.IsInStalemate() || board.IsDraw())
        {
            return EvaluatePosition(board);
        }

        Move[] legalMoves = board.GetLegalMoves();

        if (maximizingPlayer)
        {
            int maxEval = int.MinValue;

            foreach (Move move in legalMoves)
            {
                board.MakeMove(move);
                int eval = MinimaxWithPruning(board, depth - 1, alpha, beta, false);
                board.UndoMove(move);

                maxEval = Math.Max(maxEval, eval);
                alpha = Math.Max(alpha, eval);

                if (beta <= alpha)
                {
                    break;  // Beta cutoff
                }
            }

            return maxEval;
        }
        else
        {
            int minEval = int.MaxValue;

            foreach (Move move in legalMoves)
            {
                board.MakeMove(move);
                int eval = MinimaxWithPruning(board, depth - 1, alpha, beta, true);
                board.UndoMove(move);

                minEval = Math.Min(minEval, eval);
                beta = Math.Min(beta, eval);

                if (beta <= alpha)
                {
                    break;  // Alpha cutoff
                }
            }

            return minEval;
        }
    }
}
