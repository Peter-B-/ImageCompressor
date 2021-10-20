using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Spectre.Console;
using Spectre.Console.Cli;
using OpenCvSharp;

var app = new CommandApp<CompressImagesCommand>();
return app.Run(args);

public sealed class CompressImagesSettings : CommandSettings
{
    [CommandOption("-d|--delete")]
    [Description("Delete original file after conversion")]
    [DefaultValue(false)]
    public bool DeleteOriginal { get; init; }

    [Description("Include subdirectories")]
    [CommandOption("-r|--recursive")]
    public bool IncludeSubDirectories { get; init; }

    [Description("JPEG quality used for output")]
    [CommandOption("-q|--quality")]
    [DefaultValue(98)]
    public int Quality { get; init; }


    [CommandOption("-p|--pattern")]
    [Description("Search pattern to discover files. Defaults to [gray]*.bmp[/]")]
    [DefaultValue("*.bmp")]
    public string SearchPattern { get; init; } = "*.bmp";

    [Description("Path to search. Defaults to current directory.")]
    [CommandArgument(0, "[sourcePath]")]
    public string? SourcePath { get; init; }

    [Description("Path to store images. Defaults to [[sourcePath]].")]
    [CommandArgument(1, "[targetPath]")]
    public string? TargetPath { get; init; }
}

internal sealed class CompressImagesCommand : Command<CompressImagesSettings>
{
    public override int Execute([NotNull] CommandContext context, [NotNull] CompressImagesSettings settings)
    {
        var searchPath = settings.SourcePath ?? Directory.GetCurrentDirectory();
        var files = new DirectoryInfo(searchPath)
            .EnumerateFiles(settings.SearchPattern, settings.IncludeSubDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

        var stopwatch = Stopwatch.StartNew();
        var results =
            files
                .AsParallel()
                .Select(fi => ConvertFile(fi, settings))
                .ToList();

        var originalSize = results.Where(r => r.success).Sum(r => r.originalSize);
        var compressedSize = results.Where(r => r.success).Sum(r => r.compressedSize);

        AnsiConsole.MarkupLine($"Converted [blue]{results.Count:N0}[/] [green]{settings.SearchPattern}[/] files in [gray]{stopwatch.Elapsed}[/].");
        AnsiConsole.MarkupLine($"Reduced size from [blue]{originalSize >> 20}[/] to [blue]{(compressedSize >> 20)}[/] MiB: [green]{(compressedSize * 100.0 / (originalSize + 0.1)):N1} %[/].");
        if (results.Any(r => !r.success))
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[red]Error[/]: [orange]{results.Count(r => !r.success)}[/] files could not be compressed:");
            foreach (var res in results.Where(r => !r.success))
            {
                AnsiConsole.MarkupLine($"[gray]{res.path}[/]:");
                AnsiConsole.WriteLine(res.errorMessage);
                AnsiConsole.WriteLine();
            }

        }
        return 0;
    }

    private ConversionResult ConvertFile(FileInfo fi, CompressImagesSettings settings)
    {
        try
        {

            var outDir = GetOutDir(settings,fi.DirectoryName);
            Directory.CreateDirectory(outDir);
            var outPath = Path.Combine(outDir, Path.ChangeExtension(fi.Name , "jpg"));

            using (var org = Cv2.ImRead(fi.FullName))
            {
                Cv2.ImWrite(outPath, org, new ImageEncodingParam(ImwriteFlags.JpegQuality, settings.Quality));
            }

            if (settings.DeleteOriginal)
                fi.Delete();
            return new ConversionResult(fi.Length, fi.Length >> 1
                                        , true, string.Empty, fi.Name);
        }
        catch (Exception e)
        {
            return new ConversionResult(fi.Length, 0, false, e.Message, fi.Name);
        }

    }

    private string GetOutDir(CompressImagesSettings settings, string? fileDirectory)
    {
        if (settings.TargetPath == null) 
            return fileDirectory ?? settings.SourcePath?? Directory.GetCurrentDirectory();

        return fileDirectory.Replace(settings.SourcePath, settings.TargetPath, StringComparison.InvariantCultureIgnoreCase);
    }


}

internal record ConversionResult(long originalSize, long compressedSize, bool success, string errorMessage, string path)
{
}