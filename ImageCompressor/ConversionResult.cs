namespace ImageCompressor;

internal record ConversionResult(Result Result, long OriginalSize, long CompressedSize, string ErrorMessage, string Path);

public enum Result
{
    Success,
    Skipped,
    Failed
}