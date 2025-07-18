using Cocona;
using DiscUtils;
using DiscUtils.Fat;
using Spectre.Console;
using ImageCreator.Extensions;

var app = CoconaApp.Create(); // is a shorthand for `CoconaApp.CreateBuilder().Build()`

app.AddCommand("create", async ([Option(Description = "path where the image is to be created")] string image,
    [Option(Description = "path from which the files are to be written")]
    string? source,
    [Option('f', Description = "overwrites an existing image file")]
    bool force = false) =>
{
    char FAT_SEPARATOR_CHAR = '\\';
    
    AnsiConsole.Write(
        new FigletText("Image Creator")
            .LeftJustified()
            .Color(Color.Blue));

    // get the absolut path of image path (can be relative)
    string absoluteImagePath = image.ToAbsolutePath();

    // check if image already exists => display error
    if (!force && File.Exists(absoluteImagePath))
    {
        AnsiConsole.Markup("[red][underline]Error[/]: image file already exists![/]");
        return;
    }

    // if file exists, then delete it
    if (File.Exists(absoluteImagePath))
    {
        File.Delete(absoluteImagePath);
    }

    // get the floppy name (not the filename)
    var floppyName = AnsiConsole.Prompt(new TextPrompt<string>("Floppy Name:"));

    // get all files from source
    string sourcePath = string.IsNullOrWhiteSpace(source) ? Directory.GetCurrentDirectory() : source;
    string[] sourceFiles = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories);
    
    // TODO: create directory tree with all directories to create
    string[] directoriesToCreate = sourceFiles
        .Select(file => Path.GetDirectoryName(file) ?? "")
        .Select(dir => dir.Replace(sourcePath, "").TrimStart(Path.DirectorySeparatorChar))
        .Where(dir => !string.IsNullOrWhiteSpace(dir))
        .Select(dir => dir.Replace(Path.DirectorySeparatorChar, FAT_SEPARATOR_CHAR))
        .Distinct()
        .OrderBy(d => d, StringComparer.Ordinal) // Sortiert Pfade so, dass Eltern vor Kindern stehen
        .ToArray();

    // create floppy image stream
    await using FileStream fs = File.Create(absoluteImagePath);
    using FatFileSystem fatFileSystem = FatFileSystem.FormatFloppy(fs, FloppyDiskType.Extended, floppyName);
    foreach (string directoryToCreate in directoriesToCreate)
    {
        if (!string.IsNullOrEmpty(directoryToCreate)
            && !fatFileSystem.DirectoryExists(directoryToCreate))
        {
            fatFileSystem.CreateDirectory(directoryToCreate);
        }
    }

    // write source files to floppy image
    List<string> successFiles = [];
    List<string> failedFiles = [];
    foreach (string sourceFile in sourceFiles)
    {
        string fileName = sourceFile;

        // Get the part of the path relative to the sourcePath, and normalize to forward slashes.
        string floppyFileName = fileName.Replace(sourcePath, "");

        try
        {
            // if part of path, get directory
            // string floppyDirectoryPath = Path.GetDirectoryName(floppyFileName)?
            //     .TrimStart(Path.DirectorySeparatorChar)
            //     .Replace(Path.DirectorySeparatorChar, FAT_SEPARATOR_CHAR);

            // if (!string.IsNullOrEmpty(floppyDirectoryPath)
            //     && !fatFileSystem.DirectoryExists(floppyDirectoryPath))
            // {
            //     fatFileSystem.CreateDirectory(floppyDirectoryPath);
            // }

            string floppyImageTargetPath = floppyFileName.TrimStart('/')
                .Replace(Path.DirectorySeparatorChar, '\\');

            // read file from string file and write in to stream
            await using Stream floppyFileStream = fatFileSystem.OpenFile(floppyImageTargetPath, FileMode.Create);
            await using FileStream sourceFileStream = File.OpenRead(fileName);
            await sourceFileStream.CopyToAsync(floppyFileStream);

            // add current file to successFiles
            successFiles.Add(floppyFileName);
        }
        catch (Exception err)
        {
            failedFiles.Add(floppyFileName);

            continue;
        }
    }

    var layout = new Layout("Root")
        .SplitColumns(
            new Layout("Success"),
            new Layout("Failed"));

    layout["Success"].Update(
        new Panel(
                Align.Left(
                    new Text(successFiles.ToDisplayableList()),
                    VerticalAlignment.Top))
            // .Expand()
            .Header(new PanelHeader("Success")));

    layout["Failed"].Update(
        new Panel(
                Align.Left(
                    new Text(failedFiles.ToDisplayableList()),
                    VerticalAlignment.Top))
            // .Expand()
            .Header(new PanelHeader("Failed")));

    AnsiConsole.Write(layout);
});

app.Run();