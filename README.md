# CscBuilder - シンプルなC#ビルドツール

Windows内蔵のC#コンパイラ（csc.exe）を簡単に使うための汎用ビルドツールです。

## 概要

CscBuilderは、XML形式の設定ファイルからC#プロジェクトをビルドする軽量なツールです。Visual Studioや.NET SDKが不要で、Windows標準のcsc.exeを使用してビルドを行います。

### 特徴

- **シンプルな設定**: XML形式の直感的な設定ファイル
- **ワイルドカード対応**: `*.cs`や`**/*.cs`でソースファイルを一括指定
- **複数ビルド構成**: Debug/Releaseなどの構成に対応
- **プラットフォーム指定**: x86、x64、anycpuに対応
- **自動検出**: カレントディレクトリの`build.xml`を自動検出

## 使い方

### 基本的な使い方

```bash
# カレントディレクトリのbuild.xmlを使用してビルド
CscBuilder.exe

# 設定ファイルを指定してビルド
CscBuilder.exe mybuild.xml
```

### 設定ファイル（build.xml）の例

```xml
<?xml version="1.0" encoding="utf-8"?>
<Project>
  <!-- ビルド設定 -->
  <PropertyGroup>
    <!-- .NET Frameworkバージョン: v4.0.30319, v3.5, v2.0.50727 -->
    <FrameworkVersion>v4.0.30319</FrameworkVersion>

    <!-- 出力タイプ: exe または dll -->
    <OutputType>exe</OutputType>

    <!-- 出力先ディレクトリ -->
    <OutputPath>bin</OutputPath>

    <!-- 出力ファイル名（拡張子なし） -->
    <AssemblyName>MyApp</AssemblyName>

    <!-- プラットフォーム: x86, x64, anycpu -->
    <Platform>anycpu</Platform>

    <!-- 構成: Debug または Release -->
    <Configuration>Release</Configuration>
  </PropertyGroup>

  <!-- ソースファイルと参照 -->
  <ItemGroup>
    <!-- コンパイル対象のソースファイル -->
    <Compile Include="Program.cs" />
    <Compile Include="Helper.cs" />

    <!-- ワイルドカードも使用可能 -->
    <!-- <Compile Include="*.cs" /> -->
    <!-- <Compile Include="**/*.cs" /> 再帰的に全.csファイル -->

    <!-- アセンブリ参照 -->
    <Reference Include="System.dll" />
    <Reference Include="System.Core.dll" />
    <Reference Include="System.Xml.dll" />
  </ItemGroup>
</Project>
```

## 設定項目の詳細

### PropertyGroup

| 項目 | 説明 | 指定可能な値 | デフォルト値 |
|------|------|-------------|------------|
| `FrameworkVersion` | .NET Frameworkバージョン | `v4.0.30319`, `v3.5`, `v2.0.50727` | `v4.0.30319` |
| `OutputType` | 出力ファイルの種類 | `exe`, `dll` | `exe` |
| `OutputPath` | 出力先ディレクトリ | 任意のパス | `bin` |
| `AssemblyName` | 出力ファイル名（拡張子なし） | 任意の文字列 | `output` |
| `Platform` | ターゲットプラットフォーム | `x86`, `x64`, `anycpu` | `anycpu` |
| `Configuration` | ビルド構成 | `Debug`, `Release` | `Release` |

### ItemGroup

#### Compile要素

ソースファイルを指定します。`Include`属性で以下の形式が使用できます：

- 単一ファイル: `Program.cs`
- ワイルドカード: `*.cs` （カレントディレクトリ内の全.csファイル）
- 再帰的検索: `**/*.cs` （サブディレクトリを含む全.csファイル）
- パス指定: `src/*.cs` （特定ディレクトリ内のファイル）

#### Reference要素

参照するアセンブリを指定します。

一般的な参照：
```xml
<Reference Include="System.dll" />
<Reference Include="System.Core.dll" />
<Reference Include="System.Xml.dll" />
<Reference Include="System.Xml.Linq.dll" />
```

## ビルド構成による違い

### Release構成
- 最適化有効（`/optimize+`）
- デバッグ情報なし

### Debug構成
- 最適化無効（`/optimize-`）
- 完全デバッグ情報付き（`/debug+ /debug:full`）

## CscBuilderのビルド方法

CscBuilder自身も同梱のbuild.xmlを使ってビルドできます：

```bash
# まず手動でcscを使って初回ビルド
cd CscBuilder
csc /out:bin\CscBuilder.exe /target:exe /platform:anycpu /optimize+ ^
    /reference:System.dll /reference:System.Core.dll ^
    /reference:System.Xml.dll /reference:System.Xml.Linq.dll ^
    *.cs

# ビルド後は自分自身を使ってビルド可能
bin\CscBuilder.exe
```

## トラブルシューティング

### csc.exeが見つからない

CscBuilderは以下の場所からcsc.exeを検索します：

1. `.NET Framework 4.0` (`C:\Windows\Microsoft.NET\Framework\v4.0.30319\`)
2. `.NET Framework 3.5` (`C:\Windows\Microsoft.NET\Framework\v3.5\`)
3. `.NET Framework 2.0` (`C:\Windows\Microsoft.NET\Framework\v2.0.50727\`)

上記に見つからない場合は、環境変数PATHに.NET Frameworkのパスを追加してください。

### ビルドエラー

ビルドエラーが発生した場合は、cscの出力メッセージを確認してください。一般的な原因：

- ソースファイルが見つからない
- 参照アセンブリが見つからない
- C#のコンパイルエラー（構文エラーなど）
