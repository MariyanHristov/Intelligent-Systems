using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

class SlidingPuzzle
{
    private static readonly (int dx, int dy, string move)[] Moves = 
    {
        (-1, 0, "down"), (1, 0, "up"), (0, -1, "right"), (0, 1, "left")
    };

    private static bool IsSolvable(List<List<int>> board)
    {
        var flatBoard = board.SelectMany(row => row).Where(x => x != 0).ToList();
        int invCount = 0;
        for (int i = 0; i < flatBoard.Count; i++)
        {
            for (int j = i + 1; j < flatBoard.Count; j++)
            {
                if (flatBoard[i] > flatBoard[j])
                {
                    invCount++;
                }
            }
        }

        return invCount % 2 == 0;
    }

    private static int ManhattanDistance(List<List<int>> board, List<List<int>> goal)
    {
        int dist = 0;
        int size = board.Count;

        var goalPositions = new Dictionary<int, (int x, int y)>();
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                goalPositions[goal[i][j]] = (i, j);
            }
        }

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                if (board[i][j] != 0)
                {
                    int value = board[i][j];
                    (int goalX, int goalY) = goalPositions[value];
                    dist += Math.Abs(goalX - i) + Math.Abs(goalY - j);
                }
            }
        }

        return dist;
    }

    private static bool IDAStar(List<List<int>> board, List<List<int>> goal, int zeroX, int zeroY, int g, int threshold, List<string> path)
    {
        int f = g + ManhattanDistance(board, goal);
        if (f > threshold) return false;
        if (ManhattanDistance(board, goal) == 0)
        {
            return true;
        }

        int size = board.Count;

        foreach (var (dx, dy, move) in Moves)
        {
            int nx = zeroX + dx, ny = zeroY + dy;
            if (nx >= 0 && nx < size && ny >= 0 && ny < size)
            {
                (board[zeroX][zeroY], board[nx][ny]) = (board[nx][ny], board[zeroX][zeroY]);
                path.Add(move);

                if (IDAStar(board, goal, nx, ny, g + 1, threshold, path))
                    return true;

                path.RemoveAt(path.Count - 1);
                (board[zeroX][zeroY], board[nx][ny]) = (board[nx][ny], board[zeroX][zeroY]);
            }
        }
        return false;
    }

    private static void SolvePuzzle(List<List<int>> board, List<List<int>> goal)
    {
        if (!IsSolvable(board))
        {
            Console.WriteLine("-1");
            return;
        }

        int zeroX, zeroY;
        getZeroCoordinates(board, out zeroX, out zeroY);

        Stopwatch stopwatch;
        List<string> path;
        GetResultsAndOptimality(board, goal, zeroX, zeroY, out stopwatch, out path);

        Console.WriteLine(path.Count);
        foreach (string move in path)
        {
            Console.WriteLine(move);
        }
        Console.WriteLine($"Time: {stopwatch.Elapsed.TotalMilliseconds / 1000} seconds");
    }

    private static void GetResultsAndOptimality(List<List<int>> board, List<List<int>> goal, int zeroX, int zeroY, out Stopwatch stopwatch, out List<string> path)
    {
        stopwatch = Stopwatch.StartNew();
        path = GetOptimalPath(board, goal, zeroX, zeroY);
        stopwatch.Stop();
    }

    private static List<string> GetOptimalPath(List<List<int>> board, List<List<int>> goal, int zeroX, int zeroY)
    {
        List<string> path = new List<string>();
        int threshold = ManhattanDistance(board, goal);

        while (true)
        {
            if (IDAStar(board, goal, zeroX, zeroY, 0, threshold, path))
            {
                break;
            }

            threshold++;
        }

        return path;
    }

    private static void getZeroCoordinates(List<List<int>> board, out int zeroX, out int zeroY)
    {
        zeroX = 0;
        zeroY = 0;
        for (int i = 0; i < board.Count; i++)
        {
            for (int j = 0; j < board[i].Count; j++)
            {
                if (board[i][j] == 0)
                {
                    zeroX = i;
                    zeroY = j;
                    break;
                }
            }
        }
    }

    public static void Main()
    {
        Console.Write("Tiles: ");
        int tiles = int.Parse(Console.ReadLine());

        int size = (int)Math.Sqrt(tiles + 1);
        if (size * size != tiles + 1)
        {
            Console.WriteLine("Invalid puzzle size.");
            return;
        }
        Console.Write("Enter the goal zero position (default -1 for bottom right): ");
        int index = int.Parse(Console.ReadLine());
        List<List<int>> goal = SetGoal(tiles, size, index);

        Console.WriteLine($"Enter the board configuration as a square matrix:");
        List<List<int>> board = new List<List<int>>(size);
        for (int i = 0; i < size; i++)
        {
            board.Add(Console.ReadLine().Split().Select(int.Parse).ToList());
        }

        SolvePuzzle(board, goal);
    }

    private static List<List<int>> SetGoal(int tiles, int size, int index)
    {
        List<List<int>> goal = new List<List<int>>(size);
        int value = 1;
        for (int i = 0, k = 0; i < size; i++)
        {
            List<int> row = new List<int>(size);
            for (int j = 0; j < size; j++, k++)
            {
                if (k == index || index == -1 && k == tiles)
                {
                    row.Add(0);
                }
                else
                {
                    row.Add(value++);
                }

            }
            goal.Add(row);
        }

        return goal;
    }
}
