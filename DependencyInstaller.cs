using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Windows;

namespace ExhaleBatch;

public static class DependencyInstaller
{
    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromMinutes(5)
    };

    static DependencyInstaller()
    {
        HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36");
    }

    private const string FfmpegZipUrl = "https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip";
    private static readonly string[] ExhaleZipUrls = new[]
    {
        "https://www.rarewares.org/files/aac/exhale-1.2.2-x64.zip",
        "https://www.free-codecs.com/download_soft.php?d=exhale&s=1393"
    };

    public static async Task<bool> EnsureDependenciesAsync(Window ownerWindow, Action<string> logAction)
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string ffmpegPath = Path.Combine(baseDir, "ffmpeg.exe");
        string exhalePath = Path.Combine(baseDir, "exhale.exe");

        bool hasFfmpeg = File.Exists(ffmpegPath);
        bool hasExhale = File.Exists(exhalePath);

        if (hasFfmpeg && hasExhale)
        {
            return true;
        }

        var result = MessageBox.Show(
            ownerWindow,
            "動作に必要な外部コンポーネント (FFmpeg / exhale) が見つかりません。\n\n" +
            "手動で配置するか、自動ダウンロードを試行しますか？",
            "セットアップの確認 - Exhale Batch",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
        {
            return false;
        }

        try
        {
            if (!hasFfmpeg)
            {
                logAction("[セットアップ] FFmpeg をダウンロード中 (約90MB)...");
                await DownloadAndExtractSingleFileAsync(
                    new[] { FfmpegZipUrl },
                    "ffmpeg.exe",
                    ffmpegPath,
                    logAction);
                logAction("[セットアップ] ffmpeg.exe の配置が完了しました。");
            }

            if (!hasExhale)
            {
                logAction("[セットアップ] exhale (v1.2.2) をダウンロード中...");
                await DownloadAndExtractSingleFileAsync(
                    ExhaleZipUrls,
                    "exhale.exe",
                    exhalePath,
                    logAction);
                logAction("[セットアップ] exhale.exe の配置が完了しました。");
            }

            MessageBox.Show(
                ownerWindow,
                "すべてのコンポーネントの準備が整いました！",
                "完了",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ownerWindow,
                $"ダウンロード・展開中にエラーが発生しました:\n{ex.Message}\n\n" +
                "手動で ffmpeg.exe と exhale.exe をアプリと同じフォルダに配置してください。",
                "エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            return false;
        }
    }

    private static async Task DownloadAndExtractSingleFileAsync(
        string[] zipUrls,
        string targetExeName,
        string destinationFilePath,
        Action<string> logAction)
    {
        string tempZipPath = Path.GetTempFileName();
        Exception? lastEx = null;

        foreach (var url in zipUrls)
        {
            try
            {
                using var response = await HttpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                await using (var streamToReadFrom = await response.Content.ReadAsStreamAsync())
                await using (var streamToWriteTo = File.Open(tempZipPath, FileMode.Create))
                {
                    await streamToReadFrom.CopyToAsync(streamToWriteTo);
                }

                logAction($"[展開中] {targetExeName} を抽出しています...");

                using (var archive = ZipFile.OpenRead(tempZipPath))
                {
                    var entry = archive.Entries.FirstOrDefault(e =>
                        Path.GetFileName(e.FullName).Equals(targetExeName, StringComparison.OrdinalIgnoreCase));

                    if (entry == null)
                    {
                        throw new FileNotFoundException($"アーカイブ内に {targetExeName} が見つかりませんでした。");
                    }

                    entry.ExtractToFile(destinationFilePath, overwrite: true);
                }

                lastEx = null;
                break;
            }
            catch (Exception ex)
            {
                lastEx = ex;
                logAction($"[ダウンロード再試行] 次のミラーURLを試行します...");
            }
            finally
            {
                if (File.Exists(tempZipPath))
                {
                    try { File.Delete(tempZipPath); } catch { }
                }
            }
        }

        if (lastEx != null)
        {
            throw lastEx;
        }
    }
}
