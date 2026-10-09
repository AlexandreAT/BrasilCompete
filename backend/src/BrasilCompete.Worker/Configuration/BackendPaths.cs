namespace BrasilCompete.Worker.Configuration;

/// <summary>
/// Caminhos do worker, relativos à pasta <c>backend/</c> (a que contém a solução).
/// Cache, estado e saídas ficam fora do Git.
/// </summary>
public sealed class BackendPaths(string root)
{
    private const string SolutionFileName = "BrasilCompete.slnx";

    public string Root { get; } = root;

    public string CacheDirectory => Path.Combine(Root, ".cache");

    public string StateDirectory => Path.Combine(Root, ".state");

    public string OutputDirectory => Path.Combine(Root, "output");

    public string Resolve(string relativePath) => Path.GetFullPath(Path.Combine(Root, relativePath));

    public static BackendPaths Discover()
    {
        foreach (var start in new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() })
        {
            for (var directory = new DirectoryInfo(start); directory is not null; directory = directory.Parent)
            {
                if (File.Exists(Path.Combine(directory.FullName, SolutionFileName)))
                {
                    return new BackendPaths(directory.FullName);
                }

                var nested = Path.Combine(directory.FullName, "backend");

                if (File.Exists(Path.Combine(nested, SolutionFileName)))
                {
                    return new BackendPaths(nested);
                }
            }
        }

        throw new InvalidOperationException($"Não encontrei a pasta backend (com o arquivo {SolutionFileName}).");
    }
}
