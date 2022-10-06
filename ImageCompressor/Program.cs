using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using OpenCvSharp;
using Spectre.Cli;
using Spectre.Console;

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

    [Description("Number of concurrent compressions")]
    [CommandOption("--parallel")]
    [DefaultValue(4)]
    public int Parallel { get; init; }



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

    public string GetSourcePath() => Path.GetFullPath( SourcePath ?? Directory.GetCurrentDirectory());
    public string GetTargetPath() => Path.GetFullPath(TargetPath ?? SourcePath ?? Directory.GetCurrentDirectory());
}

internal sealed class CompressImagesCommand : Command<CompressImagesSettings>
{
    public override int Execute([NotNull] CommandContext context, [NotNull] CompressImagesSettings settings)
    {
        AnsiConsole.MarkupLine($"Converting [green]{settings.SearchPattern}[/] files");
        AnsiConsole.MarkupLine($"\tfrom [green]{settings.GetSourcePath()}[/]");
        AnsiConsole.MarkupLine($"\tto   [green]{settings.GetTargetPath()}[/]");
        AnsiConsole.WriteLine();

        var files = new DirectoryInfo(settings.GetSourcePath())
            .EnumerateFiles(settings.SearchPattern, settings.IncludeSubDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

        var stopwatch = Stopwatch.StartNew();
        var results =
            files
                .AsParallel()
                .WithDegreeOfParallelism(settings.Parallel)
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
            var outPath = GetOutPath(settings, fi, "jpg", false);
            Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);

            using (var org = Cv2.ImRead(fi.FullName))
            {
                Cv2.ImWrite(outPath, org, new ImageEncodingParam(ImwriteFlags.JpegQuality, settings.Quality));
            }

            if (settings.DeleteOriginal)
                fi.Delete();

            var outFi = new FileInfo(outPath);
            return new ConversionResult(fi.Length, outFi.Length, true, string.Empty, fi.Name);
        }
        catch (Exception e)
        {
            return new ConversionResult(fi.Length, 0, false, e.Message, fi.Name);
        }
    }

    private string GetOutPath(CompressImagesSettings settings, FileInfo fi, string extension, bool appendExtension)
    {
        var relativePath = Path.GetRelativePath(settings.GetSourcePath(), fi.FullName);
        if (appendExtension)
            relativePath += "." + extension;
        else
            relativePath = Path.ChangeExtension(relativePath, extension);
        
        return Path.Combine(settings.GetTargetPath(), relativePath);
    }

    private string GetOutDir(CompressImagesSettings settings, string? fileDirectory)
    {
        if (settings.TargetPath == null)
            return fileDirectory ?? settings.SourcePath ?? Directory.GetCurrentDirectory();

        return fileDirectory.Replace(settings.SourcePath, settings.TargetPath, StringComparison.InvariantCultureIgnoreCase);
    }
}

internal record ConversionResult(long originalSize, long compressedSize, bool success, string errorMessage, string path)
{
}