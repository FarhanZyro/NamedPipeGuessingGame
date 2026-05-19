using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        // Number of agents
        int agentCount = 2;

        // Optional command line parameter
        if (args.Length > 0)
        {
            agentCount = int.Parse(args[0]);
        }

        // Generate random target number
        Random random = new Random();

        int targetNumber = random.Next(1, 1001);

        Console.WriteLine("Target Number: " + targetNumber);

        // Start timer
        Stopwatch stopwatch = new Stopwatch();

        stopwatch.Start();

        // Store agent processes
        Process[] agents = new Process[agentCount];

        // Start agents
        for (int i = 0; i < agentCount; i++)
        {
            string agentName = (i + 1).ToString();

            ProcessStartInfo startInfo =
                new ProcessStartInfo();

            // Agent executable
            startInfo.FileName = "Agent.exe";

            // Pass parameters
            startInfo.Arguments =
                agentName + " " + targetNumber;

            startInfo.UseShellExecute = false;

            // Start process
            agents[i] = Process.Start(startInfo);

            Console.WriteLine(
                "Started Agent " + agentName);

            // Optional CPU core assignment
            try
            {
                int coreMask = 1 << i;

                agents[i].ProcessorAffinity =
                    (IntPtr)coreMask;
            }
            catch
            {
                Console.WriteLine(
                    "Could not assign CPU core.");
            }
        }

        // Create Named Pipe Server
        using (NamedPipeServerStream server =
            new NamedPipeServerStream(
                "GuessPipe",
                PipeDirection.In))
        {
            Console.WriteLine();
            Console.WriteLine("Waiting for winner...");

            // Wait for agent connection
            await server.WaitForConnectionAsync();

            // Read message from agent
            using (StreamReader reader =
                new StreamReader(server))
            {
                string result =
                    await reader.ReadLineAsync();

                // Stop timer
                stopwatch.Stop();

                Console.WriteLine();
                Console.WriteLine("===== WINNER =====");

                Console.WriteLine(result);

                Console.WriteLine(
                    "Time: " +
                    stopwatch.ElapsedMilliseconds +
                    " ms");
            }
        }

        // Close remaining agents
        foreach (Process agent in agents)
        {
            try
            {
                if (!agent.HasExited)
                {
                    agent.Kill();
                }
            }
            catch
            {
            }
        }

        // Prevent console from closing
        Console.WriteLine();
        Console.WriteLine("Press Enter to exit...");
        Console.ReadLine();
    }
}