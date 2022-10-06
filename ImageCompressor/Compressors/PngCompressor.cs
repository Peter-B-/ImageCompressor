using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;

namespace ImageCompressor.Compressors;

public class PngCompressor : ImageConversionCompressor
{
    public override string FileExtension { get; } = "png";

    protected override IImageEncoder Encoder { get; } = new PngEncoder();
}