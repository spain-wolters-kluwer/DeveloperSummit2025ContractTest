using System.Text;

namespace DevSummit.Blog.Consumer.Tests.Pact.Broker;

internal static class PactFileHandler
{
    public static void CleanPactDirectory(string? pactDir)
    {
        if (string.IsNullOrEmpty(pactDir))
        {
            return;
        }

        var directory = new DirectoryInfo(pactDir);
        if (!directory.Exists)
        {
            return;
        }

        foreach (var file in directory.GetFiles())
        {
            file.Delete();
        }
    }

    public static string GetContentFromPactFile(string pactDir, string provider, string consumer)
    {
        var fileName = GetPactPathFileName(pactDir, provider, consumer);
        if (!File.Exists(fileName))
        {
            throw new FileNotFoundException($"Pact File {fileName} doesn't exist.");
        }
        return File.ReadAllText(fileName, Encoding.UTF8);
    }

    private static string GetPactPathFileName(string pactDir, string provider, string consumer)
    {
        return Path.Combine(pactDir ?? string.Empty, $"{consumer}-{provider}.json");
    }
}

