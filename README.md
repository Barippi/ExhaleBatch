# Exhale Batch - xHE-AAC Encoder Frontend

A lightweight, multi-threaded GUI frontend for the xHE-AAC encoder (**exhale**) integrated with **FFmpeg** on Windows 11.

[日本語の案内はこちら](#日本語)

---

## English

### Overview
**Exhale Batch** is a multi-threaded batch encoding utility designed to encode various audio formats into high-efficiency xHE-AAC (`.m4a`) files using `exhale.exe` and `ffmpeg.exe`.

### Key Features
- **Parallel Multi-Core Encoding**: Efficiently processes batches using configurable thread counts (1 to 16 parallel tasks).
- **Directory Hierarchy Mirroring**: Recursively scans folders and mirrors the full directory structure to the destination path.
- **Artwork & Metadata Transfer**:
  - Automatically copies embedded tags (Title, Artist, Album, Year, etc.) and embedded cover artwork to `.m4a` via `TagLibSharp`.
  - **External Image Copying**: Automatically detects external artwork files (`cover.jpg`, `folder.png`, etc.) in the source folder and copies them directly to the destination folder.
- **Safe Pipeline Execution**: Decodes input audio to temporary PCM WAV buffers before passing data to `exhale`, automatically removing intermediate files upon completion.
- **Windows 11 Native Integration**: Toast notifications with a "Open Folder" action, dark theme UI, and safe resource management.

### Requirements & Setup

To comply with GPL (FFmpeg) and licensing restrictions (xHE-AAC patents), external binaries are **not bundled**. Please place them into the application directory manually.

#### Folder Layout
Place `ffmpeg.exe` and `exhale.exe` alongside `ExhaleBatch.exe`:
```text
ExhaleBatch/
├── ExhaleBatch.exe
├── ffmpeg.exe    <-- Required (Place manually)
├── exhale.exe    <-- Required (Place manually)
└── README.md
```

#### Binary Downloads
1. **FFmpeg (`ffmpeg.exe`)**
   - Download the official essentials release from [gyan.dev FFmpeg Builds](https://www.gyan.dev/ffmpeg/builds/).
   - Extract `ffmpeg-release-essentials.zip` and copy `bin/ffmpeg.exe` to the application folder.
2. **exhale (`exhale.exe`)**
   - Download the Windows x64 binary from [RareWares AAC Encoders](https://www.rarewares.org/aac-encoders.php).
   - Extract the ZIP archive and copy `exhale.exe` to the application folder.

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
- **マルチコア並列バッチ処理**: 1〜16並列の範囲で CPU 負荷に合わせた並列エンコードが可能。
- **フォルダ再帰探索 & 階層ミラーリング**: フォルダを追加するとサブフォルダを網羅し、出力先でも元のフォルダ構造を完全再現。
- **メタデータ & アートワークの完全継承**:
  - `TagLibSharp` により、曲名・アーティスト・アルバム・埋め込みジャケット画像を `.m4a` へ自動反映。
  - **外部画像ファイルの自動コピー**: 音声ファイルと同じフォルダにあるジャケット画像（`cover.jpg`, `folder.png` など）を出力先フォルダへ自動複製。
- **安全なパイプライン処理**: 一時 PCM WAV 経由でデコード・エンコードを行い、処理完了後に中間テンポラリを安全に自動削除。
- **Windows 11 ネイティブ対応**: 出力フォルダを直接開けるアクションボタン付きトースト通知、ダークテーマ UI。

### 導入手順（外部バイナリの手動配置）

ライセンス（GPL）および xHE-AAC 特許規約を遵守するため、外部バイナリは同梱していません。以下の手順に従って手動で配置してください。

#### フォルダ構成
`ExhaleBatch.exe` と同じフォルダに `ffmpeg.exe` と `exhale.exe` を配置します。
```text
ExhaleBatch/
├── ExhaleBatch.exe
├── ffmpeg.exe    <-- 必須（手動で配置）
├── exhale.exe    <-- 必須（手動で配置）
└── README.md
```

#### 入手先
1. **FFmpeg (`ffmpeg.exe`)**
   - [gyan.dev FFmpeg Builds](https://www.gyan.dev/ffmpeg/builds/) より `ffmpeg-release-essentials.zip` をダウンロード。
   - `bin` フォルダ内にある `ffmpeg.exe` のみを本アプリと同じフォルダへコピー。
2. **exhale (`exhale.exe`)**
   - [RareWares AAC Encoders](https://www.rarewares.org/aac-encoders.php) より Windows x64 版 ZIP をダウンロード。
   - 解凍後、`exhale.exe` を本アプリと同じフォルダへコピー。

### 免責事項・ライセンス
- Application: MIT License (See `LICENSE` file)
- FFmpeg: LGPL / GPL (FFmpeg team)
- exhale: Copyright (c) Christian R. Helmrich / Fraunhofer IIS (Non-commercial evaluation only)