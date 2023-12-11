namespace ImageCompressor.Service;

public class CompressionTasks
{
    public string Name { get; set; }
    public TimeSpan InitialDelay { get; set; } = TimeSpan.FromSeconds(1);
    public TimeSpan Period { get; set; } = TimeSpan.FromMinutes(5);
    public CompressImagesSettings CompressionSettings { get; set; }
}