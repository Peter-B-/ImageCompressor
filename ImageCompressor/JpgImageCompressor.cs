using OpenCvSharp;

namespace ImageCompressor;

public class JpgImageCompressor : ICompressor
{
    private readonly int quality;

    public JpgImageCompressor(int? quality)
    {
        this.quality = quality ?? 98;
    }

    public bool AppendExtension { get; } = false;
    public string FileExtension { get; } = "jpg";

    public void Compress(string inPath, string outPath)
    {
        using var org = Cv2.ImRead(inPath);

        Cv2.ImWrite(outPath, org, new ImageEncodingParam(ImwriteFlags.JpegQuality, quality));
    }
}