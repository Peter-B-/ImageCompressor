﻿using System.IO;
using System.IO.Compression;

namespace ImageCompressor;

public class BrotliCompressor : ICompressor
{
    private readonly int quality;

    public BrotliCompressor(int? quality)
    {
        this.quality = quality ?? 4;
    }

    public bool AppendExtension { get; } = true;
    public string FileExtension { get; } = "br";

    public void Compress(string inPath, string outPath)
    {
        using var input = File.OpenRead(inPath);
        using var output = File.Create(outPath);
        using var compressStream = new BrotliStream(output, (CompressionLevel) quality);

        input.CopyTo(compressStream);
        compressStream.Flush();
    }
}