using System.Windows;
using Microsoft.Toolkit.Uwp.Notifications;

namespace ExhaleBatch;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        AppDomain.CurrentDomain.UnhandledException += (s, args) =>
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n[未処理例外 (AppDomain)]:\n" + args.ExceptionObject);
            Console.ResetColor();
        };

        DispatcherUnhandledException += (s, args) =>
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n[未処理例外 (Dispatcher)]:\n" + args.Exception);
            Console.ResetColor();
        };

        base.OnStartup(e);

        try
        {
            NotificationService.Initialize();
        }
        catch (Exception ex)
        {
            Console.WriteLine("[通知初期化スキップ]: " + ex.Message);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        try
        {
            ToastNotificationManagerCompat.Uninstall();
        }
        catch { }
        base.OnExit(e);
    }
}
