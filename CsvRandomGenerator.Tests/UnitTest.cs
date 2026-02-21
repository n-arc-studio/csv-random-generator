using System.IO;
using Xunit;
using CsvRandomGenerator;
using System.Text;

namespace CsvRandomGenerator.Tests;

[Collection("Sequential")]
public class UnitTest
{
    [Fact]
    public void TestCsvGeneration()
    {
        // Arrange
        string tempPath = Path.GetTempPath();
        string folder = Path.Combine(tempPath, "CsvRandomGeneratorTests");
        Directory.CreateDirectory(folder);
        string output = "test.csv";

        // Act
        var generator = new CsvGenerator();
        generator.GenerateCsv(5, 3, folder, output, null, false, null);

        // Assert
        var files = Directory.GetFiles(folder, "test.csv");
        Assert.True(files.Length > 0);
        var outputPath = files.First();
        var lines = File.ReadAllLines(outputPath);
        Assert.Equal(5, lines.Length);
        foreach (var line in lines)
        {
            var columns = line.Split(',');
            Assert.Equal(3, columns.Length);
        }

        // Cleanup
        Directory.Delete(folder, true);
    }

    [Fact]
    public void TestCsvSorting()
    {
        // Arrange
        string tempPath = Path.GetTempPath();
        string folder = Path.Combine(tempPath, "CsvRandomGeneratorTests");
        Directory.CreateDirectory(folder);
        string output = "test_sorted.csv";

        // Act
        var generator = new CsvGenerator();
        generator.GenerateCsv(10, 2, folder, output, 0, false, null); // Sort by first column

        // Assert
        var files = Directory.GetFiles(folder, "test_sorted.csv");
        Assert.True(files.Length > 0);
        var outputPath = files.First();
        Assert.True(File.Exists(outputPath));
        var lines = File.ReadAllLines(outputPath);
        Assert.Equal(10, lines.Length);

        // Check if sorted (assuming first column is numeric or string)
        for (int i = 1; i < lines.Length; i++)
        {
            var prevParts = lines[i - 1].Split(',');
            var currParts = lines[i].Split(',');
            var prev = prevParts[0];
            var curr = currParts[0];
            // Try to parse as double, if not, compare as string
            if (double.TryParse(prev, out var prevNum) && double.TryParse(curr, out var currNum))
            {
                Assert.True(prevNum <= currNum);
            }
            else if (DateTime.TryParse(prev, out var prevDate) && DateTime.TryParse(curr, out var currDate))
            {
                Assert.True(prevDate <= currDate);
            }
            else
            {
                Assert.True(string.Compare(prev, curr) <= 0);
            }
        }

        // Cleanup
        Directory.Delete(folder, true);
    }

    [Fact]
    public void TestSortingDoubleAndString()
    {
        // Arrange
        string tempPath = Path.GetTempPath();
        string folder = Path.Combine(tempPath, "CsvRandomGeneratorTests");
        Directory.CreateDirectory(folder);
        string output = "test_sort_double_string.csv";

        // Act: Generate CSV with sorting by first column
        var generator = new CsvGenerator();
        generator.GenerateCsv(20, 3, folder, output, 0, false, null); // Sort by first column

        // Assert
        var files = Directory.GetFiles(folder, "test_sort_double_string.csv");
        Assert.True(files.Length > 0);
        var outputPath = files.First();
        var lines = File.ReadAllLines(outputPath);
        Assert.Equal(20, lines.Length);

        // Check if sorted (first column should be sorted as double or string)
        for (int i = 1; i < lines.Length; i++)
        {
            var prevParts = lines[i - 1].Split(',');
            var currParts = lines[i].Split(',');
            var prev = prevParts[0];
            var curr = currParts[0];
            // Try to parse as double, if not, compare as string
            if (double.TryParse(prev, out var prevNum) && double.TryParse(curr, out var currNum))
            {
                Assert.True(prevNum <= currNum, $"Double sort failed: {prevNum} > {currNum}");
            }
            else
            {
                Assert.True(string.Compare(prev, curr) <= 0, $"String sort failed: {prev} > {curr}");
            }
        }

        // Cleanup
        Directory.Delete(folder, true);
    }

    [Fact]
    public void TestCsvAppend()
    {
        // Arrange
        string tempPath = Path.GetTempPath();
        string folder = Path.Combine(tempPath, "CsvRandomGeneratorTests");
        Directory.CreateDirectory(folder);
        string output = "test_append.csv";

        // Act: First generate
        var generator = new CsvGenerator();
        generator.GenerateCsv(3, 2, folder, output, null, false, null);
        // Then append
        generator.GenerateCsv(2, 2, folder, output, null, true, null);

        // Assert
        var files = Directory.GetFiles(folder, "test_append.csv");
        Assert.True(files.Length >= 1);
        var outputPath = files.First();
        var lines = File.ReadAllLines(outputPath);
        Assert.Equal(5, lines.Length); // 3 + 2
        foreach (var line in lines)
        {
            var columns = line.Split(',');
            Assert.Equal(2, columns.Length);
        }

        // Cleanup
        Directory.Delete(folder, true);
    }

    [Fact]
    public void TestInvalidSortColumn()
    {
        // Arrange
        string tempPath = Path.GetTempPath();
        string outputPath = Path.Combine(tempPath, "test_invalid_sort.csv");
        string folder = Path.GetDirectoryName(outputPath) ?? tempPath;
        string output = Path.GetFileName(outputPath);

        // Act: Sort by invalid column (out of range)
        var generator = new CsvGenerator();
        generator.GenerateCsv(5, 2, folder, output, 5, false, null); // cols=2, so 5 is invalid

        // Assert: Should still generate file without sorting
        Assert.True(File.Exists(outputPath));
        var lines = File.ReadAllLines(outputPath);
        Assert.Equal(5, lines.Length);

        // Cleanup
        File.Delete(outputPath);
    }

    [Fact]
    public void TestHelp()
    {
        // Arrange
        var stringWriter = new StringWriter();
        var originalOut = Console.Out;
        Console.SetOut(stringWriter);

        // Act
        CsvRandomGenerator.Program.Main(new string[] { "--help" });

        // Assert
        var output = stringWriter.ToString();
        Assert.Contains("CSV Random Generator", output);
        Assert.Contains("--rows", output);
        Assert.Contains("--cols", output);
        Assert.Contains("--output", output);
        Assert.Contains("--sort-column", output);
        Assert.Contains("--duration", output);
        Assert.Contains("--help", output);

        // Cleanup
        Console.SetOut(originalOut);
        stringWriter.Dispose();
    }

    [Fact]
    public void TestHelpShort()
    {
        // Arrange
        var stringWriter = new StringWriter();
        var originalOut = Console.Out;
        Console.SetOut(stringWriter);

        // Act
        CsvRandomGenerator.Program.Main(new string[] { "-h" });

        // Assert
        var output = stringWriter.ToString();
        Assert.Contains("CSV Random Generator", output);
        Assert.Contains("--rows", output);
        Assert.Contains("--cols", output);
        Assert.Contains("--output", output);
        Assert.Contains("--sort-column", output);
        Assert.Contains("--duration", output);
        Assert.Contains("--help", output);

        // Cleanup
        Console.SetOut(originalOut);
        stringWriter.Dispose();
    }

    [Fact]
    public void TestMainEmptyArgs()
    {
        // Arrange
        var stringWriter = new StringWriter();
        var originalOut = Console.Out;
        Console.SetOut(stringWriter);

        // Act
        CsvRandomGenerator.Program.Main(new string[] { });

        // Assert: Should show help when no args
        var output = stringWriter.ToString();
        Assert.Contains("CSV Random Generator", output);

        // Cleanup
        Console.SetOut(originalOut);
        stringWriter.Dispose();
    }

    [Theory]
    [InlineData(new string[] {}, 0)]
    [InlineData(new string[] { "--rows", "10" }, 1)]
    [InlineData(new string[] { "--rows", "10", "--cols", "5" }, 2)]
    [InlineData(new string[] { "--rows", "10", "--cols", "5", "--output", "test.csv" }, 3)]
    [InlineData(new string[] { "--rows", "10", "--cols", "5", "--output", "test.csv", "--sort-column", "0" }, 4)]
    [InlineData(new string[] { "--rows", "10", "--cols", "5", "--output", "test.csv", "--sort-column", "0", "--duration", "30" }, 5)]
    [InlineData(new string[] { "--rows", "10", "--cols", "5", "--output", "test.csv", "--sort-column", "0", "--duration", "30", "--max-files", "10" }, 6)]
    [InlineData(new string[] { "--rows" }, 0)] // Odd number, no value
    [InlineData(new string[] { "rows", "10" }, 0)] // Not starting with --
    [InlineData(new string[] { "--help" }, 0)] // No value
    [InlineData(new string[] { "--rows", "10", "extra" }, 1)] // Extra arg
    public void TestParseArgs(string[] args, int expectedCount)
    {
        // Act
        var result = ArgumentParser.ParseArgs(args);

        // Assert
        Assert.Equal(expectedCount, result.Count);
    }

    [Fact]
    public void TestParseArgsValues()
    {
        // Arrange
        var args = new string[] { "--rows", "20", "--cols", "3", "--output", "myfile.csv", "--sort-column", "1", "--duration", "60", "--max-files", "5" };

        // Act
        var result = ArgumentParser.ParseArgs(args);

        // Assert
        Assert.Equal("20", result["rows"]);
        Assert.Equal("3", result["cols"]);
        Assert.Equal("myfile.csv", result["output"]);
        Assert.Equal("1", result["sort-column"]);
        Assert.Equal("60", result["duration"]);
        Assert.Equal("5", result["max-files"]);
    }

    [Fact(Timeout = 2000)]
    public async Task TestDurationMode()
    {
        // Arrange
        var stringWriter = new StringWriter();
        var originalOut = Console.Out;
        Console.SetOut(stringWriter);

        // Act: Start Main with duration > 0 in a separate task
        var task = Task.Run(() => CsvRandomGenerator.Program.Main(new string[] { "--rows", "5", "--cols", "3", "--duration", "1" }));

        // Wait a bit for the output
        await Task.Delay(500);

        // Assert
        var output = stringWriter.ToString();
        Assert.Contains("Generating CSV every 1 seconds. Press Ctrl+C to stop.", output);

        // Cleanup: Cancel the task (simulate Ctrl+C)
        // Since it's infinite loop, we need to abort the task
        // For simplicity, just check the output and let the test timeout if needed
        // In practice, this test will timeout after 2 seconds due to Timeout attribute

        Console.SetOut(originalOut);
        stringWriter.Dispose();
    }

    [Fact(Timeout = 10000)]
    public async Task TestMaxFilesExceeded()
    {
        // Arrange
        string tempPath = Path.GetTempPath();
        string folder = Path.Combine(tempPath, "CsvRandomGeneratorTests_" + Guid.NewGuid().ToString());
        Directory.CreateDirectory(folder);

        // Clean up any existing files
        foreach (var file in Directory.GetFiles(folder, "test_maxfiles_exceeded_*.csv"))
        {
            File.Delete(file);
        }

        var stringWriter = new StringWriter();
        var originalOut = Console.Out;
        Console.SetOut(stringWriter);

        // Act: Start Main with duration > 0 and max-files = 2
        var task = Task.Run(() => CsvRandomGenerator.Program.Main(new string[] { "--rows", "5", "--cols", "3", "--output", Path.Combine(folder, "test_maxfiles_exceeded.csv"), "--duration", "1", "--max-files", "2" }));

        // Wait for more than 2 seconds to allow 3 generations, but should stop at 2
        await Task.Delay(3500);

        // Assert
        var files = Directory.GetFiles(folder, "test_maxfiles_exceeded_*.csv");
        Assert.True(files.Length <= 2, $"Expected <= 2 files, but got {files.Length}");

        // Cleanup
        foreach (var file in files)
        {
            File.Delete(file);
        }
        Directory.Delete(folder);
        Console.SetOut(originalOut);
        stringWriter.Dispose();
    }

    [Fact]
    public void TestDataTypes()
    {
        // Arrange
        string tempPath = Path.GetTempPath();
        string folder = Path.Combine(tempPath, "CsvRandomGeneratorTests_" + Guid.NewGuid().ToString());
        Directory.CreateDirectory(folder);
        string output = "test_datatypes.csv";

        // Specify types for each column to ensure all types are present
        var specifiedTypes = new Dictionary<int, DataType>
        {
            {0, DataType.Int},
            {1, DataType.Double},
            {2, DataType.String},
            {3, DataType.DateTimeRandom},
            {4, DataType.DateTimeNow},
            {5, DataType.Guid}
        };

        // Act: Generate CSV with specified types
        var generator = new CsvGenerator();
        generator.GenerateCsv(100, 6, folder, output, null, false, specifiedTypes);

        // Assert
        var files = Directory.GetFiles(folder, "test_datatypes.csv");
        Assert.True(files.Length > 0);
        var outputPath = files.First();
        var lines = File.ReadAllLines(outputPath);
        Assert.Equal(100, lines.Length);

        bool hasInt = false, hasDouble = false, hasString = false, hasDateTimeRandom = false, hasDateTimeNow = false, hasGuid = false;

        foreach (var line in lines)
        {
            var columns = line.Split(',');
            for (int i = 0; i < columns.Length; i++)
            {
                var col = columns[i];
                switch (i)
                {
                    case 0:
                        if (int.TryParse(col, out _))
                            hasInt = true;
                        break;
                    case 1:
                        if (double.TryParse(col, out _) && col.Contains('.'))
                            hasDouble = true;
                        break;
                    case 2:
                        if (System.Text.RegularExpressions.Regex.IsMatch(col, @"^[A-Z]{5,10}$"))
                            hasString = true;
                        break;
                    case 3:
                        if (DateTime.TryParseExact(col, "yyyy/MM/dd HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out _))
                            hasDateTimeRandom = true;
                        break;
                    case 4:
                        if (DateTime.TryParse(col, out _))
                            hasDateTimeNow = true;
                        break;
                    case 5:
                        if (Guid.TryParse(col, out _))
                            hasGuid = true;
                        break;
                }
            }
        }

        Assert.True(hasInt, "Should have integer columns");
        Assert.True(hasDouble, "Should have double columns");
        Assert.True(hasString, "Should have string columns");
        Assert.True(hasDateTimeRandom, "Should have datetime (random) columns");
        Assert.True(hasDateTimeNow, "Should have datetime (now) columns");
        Assert.True(hasGuid, "Should have guid columns");

        // Cleanup
        Directory.Delete(folder, true);
    }

    [Fact]
    public void TestNormalMode()
    {
        // Arrange
        string tempPath = Path.GetTempPath();
        string folder = Path.Combine(tempPath, "CsvRandomGeneratorTests");
        Directory.CreateDirectory(folder);
        string output = "test_normal.csv";

        // Act: Generate CSV in normal mode (duration = 0)
        var generator = new CsvGenerator();
        generator.GenerateCsv(5, 3, folder, output, null, false, null);

        // Assert
        var files = Directory.GetFiles(folder, "test_normal.csv");
        Assert.True(files.Length > 0);
        var outputPath = files.First();
        var lines = File.ReadAllLines(outputPath);
        Assert.Equal(5, lines.Length);
        foreach (var line in lines)
        {
            var columns = line.Split(',');
            Assert.Equal(3, columns.Length);
        }

        // Cleanup
        Directory.Delete(folder, true);
    }

    [Fact]
    public void TestFolderCreation()
    {
        // Arrange
        string tempPath = Path.GetTempPath();
        string nonExistentFolder = Path.Combine(tempPath, "NonExistentFolder");
        string output = "test_folder_creation.csv";

        // Ensure folder does not exist
        if (Directory.Exists(nonExistentFolder))
        {
            Directory.Delete(nonExistentFolder, true);
        }

        // Act: Generate CSV, folder should be created automatically
        var generator = new CsvGenerator();
        generator.GenerateCsv(3, 2, nonExistentFolder, output, null, false, null);

        // Assert
        Assert.True(Directory.Exists(nonExistentFolder));
        var files = Directory.GetFiles(nonExistentFolder, "test_folder_creation.csv");
        Assert.True(files.Length > 0);

        // Cleanup
        Directory.Delete(nonExistentFolder, true);
    }

    [Fact]
    public void TestColumnTypesSpecified()
    {
        // Arrange
        string tempPath = Path.GetTempPath();
        string folder = Path.Combine(tempPath, "CsvRandomGeneratorTests");
        Directory.CreateDirectory(folder);
        string output = "test_column_types.csv";
        var specifiedTypes = new Dictionary<int, CsvRandomGenerator.DataType>
        {
            {0, CsvRandomGenerator.DataType.Int},
            {1, CsvRandomGenerator.DataType.String},
            {2, CsvRandomGenerator.DataType.Double},
            {3, CsvRandomGenerator.DataType.Guid}
        };

        // Act
        var generator = new CsvGenerator();
        generator.GenerateCsv(5, 4, folder, output, null, false, specifiedTypes);

        // Assert
        var files = Directory.GetFiles(folder, "test_column_types.csv");
        Assert.True(files.Length > 0);
        var outputPath = files.First();
        var lines = File.ReadAllLines(outputPath);
        Assert.Equal(5, lines.Length);
        foreach (var line in lines)
        {
            var columns = line.Split(',');
            Assert.Equal(4, columns.Length);
            // Check types: col0 int, col1 string, col2 double, col3 guid
            Assert.True(int.TryParse(columns[0], out _));
            Assert.True(double.TryParse(columns[2], out _));
            Assert.True(Guid.TryParse(columns[3], out _));
            // String is harder to check, but assume it's uppercase letters
        }

        // Cleanup
        Directory.Delete(folder, true);
    }

    [Fact]
    public void TestArgumentParserParseArgs()
    {
        // Arrange
        string[] args = { "--rows", "100", "--cols", "5", "--output", "test.csv" };

        // Act
        var options = ArgumentParser.ParseArgs(args);

        // Assert
        Assert.Equal("100", options["rows"]);
        Assert.Equal("5", options["cols"]);
        Assert.Equal("test.csv", options["output"]);
    }

    [Fact]
    public void TestArgumentParserGetOption()
    {
        // Arrange
        var options = new Dictionary<string, string> { { "rows", "50" }, { "cols", "10" } };

        // Act & Assert
        Assert.Equal(50, ArgumentParser.GetOption(options, "rows", 10));
        Assert.Equal(10, ArgumentParser.GetOption(options, "cols", 5));
        Assert.Equal(5, ArgumentParser.GetOption(options, "missing", 5));
    }

    [Fact]
    public void TestArgumentParserParseColumnTypes()
    {
        // Arrange
        string typesStr = "0:int,1:string,2:double";

        // Act
        var result = ArgumentParser.ParseColumnTypes(typesStr);

        // Assert
        Assert.Equal(DataType.Int, result[0]);
        Assert.Equal(DataType.String, result[1]);
        Assert.Equal(DataType.Double, result[2]);
    }

    [Fact]
    public void TestCsvGeneratorWithSeed()
    {
        // Arrange
        string tempPath = Path.GetTempPath();
        string folder = Path.Combine(tempPath, "CsvRandomGeneratorTests");
        Directory.CreateDirectory(folder);
        string output1 = "test_seed1.csv";
        string output2 = "test_seed2.csv";

        // Act
        var generator1 = new CsvGenerator(42);
        generator1.GenerateCsv(5, 3, folder, output1, null, false, null);
        var generator2 = new CsvGenerator(42);
        generator2.GenerateCsv(5, 3, folder, output2, null, false, null);

        // Assert
        var lines1 = File.ReadAllLines(Path.Combine(folder, output1));
        var lines2 = File.ReadAllLines(Path.Combine(folder, output2));
        Assert.Equal(lines1.Length, lines2.Length);
        for (int i = 0; i < lines1.Length; i++)
        {
            Assert.Equal(lines1[i], lines2[i]);
        }

        // Cleanup
        Directory.Delete(folder, true);
    }

    [Fact]
    public void TestMainUnknownOptionShowsError()
    {
        var originalOut = Console.Out;
        var originalErr = Console.Error;
        var outWriter = new StringWriter();
        var errWriter = new StringWriter();
        Console.SetOut(outWriter);
        Console.SetError(errWriter);

        try
        {
            CsvRandomGenerator.Program.Main(new string[] { "--unknown", "1" });
            var err = errWriter.ToString();
            Assert.Contains("Error:", err);
            Assert.Contains("未知のオプション", err);
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalErr);
            outWriter.Dispose();
            errWriter.Dispose();
        }
    }

    [Fact]
    public void TestMainInvalidRowsShowsError()
    {
        var originalOut = Console.Out;
        var originalErr = Console.Error;
        var outWriter = new StringWriter();
        var errWriter = new StringWriter();
        Console.SetOut(outWriter);
        Console.SetError(errWriter);

        try
        {
            CsvRandomGenerator.Program.Main(new string[] { "--rows", "0", "--cols", "3" });
            var err = errWriter.ToString();
            Assert.Contains("Error:", err);
            Assert.Contains("--rows", err);
            Assert.Contains("1 以上", err);
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalErr);
            outWriter.Dispose();
            errWriter.Dispose();
        }
    }

    [Fact]
    public void TestMainWithSeedOutputsSeedMessage()
    {
        var originalOut = Console.Out;
        var outWriter = new StringWriter();
        Console.SetOut(outWriter);

        string tempPath = Path.GetTempPath();
        string folder = Path.Combine(tempPath, "CsvRandomGeneratorTests_" + Guid.NewGuid().ToString());
        string outputPath = Path.Combine(folder, "seed_output.csv");

        try
        {
            CsvRandomGenerator.Program.Main(new string[] { "--rows", "3", "--cols", "2", "--seed", "42", "--output", outputPath });
            var output = outWriter.ToString();
            Assert.Contains("Seed: 42", output);
            Assert.True(Directory.GetFiles(folder, "seed_output_*.csv").Length >= 1);
        }
        finally
        {
            Console.SetOut(originalOut);
            outWriter.Dispose();
            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, true);
            }
        }
    }
}
