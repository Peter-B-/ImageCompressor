using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Webp;

namespace ImageCompressor.Compressors;

public class WebpCompressor : ImageConversionCompressor
{
    public WebpCompressor(int? quality)
    {
        Encoder = new WebpEncoder()
        {
            Quality = quality ?? 75 ,
            FileFormat = WebpFileFormatType.Lossy
        };
    }

    public override string FileExtension { get; } = "webp";

    protected override IImageEncoder Encoder { get; }
}