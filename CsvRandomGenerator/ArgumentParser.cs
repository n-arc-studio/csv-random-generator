using System;
using System.Collections.Generic;
using System.Linq;

namespace CsvRandomGenerator
{
    public class CommandLineParseResult
    {
        public Dictionary<string, string> Options { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public List<string> Errors { get; } = new List<string>();
        public List<string> Warnings { get; } = new List<string>();
        public bool ShowHelp { get; set; }

        public bool HasErrors => Errors.Count > 0;
    }

    public class ArgumentParser
    {
        private static readonly HashSet<string> KnownOptions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "rows",
            "cols",
            "output",
            "sort-column",
            "duration",
            "max-files",
            "column-types",
            "seed",
            "help",
            "h"
        };

        public static Dictionary<string, string> ParseArgs(string[] args)
        {
            var options = new Dictionary<string, string>();
            for (int i = 0; i < args.Length; i += 2)
            {
                if (args[i].StartsWith("--") && i + 1 < args.Length)
                {
                    options[args[i].Substring(2)] = args[i + 1];
                }
            }
            return options;
        }

        public static CommandLineParseResult ParseCommandLine(string[] args)
        {
            var result = new CommandLineParseResult();

            if (args.Length == 0)
            {
                result.ShowHelp = true;
                return result;
            }

            for (int i = 0; i < args.Length; i++)
            {
                string token = args[i];

                if (token == "--help" || token == "-h")
                {
                    result.ShowHelp = true;
                    continue;
                }

                if (!token.StartsWith("--"))
                {
                    result.Errors.Add($"不明な引数です: '{token}'");
                    continue;
                }

                string name;
                string? value = null;
                int equalIndex = token.IndexOf('=');
                if (equalIndex > 2)
                {
                    name = token.Substring(2, equalIndex - 2);
                    value = token.Substring(equalIndex + 1);
                }
                else
                {
                    name = token.Substring(2);
                }

                if (!KnownOptions.Contains(name))
                {
                    result.Errors.Add($"未知のオプションです: '--{name}'");
                    continue;
                }

                if (name.Equals("help", StringComparison.OrdinalIgnoreCase) || name.Equals("h", StringComparison.OrdinalIgnoreCase))
                {
                    result.ShowHelp = true;
                    continue;
                }

                if (value == null)
                {
                    if (i + 1 >= args.Length || args[i + 1].StartsWith("--"))
                    {
                        result.Errors.Add($"オプション '--{name}' に値が指定されていません。");
                        continue;
                    }

                    value = args[++i];
                }

                if (result.Options.ContainsKey(name))
                {
                    result.Warnings.Add($"オプション '--{name}' が複数回指定されたため、最後の値 '{value}' を使用します。");
                }

                result.Options[name] = value;
            }

            return result;
        }

        public static int GetOption(Dictionary<string, string> options, string key, int defaultValue)
        {
            return options.TryGetValue(key, out var value) && int.TryParse(value, out var result) ? result : defaultValue;
        }

        public static string GetOption(Dictionary<string, string> options, string key, string defaultValue)
        {
            return options.TryGetValue(key, out var value) ? value : defaultValue;
        }

        public static int? GetOptionNullable(Dictionary<string, string> options, string key)
        {
            return options.TryGetValue(key, out var value) && int.TryParse(value, out var result) ? result : (int?)null;
        }

        public static bool TryGetNonNegativeInt(Dictionary<string, string> options, string key, int defaultValue, out int value, out string? error)
        {
            value = defaultValue;
            error = null;

            if (!options.TryGetValue(key, out var rawValue))
            {
                return true;
            }

            if (!int.TryParse(rawValue, out value))
            {
                error = $"--{key} には整数を指定してください。入力値: '{rawValue}'";
                return false;
            }

            if (value < 0)
            {
                error = $"--{key} には 0 以上の整数を指定してください。入力値: {value}";
                return false;
            }

            return true;
        }

        public static bool TryGetPositiveInt(Dictionary<string, string> options, string key, int defaultValue, out int value, out string? error)
        {
            if (!TryGetNonNegativeInt(options, key, defaultValue, out value, out error))
            {
                return false;
            }

            if (value == 0)
            {
                error = $"--{key} には 1 以上の整数を指定してください。";
                return false;
            }

            return true;
        }

        public static bool TryParseColumnTypes(string typesStr, out Dictionary<int, DataType> columnTypes, out List<string> errors)
        {
            columnTypes = new Dictionary<int, DataType>();
            errors = new List<string>();

            if (string.IsNullOrWhiteSpace(typesStr))
            {
                errors.Add("--column-types が空です。例: 0:int,1:string,2:datetime:random");
                return false;
            }

            var pairs = typesStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var pair in pairs)
            {
                var parts = pair.Split(':', StringSplitOptions.TrimEntries);
                if (parts.Length < 2)
                {
                    errors.Add($"--column-types の形式が不正です: '{pair}'");
                    continue;
                }

                if (!int.TryParse(parts[0], out var col) || col < 0)
                {
                    errors.Add($"列インデックスが不正です: '{parts[0]}'");
                    continue;
                }

                string typeStr = parts[1].ToLowerInvariant();
                DataType type;

                if (typeStr == "datetime")
                {
                    if (parts.Length >= 3)
                    {
                        string subType = parts[2].ToLowerInvariant();
                        if (subType == "random")
                        {
                            type = DataType.DateTimeRandom;
                        }
                        else if (subType == "now")
                        {
                            type = DataType.DateTimeNow;
                        }
                        else
                        {
                            errors.Add($"datetime のサブタイプが不正です: '{parts[2]}'（使用可: random, now）");
                            continue;
                        }
                    }
                    else
                    {
                        type = DataType.DateTimeRandom;
                    }
                }
                else if (!Enum.TryParse(typeStr, true, out type))
                {
                    errors.Add($"データ型が不正です: '{parts[1]}'");
                    continue;
                }

                columnTypes[col] = type;
            }

            return errors.Count == 0;
        }

        public static Dictionary<int, DataType> ParseColumnTypes(string typesStr)
        {
            TryParseColumnTypes(typesStr, out var dict, out _);
            return dict;
        }
    }
}