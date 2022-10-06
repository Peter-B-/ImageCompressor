using System;
using System.Text;
using ImageCompressor;
using Spectre.Cli;

Console.OutputEncoding = Encoding.UTF8;
var app = new CommandApp<CompressImagesCommand>();
return app.Run(args);