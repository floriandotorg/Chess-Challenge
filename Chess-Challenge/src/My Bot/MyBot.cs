using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessChallenge.Example
{
    class Node
    {
        Node Parent;
        int N;
        bool IsExpanded = false;
        readonly bool IsDummy;
        readonly float[] ChildValues;
        readonly int[] ChildVisits;
        readonly Move[] ChildMoves;
        public static Board RootBoard;
        Dictionary<Move, Node> Children = new();
        public static bool IsWhiteToMove;
        static Random Random = new Random();
        static readonly Dictionary<PieceType, float> PieceValues = new()
        {
            { PieceType.Pawn, 1 },
            { PieceType.Knight, 3 },
            { PieceType.Bishop, 3 },
            { PieceType.Rook, 5 },
            { PieceType.Queen, 9 },
            { PieceType.King, 0 },
        };

        static float EvalBoard(Board board)
        {
            if (board.IsInCheckmate())
                return board.IsWhiteToMove == Node.IsWhiteToMove ? -1 : 1;
            if (board.IsDraw())
                return 0;

            float score = 0;
            foreach (var pieceList in board.GetAllPieceLists())
                foreach (var piece in pieceList)
                    score += PieceValues[piece.PieceType] * (piece.IsWhite == Node.IsWhiteToMove ? 1 : -1);

            return score / 39;
        }

        Board BorrowBoard()
        {
            if (IsDummy) return Node.RootBoard;

            var board = Parent.BorrowBoard();
            board.MakeMove(Move);
            return board;
        }

        void ReturnBoard()
        {
            if (IsDummy) return;
            Node.RootBoard.UndoMove(Move);
            Parent.ReturnBoard();
        }

        public Node()
        {
            IsDummy = true;
            ChildValues = new float[1];
            ChildVisits = new int[1];
            ChildMoves = new Move[1];
        }

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
            }
            else
            {
                var n = Node.Random.Next(ChildMoves.Length);
                var childMove = ChildMoves[n];
                var child = new Node(this, n);
                Children[childMove] = child;
                result = EvalBoard(child.BorrowBoard());
                child.ReturnBoard();
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
                    root.Parent = new Node();
                    root.N = 0;
                    root.Visits = visits;
                    return root;
                }
                catch (KeyNotFoundException) { }

            return new Node(new Node(), 0);
        }
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

            float mu = 18.61447f;
            var iter = 100_000f + 50_000f * MathF.Max(1.015729f * MathF.Exp(-(move - mu) * (move - mu) / (2 * MathF.Pow(7.229496f, 2))), 0.3f);
            Console.WriteLine($"Iterations: {iter}, Move: {board.PlyCount / 2}");

            for (var i = 0; i < iter; ++i)
                root.Select().ExpandAndPropagate();

            return root.BestMove();
        }
    }
}
