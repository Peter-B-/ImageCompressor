using System;
using System.ComponentModel;
using System.IO;
using Spectre.Console;
using Spectre.Console.Cli;

namespace ImageCompressor;

public sealed class CompressImagesSettings : CommandSettings
{
    public enum OutputMode
    {
        Jpeg,
        Png,
        Brotli,
        BrotliUncompress,
        Webp,
        WebpLl
    }

    [CommandOption("-d|--delete")]
    [Description("Delete original file after conversion")]
    [DefaultValue(false)]
    public bool DeleteOriginal { get; init; }

    [CommandOption("-f|--force")]
    [Description("Overwrite existing files. If [white]false[/], existing files are skipped.")]
    [DefaultValue(false)]
    public bool OverwriteExisting { get; init; }

    [Description("Include subdirectories")]
    [CommandOption("-r|--recursive")]
    public bool IncludeSubDirectories { get; init; }

    [Description($"The compression file format to be used: \r\n" +
        $"[darkorange]image:      [/] [green]{nameof(OutputMode.Jpeg)}[/], [green]{nameof(OutputMode.Webp)}[/]\r\n" +
        $"[darkorange]lossless:   [/] [green]{nameof(OutputMode.Png)}[/], [green]{nameof(OutputMode.WebpLl)}[/]\r\n" +
        $"[darkorange]compression:[/] [green]{nameof(OutputMode.Brotli)}[/], [green]{nameof(OutputMode.BrotliUncompress)}[/]")]
    [CommandOption("-m|--mode")]
    [DefaultValue(typeof(OutputMode), "Jpeg")]
    public OutputMode OutMode { get; set; }

    [Description("Number of concurrent compressions (default: [white]4[/])")]
    [CommandOption("--parallel")]
    [DefaultValue(4)]
    public int Parallel { get; init; }

    [Description("Quality used for output\r\ndefault: [white]98[/] for Jpeg, [white]4[/] for Brotli)")]
    [CommandOption("-q|--quality")]
    public int? Quality { get; init; }

    [Description("The ratio of files to process.\r\nUse [white]0.025[/] to convert [white]2.5%[/] of all images.")]
    [CommandOption("--sample")]
    public double? SampleRatio { get; init; }

    [CommandOption("--pattern")]
    [Description("Search pattern to discover files. Defaults to [white]*.bmp[/]")]
    [DefaultValue("*.bmp")]
    public string SearchPattern { get; init; } = "*.bmp";

    [Description("Path to search. Defaults to current directory.")]
    [CommandArgument(0, "[sourcePath]")]
    public string? SourcePath { get; init; }

    [Description("Path to store images. Defaults to [[sourcePath]].")]
    [CommandArgument(1, "[targetPath]")]
    public string? TargetPath { get; init; }

    public string GetSourcePath() => Path.GetFullPath(SourcePath ?? Directory.GetCurrentDirectory());
    public string GetTargetPath() => Path.GetFullPath(TargetPath ?? SourcePath ?? Directory.GetCurrentDirectory());

    public override ValidationResult Validate()
    {
        if (SampleRatio.HasValue && SampleRatio < 0 || SampleRatio > 1)
            return ValidationResult.Error("SampleRatio must be in the range [0, 1]");
        return ValidationResult.Success();
    }
}