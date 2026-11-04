# Exhale Batch - xHE-AAC Encoder Frontend

A lightweight, high-performance GUI frontend for the xHE-AAC encoder (**exhale**) integrated with **FFmpeg** on Windows 11.

[日本語の案内はこちら](#日本語)

---

## English

### Overview
**Exhale Batch** is a multi-threaded batch encoding utility designed to encode various audio formats into high-efficiency xHE-AAC (`.m4a`) files using `exhale.exe` and `ffmpeg.exe`.

### Key Features
- **Reliable Shell Piping Pipeline**: Streams decoded PCM WAV output from FFmpeg directly into exhale via a standard shell pipeline (`cmd.exe /c`), completely eliminating intermediate WAV files on disk and reducing SSD wear.
- **Parallel Multi-Core Encoding**: Fully utilizes CPU resources with configurable concurrency limits (1 to 16 parallel tasks).
- **Directory Structure Mirroring**: Recursively scans folders and mirrors the input folder hierarchy directly into the destination folder.
- **External Image Copying**: Automatically copies folder artwork (`cover.jpg`, `folder.png`, etc.) directly into destination album directories without re-compression.
- **Robust Metadata & Artwork Mapping**: Explicitly maps ID3/FLAC metadata tags and picture frames into Apple/MP4 container specifications via `TagLibSharp`.
- **Windows 11 Native Integration**: Native Toast notifications with one-click actions to open output directories, dark mode UI, and safe resource loading.

### Requirements & Setup

To comply with GPL (FFmpeg) and licensing restrictions (xHE-AAC reference code patents), external binaries are **not bundled** with this application. You must place them into the application directory manually.

#### Folder Layout
Place `ffmpeg.exe` and `exhale.exe` in the same directory as `ExhaleBatch.exe`:
```text
ExhaleBatch/
├── ExhaleBatch.exe
├── ffmpeg.exe    <-- Required (Place manually)
└── exhale.exe    <-- Required (Place manually)
```

#### Binary Download Links
1. **FFmpeg (`ffmpeg.exe`)**
   - Download the official essentials release from [gyan.dev FFmpeg Builds](https://www.gyan.dev/ffmpeg/builds/).
   - Extract `ffmpeg-release-essentials.zip` and copy `bin/ffmpeg.exe` into the application directory.
2. **exhale (`exhale.exe`)**
   - Download the Windows x64 binary from [RareWares AAC Encoders](https://www.rarewares.org/aac-encoders.php) (or the official project page).
   - Extract `exhale-x64.zip` and copy `exhale.exe` into the application directory.

### exhale Quality Presets (Stereo 2ch)

| Preset | Target Bitrate | Typical Use Case |
| :---: | :---: | :--- |
| **0** | ~12 - 16 kbps | Ultra-low bitrate / Speech |
| **1** | ~16 - 24 kbps | Very low bitrate |
| **2** | ~24 - 32 kbps | Low bitrate |
| **3** | ~32 - 48 kbps | Podcasts, background audio |
| **4** | ~48 - 64 kbps | Mobile streaming standard |
| **5** | **~64 - 96 kbps** | **Recommended Default (High-efficiency Music)** |
| **6** | ~96 - 128 kbps | High Quality |
| **7** | ~128 - 160 kbps | Near-transparent quality |
| **8** | ~160 - 192 kbps | Transparent studio quality |
| **9** | ~192 - 256+ kbps | Maximum quality mode |

---

<a id="日本語"></a>
## 日本語 (Japanese)

### 概要
**Exhale Batch** は、xHE-AAC エンコーダー「**exhale**」と「**FFmpeg**」を組み合わせ、直感的な操作で高音質・高圧縮エンコードを行うための Windows 向けマルチスレッド GUI バッチエンコーダーです。

### 主な機能
- **シェルパイプライン経由のダイレクト変換**: `cmd.exe /c` を介して FFmpeg の出力を exhale の標準入力へ直接パイピング。中間 WAV ファイルのディスク生成・削除を完全に撤廃し、SSD 負荷を大幅に削減。
- **マルチコア並列バッチ処理**: 余剰 CPU コアを最大限に活用できる並列処理制御（1〜16 並列から選択可能）。
- **フォルダ再帰読み込み & 階層ミラーリング**: フォルダを丸ごと追加するとサブフォルダまで探索し、出力先でも元の階層構造を保持して保存。
- **外部画像ファイルの自動複製**: アルバムフォルダ内にあるジャケット画像（`cover.jpg`, `folder.png` 等）を出力先フォルダへそのまま自動コピー。
- **MP3/ID3 明示的タグ＆カバーアート転送**: `TagLibSharp` による個別プロパティの明示的マッピングにより、MP3 からの変換時もタイトル、アーティスト、アルバム、ジャケット画像を MP4 コンテナへ確実に反映。
- **Windows 11 ネイティブ対応**: 完了時のトースト通知（出力フォルダを開くボタン付き）、ダークテーマ UI。

### 導入手順（外部バイナリの手動配置）

ライセンス規約（GPL および xHE-AAC 特許制限）を遵守するため、本パッケージには外部バイナリを同梱していません。以下の手順に従ってバイナリを配置してください。

#### フォルダ構成
`ExhaleBatch.exe` と同じフォルダに `ffmpeg.exe` と `exhale.exe` を配置します。
```text
ExhaleBatch/
├── ExhaleBatch.exe
├── ffmpeg.exe    <-- 必須（手動で配置）
└── exhale.exe    <-- 必須（手動で配置）
```

#### 各ツールの入手先
1. **FFmpeg (`ffmpeg.exe`)**
   - [gyan.dev FFmpeg Builds](https://www.gyan.dev/ffmpeg/builds/) から `ffmpeg-release-essentials.zip` をダウンロードします。
   - 解凍したフォルダの `bin` 内にある `ffmpeg.exe` を本アプリと同じ階層にコピーします。
2. **exhale (`exhale.exe`)**
   - [RareWares AAC Encoders](https://www.rarewares.org/aac-encoders.php)（または公式リポジトリ）から Windows x64 版 ZIP をダウンロードします。
   - 解凍して取り出した `exhale.exe` を本アプリと同じ階層にコピーします。

### ライセンス
- Application: MIT License
- ffmpeg: GPL / LGPL
- exhale: Copyright (c) Christian R. Helmrich / Fraunhofer IIS (Non-commercial evaluation only)