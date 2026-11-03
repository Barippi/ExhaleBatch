using System.Diagnostics;
using System.IO;
using Microsoft.Toolkit.Uwp.Notifications;

namespace ExhaleBatch;

public static class NotificationService
{
    private static bool _isInitialized = false;

    public static void Initialize()
    {
        if (_isInitialized) return;

        ToastNotificationManagerCompat.OnActivated += toastArgs =>
        {
            var args = ToastArguments.Parse(toastArgs.Argument);

            if (args.TryGetValue("action", out string? action) && action == "openFolder")
            {
                if (args.TryGetValue("folderPath", out string? folderPath) && Directory.Exists(folderPath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = folderPath,
                        UseShellExecute = true
                    });
                }
            }
        };

        _isInitialized = true;
    }

    public static void ShowBatchCompleted(int successCount, int failedCount, string outputFolder)
    {
        string title = failedCount == 0 
            ? "エンコードが完了しました" 
            : "エンコードが終了しました（一部エラー）";

        string body = failedCount == 0
            ? $"{successCount} 件すべての変換が正常に完了しました。"
            : $"成功: {successCount} 件 / 失敗: {failedCount} 件";

        new ToastContentBuilder()
            .AddArgument("action", "openFolder")
            .AddArgument("folderPath", outputFolder)
            .AddText(title)
            .AddText(body)
            .AddAttributionText("Exhale Batch")
            .AddButton(new ToastButton()
                .SetContent("出力フォルダを開く")
                .AddArgument("action", "openFolder")
                .AddArgument("folderPath", outputFolder))
            .Show();
    }

    public static void ShowBatchCancelled()
    {
        new ToastContentBuilder()
            .AddText("処理を中断しました")
            .AddText("ユーザーによってバッチエンコードが停止されました。")
            .AddAttributionText("Exhale Batch")
            .Show();
    }
}
