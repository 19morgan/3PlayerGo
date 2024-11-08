using System;
using System.Collections.Generic;

public class GoGame
{
    private const int BoardSize = 9;
    private char[,] board = new char[BoardSize, BoardSize];
    private List<char> players = new List<char> { 'X', 'O', 'Y' }; // 3 players
    private Random rand = new Random();
    private int currentPlayerIndex;
    private bool gameRunning;
    private Dictionary<char, int> scores = new Dictionary<char, int>();

    public GoGame()
    {
        InitializeBoard();
        RandomizePlayerOrder();
        InitializeScores();
        gameRunning = true;
    }

    // Initialize the board with empty spots (denoted by '.')
    public void InitializeBoard()
    {
        for (int i = 0; i < BoardSize; i++)
        {
            for (int j = 0; j < BoardSize; j++)
            {
                board[i, j] = '.';
            }
        }
    }

    // Initialize player scores
    public void InitializeScores()
    {
        foreach (char player in players)
        {
            scores[player] = 0;
        }
    }

    // Randomize player order for each game
    public void RandomizePlayerOrder()
    {
        // Shuffle the player list to randomize the turn order
        for (int i = 0; i < players.Count; i++)
        {
            int j = rand.Next(i, players.Count);
            char temp = players[i];
            players[i] = players[j];
            players[j] = temp;
        }
        currentPlayerIndex = 0;
    }

    // Print the board with borders and grid numbers
    private void PrintBoard()
    {
        Console.Clear();
        Console.WriteLine("   0 1 2 3 4 5 6 7 8");
        Console.WriteLine("  +-----------------+");
        for (int i = 0; i < BoardSize; i++)
        {
            Console.Write(i + " |");
            for (int j = 0; j < BoardSize; j++)
            {
                Console.Write(board[i, j] + " ");
            }
            Console.WriteLine("|");
        }
        Console.WriteLine("  +-----------------+");
        Console.WriteLine();
        PrintScores();
    }

    // Print the scores for each player
    private void PrintScores()
    {
        Console.WriteLine("Scores:");
        foreach (var player in players)
        {
            Console.WriteLine($"Player {player}: {scores[player]} points");
        }
        Console.WriteLine();
    }

    // Check if a given position is valid and empty
    private bool IsValidMove(int row, int col)
    {
        return row >= 0 && row < BoardSize && col >= 0 && col < BoardSize && board[row, col] == '.';
    }

    // Get liberties for a group of stones
    private HashSet<(int, int)> GetLiberties(int row, int col, char player)
    {
        HashSet<(int, int)> liberties = new HashSet<(int, int)>();
        bool[,] visited = new bool[BoardSize, BoardSize];

        void Explore(int r, int c)
        {
            if (r < 0 || r >= BoardSize || c < 0 || c >= BoardSize || visited[r, c])
                return;

            visited[r, c] = true;

            if (board[r, c] == player)
            {
                // Explore adjacent stones of the same color
                Explore(r - 1, c);
                Explore(r + 1, c);
                Explore(r, c - 1);
                Explore(r, c + 1);
            }
            else if (board[r, c] == '.')
            {
                liberties.Add((r, c)); // Empty spot is a liberty
            }
        }

        Explore(row, col);
        return liberties;
    }

    // Capture a group of stones by removing them from the board
    private void CaptureGroup(int row, int col, char player)
    {
        bool[,] visited = new bool[BoardSize, BoardSize];

        void RemoveGroup(int r, int c)
        {
            if (r < 0 || r >= BoardSize || c < 0 || c >= BoardSize || visited[r, c] || board[r, c] != player)
                return;

            visited[r, c] = true;
            board[r, c] = '.';

            // Explore all connected stones of the same color
            RemoveGroup(r - 1, c);
            RemoveGroup(r + 1, c);
            RemoveGroup(r, c - 1);
            RemoveGroup(r, c + 1);
        }

        RemoveGroup(row, col);
    }

    // Check if the move results in any captured stones and return true if so
    private bool CaptureStones(int row, int col, char player)
    {
        bool captured = false;

        // Check all 4 directions for liberties of the opposite color (capture if no liberties)
        for (int r = 0; r < BoardSize; r++)
        {
            for (int c = 0; c < BoardSize; c++)
            {
                if (board[r, c] != player && board[r, c] != '.')
                {
                    var liberties = GetLiberties(r, c, board[r, c]);
                    if (liberties.Count == 0)
                    {
                        CaptureGroup(r, c, board[r, c]);
                        captured = true;
                        scores[board[r, c]]++;  // Increment captured player score
                    }
                }
            }
        }

        return captured;
    }

    // Handle a player's move
    private void PlayTurn()
    {
        char currentPlayer = players[currentPlayerIndex];
        Console.WriteLine($"Player {currentPlayer}'s turn!");

        int row, col;
        bool validMove = false;

        while (!validMove)
        {
            Console.Write($"Enter row (0-{BoardSize - 1}) and column (0-{BoardSize - 1}) separated by a space: ");
            string input = Console.ReadLine();
            string[] tokens = input.Split(' ');

            if (tokens.Length == 2 && int.TryParse(tokens[0], out row) && int.TryParse(tokens[1], out col))
            {
                if (IsValidMove(row, col))
                {
                    board[row, col] = currentPlayer;
                    validMove = true;

                    // After placing the piece, check for captures
                    if (CaptureStones(row, col, currentPlayer))
                    {
                        Console.WriteLine("A group was captured!");
                    }
                }
                else
                {
                    Console.WriteLine("Invalid move, the spot is either occupied or out of bounds.");
                }
            }
            else
            {
                Console.WriteLine("Invalid input. Please enter two integers separated by a space.");
            }
        }

        // After a valid move, update the current player index
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
    }

    // Main game loop
    public void PlayGame()
    {
        int turns = 0;
        while (gameRunning && turns < BoardSize * BoardSize)
        {
            PrintBoard();
            PlayTurn();
            turns++;
        }

        PrintBoard();
        Console.WriteLine("Game Over!");
    }

    public static void Main()
    {
        while (true)
        {
            GoGame game = new GoGame();
            game.PlayGame();

            Console.WriteLine("Do you want to play another game? (y/n)");
            string input = Console.ReadLine().ToLower();
            if (input != "y")
            {
                break;
            }
        }
    }
}
