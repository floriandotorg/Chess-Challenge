using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessChallenge.Example
{
    public class Node
    {
        public Node[] Children { get; set; }
        private Node? Parent { get; }
        private int V { get; set; }
        private int N { get; set; }
        public Move? Move { get; }
        private bool IsFinished { get; }
        private Board RootBoard { get; }
        private List<Move> Moves { get; }
        private Move[] LegalMoves { get; }
        private static readonly Random rng = new();
        private static Dictionary<ulong, int> transpositionTable = new();

        public Node(Node? parent, Board rootBoard, Move? move)
        {
            Parent = parent;
            Children = Array.Empty<Node>();
            Move = move;

            RootBoard = rootBoard;

            Moves = new();
            Node? node = this;
            while (node.Move != null)
            {
                Moves.Add(node.Move ?? throw new Exception("Node has no move"));
                node = node.Parent;
            }

            foreach (var toMove in Moves.Reverse<Move>())
                rootBoard.MakeMove(toMove);
            IsFinished = rootBoard.IsInCheckmate() || rootBoard.IsDraw();
            LegalMoves = rootBoard.GetLegalMoves();
            foreach (var toMove in Moves)
                rootBoard.UndoMove(toMove);
        }

        public double Ucb1()
        {
            if (N == 0)
                return double.MaxValue;

            return (double)V / N + Math.Sqrt(2 * Math.Log(Parent.N) / N);
        }

        public Node Select()
        {
            var node = this;
            while (node.Children.Length > 0)
            {
                Node? bestChild = null;
                var bestUcb1 = double.MinValue;
                foreach (var child in node.Children)
                {
                    var ucb1 = child.Ucb1();
                    if (ucb1 > bestUcb1 && !child.IsFinished)
                    {
                        bestUcb1 = ucb1;
                        bestChild = child;
                    }
                }
                node = bestChild!;
            }
            return node;
        }

        public Node Expand()
        {
            Children = LegalMoves.Select(move => new Node(this, RootBoard, move)).ToArray();
            return Children[rng.Next(Children.Length)];
        }

        public void RolloutAndBackpropagate()
        {
            foreach (var toMove in Moves.Reverse<Move>())
                RootBoard.MakeMove(toMove);

            var key = RootBoard.ZobristKey;
            var result = transpositionTable.GetValueOrDefault(key, 42);
            if (result == 42) {
                List<Move> movesList = new();
                while (!RootBoard.IsInCheckmate() && !RootBoard.IsDraw())
                {
                    var moves = RootBoard.GetLegalMoves();
                    var move = moves.FirstOrDefault(move => move.IsCapture);
                    if (move.IsNull)
                        move = moves[rng.Next(moves.Length)];
                    movesList.Add(move);
                    RootBoard.MakeMove(move);
                }

                result = RootBoard.IsInCheckmate() ? (RootBoard.IsWhiteToMove ? -1 : 1) : 0;
                transpositionTable.Add(key, result);

                foreach (var toMove in movesList.Reverse<Move>())
                    RootBoard.UndoMove(toMove);
            }

            foreach (var toMove in Moves)
                RootBoard.UndoMove(toMove);

            Node? current = this;
            N = -1;
            while(current != null)
            {
                current.N += 1;
                current.V += result;
                current = current.Parent;
            }
        }

        public void PrintTree(int depth = 0)
        {
            Console.WriteLine($"{new string(' ', depth * 2)}{Move?.ToString() ?? "Root"}: {V}/{N}");
            foreach (var child in Children)
                child.PrintTree(depth + 1);
        }
    }

    public class MyBot : IChessBot
    {
        public Move Think(Board board, Timer timer)
        {
            var root = new Node(null, board, null);
            root.Expand();

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            for (var i = 0; i < 50000; ++i)
                root.Select().Expand().RolloutAndBackpropagate();

            Console.WriteLine($"Time: {stopwatch.ElapsedMilliseconds}ms");

            // root.PrintTree();

            Node? bestChild = null;
            var bestUcb1 = double.MinValue;
            foreach (var child in root.Children)
            {
                var ucb1 = child.Ucb1();
                if (ucb1 > bestUcb1)
                {
                    bestUcb1 = ucb1;
                    bestChild = child;
                }
            }

            return bestChild.Move ?? throw new Exception("No move found");
        }
    }
}
