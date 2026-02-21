using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

namespace CsvRandomGenerator
{
    public enum DataType { Int, Double, String, DateTimeRandom, DateTimeNow, Guid }

    public class Program
    {
        public static void Main(string[] args)
        {
            var parseResult = ArgumentParser.ParseCommandLine(args);

            if (parseResult.ShowHelp)
            {
                PrintHelp();
                return;
            }

            if (parseResult.HasErrors)
            {
                PrintErrors(parseResult.Errors);
                return;
            }

            foreach (var warning in parseResult.Warnings)
            {
                Console.WriteLine($"Warning: {warning}");
            }

            var options = parseResult.Options;

            if (!ArgumentParser.TryGetPositiveInt(options, "rows", 10, out int rows, out string? rowsError))
            {
                PrintErrors(new[] { rowsError! });
                return;
            }

            if (!ArgumentParser.TryGetPositiveInt(options, "cols", 5, out int cols, out string? colsError))
            {
                PrintErrors(new[] { colsError! });
                return;
            }

            if (!ArgumentParser.TryGetNonNegativeInt(options, "duration", 0, out int duration, out string? durationError))
            {
                PrintErrors(new[] { durationError! });
                return;
            }

            if (!ArgumentParser.TryGetNonNegativeInt(options, "max-files", 0, out int maxFiles, out string? maxFilesError))
            {
                PrintErrors(new[] { maxFilesError! });
                return;
            }

            int? sortColumn = ArgumentParser.GetOptionNullable(options, "sort-column");
            if (options.ContainsKey("sort-column") && !sortColumn.HasValue)
            {
                PrintErrors(new[] { $"--sort-column には整数を指定してください。入力値: '{options["sort-column"]}'" });
                return;
            }

            if (sortColumn.HasValue && (sortColumn.Value < 0 || sortColumn.Value >= cols))
            {
                PrintErrors(new[] { $"--sort-column は 0 以上 {cols - 1} 以下で指定してください。入力値: {sortColumn.Value}" });
                return;
            }

            string outputPath = ArgumentParser.GetOption(options, "output", "output.csv");
            string? folderCandidate = Path.GetDirectoryName(outputPath);
            string folder = string.IsNullOrWhiteSpace(folderCandidate) ? "." : folderCandidate;
            string output = Path.GetFileName(outputPath);
            string baseName = Path.GetFileNameWithoutExtension(output);
            string extension = Path.GetExtension(output);

            if (!string.IsNullOrWhiteSpace(folder) && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            Dictionary<int, DataType>? columnTypes = null;
            if (options.TryGetValue("column-types", out var typesStr))
            {
                if (!ArgumentParser.TryParseColumnTypes(typesStr, out var parsedColumnTypes, out var parseErrors))
                {
                    PrintErrors(parseErrors);
                    return;
                }

                if (parsedColumnTypes.Keys.Any(k => k >= cols))
                {
                    var invalidColumns = string.Join(", ", parsedColumnTypes.Keys.Where(k => k >= cols).OrderBy(k => k));
                    PrintErrors(new[] { $"--column-types の列インデックスが範囲外です（cols={cols}）。範囲外: {invalidColumns}" });
                    return;
                }

                columnTypes = parsedColumnTypes;
            }

            CsvGenerator generator;
            if (options.TryGetValue("seed", out var seedRaw))
            {
                if (!int.TryParse(seedRaw, out var seed))
                {
                    PrintErrors(new[] { $"--seed には整数を指定してください。入力値: '{seedRaw}'" });
                    return;
                }

                generator = new CsvGenerator(seed);
                Console.WriteLine($"Seed: {seed}");
            }
            else
            {
                generator = new CsvGenerator();
            }

            if (duration > 0)
            {
                Console.WriteLine($"Generating CSV every {duration} seconds. Press Ctrl+C to stop.");
                while (true)
                {
                    if (maxFiles == 0 || Directory.GetFiles(folder).Length < maxFiles)
                    {
                        string timestampedOutput = baseName + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + extension;
                        generator.GenerateCsv(rows, cols, folder, timestampedOutput, null, append: false, columnTypes);
                    }
                    Thread.Sleep(duration * 1000);
                }
            }
            else
            {
                string timestampedOutput = baseName + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + extension;
                generator.GenerateCsv(rows, cols, folder, timestampedOutput, sortColumn, append: false, columnTypes);
            }
        }

        static void PrintHelp()
        {
            Console.WriteLine("CSV Random Generator");
            Console.WriteLine("Generate random CSV files with specified rows and columns.");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  dotnet run -- [options]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  --rows <number>        Number of rows (1 or more, default: 10)");
            Console.WriteLine("  --cols <number>        Number of columns (1 or more, default: 5)");
            Console.WriteLine("  --output <path>        Output file path (default: output.csv)");
            Console.WriteLine("  --sort-column <index>  Column to sort by (0-based index, optional)");
            Console.WriteLine("  --duration <seconds>   Interval to append data (0 or more, optional, continuous mode)");
            Console.WriteLine("  --max-files <number>   Maximum number of files in output folder (0 for unlimited, default: 0)");
            Console.WriteLine("  --column-types <types> Specify data types for columns (e.g., '0:int,1:string,2:datetime:random')");
            Console.WriteLine("  --seed <number>        Random seed (optional, deterministic output)");
            Console.WriteLine("  --help, -h             Show this help message");
            Console.WriteLine();
            Console.WriteLine("Data Types:");
            Console.WriteLine("  int: Integer (0-100)");
            Console.WriteLine("  double: Double precision float (0-100)");
            Console.WriteLine("  string: Random uppercase string (5-10 chars)");
            Console.WriteLine("  datetime:random: Random datetime (yyyy/MM/dd HH:mm:ss)");
            Console.WriteLine("  datetime:now: Current datetime (yyyy/MM/dd HH:mm:ss)");
            Console.WriteLine("  guid: Random GUID");
            Console.WriteLine("  If no subtype specified for datetime, defaults to random.");
        }

        static void PrintErrors(IEnumerable<string> errors)
        {
            foreach (var error in errors)
            {
                Console.Error.WriteLine($"Error: {error}");
            }

            Console.WriteLine();
            Console.WriteLine("ヘルプ表示: dotnet run -- --help");
        }
    }
}
