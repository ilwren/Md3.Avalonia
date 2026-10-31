using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Input;
using Md3.Avalonia.Controls;

namespace Md3.Avalonia.Gallery.Pages;

/// <summary>The model selected by the View's type-specific banner DataTemplate.</summary>
public sealed class SyncBannerModel
{
    public SyncBannerModel(IMdBannerService banners, string message)
    {
        Message = message;
        KeepOfflineCommand = new RelayCommand(() => banners.Close("offline"));
        RetryCommand = new RelayCommand(() => banners.Close("retry"));
    }

    public string Message { get; }
    public RelayCommand KeepOfflineCommand { get; }
    public RelayCommand RetryCommand { get; }
}

/// <summary>Knows only the injectable interface—not the host, template, or banner control.</summary>
public sealed class BannerServiceDemoViewModel : INotifyPropertyChanged
{
    private readonly IMdBannerService _banners;
    private string _status = "No banner requested yet.";

    public BannerServiceDemoViewModel(IMdBannerService banners)
    {
        _banners = banners;
        ShowBannerCommand = new AsyncRelayCommand(ShowBannerAsync);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public AsyncRelayCommand ShowBannerCommand { get; }

    public string Status
    {
        get => _status;
        private set
        {
            if (_status == value) return;
            _status = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
        }
    }

    private async Task ShowBannerAsync()
    {
        Status = "Waiting for a banner action…";
        var result = await _banners.ShowAsync<string>(
            new SyncBannerModel(_banners, "Your changes could not be synchronized."));
        Status = result switch
        {
            "retry" => "Retry selected in the model-backed banner.",
            "offline" => "Keeping the changes offline.",
            _ => "The model-backed banner was dismissed."
        };
    }
}

public partial class BannerGalleryPage : UserControl
{
    private static string L(string english, string chinese) => GalleryLocalization.Choose(english, chinese);

    public BannerGalleryPage()
    {
        InitializeComponent();

        // Application composition does this once: the concrete service is assigned to the View's
        // host and the same instance is injected as IMdBannerService into the ViewModel.
        var banners = new MdBannerService();
        ServiceBannerHost.Service = banners;
        ServiceDemoRoot.DataContext = new BannerServiceDemoViewModel(banners);
    }

    private void ShowBanner(object? sender, RoutedEventArgs e)
    {
        DemoBanner.Show();
        BannerStatus.Text = L("Banner shown.", "横幅已显示。");
    }

    private void BannerDismissed(object? sender, EventArgs e) => BannerStatus.Text = L("Banner dismissed.", "横幅已关闭。");

    private void BannerAction(object? sender, RoutedEventArgs e)
    {
        BannerStatus.Text = L($"{(sender as ContentControl)?.Content} selected.", $"已选择 {(sender as ContentControl)?.Content}。");
        DemoBanner.Dismiss();
    }
}
