using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using ImageCompressor.Compressors;
using Spectre.Cli;
using Spectre.Console;

namespace ImageCompressor;

internal sealed class CompressImagesCommand : Command<CompressImagesSettings>
{
    public override int Execute([NotNull] CommandContext context, [NotNull] CompressImagesSettings settings)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.WriteLine();
        AnsiConsole.Markup($"Compressing [green]{settings.SearchPattern}[/] files to [green]{settings.OutMode}[/]");
        if (settings.SampleRatio is < 1)
            AnsiConsole.Markup($", sampeling [green]{settings.SampleRatio * 100} %[/]");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"\tfrom [green]{settings.GetSourcePath()}[/]");
        AnsiConsole.MarkupLine($"\tto   [green]{settings.GetTargetPath()}[/]");
        AnsiConsole.WriteLine();

        var stopwatch = Stopwatch.StartNew();

        var results = AnsiConsole.Status()
            .Spinner(Spinner.Known.BouncingBar)
            .SpinnerStyle(Style.Parse("green bold"))
            .Start("Converting...", ctx => ConvertFiles(settings));

        var originalSize = results.Where(r => r.Result == Result.Success).Sum(r => r.OriginalSize);
        var compressedSize = results.Where(r => r.Result == Result.Success).Sum(r => r.CompressedSize);

        AnsiConsole.MarkupLine($"Converted [blue]{results.Count(r => r.Result == Result.Success):N0}[/] [green]{settings.SearchPattern}[/] files in [gray]{stopwatch.Elapsed}[/].");
        AnsiConsole.MarkupLine($"Reduced size from [blue]{originalSize >> 20}[/] to [blue]{(compressedSize >> 20)}[/] MiB: [green]{(compressedSize * 100.0 / (originalSize + 0.1)):N1} %[/].");
        if (results.Any(r => r.Result == Result.Skipped))
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[blue]{results.Count(r => r.Result == Result.Skipped)}[/] files already existed and were skipped.");
        }
        if (results.Any(r => r.Result == Result.Failed))
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[red]Error[/]: [orange]{results.Count(r => r.Result == Result.Failed)}[/] files could not be compressed:");
            foreach (var res in results.Where(r => r.Result == Result.Failed).Take(50))
            {
                AnsiConsole.MarkupLine($"[gray]{res.Path}[/]:");
                AnsiConsole.WriteLine(res.ErrorMessage);
                AnsiConsole.WriteLine();
            }
        }

        AnsiConsole.WriteLine();
        AnsiConsole.WriteLine();

        return 0;
    }

    private ConversionResult ConvertFile(FileInfo fi, CompressImagesSettings settings)
    {
        try
        {
            var originalSize = fi.Length;

            var compressor = settings.CreateCompressor();

            var outPath = GetOutPath(settings, fi, compressor.FileExtension, compressor.AppendExtension);

            if (File.Exists(outPath) && !settings.OverwriteExisting)
                return new ConversionResult(Result.Skipped, originalSize, new FileInfo(outPath).Length, string.Empty, fi.Name);

            Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);

            compressor.Compress(fi.FullName, outPath);

            if (settings.DeleteOriginal)
                fi.Delete();

            var outFi = new FileInfo(outPath);
            return new ConversionResult(Result.Success, originalSize, outFi.Length, string.Empty, fi.Name);
        }
        catch (Exception e)
        {
            return new ConversionResult(Result.Failed, fi.Exists ? fi.Length : 0, 0, e.Message, fi.Name);
        }
    }

    private List<ConversionResult> ConvertFiles(CompressImagesSettings settings)
    {
        var files = new DirectoryInfo(settings.GetSourcePath())
            .EnumerateFiles(settings.SearchPattern, settings.IncludeSubDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

        if (settings.SampleRatio is < 1)
        {
            var rand = new Random();
            files = files.Where(_ => rand.NextDouble() < settings.SampleRatio.Value);
        }

        var results =
            files
                .AsParallel()
                .WithDegreeOfParallelism(settings.Parallel)
                .Select(fi => ConvertFile(fi, settings))
                .ToList();

        return results;
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
}