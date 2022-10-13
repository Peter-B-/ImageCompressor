using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;

namespace ImageCompressor.Compressors;

public abstract class ImageConversionCompressor : ICompressor
{
    protected abstract IImageEncoder Encoder { get; }

    public ExtensionHandling ExtensionHandling { get; } = ExtensionHandling.Replace;

    public void Compress(string inPath, string outPath)
    {
        using var image = Image.Load(inPath);
        image.Save(outPath, Encoder);
    }

    public abstract string FileExtension { get; }
}