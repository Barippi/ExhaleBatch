using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace ExhaleBatch;

public partial class MainWindow : Window
{
    private readonly ObservableCollection<EncodeItem> _queue = new();
    private static readonly string[] SupportedExtensions = { ".wav", ".flac", ".mp3", ".m4a", ".ogg", ".aiff", ".wma", ".aac" };
    private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".bmp" };
    
    private CancellationTokenSource? _cts;
    private string? _customOutputDirectory = null;

    public MainWindow()
    {
        InitializeComponent();
        ListQueue.ItemsSource = _queue;

        try
        {
            var iconUri = new Uri("pack://application:,,,/app.ico", UriKind.RelativeOrAbsolute);
            this.Icon = BitmapFrame.Create(iconUri);
        }
        catch { }

        this.Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        this.Loaded -= MainWindow_Loaded;

        bool isReady = await DependencyInstaller.EnsureDependenciesAsync(this, AppendLog);
        if (!isReady)
        {
            AppendLog("[警告] 外部依存ツール (ffmpeg/exhale) が未設定です。手動で配置するか再度起動してください。");
        }
    }

    private void Window_DragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }
    }

    private void Window_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            string[] droppedPaths = (string[])e.Data.GetData(DataFormats.FileDrop);
            ProcessInputPaths(droppedPaths);
        }
    }

    private void BtnAddFiles_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Multiselect = true,
            Filter = "音声ファイル|*.wav;*.flac;*.mp3;*.m4a;*.ogg;*.aiff;*.wma;*.aac|すべてのファイル|*.*"
        };

        if (dialog.ShowDialog() == true)
        {
            AddFilesToQueue(dialog.FileNames, rootDir: null);
        }
    }

    private void BtnAddFolder_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog
        {
            Title = "追加する音声フォルダを選択してください"
        };

        if (dialog.ShowDialog() == true)
        {
            AddFolderToQueue(dialog.FolderName);
        }
    }

    private void ProcessInputPaths(IEnumerable<string> paths)
    {
        var singleFiles = new List<string>();

        foreach (var path in paths)
        {
            if (Directory.Exists(path))
            {
                AddFolderToQueue(path);
            }
            else if (File.Exists(path))
            {
                singleFiles.Add(path);
            }
        }

        if (singleFiles.Count > 0)
        {
            AddFilesToQueue(singleFiles, rootDir: null);
        }
    }

    private void AddFolderToQueue(string folderPath)
    {
        try
        {
            var files = Directory.EnumerateFiles(folderPath, "*.*", SearchOption.AllDirectories)
                                 .Where(f => SupportedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()));

            AddFilesToQueue(files, rootDir: folderPath);
        }
        catch (Exception ex)
        {
            AppendLog($"[フォルダ走査エラー] {folderPath}: {ex.Message}");
        }
    }

    private void AddFilesToQueue(IEnumerable<string> filePaths, string? rootDir)
    {
        foreach (var path in filePaths)
        {
            if (File.Exists(path))
            {
                var ext = Path.GetExtension(path).ToLowerInvariant();
                if (SupportedExtensions.Contains(ext) && !_queue.Any(x => x.SourcePath == path))
                {
                    string targetFolder = _customOutputDirectory ?? (rootDir ?? Path.GetDirectoryName(path)!);
                    string defaultOut = BuildOutputPath(path, targetFolder, rootDir);

                    _queue.Add(new EncodeItem
                    {
                        SourcePath = path,
                        FileName = Path.GetFileName(path),
                        OutputPath = defaultOut,
                        RootDirectory = rootDir
                    });
                }
            }
        }
        UpdateControlsState(isEncoding: false);
    }

    private static string BuildOutputPath(string sourcePath, string targetBaseFolder, string? rootDir)
    {
        string destinationFolder;

        if (!string.IsNullOrEmpty(rootDir) && sourcePath.StartsWith(rootDir, StringComparison.OrdinalIgnoreCase))
        {
            string relativeDir = Path.GetDirectoryName(Path.GetRelativePath(rootDir, sourcePath)) ?? string.Empty;
            destinationFolder = Path.Combine(targetBaseFolder, relativeDir);
        }
        else
        {
            destinationFolder = targetBaseFolder;
        }

        string baseName = Path.GetFileNameWithoutExtension(sourcePath);
        string ext = Path.GetExtension(sourcePath).ToLowerInvariant();

        string outFileName = (ext == ".m4a" && string.Equals(destinationFolder, Path.GetDirectoryName(sourcePath), StringComparison.OrdinalIgnoreCase))
            ? $"{baseName}_exhale.m4a"
            : $"{baseName}.m4a";

        return Path.Combine(destinationFolder, outFileName);
    }

    private void BtnClear_Click(object sender, RoutedEventArgs e)
    {
        _queue.Clear();
        UpdateControlsState(isEncoding: false);
    }

    private void ChkCustomOutputDir_Changed(object sender, RoutedEventArgs e)
    {
        bool isCustom = ChkCustomOutputDir.IsChecked == true;
        TxtOutputDir.IsEnabled = isCustom;
        BtnBrowseOutputDir.IsEnabled = isCustom;
        if (!isCustom)
        {
            _customOutputDirectory = null;
            TxtOutputDir.Text = string.Empty;
        }
    }

    private void BtnBrowseOutputDir_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog
        {
            Title = "出力先フォルダを選択してください"
        };

        if (dialog.ShowDialog() == true)
        {
            _customOutputDirectory = dialog.FolderName;
            TxtOutputDir.Text = dialog.FolderName;
        }
    }

    private void UpdateControlsState(bool isEncoding)
    {
        BtnStart.IsEnabled = !isEncoding && _queue.Any(x => x.Status == EncodeStatus.Queued);
        BtnCancel.IsEnabled = isEncoding;
        BtnClear.IsEnabled = !isEncoding;
        BtnAddFiles.IsEnabled = !isEncoding;
        BtnAddFolder.IsEnabled = !isEncoding;
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        if (_cts != null && !_cts.IsCancellationRequested)
        {
            BtnCancel.IsEnabled = false;
            AppendLog("[要求] 処理の中断をリクエストしました...");
            _cts.Cancel();
        }
    }

    private async void BtnStart_Click(object sender, RoutedEventArgs e)
    {
        var itemsToProcess = _queue.Where(x => x.Status == EncodeStatus.Queued).ToList();
        if (itemsToProcess.Count == 0) return;

        string preset = (CmbPreset.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "5";

        int maxConcurrency = 4;
        if (CmbConcurrency.SelectedItem is ComboBoxItem cItem && int.TryParse(cItem.Content.ToString(), out int parsed))
        {
            maxConcurrency = parsed;
        }

        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        UpdateControlsState(isEncoding: true);
        PrgTotal.Maximum = itemsToProcess.Count;
        PrgTotal.Value = 0;
        TxtLog.Clear();
        AppendLog($"[バッチ開始] {itemsToProcess.Count} 件の処理を開始します (プリセット: {preset} / 並列数: {maxConcurrency})");

        int completedCount = 0;
        using var semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);

        try
        {
            var tasks = itemsToProcess.Select(async item =>
            {
                await semaphore.WaitAsync(token);
                try
                {
                    token.ThrowIfCancellationRequested();

                    string baseFolder = _customOutputDirectory ?? (item.RootDirectory ?? Path.GetDirectoryName(item.SourcePath)!);
                    item.OutputPath = BuildOutputPath(item.SourcePath, baseFolder, item.RootDirectory);

                    string? outDir = Path.GetDirectoryName(item.OutputPath);
                    if (!string.IsNullOrEmpty(outDir) && !Directory.Exists(outDir))
                    {
                        Directory.CreateDirectory(outDir);
                    }

                    item.Status = EncodeStatus.Processing;
                    item.StatusMessage = "処理中...";
                    AppendLog($"[開始] {item.FileName}");

                    try
                    {
                        await RunEncodingPipelineAsync(item.SourcePath, item.OutputPath, preset, token);
                        CopyMetadata(item.SourcePath, item.OutputPath);
                        CopyFolderImages(item.SourcePath, item.OutputPath);

                        item.Status = EncodeStatus.Completed;
                        item.StatusMessage = "完了";
                        AppendLog($"[完了] {item.FileName}");
                    }
                    catch (OperationCanceledException)
                    {
                        item.Status = EncodeStatus.Failed;
                        item.StatusMessage = "中断";
                        if (File.Exists(item.OutputPath))
                        {
                            try { File.Delete(item.OutputPath); } catch { }
                        }
                        throw;
                    }
                    catch (Exception ex)
                    {
                        item.Status = EncodeStatus.Failed;
                        item.StatusMessage = "エラー";
                        AppendLog($"[エラー] {item.FileName}: {ex.Message}");
                    }
                    finally
                    {
                        int current = Interlocked.Increment(ref completedCount);
                        Dispatcher.Invoke(() => PrgTotal.Value = current);
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);

            int successCount = _queue.Count(x => x.Status == EncodeStatus.Completed);
            int failedCount = _queue.Count(x => x.Status == EncodeStatus.Failed);

            AppendLog($"\n[バッチ完了] 全処理が終了しました。成功: {successCount} 件 / エラー: {failedCount} 件");

            string notifyFolder = _customOutputDirectory 
                ?? Path.GetDirectoryName(itemsToProcess.FirstOrDefault()?.OutputPath) 
                ?? Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);

            NotificationService.ShowBatchCompleted(successCount, failedCount, notifyFolder);
        }
        catch (OperationCanceledException)
        {
            AppendLog("\n[中断完了] バッチ処理はユーザーによって安全に停止されました。");
            NotificationService.ShowBatchCancelled();
        }
        finally
        {
            _cts.Dispose();
            _cts = null;
            UpdateControlsState(isEncoding: false);
        }
    }

    private static string ResolveBinaryPath(string exeName)
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string localPath = Path.Combine(baseDir, exeName);
        if (File.Exists(localPath)) return localPath;

        string subPath = Path.Combine(baseDir, "bin", exeName);
        if (File.Exists(subPath)) return subPath;

        return exeName;
    }

    private async Task RunEncodingPipelineAsync(string inputPath, string outputPath, string preset, CancellationToken token)
    {
        string tempWav = Path.Combine(Path.GetDirectoryName(outputPath)!, $"_temp_{Guid.NewGuid():N}.wav");

        try
        {
            var ffmpegInfo = new ProcessStartInfo
            {
                FileName = ResolveBinaryPath("ffmpeg.exe"),
                Arguments = $"-nostdin -y -v error -i \"{inputPath}\" -c:a pcm_s16le \"{tempWav}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (var procFfmpeg = new Process { StartInfo = ffmpegInfo })
            {
                procFfmpeg.ErrorDataReceived += (s, e) => { if (!string.IsNullOrEmpty(e.Data)) AppendLog($"[FFmpeg] {e.Data}"); };
                procFfmpeg.Start();
                procFfmpeg.BeginErrorReadLine();

                using var reg = token.Register(() => { try { if (!procFfmpeg.HasExited) procFfmpeg.Kill(); } catch { } });
                await procFfmpeg.WaitForExitAsync(token);

                if (procFfmpeg.ExitCode != 0)
                {
                    throw new InvalidOperationException($"FFmpegデコード失敗 (Code: {procFfmpeg.ExitCode})");
                }
            }

            var exhaleInfo = new ProcessStartInfo
            {
                FileName = ResolveBinaryPath("exhale.exe"),
                Arguments = $"{preset} \"{tempWav}\" \"{outputPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (var procExhale = new Process { StartInfo = exhaleInfo })
            {
                procExhale.OutputDataReceived += (s, e) => { if (!string.IsNullOrEmpty(e.Data)) AppendLog(e.Data); };
                procExhale.ErrorDataReceived += (s, e) => { if (!string.IsNullOrEmpty(e.Data)) AppendLog(e.Data); };

                procExhale.Start();
                procExhale.BeginOutputReadLine();
                procExhale.BeginErrorReadLine();

                using var reg = token.Register(() => { try { if (!procExhale.HasExited) procExhale.Kill(); } catch { } });
                await procExhale.WaitForExitAsync(token);

                if (procExhale.ExitCode != 0)
                {
                    throw new InvalidOperationException($"exhaleエンコード失敗 (Code: {procExhale.ExitCode})");
                }
            }
        }
        finally
        {
            if (File.Exists(tempWav))
            {
                try { File.Delete(tempWav); } catch { }
            }
        }
    }

    private void CopyMetadata(string srcFile, string destFile)
    {
        try
        {
            using var src = TagLib.File.Create(srcFile);
            using var dest = TagLib.File.Create(destFile);

            src.Tag.CopyTo(dest.Tag, true);
            dest.Save();
        }
        catch (Exception ex)
        {
            AppendLog($"[メタデータ警告] {ex.Message}");
        }
    }

    private void CopyFolderImages(string srcFilePath, string destFilePath)
    {
        try
        {
            string? srcDir = Path.GetDirectoryName(srcFilePath);
            string? destDir = Path.GetDirectoryName(destFilePath);

            if (string.IsNullOrEmpty(srcDir) || string.IsNullOrEmpty(destDir) || !Directory.Exists(srcDir))
            {
                return;
            }

            if (string.Equals(srcDir, destDir, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var imageFiles = Directory.EnumerateFiles(srcDir, "*.*", SearchOption.TopDirectoryOnly)
                                      .Where(f => ImageExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()));

            foreach (var imgPath in imageFiles)
            {
                string destImgPath = Path.Combine(destDir, Path.GetFileName(imgPath));

                if (!File.Exists(destImgPath))
                {
                    File.Copy(imgPath, destImgPath, overwrite: false);
                    AppendLog($"[画像コピー] {Path.GetFileName(imgPath)} を出力フォルダにコピーしました。");
                }
            }
        }
        catch (Exception ex)
        {
            AppendLog($"[画像コピー警告] {ex.Message}");
        }
    }

    private void AppendLog(string text)
    {
        Dispatcher.Invoke(() =>
        {
            TxtLog.AppendText(text + Environment.NewLine);
            TxtLog.ScrollToEnd();
        });
    }

    private EncodeItem? GetSelectedQueueItem(object sender)
    {
        if (sender is MenuItem menuItem && menuItem.DataContext is EncodeItem item)
        {
            return item;
        }
        return ListQueue.SelectedItem as EncodeItem;
    }

    private void MenuRemoveItem_Click(object sender, RoutedEventArgs e)
    {
        var item = GetSelectedQueueItem(sender);
        if (item == null) return;

        if (item.Status == EncodeStatus.Processing)
        {
            MessageBox.Show("処理中のタスクは削除できません。", "情報", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        _queue.Remove(item);
        UpdateControlsState(isEncoding: _cts != null);
    }

    private void MenuOpenSourceLocation_Click(object sender, RoutedEventArgs e)
    {
        var item = GetSelectedQueueItem(sender);
        if (item == null) return;
        OpenInExplorer(item.SourcePath);
    }

    private void MenuOpenOutputLocation_Click(object sender, RoutedEventArgs e)
    {
        var item = GetSelectedQueueItem(sender);
        if (item == null) return;

        if (File.Exists(item.OutputPath))
        {
            OpenInExplorer(item.OutputPath);
        }
        else
        {
            string folder = Path.GetDirectoryName(item.OutputPath)!;
            if (Directory.Exists(folder))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = folder,
                    UseShellExecute = true
                });
            }
        }
    }

    private static void OpenInExplorer(string filePath)
    {
        if (!File.Exists(filePath)) return;

        Process.Start(new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = $"/select,\"{filePath}\"",
            UseShellExecute = true
        });
    }
}
