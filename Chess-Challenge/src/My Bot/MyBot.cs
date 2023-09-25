using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessChallenge.Example
{
    class Node
    {
        public static int PieceIndex(PieceType type) => ((int)type) - 1;

        public static readonly int WHITE = 0;
        public static readonly int BLACK = 1;

        public static readonly int WHITE_PAWN = (2 * PieceIndex(PieceType.Pawn) + WHITE);
        // public static readonly int BLACK_PAWN = (2 * PAWN + BLACK);
        // public static readonly int WHITE_KNIGHT = (2 * KNIGHT + WHITE);
        // public static readonly int BLACK_KNIGHT = (2 * KNIGHT + BLACK);
        // public static readonly int WHITE_BISHOP = (2 * BISHOP + WHITE);
        // public static readonly int BLACK_BISHOP = (2 * BISHOP + BLACK);
        // public static readonly int WHITE_ROOK = (2 * ROOK + WHITE);
        // public static readonly int BLACK_ROOK = (2 * ROOK + BLACK);
        // public static readonly int WHITE_QUEEN = (2 * QUEEN + WHITE);
        // public static readonly int BLACK_QUEEN = (2 * QUEEN + BLACK);
        // public static readonly int WHITE_KING = (2 * KING + WHITE);
        public static readonly int BLACK_KING = (2 * PieceIndex(PieceType.King) + BLACK);
        public static readonly int EMPTY = (BLACK_KING + 1);

        public static readonly int[] mg_value = { 82, 337, 365, 477, 1025, 0 };
        public static readonly int[] eg_value = { 94, 281, 297, 512, 936, 0 };

        public static readonly int[] mg_pawn_table = {  0,   0,   0,   0,   0,   0,  0,   0,
     98, 134,  61,  95,  68, 126, 34, -11,
     -6,   7,  26,  31,  65,  56, 25, -20,
    -14,  13,   6,  21,  23,  12, 17, -23,
    -27,  -2,  -5,  12,  17,   6, 10, -25,
    -26,  -4,  -4, -10,   3,   3, 33, -12,
    -35,  -1, -20, -23, -15,  24, 38, -22,
      0,   0,   0,   0,   0,   0,  0,   0, };
        public static readonly int[] eg_pawn_table = {  0,   0,   0,   0,   0,   0,   0,   0,
    178, 173, 158, 134, 147, 132, 165, 187,
     94, 100,  85,  67,  56,  53,  82,  84,
     32,  24,  13,   5,  -2,   4,  17,  17,
     13,   9,  -3,  -7,  -7,  -8,   3,  -1,
      4,   7,  -6,   1,   0,  -5,  -1,  -8,
     13,   8,   8,  10,  13,   0,   2,  -7,
      0,   0,   0,   0,   0,   0,   0,   0, };
        public static readonly int[] mg_knight_table = { -167, -89, -34, -49,  61, -97, -15, -107,
     -73, -41,  72,  36,  23,  62,   7,  -17,
     -47,  60,  37,  65,  84, 129,  73,   44,
      -9,  17,  19,  53,  37,  69,  18,   22,
     -13,   4,  16,  13,  28,  19,  21,   -8,
     -23,  -9,  12,  10,  19,  17,  25,  -16,
     -29, -53, -12,  -3,  -1,  18, -14,  -19,
    -105, -21, -58, -33, -17, -28, -19,  -23, };
        public static readonly int[] eg_knight_table = { -58, -38, -13, -28, -31, -27, -63, -99,
    -25,  -8, -25,  -2,  -9, -25, -24, -52,
    -24, -20,  10,   9,  -1,  -9, -19, -41,
    -17,   3,  22,  22,  22,  11,   8, -18,
    -18,  -6,  16,  25,  16,  17,   4, -18,
    -23,  -3,  -1,  15,  10,  -3, -20, -22,
    -42, -20, -10,  -5,  -2, -20, -23, -44,
    -29, -51, -23, -15, -22, -18, -50, -64, };
        public static readonly int[] mg_bishop_table = { -29,   4, -82, -37, -25, -42,   7,  -8,
    -26,  16, -18, -13,  30,  59,  18, -47,
    -16,  37,  43,  40,  35,  50,  37,  -2,
     -4,   5,  19,  50,  37,  37,   7,  -2,
     -6,  13,  13,  26,  34,  12,  10,   4,
      0,  15,  15,  15,  14,  27,  18,  10,
      4,  15,  16,   0,   7,  21,  33,   1,
    -33,  -3, -14, -21, -13, -12, -39, -21,};
        public static readonly int[] eg_bishop_table = { -14, -21, -11,  -8, -7,  -9, -17, -24,
     -8,  -4,   7, -12, -3, -13,  -4, -14,
      2,  -8,   0,  -1, -2,   6,   0,   4,
     -3,   9,  12,   9, 14,  10,   3,   2,
     -6,   3,  13,  19,  7,  10,  -3,  -9,
    -12,  -3,   8,  10, 13,   3,  -7, -15,
    -14, -18,  -7,  -1,  4,  -9, -15, -27,
    -23,  -9, -23,  -5, -9, -16,  -5, -17,};
        public static readonly int[] mg_rook_table = {  32,  42,  32,  51, 63,  9,  31,  43,
     27,  32,  58,  62, 80, 67,  26,  44,
     -5,  19,  26,  36, 17, 45,  61,  16,
    -24, -11,   7,  26, 24, 35,  -8, -20,
    -36, -26, -12,  -1,  9, -7,   6, -23,
    -45, -25, -16, -17,  3,  0,  -5, -33,
    -44, -16, -20,  -9, -1, 11,  -6, -71,
    -19, -13,   1,  17, 16,  7, -37, -26, };
        public static readonly int[] eg_rook_table = { 13, 10, 18, 15, 12,  12,   8,   5,
    11, 13, 13, 11, -3,   3,   8,   3,
     7,  7,  7,  5,  4,  -3,  -5,  -3,
     4,  3, 13,  1,  2,   1,  -1,   2,
     3,  5,  8,  4, -5,  -6,  -8, -11,
    -4,  0, -5, -1, -7, -12,  -8, -16,
    -6, -6,  0,  2, -9,  -9, -11,  -3,
    -9,  2,  3, -1, -5, -13,   4, -20, };
        public static readonly int[] mg_queen_table = { -28,   0,  29,  12,  59,  44,  43,  45,
    -24, -39,  -5,   1, -16,  57,  28,  54,
    -13, -17,   7,   8,  29,  56,  47,  57,
    -27, -27, -16, -16,  -1,  17,  -2,   1,
     -9, -26,  -9, -10,  -2,  -4,   3,  -3,
    -14,   2, -11,  -2,  -5,   2,  14,   5,
    -35,  -8,  11,   2,   8,  15,  -3,   1,
     -1, -18,  -9,  10, -15, -25, -31, -50, };
        public static readonly int[] eg_queen_table = { -9,  22,  22,  27,  27,  19,  10,  20,
    -17,  20,  32,  41,  58,  25,  30,   0,
    -20,   6,   9,  49,  47,  35,  19,   9,
      3,  22,  24,  45,  57,  40,  57,  36,
    -18,  28,  19,  47,  31,  34,  39,  23,
    -16, -27,  15,   6,   9,  17,  10,   5,
    -22, -23, -30, -16, -16, -23, -36, -32,
    -33, -28, -22, -43,  -5, -32, -20, -41, };
        public static readonly int[] mg_king_table = { -65,  23,  16, -15, -56, -34,   2,  13,
     29,  -1, -20,  -7,  -8,  -4, -38, -29,
     -9,  24,   2, -16, -20,   6,  22, -22,
    -17, -20, -12, -27, -30, -25, -14, -36,
    -49,  -1, -27, -39, -46, -44, -33, -51,
    -14, -14, -22, -46, -44, -30, -15, -27,
      1,   7,  -8, -64, -43, -16,   9,   8,
    -15,  36,  12, -54,   8, -28,  24,  14,
        };
        public static readonly int[] eg_king_table = { -74, -35, -18, -18, -11,  15,   4, -17,
    -12,  17,  14,  17,  17,  38,  23,  11,
     10,  17,  23,  15,  20,  45,  44,  13,
     -8,  22,  24,  27,  26,  33,  26,   3,
    -18,  -4,  21,  24,  27,  23,   9, -11,
    -19,  -3,  11,  21,  23,  16,   7,  -9,
    -27, -11,   4,  13,  14,   4,  -5, -17,
    -53, -34, -21, -11, -28, -14, -24, -43};

        public static readonly int[][] mg_pesto_table = { mg_pawn_table, mg_knight_table, mg_bishop_table, mg_rook_table, mg_queen_table, mg_king_table
    };
        public static readonly int[][] eg_pesto_table = { eg_pawn_table, eg_knight_table, eg_bishop_table, eg_rook_table, eg_queen_table, eg_king_table
};

        public static readonly int[] gamephaseInc = { 0, 0, 1, 1, 1, 1, 2, 2, 4, 4, 0, 0 };

        public static int[,] mg_table = new int[12, 64];
        public static int[,] eg_table = new int[12, 64];

        public int[] board = new int[64];

        public static int Flip(int sq) => sq ^ 56;
        public static int Other(int side) => side ^ 1;
        public static int PColor(int p) => p & 1;

        static Node()
        {
            int pc, p, sq;
            for (p = PieceIndex(PieceType.Pawn), pc = WHITE_PAWN; p <= PieceIndex(PieceType.King); pc += 2, p++)
            {
                for (sq = 0; sq < 64; sq++)
                {
                    mg_table[pc, sq] = mg_value[p] + mg_pesto_table[p][sq];
                    eg_table[pc, sq] = eg_value[p] + eg_pesto_table[p][sq];
                    mg_table[pc + 1, sq] = mg_value[p] + mg_pesto_table[p][Flip(sq)];
                    eg_table[pc + 1, sq] = eg_value[p] + eg_pesto_table[p][Flip(sq)];
                }
            }
        }

        public static float EvalBoard(Board board)
        {
            if (board.IsInCheckmate())
                return board.IsWhiteToMove == Node.IsWhiteToMove ? -1 : 1;
            if (board.IsDraw())
                return 0;

            Span<int> mg = stackalloc int[2];
            Span<int> eg = stackalloc int[2];
            int gamePhase = 0;
            int side2move = board.IsWhiteToMove ? WHITE : BLACK;

            mg[WHITE] = 0;
            mg[BLACK] = 0;
            eg[WHITE] = 0;
            eg[BLACK] = 0;

            for (int sq = 0; sq < 64; ++sq)
            {
                var piece = board.GetPiece(new Square(sq));
                int pc = EMPTY;
                if (!piece.IsNull)
                    pc = PieceIndex(piece.PieceType) * 2 + (piece.IsWhite ? WHITE : BLACK);
                if (pc != EMPTY)
                {
                    mg[PColor(pc)] += mg_table[pc, sq];
                    eg[PColor(pc)] += eg_table[pc, sq];
                    gamePhase += gamephaseInc[pc];
                }
            }

            int mgScore = mg[side2move] - mg[Other(side2move)];
            int egScore = eg[side2move] - eg[Other(side2move)];
            int egPhase = 24 - Math.Min(gamePhase, 24);
            return (mgScore * gamePhase + egScore * egPhase) / (24 * 2000f) * (IsWhiteToMove == board.IsWhiteToMove ? 1 : -1);
        }

        Node Parent;
        int N;
        bool IsExpanded = false;
        protected float[] ChildValues;
        protected int[] ChildVisits;
        protected Move[] ChildMoves;
        public static Board RootBoard;
        Dictionary<Move, Node> Children = new();
        public static bool IsWhiteToMove;
        static Random Random = new Random();
        // static readonly Dictionary<PieceType, float> PieceValues = new()
        // {
        //     { PieceType.Pawn, 1 },
        //     { PieceType.Knight, 3 },
        //     { PieceType.Bishop, 3 },
        //     { PieceType.Rook, 5 },
        //     { PieceType.Queen, 9 },
        //     { PieceType.King, 0 },
        // };

        virtual protected bool IsDummy => false;

        // static float EvalBoard(Board board)
        // {
        //     if (board.IsInCheckmate())
        //         return board.IsWhiteToMove == Node.IsWhiteToMove ? -1 : 1;
        //     if (board.IsDraw())
        //         return 0;

        //     float score = 0;
        //     foreach (var pieceList in board.GetAllPieceLists())
        //         foreach (var piece in pieceList)
        //             score += PieceValues[piece.PieceType] * (piece.IsWhite == Node.IsWhiteToMove ? 1 : -1);

        //     Console.WriteLine($"Score: {score}");

        //     return score / 39;
        // }

        virtual protected Board BorrowBoard()
        {
            var board = Parent.BorrowBoard();
            board.MakeMove(Move);
            return board;
        }

        virtual protected void ReturnBoard()
        {
            Node.RootBoard.UndoMove(Move);
            Parent.ReturnBoard();
        }

        protected Node()
        { }

        public Node(Node parent, int n)
        {
            Parent = parent;
            N = n;
            ChildMoves = BorrowBoard().GetLegalMoves();
            ChildValues = new float[ChildMoves.Length];
            ChildVisits = new int[ChildMoves.Length];
            ReturnBoard();
        }

        public Move Move => Parent.ChildMoves[N];

        float Value
        {
            get => Parent.ChildValues[N];
            set => Parent.ChildValues[N] = value;
        }

        public int Visits
        {
            get => Parent.ChildVisits[N];
            set => Parent.ChildVisits[N] = value;
        }

        int BestChild()
        {
            int bestIndex = -1;
            float bestScore = float.MinValue;
            for (var i = 0; i < ChildMoves.Length; ++i)
            {
                float score = ChildValues[i] / (ChildVisits[i] + 1e-10f)
                    + 1f * MathF.Sqrt(MathF.Log(Visits) / (ChildVisits[i] + 1e-10f));
                if (score > bestScore)
                {
                    bestScore = score;
                    bestIndex = i;
                }
            }
            return bestIndex;
        }

        public Move BestMove() => ChildMoves[Array.IndexOf(ChildVisits, ChildVisits.Max())];

        public Node Select()
        {
            Node current = this;
            while (current.IsExpanded && current.ChildMoves.Length > 0)
            {
                var bestChildN = current.BestChild();
                var bestChildMove = current.ChildMoves[bestChildN];
                if (!current.Children.ContainsKey(bestChildMove))
                {
                    current.Children[bestChildMove] = new Node(current, bestChildN);
                }
                current = current.Children[bestChildMove];
            }
            return current;
        }

        public void ExpandAndPropagate()
        {
            IsExpanded = true;
            float result = 0;

            if (ChildMoves.Length == 0)
            {
                result = EvalBoard(BorrowBoard());
                ReturnBoard();
                // Console.WriteLine($"Result: {result}");
            }
            else
            {
                var n = Node.Random.Next(ChildMoves.Length);
                var childMove = ChildMoves[n];
                var child = new Node(this, n);
                Children[childMove] = child;
                result = EvalBoard(child.BorrowBoard());
                child.ReturnBoard();
                // Console.WriteLine($"Move: {childMove}, Result: {result}");
                child.Value = result;
            }

            var current = this;
            while (!current.IsDummy)
            {
                current.Visits += 1;
                current.Value += result;
                current = current.Parent;
            }
        }

        static public Node GetNewRoot(Node? root, Board board)
        {
            Node.IsWhiteToMove = board.IsWhiteToMove;
            Node.RootBoard = board;

            if (root != null)
                try
                {
                    root = root.Children[board.GameMoveHistory[^2]].Children[board.GameMoveHistory[^1]];
                    var visits = root.Visits;
                    Console.WriteLine($"Reusing {root.Visits} visits");
                    root.Parent = new DummyNode();
                    root.N = 0;
                    root.Visits = visits;
                    return root;
                }
                catch (KeyNotFoundException) { }

            return new Node(new DummyNode(), 0);
        }

        public void PrintTree(int depth = 1, int pad = 0)
        {
            if (depth == 0)
                return;

            Console.WriteLine($"{new String(' ', pad)}Move: {Move}, Value: {Value}, Visits: {Visits}, Avg: {Value / Visits}");
            foreach (var child in Children.Values)
                child.PrintTree(depth - 1, pad + 2);
        }
    }

    class DummyNode : Node
    {
        public DummyNode() : base()
        {
            ChildValues = new float[1];
            ChildVisits = new int[1];
            ChildMoves = new Move[1];
        }

        override protected Board BorrowBoard() => Node.RootBoard;

        override protected void ReturnBoard()
        { }

        override protected bool IsDummy => true;
    }

    public class MyBot : IChessBot
    {
        Node? root;

        public Move Think(Board board, Timer timer)
        {
            if (board.GetLegalMoves().Length == 1)
                return board.GetLegalMoves()[0];

            float move = board.PlyCount / 2;

            if (move == 0 && board.IsWhiteToMove)
                return new Move("d2d4", board);

            root = Node.GetNewRoot(root, board);

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            float mu = 18.61447f;
            var iter = 200_000f * MathF.Max(1.015729f * MathF.Exp(-(move - mu) * (move - mu) / (2 * MathF.Pow(7.229496f, 2))), 0.3f);
            Console.WriteLine($"Iterations: {iter}, Move: {board.PlyCount / 2}");

            for (var i = 0; i < iter; ++i)
                root.Select().ExpandAndPropagate();

            // var iter = 50;

            // for (var i = 0; i < iter; ++i)
            // {
            //     root.Select().ExpandAndPropagate();
            //     root.PrintTree(3);
            //     Console.WriteLine("------\n");
            // }


            root.PrintTree(2);

            Console.WriteLine($"Time: {stopwatch.ElapsedMilliseconds}ms");

            return root.BestMove();

            // var white_minus_10 = "4k2r/1pp1b2p/r1n2np1/3q1p2/3p4/P4N1P/1PPP1P1P/R1B2RK1 w k - 2 15";
            // var white_plus_7 = "rnb1kbnr/ppp2ppp/8/8/3pQ3/5N2/PPPP1PPP/RNB1KB1R b KQkq - 0 7";
            // var nil = "rnbqk2r/pp2ppbp/2p2np1/3p4/2PP4/4PN2/PP2BPPP/RNBQK2R w KQkq - 2 6";
            // var white_lost = "rnb1kbnr/pppp1ppp/4p3/8/6Pq/5P2/PPPPP2P/RNBQKBNR w KQkq - 1 3";

            // Node.IsWhiteToMove = true;
            // Console.WriteLine(Node.EvalBoard(Board.CreateBoardFromFEN(white_minus_10)));
            // Console.WriteLine(Node.EvalBoard(Board.CreateBoardFromFEN(white_plus_7)));
            // Console.WriteLine(Node.EvalBoard(Board.CreateBoardFromFEN(nil)));
            // Console.WriteLine(Node.EvalBoard(Board.CreateBoardFromFEN(white_lost)));

            // return board.GetLegalMoves()[0];
        }
    }
}
