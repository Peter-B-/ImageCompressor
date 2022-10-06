namespace ImageCompressor;

public interface ICompressor
{
    bool AppendExtension { get; }
    string FileExtension { get; }

    void Compress(string inPath, string outPath);
}