using MapJsonComparator;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class Program
{
    private static async Task Main(string[] args)
    {
        string oldFile;
        string newFile;

        if (args.Length != 2)
        {
            Console.WriteLine("Enter file path to old json.");
            Console.Write("> ");
            oldFile = Console.ReadLine();
            Console.WriteLine("Enter file path to new json.");
            Console.Write("> ");
            newFile = Console.ReadLine();
            Console.WriteLine();
        }
        else
        {
            oldFile = args[0];
            newFile = args[1];
        }

        try
        {
            // init
            JToken oldJson = JToken.Parse(await File.ReadAllTextAsync(oldFile));
            JToken newJson = JToken.Parse(await File.ReadAllTextAsync(newFile));
            var diffs = new List<string>();

            // compare
            JsonComparator.CompareJsonTokens(oldJson, newJson, string.Empty, diffs);

            // output
            PrintOutput(diffs);
        }
        catch (UnauthorizedAccessException)
        {
            Console.WriteLine("Error: You do not have permission to access one or both files.");
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine("Error: One or both files not found.");
        }
        catch (IOException e)
        {
            Console.WriteLine($"Error reading file: {e.Message}");
        }
        catch (JsonReaderException e)
        {
            Console.WriteLine($"Error parsing JSON: {e.Message}");
        }
        catch (JsonException e)
        {
            Console.WriteLine($"Error in JSON structure: {e.Message}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"An unexpected error occurred: {e.Message}");
        }
        finally
        {
            Console.WriteLine("\nEnter 'e' or 'exit' to terminate the program:");
            string input;
            do
            {
                input = Console.ReadLine();
            } while (input?.ToLowerInvariant() != "e"
                  && input?.ToLowerInvariant() != "exit");

            Environment.Exit(0);
        }
    }

    private static void PrintOutput(List<string> diffs)
    {
        if (diffs.Count == 0)
        {
            Console.WriteLine("No changes.");
        }
        else
        {
            diffs.Sort();
            var outputNodes = diffs.Where(x => x.Contains("nodes"));
            var outputEdges = diffs.Where(x => x.Contains("edges"));
            var outputOthers = diffs.Where(x => !x.Contains("nodes") && !x.Contains("edges"));

            var output = outputNodes
                .Concat(outputEdges)
                .Concat(outputOthers);

            foreach (var diff in output)
            {
                Console.WriteLine(diff);
            }
        }
    }
}