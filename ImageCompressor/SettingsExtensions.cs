using System;

namespace ImageCompressor;

public static class SettingsExtensions
{
    public static ICompressor CreateCompressor(this CompressImagesSettings settings) =>
        settings.OutMode switch
        {
            CompressImagesSettings.OutputMode.Jpeg => new JpgImageCompressor(settings.Quality),
            CompressImagesSettings.OutputMode.Brotli => new BrotliCompressor(settings.Quality),
            _ => throw new NotSupportedException($"Mode {settings.OutMode} is not supported.")
        };
}