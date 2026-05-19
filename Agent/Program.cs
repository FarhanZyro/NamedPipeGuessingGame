using System;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine(
                "Usage: Agent.exe <name> <target>");
            Console.ReadLine();
            return;
        }

        string agentName = args[0];

        int targetNumber = int.Parse(args[1]);

        Console.WriteLine(
            "Agent " + agentName + " started.");

        Random random = new Random();

        while (true)
        {
            // Slow down guessing
            await Task.Delay(10);

            int guess = random.Next(1, 1001);

            // REMOVE SPAM OUTPUT
            // Console.WriteLine(
            //     "Agent " + agentName +
            //     " guessed " + guess);

            if (guess == targetNumber)
            {
                using (NamedPipeClientStream pipe =
                    new NamedPipeClientStream(
                        ".",
                        "GuessPipe",
                        PipeDirection.Out))
                {
                    await pipe.ConnectAsync();

                    using (StreamWriter writer =
                        new StreamWriter(pipe))
                    {
                        writer.AutoFlush = true;

                        string message =
                            "Agent " + agentName +
                            " guessed the number " +
                            targetNumber;

                        await writer.WriteLineAsync(
                            message);

                        Console.WriteLine(message);
                    }
                }

                break;
            }
        }

        Console.ReadLine();
    }
}