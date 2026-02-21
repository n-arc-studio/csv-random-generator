# CSV Random Generator

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![codecov](https://codecov.io/gh/symrsonline/csv-random-generator/branch/master/graph/badge.svg)](https://codecov.io/gh/symrsonline/csv-random-generator/tree/master)
[![CI](https://github.com/symrsonline/csv-random-generator/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/symrsonline/csv-random-generator/actions/workflows/ci-cd.yml)

このプロジェクトは、指定された行数と列数のランダムなCSVファイルを生成し、指定されたフォルダに出力します。

## 使用方法

以下のコマンドでプログラムを実行します:

```bash
dotnet run -- --rows <行数> --cols <列数> --output <出力ファイルパス> --sort-column <ソート列> --duration <間隔秒> --max-files <最大ファイル数> --column-types <列データ型> --seed <シード>
```

オプションは順不同で指定可能。ソート列は0から始まる列インデックス、省略可能。durationを指定すると、指定秒ごとにCSVを生成し続けます。max-filesを指定すると、出力フォルダ内のファイル数を制限します。column-typesで特定の列にデータ型を指定できます。`--seed`を指定すると再現可能な乱数で生成できます。

### オプション

- `--rows <number>`: 行数 (1以上、デフォルト: 10)
- `--cols <number>`: 列数 (1以上、デフォルト: 5)
- `--output <path>`: 出力ファイルパス (デフォルト: output.csv)
- `--sort-column <index>`: ソートする列 (0から始まるインデックス、省略可能)
- `--duration <seconds>`: CSV生成間隔 (0以上、継続モード、省略可能)
- `--max-files <number>`: 出力フォルダ内の最大ファイル数 (0で無制限、デフォルト: 0)
- `--column-types <types>`: 列ごとのデータ型指定 (例: '0:int,1:string,2:double')
- `--seed <number>`: 乱数シード (省略可能、同じシードで再現可能)
- `--help, -h`: ヘルプ表示

### 例

基本的な使用:

```bash
dotnet run -- --cols 10 --rows 100 --output ./output/data.csv --sort-column 2
```

これにより、100行10列のランダムデータを`./output`フォルダに生成し、3列目でソートして出力します。

継続的にデータを追加する場合:

```bash
dotnet run -- --rows 10 --cols 5 --output data.csv --duration 5 --max-files 10
```

これにより、5秒ごとに10行5列のCSVを生成し続け、フォルダ内のファイル数を10に制限します。Ctrl+Cで停止。

特定の列にデータ型を指定:

```bash
dotnet run -- --rows 20 --cols 3 --column-types "0:int,1:string,2:datetime" --output custom_types.csv
```

これにより、1列目を整数、2列目を文字列、3列目を日時として20行3列のデータを生成します。

ヘルプを表示するには:

```bash
dotnet run -- --help
```

同じデータを再現したい場合:

```bash
dotnet run -- --rows 20 --cols 4 --seed 42 --output reproducible.csv
```

## 要件

- .NET 8.0 以降

## CI/CD

このプロジェクトはGitHub Actionsを使用してCI/CDを実装しています。

- **ビルドとテスト**: masterブランチへのプッシュまたはプルリクエストで自動的にビルドとテストが実行されます。
- **リリース**: GitHub Releasesを作成すると、Windows用の自己完結型exeファイルが自動的にビルドされ、リリースにアップロードされます。

## テスト

プロジェクトにはxUnitを使用したユニットテストが含まれています。テストを実行するには:

```bash
dotnet test
```

テストはCSV生成の行数列数確認、ソート機能、データ型指定、継続モードなどを検証します。

## データ型

プログラムは列ごとにデータ型を生成します。デフォルトではランダムに以下の型が割り当てられますが、`--column-types`オプションで特定の列に型を指定できます。

利用可能なデータ型:
- `int`: 整数 (0-100)
- `double`: 倍精度浮動小数点数 (0-100、小数点以下2桁)
- `string`: 文字列 (ランダムな大文字アルファベット、5-10文字)
- `datetime:random`: 日時 (yyyy/MM/dd HH:mm:ss形式、ランダムな日時)
- `datetime:now`: 日時 (yyyy/MM/dd HH:mm:ss形式、現在の日時)
- `guid`: GUID (ランダムなGUID)

## ライセンス

このプロジェクトはMITライセンスの下で公開されています。詳細は[LICENSE](LICENSE)ファイルを参照してください。