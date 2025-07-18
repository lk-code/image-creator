using ImageCreator.Extensions;
using Shouldly;

namespace Tests.Extensions;

public class PathExtensionsTests
{
    [Test]
    public void ToAbsolutePath_ShouldThrow_WhenPathIsNull()
    {
        string? input = null;
        Should.Throw<ArgumentException>(() => input!.ToAbsolutePath());
    }

    [Test]
    public void ToAbsolutePath_ShouldThrow_WhenPathIsEmpty()
    {
        var input = "";
        Should.Throw<ArgumentException>(() => input.ToAbsolutePath());
    }

    [Test]
    public void ToAbsolutePath_ShouldReturnHomeResolvedPath_WhenPathStartsWithTilde()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var relativePath = "~/folder/file.txt";
        var expected = Path.Combine(home, "folder", "file.txt");

        var result = relativePath.ToAbsolutePath();

        result.ShouldBe(Path.GetFullPath(expected));
    }

    [Test]
    public void ToAbsolutePath_ShouldReturnAbsolutePath_WhenPathIsRelative()
    {
        var current = Directory.GetCurrentDirectory();
        var relativePath = "data/file.txt";
        var expected = Path.Combine(current, "data", "file.txt");

        var result = relativePath.ToAbsolutePath();

        result.ShouldBe(Path.GetFullPath(expected));
    }

    [Test]
    public void ToAbsolutePath_ShouldReturnUnchangedPathForWindows_WhenPathIsAlreadyAbsolute()
    {
        var input = Path.GetFullPath("C:/example/test.txt");

        var result = input.ToAbsolutePath();

        result.ShouldBe(input);
    }

    [Test]
    public void ToAbsolutePath_ShouldReturnUnchangedPathForUnix_WhenPathIsAlreadyAbsolute()
    {
        var input = Path.GetFullPath("/var/data/test.txt");

        var result = input.ToAbsolutePath();

        result.ShouldBe(input);
    }
}