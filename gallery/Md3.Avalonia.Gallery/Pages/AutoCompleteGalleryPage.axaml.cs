using Avalonia.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class AutoCompleteGalleryPage : UserControl
{
    public AutoCompleteGalleryPage()
    {
        InitializeComponent();
        TechnologyAutoComplete.ItemsSource = new[]
        {
            "Avalonia", "Android", "ASP.NET Core", "Flutter", "Jetpack Compose",
            "MAUI", "Material Design", "React", "SwiftUI", "WinUI", "WPF"
        };
    }


}
