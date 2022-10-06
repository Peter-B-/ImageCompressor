using System.ComponentModel;
using System.IO;
using Spectre.Cli;

namespace ImageCompressor;

public sealed class CompressImagesSettings : CommandSettings
{
    public enum OutputMode
    {
        Jpeg,
        Png,
        Brotli
    }

    [CommandOption("-d|--delete")]
    [Description("Delete original file after conversion")]
    [DefaultValue(false)]
    public bool DeleteOriginal { get; init; }

    [Description("Include subdirectories")]
    [CommandOption("-r|--recursive")]
    public bool IncludeSubDirectories { get; init; }

    [Description($"The compression file format to be used: [darkgreen]{nameof(OutputMode.Jpeg)}, {nameof(OutputMode.Brotli)}[/]")]
    [CommandOption("-m|--mode")]
    [DefaultValue(typeof(OutputMode), "Jpeg")]
    public OutputMode OutMode { get; set; }

    [Description("Number of concurrent compressions")]
    [CommandOption("--parallel")]
    [DefaultValue(4)]
    public int Parallel { get; init; }

    [Description("Quality used for output (default: 98 for Jpeg, 4 for Brotli")]
    [CommandOption("-q|--quality")]
    public int? Quality { get; init; }

    [Description("The ratio of files to process. Use [gray]0.025[/] to convert [gray]2.5%[/] of all images.")]
    [CommandOption("--sample")]
    public double? SampleRatio { get; init; }

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

    public string GetSourcePath() => Path.GetFullPath(SourcePath ?? Directory.GetCurrentDirectory());
    public string GetTargetPath() => Path.GetFullPath(TargetPath ?? SourcePath ?? Directory.GetCurrentDirectory());

    public override ValidationResult Validate()
    {
        if (SampleRatio.HasValue && SampleRatio < 0 || SampleRatio > 1)
            return ValidationResult.Error("SampleRatio must be in the range [0, 1]");
        return ValidationResult.Success();
    }
}