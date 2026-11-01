using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ExhaleBatch;

public enum EncodeStatus
{
    Queued,     // 待機中
    Processing, // 処理中
    Completed,  // 完了
    Failed      // エラー
}

public class EncodeItem : INotifyPropertyChanged
{
    private EncodeStatus _status = EncodeStatus.Queued;
    private string _statusMessage = "待機中";

    public required string SourcePath { get; init; }
    public required string FileName { get; init; }
    public required string OutputPath { get; set; }
    public string? RootDirectory { get; init; }

    public EncodeStatus Status
    {
        get => _status;
        set
        {
            if (_status != value)
            {
                _status = value;
                OnPropertyChanged();
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            if (_statusMessage != value)
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
