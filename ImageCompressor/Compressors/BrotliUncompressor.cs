using System.IO;
using System.IO.Compression;

namespace ImageCompressor.Compressors;

public class BrotliUncompressor : ICompressor
{
    public ExtensionHandling ExtensionHandling { get; } = ExtensionHandling.Remove;
    public string FileExtension { get; } = "br";

    public void Compress(string inPath, string outPath)
    {
        using var input = File.OpenRead(inPath);
        using var output = File.Create(outPath);
        using var compressStream = new BrotliStream(input, CompressionMode.Decompress);

        compressStream.CopyTo(output);
        compressStream.Flush();
    }
}