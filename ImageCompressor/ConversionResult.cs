namespace ImageCompressor;

internal record ConversionResult(long OriginalSize, long CompressedSize, bool Success, string ErrorMessage, string Path);