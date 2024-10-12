using System;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using ConsoleTables;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length <= 1 || args.Length % 2 == 0)
        {
            Console.WriteLine("Error: Please enter an odd number of moves, three or more, separated by a space.");
            return;
        }
        else if (args.Select(s => s.ToLowerInvariant()).ToArray().Distinct().Count() != args.Length)
        {
            Console.WriteLine("Error: duplicate moves. All moves must be unique.");
            return;
        }

        string[] moves = args;
        int numberOfMoves = moves.Length;
        string gameName = String.Join(", ", moves);

        Console.WriteLine($"Welcome to the game '{gameName}'. The game is played until one win. Good luck.\n");

        PlayGame(moves, numberOfMoves);
    }

    static void PlayGame(string[] moves, int numberOfMoves)
    {
        Random random = new Random();
        int computerMove = random.Next(0, numberOfMoves);
        int playerMove;
        byte[] key = GenerateSecureKey();
        string keyHex = BitConverter.ToString(key).Replace("-", "");
        string hmac = ComputeHMAC(key, moves[computerMove]);
        Console.WriteLine($"Computer move HMAC: {hmac}");

        playerMove = UserInput(moves, numberOfMoves);

        Console.WriteLine($"Your move is: {playerMove} ({moves[playerMove - 1]})");
        Console.WriteLine($"Computer move: {computerMove + 1} ({moves[computerMove]})");

        int result = GetWinner(playerMove - 1, computerMove, numberOfMoves);


        string resultMessage = result == 0 ? "Draw" :
            result == 1 ? $"You win: {moves[playerMove - 1]} beats {moves[computerMove]}." :
            $"You win: {moves[playerMove - 1]} beats {moves[computerMove]}.";

        Console.WriteLine($"{resultMessage}\nComputer move key: {keyHex}");

    }

    static byte[] GenerateSecureKey()
    {
        byte[] key = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(key);
        }
        return key;
    }

    static string ComputeHMAC(byte[] key, string message)
    {
        using (var hmacSha256 = new HMACSHA256(key))
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            byte[] hash = hmacSha256.ComputeHash(messageBytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }

    static int GetWinner(int a, int b, int n)
    {
        int p = n / 2;
        return Math.Sign((a - b + p + n) % n - p);
    }
    static int UserInput(string[] moves, int numberOfMoves)
    {
        int playerMove = -1;
        bool playerTurns = false;

        ShowAvailableMoves(moves);

        while (!playerTurns)
        {
            string? input = Console.ReadLine();

            switch (input)
            {
                case "help":
                    DisplayHelpTable(moves, numberOfMoves);
                    break;
                case "0":
                    Console.WriteLine("The program is complete.");
                    Environment.Exit(0);
                    break;
                default:
                    if (int.TryParse(input, out playerMove) && playerMove >= 1 && playerMove <= numberOfMoves)
                        playerTurns = true;
                    else
                        Console.WriteLine($"Error: Type 'help' for display help table, '0' to quit, or one of the valid move numbers (1 - {numberOfMoves}).");
                    break;
            }
        }

        return playerMove;
    }

    static void ShowAvailableMoves(string[] moves)
    {
        Console.WriteLine("Type 'help' for display help table. \n" +
            "Type '0' to quit, or select one of the valid move numbers.\n" +
            "Moves list:");
        for (int i = 0; i < moves.Length; i++)
        {
            Console.WriteLine($"{i + 1}: {moves[i]}");
        }
    }

    static void DisplayHelpTable(string[] moves, int numberOfMoves)
    {
        var table = new ConsoleTable("v User\\Computer > ");
        foreach (var move in moves)
        {
            table.AddColumn(new[] { move });
        }

        for (int i = 0; i < numberOfMoves; i++)
        {
            string[] row = new string[numberOfMoves + 1];
            row[0] = moves[i];

            for (int j = 0; j < numberOfMoves; j++)
            {
                int result = GetWinner(i, j, numberOfMoves);
                row[j + 1] = result == 0 ? "Draw" : result == 1 ? "Win" : "Lose";
            }

            table.AddRow(row);
        }

        table.Write(Format.Alternative);
    }
}