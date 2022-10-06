
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace ImageCompressor;

public class JpgImageCompressor : ICompressor
{
    private readonly JpegEncoder jpegEncoder;

    public JpgImageCompressor(int? quality)
    {
        jpegEncoder = new JpegEncoder() { Quality = quality ?? 98 };
    }

    public bool AppendExtension { get; } = false;
    public string FileExtension { get; } = "jpg";

    public void Compress(string inPath, string outPath)
    {
        using var image = Image.Load(inPath);
        image.Save(outPath, jpegEncoder);
    }
}