# ImageCompressor

[![.NET](https://github.com/Peter-B-/ImageCompressor/actions/workflows/dotnet.yml/badge.svg)](https://github.com/Peter-B-/ImageCompressor/actions/workflows/dotnet.yml)

# How to get it?

Download the utility from the [releases](https://github.com/Peter-B-/ImageCompressor/releases/) and put it wherever you like to execute it.

# Use it from the CLI

```powershell
ImageCompressor.exe [sourcePath] [targetPath] [OPTIONS]
```
(powershell or bash)

## Arguments
- `[sourcePath]` Path to search. Defaults to current directory
- `[targetPath]` Path to store images. Defaults to `[sourcePath]`

## Options
```
    -h, --help         Prints help information
    -d, --delete       Delete original file after conversion
    -f, --force        Overwrite existing files. If false, existing files are skipped
    -r, --recursive    Include subdirectories
    -m, --mode         The compression file format to be used:
                       image:       Jpeg, Webp
                       lossless:    Png, WebpLl
                       compression: Brotli, BrotliUncompress
        --parallel     Number of concurrent compressions (default: 4)
    -q, --quality      Quality used for output
                       default: 98 for Jpeg, 4 for Brotli
        --sample       The ratio of files to process.
                       Use 0.025 to convert 2.5% of all images
        --pattern      Search pattern to discover files. Defaults to *.bmp
        --minage       Minimal age in days of files to process - exclude files younger than n days
        --maxage       Maximal age in days of files to process - exclude files older than n days

```

# Use it as a service

ImageCompressor can also run as service and regular trigger different task so you don't have to trigger them manually. 
Since it also able to copy (recreating the relative path) and delete the original files, it basically works like a `robocopy` with compression power for images!
Follow all the following steps to get ImageCompressor running as a Service.

## Download the service
Download the `ImageCompressor.Service` from the [releases](https://github.com/Peter-B-/ImageCompressor/releases/) and put it into your favorite location (e.g.: in `C:\tools\imagecompressor`)

## Create a new service

```powershell
> New-Service -Name ImageCompressor -BinaryPathName "C:\tools\imagecompressor\ImageCompressor.Service.exe --contentRoot C:\tools\imagecompressor\I" -Description "ImageCompressor" -DisplayName "ImageCompressor" -StartupType Automatic
```

## Create your settings

Put the json content into a file `appsettings.json` located near to the `ImageCompressor.Service.exe`.

You can even add new compression tasks to be run in certain periods

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "Serilog": {
    "Using":  [ "Serilog.Sinks.Console", "Serilog.Sinks.File" ],
    "MinimumLevel": "Debug",
    "WriteTo": [
      { "Name": "Console" },
      { "Name": "File", "Args": { "path": "Logs/log.txt" } }
    ]
  },
  "Settings": {
    "CompressionTasks": [
      {
        "Name": "Compress",
        "Period": "00:00:30",
        "InitialDelay": "00:00:01",
        "CompressionSettings": {
          "SourcePath": "C:\\temp\\pictures",
          "TargetPath": "C:\\temp\\pictures\\out",
          "DeleteOriginal": true,
          "OverwriteExisting": true,
          "IncludeSubDirectories": true,
          "OutMode": "Jpeg",
          "MinAgeInDays": 30
        }
      }
    ]
  }
}
```
For logging configuration refer to [Serilog](https://github.com/serilog/serilog-settings-configuration) .

## Start the service

```powershell
> Start-Service -Name "ImageCompressor"
```

# Got troubles or wishes?

- Give a star
- Raise an [issue](https://github.com/Peter-B-/ImageCompressor/issues/new)
- Start contribute

