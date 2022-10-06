using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace ImageCompressor.Compressors;

public class JpegCompressor : ImageConversionCompressor
{
    public JpegCompressor(int? quality)
    {
        Encoder = new JpegEncoder {Quality = quality ?? 98};
    }

    public override string FileExtension { get; } = "jpg";

    protected override IImageEncoder Encoder { get; }
}