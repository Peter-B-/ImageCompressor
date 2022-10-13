namespace ImageCompressor;

public interface ICompressor
{
    ExtensionHandling ExtensionHandling { get; }
    string FileExtension { get; }

    void Compress(string inPath, string outPath);
}

public enum ExtensionHandling
{
    Append,
    Replace,
    Remove
}