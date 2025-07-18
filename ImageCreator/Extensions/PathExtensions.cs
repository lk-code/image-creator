namespace ImageCreator.Extensions;

public static class PathExtensions
{
    /// <summary>
    /// Resolves a given path to an absolute path, expanding ~ to the user's home directory.
    /// </summary>
    /// <param name="path">A relative or user-relative path.</param>
    /// <returns>The absolute, fully-qualified path.</returns>
    public static string ToAbsolutePath(this string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path must not be empty", nameof(path));

        if (path.StartsWith("~"))
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            path = Path.Combine(home, path.Substring(1).TrimStart(Path.DirectorySeparatorChar));
        }

        return Path.GetFullPath(path);
    }
}