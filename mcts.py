from typing import Optional
import chess
import pygraphviz as pgv
import numpy as np

PIECE_VALUES = {
    chess.PAWN: 1,
    chess.KNIGHT: 3,
    chess.BISHOP: 3,
    chess.ROOK: 5,
    chess.QUEEN: 9,
    chess.KING: 0,
}


def get_best_move(board: chess.Board) -> chess.Move:
    to_move = board.turn

    # def get_best_move(board: chess.Board, moves: list[chess.Move]) -> chess.Move:
    #     def score_move(move: chess.Move) -> float:
    #         if board.is_capture(move):
    #             if board.is_en_passant(move):
    #                 return 100

    #             return PIECE_VALUES[board.piece_at(move.to_square).piece_type] * 100

    #         return random.random()

    #     return max(moves, key=score_move)

    def eval_board_fast(board: chess.Board) -> float:
        if board.is_checkmate():
            return 1 if board.turn == to_move else -1
        if board.is_game_over():
            return 0

        score = 0
        for piece in board.piece_map().values():
            score += PIECE_VALUES[piece.piece_type] * (1 if piece.color == to_move else -1)

        return score / 39

    def eval_board(board: chess.Board) -> float:
        return (eval_board_fast(board) / 39) * 0.7 + ((len(list(board.legal_moves)) * (1 if board.turn == to_move else -1)) / 219) * 0.3  # + (random.random()) * 0.2

    def eval_move(board: chess.Board, move: chess.Move) -> float:
        board.push(move)
        score = eval_board_fast(board)
        board.pop()
        return score

    class Node:
        def __init__(self, board: chess.Board, parent: Optional['Node'], n: int):
            self.n = n
            self.children = {}
            self.is_expanded = False
            self.board = board
            self.parent = parent
            self.child_moves = list(self.board.legal_moves)
            self.child_values = np.zeros(len(self.child_moves), dtype=np.float32)
            self.child_visits = np.zeros(len(self.child_moves), dtype=np.float32)
            self.child_evaluations = np.array([eval_move(self.board, move) for move in self.child_moves], dtype=np.float32)

        @property
        def move(self) -> chess.Move:
            return self.parent.child_moves[self.n]

        @property
        def value(self) -> float:
            return self.parent.child_values[self.n]

        @value.setter
        def value(self, value):
            self.parent.child_values[self.n] = value

        @property
        def visits(self) -> float:
            return self.parent.child_visits[self.n]

        @visits.setter
        def visits(self, visits):
            self.parent.child_visits[self.n] = visits

        def best_child(self) -> np.int64:
            return (
                np.argmax(
                    self.child_values / (self.child_visits + 1e-10)
                    + np.sqrt(2) * np.sqrt(np.log(self.visits) / (self.child_visits + 1e-10))
                    + self.child_evaluations / (self.child_visits + 1)
                )
            )

        def select(self) -> 'Node':
            current = self
            while current.is_expanded and len(current.child_moves) > 0:
                best_child_n = current.best_child()
                best_child_move = current.child_moves[best_child_n]
                if best_child_move not in current.children:
                    board = current.board.copy()
                    board.push(best_child_move)
                    current.children[best_child_move] = Node(board, current, best_child_n)
                current = current.children[best_child_move]

            return current

        def expand_rollout_and_propagate(self) -> None:
            self.is_expanded = True

            if len(self.child_moves) == 0:
                result = eval_board(self.board)
            else:
                child_n = np.random.randint(len(self.child_moves))
                self.board.push(self.child_moves[child_n])
                # while not board.is_game_over():
                #     board.push(get_best_move(board, list(board.legal_moves)))

                # result = 1 if board.outcome().winner == to_move else (0 if board.outcome().winner == None else -1)
                result = eval_board(self.board)
                self.board.pop()
                self.child_values[child_n] = result

            current = self
            while hasattr(current, 'parent'):
                current.visits += 1
                current.value += result  # * (1 if current.board.turn == to_move else -1)
                current = current.parent

        def get_tree_plot(self, max_depth=3) -> pgv.AGraph:
            graph = pgv.AGraph(directed=True, strict=True)
            graph.graph_attr['ranksep'] = 3
            graph.add_node(self.uci_history, label=f'{self.visits:.0f}/{self.value:.1f}')
            self.add_children_to_graph(graph, max_depth)
            graph.layout(prog='dot')
            return graph

        def add_children_to_graph(self, graph: pgv.AGraph, depth: int) -> None:
            if depth == 0:
                return
            depth -= 1

            for n, move in enumerate(self.child_moves):
                uci = f'{self.uci_history} {move.uci()}'
                graph.add_node(uci, label=f'{self.child_visits[n]:.0f}/{self.child_values[n]:.1f}')
                graph.add_edge(self.uci_history, uci, label=self.board.san(move))
                if move in self.children:
                    self.children[move].add_children_to_graph(graph, depth)

        @property
        def uci_history(self):
            uci = ' '.join([move.uci() for move in self.board.move_stack])
            if len(uci) == 0:
                return 'startpos'
            return f'startpos {uci}'

    class DummyParentNode:
        def __init__(self):
            self.child_visits = np.zeros(1, dtype=np.float32)
            self.child_values = np.zeros(1, dtype=np.float32)

    root = Node(board, DummyParentNode(), 0)

    for _ in range(10_000):
        root.select().expand_rollout_and_propagate()
        # display(root.get_tree_plot())

    # display(root.get_tree_plot(max_depth=1))
    # eval_board(chess.Board('4Bn1k/P1Q5/5K2/8/3P2p1/8/7p/6NN w - - 0 7'))
    # eval_board(chess.Board('4Bn1k/P1Q5/5K2/6p1/3P4/8/7p/6NN b - - 0 6'))

    return root.child_moves[root.best_child()]
