using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Webp;

namespace ImageCompressor.Compressors;

public class WebpLlCompressor : ImageConversionCompressor
{
    public WebpLlCompressor(int? quality)
    {
        Encoder = new WebpEncoder()
        {
            Quality = quality ?? 75 ,
            FileFormat = WebpFileFormatType.Lossless
        };
    }

    public override string FileExtension { get; } = "webp";

    protected override IImageEncoder Encoder { get; }
}