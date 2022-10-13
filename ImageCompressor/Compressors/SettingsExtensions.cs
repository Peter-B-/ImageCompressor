using System;

namespace ImageCompressor.Compressors;

public static class SettingsExtensions
{
    public static ICompressor CreateCompressor(this CompressImagesSettings settings) =>
        settings.OutMode switch
        {
            CompressImagesSettings.OutputMode.Jpeg => new JpegCompressor(settings.Quality),
            CompressImagesSettings.OutputMode.Png => new PngCompressor(),
            CompressImagesSettings.OutputMode.Brotli => new BrotliCompressor(settings.Quality),
            CompressImagesSettings.OutputMode.Webp => new WebpCompressor(settings.Quality),
            CompressImagesSettings.OutputMode.WebpLl => new WebpLlCompressor(settings.Quality),
            CompressImagesSettings.OutputMode.BrotliUncompress => new BrotliUncompressor(),
            _ => throw new NotSupportedException($"Mode {settings.OutMode} is not supported.")
        };
}