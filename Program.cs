using MapJsonComparator;
using Newtonsoft.Json.Linq;

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
    JToken oldJson = JToken.Parse(File.ReadAllText(oldFile));
    JToken newJson = JToken.Parse(File.ReadAllText(newFile));

    var diffs = new List<string>();

    // Spusti rekurzívne porovnanie
    JsonComparator.CompareJsonTokens(oldJson, newJson, "", diffs);

    // output
    if (diffs.Count != 0)
    {
        diffs.Sort();
        var outputNodes = diffs.Where(x => x.Contains("nodes"));
        var outputEdges = diffs.Where(x => x.Contains("edges"));
        var outputOthers = diffs.Where(x => !x.Contains("nodes") && !x.Contains("edges"));

        var output = outputNodes
            .Concat(outputEdges)
            .Concat(outputOthers)
            .ToList();

        output.ForEach(Console.WriteLine);
    }
    else
    {
        Console.WriteLine("No changes.");
    }
}
catch (FileNotFoundException)
{
    Console.WriteLine("Chyba: Jeden alebo oba súbory neboli nájdené.");
}
catch (IOException e)
{
    Console.WriteLine($"Error reading file: {e.Message}");
}
catch (Exception e)
{
    Console.WriteLine($"An unexpected error occurred: {e.Message}");
}
finally
{
    Console.WriteLine("\nEnter 'exit' to terminate the program:");
    string input;
    do
    {
        input = Console.ReadLine();
    } while (input?.ToLower() != "exit");
}
