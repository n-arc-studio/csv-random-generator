using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace CsvRandomGenerator
{
    public class CsvGenerator
    {
        private Random _random;

        public CsvGenerator()
        {
            _random = new Random();
        }

        public CsvGenerator(int seed)
        {
            _random = new Random(seed);
        }

        public void GenerateCsv(int rows, int cols, string folder, string output, int? sortColumn, bool append = false, Dictionary<int, DataType>? specifiedTypes = null)
        {
            Directory.CreateDirectory(folder);

            DataType[] columnTypes = new DataType[cols];
            for (int j = 0; j < cols; j++)
            {
                if (specifiedTypes != null && specifiedTypes.TryGetValue(j, out var type))
                {
                    columnTypes[j] = type;
                }
                else
                {
                    columnTypes[j] = (DataType)_random.Next(5);
                }
            }
            List<string[]> data = new List<string[]>();
            for (int i = 0; i < rows; i++)
            {
                string[] row = new string[cols];
                for (int j = 0; j < cols; j++)
                {
                    DataType type = columnTypes[j];
                    switch (type)
                    {
                        case DataType.Int:
                            row[j] = _random.Next(0, 101).ToString();
                            break;
                        case DataType.Double:
                            row[j] = (_random.NextDouble() * 100).ToString("F2");
                            break;
                        case DataType.String:
                            row[j] = new string(Enumerable.Range(0, _random.Next(5, 11)).Select(_ => (char)_random.Next(65, 91)).ToArray());
                            break;
                        case DataType.DateTimeRandom:
                            row[j] = DateTime.Now.AddDays(_random.Next(-365, 365)).AddHours(_random.Next(24)).AddMinutes(_random.Next(60)).AddSeconds(_random.Next(60)).ToString("yyyy/MM/dd HH:mm:ss");
                            break;
                        case DataType.DateTimeNow:
                            row[j] = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                            break;
                        case DataType.Guid:
                            row[j] = Guid.NewGuid().ToString();
                            break;
                    }
                }
                data.Add(row);
            }

            if (!append && sortColumn.HasValue && sortColumn.Value >= 0 && sortColumn.Value < cols)
            {
                data.Sort((a, b) =>
                {
                    DataType type = columnTypes[sortColumn.Value];
                    switch (type)
                    {
                        case DataType.Int:
                            return int.Parse(a[sortColumn.Value]).CompareTo(int.Parse(b[sortColumn.Value]));
                        case DataType.Double:
                            return double.Parse(a[sortColumn.Value]).CompareTo(double.Parse(b[sortColumn.Value]));
                        case DataType.String:
                            return string.Compare(a[sortColumn.Value], b[sortColumn.Value]);
                        case DataType.DateTimeRandom:
                        case DataType.DateTimeNow:
                            return DateTime.Parse(a[sortColumn.Value]).CompareTo(DateTime.Parse(b[sortColumn.Value]));
                        case DataType.Guid:
                            return Guid.Parse(a[sortColumn.Value]).CompareTo(Guid.Parse(b[sortColumn.Value]));
                        default:
                            return 0;
                    }
                });
            }

            using (StreamWriter writer = new StreamWriter(Path.Combine(folder, output), append))
            {
                foreach (var row in data)
                {
                    writer.WriteLine(string.Join(",", row));
                }
            }

            try
            {
                Console.WriteLine($"CSV data appended to {Path.Combine(folder, output)}");
            }
            catch (ObjectDisposedException)
            {
            }
        }
    }
}