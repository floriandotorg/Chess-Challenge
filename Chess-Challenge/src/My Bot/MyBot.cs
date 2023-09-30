using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessChallenge.Example
{
    class Node
    {
        Node Parent;
        short N;
        bool IsExpanded = false;
        readonly bool IsDummy;
        float[] ChildValues = new float[218];
        int[] ChildVisits = new int[218];
        Move[] ChildMoves;
        Dictionary<Move, Node> Children = new();

        public static Board RootBoard;
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

            return score / 39f;

            // var winchance = MathF.Max(MathF.Min(score / 6f, 1f), -1f);

            // var rand = (float)Random.NextDouble();

            // if (winchance >= rand)
            //     return 1;
            // if (winchance <= -rand)
            //     return -1;

            // return 0;
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

        private static Queue<Node> _availableNodes = new Queue<Node>();

        public static Node GetNode(Node parent, short n)
        {
            if (_availableNodes.Count > 0)
            {
                Node node = _availableNodes.Dequeue();
                node.Parent = parent;
                node.N = n;
                node.init();
                return node;
            }

            return new Node(parent, n);
        }

        public static void ReturnNode(Node node)
        {
            _availableNodes.Enqueue(node);
        }

        public Node()
        {
            IsDummy = true;
            ChildMoves = new Move[1];
        }

        public Node(Node parent, short n)
        {
            Parent = parent;
            N = n;
            init();
        }

        private void init()
        {
            IsExpanded = false;
            Children = new();
            ChildMoves = BorrowBoard().GetLegalMoves();
            Array.Clear(ChildValues, 0, ChildValues.Length);
            Array.Clear(ChildVisits, 0, ChildVisits.Length);
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

        short BestChild()
        {
            short bestIndex = -1;
            float bestScore = float.MinValue;
            float precalcLog = MathF.Sqrt(MathF.Log(Visits));
            for (short i = 0; i < ChildMoves.Length; ++i)
            {
                float score = ChildValues[i] / (ChildVisits[i] + 1e-10f) + precalcLog / (ChildVisits[i] + 1e-10f);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestIndex = i;
                }
            }
            return bestIndex;
        }

        public Move BestMove() => ChildMoves[Array.IndexOf(ChildVisits, ChildVisits.Take(ChildMoves.Length).Max())];

        public Node Select()
        {
            Node current = this;
            while (current.IsExpanded && current.ChildMoves.Length > 0)
            {
                var bestChildN = current.BestChild();
                var bestChildMove = current.ChildMoves[bestChildN];
                if (!current.Children.ContainsKey(bestChildMove))
                {
                    current.Children[bestChildMove] = Node.GetNode(current, bestChildN);
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
                var n = (short)Node.Random.Next(ChildMoves.Length);
                var childMove = ChildMoves[n];
                var child = Node.GetNode(this, n);
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

        private void ReturnNodeAndChildren(Node except)
        {
            foreach (var child in Children.Values)
                if (child != except)
                    child.ReturnNodeAndChildren(except);
            Node.ReturnNode(this);
        }

        static public Node GetNewRoot(Node? root, Board board)
        {
            Node.IsWhiteToMove = board.IsWhiteToMove;
            Node.RootBoard = board;

            if (root != null)
                try
                {
                    var newRoot = root.Children[board.GameMoveHistory[^2]].Children[board.GameMoveHistory[^1]];
                    root.ReturnNodeAndChildren(newRoot);
                    root = newRoot;
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

            if (board.PlyCount == 0 && board.IsWhiteToMove)
                return new Move("d2d4", board);

            root = Node.GetNewRoot(root, board);

            var iter = 250_000;

            var stopwatch = new System.Diagnostics.Stopwatch(); // #DEBUG
            stopwatch.Start(); // #DEBUG

            Move? prevBest = null;
            int prevVisits = 0;
            for (var i = 0; i < iter; ++i)
            {
                root.Select().ExpandAndPropagate();
                if (i % 1000 == 0)
                {
                    if (prevBest == root.BestMove())
                    {
                        if (++prevVisits > (timer.MillisecondsRemaining / 60_000f) * 50)
                        {
                            Console.WriteLine($"Stopped after {i} iterations");
                            break;
                        }
                    }
                    else
                    {
                        prevBest = root.BestMove();
                        prevVisits = 0;
                    }
                }
            }

            stopwatch.Stop(); // #DEBUG
            Console.WriteLine($"Avg. time: {stopwatch.ElapsedMilliseconds} ms"); // #DEBUG

            long memoryBytes = System.Diagnostics.Process.GetCurrentProcess().WorkingSet64; // #DEBUG
            Console.WriteLine($"Memory: {memoryBytes / 1024 / 1024} MB"); // #DEBUG

            return root.BestMove();
        }
    }
}
