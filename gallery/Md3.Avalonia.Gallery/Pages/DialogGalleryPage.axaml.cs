using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Md3.Avalonia.Controls;

namespace Md3.Avalonia.Gallery.Pages;

public partial class DialogGalleryPage : UserControl
{
    private string _currentProjectName = "My Material App";

    public DialogGalleryPage() => InitializeComponent();

    private void OpenConfirmationDialog(object? sender, RoutedEventArgs e)
    {
        DemoDialog.Variant = MdDialogVariant.Basic;
        DemoDialog.Headline = "Discard draft?";
        BasicDialogHost.Dialog = DemoDialog;
        BasicDialogHost.IsOpen = true;
    }

    private void OnCancelDiscardClicked(object? sender, RoutedEventArgs e)
    {
        BasicDialogHost.Close("cancel");
        SetFeedback("ℹ Action Cancelled: Kept the unsaved draft in editor.", MdSymbols.Info, Brushes.Gray);
    }

    private void OnConfirmDiscardClicked(object? sender, RoutedEventArgs e)
    {
        BasicDialogHost.Close("discard");
        SetFeedback("✓ Action Confirmed: Unsaved draft was discarded.", MdSymbols.CheckCircle, Color.Parse("#4CAF50"));
    }

    private async void OpenAsyncInputDialog(object? sender, RoutedEventArgs e)
    {
        var textBox = new MdTextBox
        {
            Label = "Project Name",
            Text = _currentProjectName,
            Width = 320,
            Variant = MdTextBoxVariant.Outlined,
            Margin = new global::Avalonia.Thickness(0, 8, 0, 8)
        };

        var dialog = new MdDialog
        {
            Headline = "Rename Project",
            Icon = new MdIcon { Glyph = MdSymbols.Edit, Size = 24 },
            Content = new StackPanel
            {
                Spacing = 12,
                Children =
                {
                    new TextBlock
                    {
                        Text = "Enter a new descriptive name for your workspace project:",
                        TextWrapping = TextWrapping.Wrap,
                        Width = 320,
                        FontSize = 13,
                        Foreground = Brushes.Gray
                    },
                    textBox
                }
            }
        };

        var cancelButton = new MdButton { Content = "Cancel", Variant = MdButtonVariant.Text };
        var saveButton = new MdButton { Content = "Save Name", Variant = MdButtonVariant.Filled };

        cancelButton.Click += (_, _) => BasicDialogHost.Close(null);
        saveButton.Click += (_, _) => BasicDialogHost.Close(textBox.Text);

        dialog.Actions = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children = { cancelButton, saveButton }
        };

        var result = await BasicDialogHost.ShowAsync(dialog);
        if (result is string newName && !string.IsNullOrWhiteSpace(newName))
        {
            _currentProjectName = newName.Trim();
            SetFeedback($"✓ Project renamed successfully to: '{_currentProjectName}'", MdSymbols.CheckCircle, Color.Parse("#2196F3"));
        }
        else
        {
            SetFeedback("ℹ Project rename was cancelled.", MdSymbols.Info, Brushes.Gray);
        }
    }

    private async void OpenChoiceDialog(object? sender, RoutedEventArgs e)
    {
        var localRadio = new MdRadioButton { Content = "Local SSD Storage (/data/backups)", IsChecked = true, Margin = new global::Avalonia.Thickness(0, 4) };
        var cloudRadio = new MdRadioButton { Content = "Google Cloud Storage Bucket", Margin = new global::Avalonia.Thickness(0, 4) };
        var vaultRadio = new MdRadioButton { Content = "Encrypted Vault Archive (.zip.enc)", Margin = new global::Avalonia.Thickness(0, 4) };

        var dialog = new MdDialog
        {
            Headline = "Select Backup Target",
            Icon = new MdIcon { Glyph = MdSymbols.CloudUpload, Size = 24 },
            Content = new StackPanel
            {
                Spacing = 8,
                Width = 340,
                Children =
                {
                    new TextBlock
                    {
                        Text = "Choose the primary destination for your automated snapshots:",
                        TextWrapping = TextWrapping.Wrap,
                        FontSize = 13,
                        Foreground = Brushes.Gray
                    },
                    localRadio,
                    cloudRadio,
                    vaultRadio
                }
            }
        };

        var cancelBtn = new MdButton { Content = "Cancel", Variant = MdButtonVariant.Text };
        var confirmBtn = new MdButton { Content = "Confirm Destination", Variant = MdButtonVariant.Filled };

        cancelBtn.Click += (_, _) => BasicDialogHost.Close(null);
        confirmBtn.Click += (_, _) =>
        {
            var choice = localRadio.IsChecked == true ? "Local Storage"
                       : cloudRadio.IsChecked == true ? "Google Cloud"
                       : "Encrypted Vault";
            BasicDialogHost.Close(choice);
        };

        dialog.Actions = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children = { cancelBtn, confirmBtn }
        };

        var result = await BasicDialogHost.ShowAsync(dialog);
        if (result is string destination)
        {
            SetFeedback($"✓ Backup destination configured to: [{destination}]", MdSymbols.CheckCircle, Color.Parse("#4CAF50"));
        }
        else
        {
            SetFeedback("ℹ Backup destination selection was cancelled.", MdSymbols.Info, Brushes.Gray);
        }
    }

    private void OpenFullScreenDialog(object? sender, RoutedEventArgs e)
    {
        var fullDialog = new MdDialog
        {
            Variant = MdDialogVariant.FullScreen,
            Headline = "Review Security & Permissions",
            Content = new StackPanel
            {
                Spacing = 16,
                Margin = new global::Avalonia.Thickness(24),
                Children =
                {
                    new TextBlock
                    {
                        Text = "Security Audit Overview",
                        FontSize = 18,
                        FontWeight = FontWeight.SemiBold
                    },
                    new TextBlock
                    {
                        Text = "Review active OAuth permissions, role-based access policies, and device token expirations before applying changes.",
                        TextWrapping = TextWrapping.Wrap,
                        FontSize = 14
                    },
                    new MdCheckBox { Content = "Grant read/write access to external storage", IsChecked = true },
                    new MdCheckBox { Content = "Allow background synchronization service", IsChecked = true },
                    new MdCheckBox { Content = "Enable end-to-end encrypted local caching", IsChecked = false }
                }
            }
        };

        var cancelBtn = new MdButton { Content = "Discard", Variant = MdButtonVariant.Outlined };
        var saveBtn = new MdButton { Content = "Apply All Permissions", Variant = MdButtonVariant.Filled };

        cancelBtn.Click += (_, _) =>
        {
            BasicDialogHost.Close();
            SetFeedback("ℹ Full-screen review discarded without changes.", MdSymbols.Info, Brushes.Gray);
        };
        saveBtn.Click += (_, _) =>
        {
            BasicDialogHost.Close();
            SetFeedback("✓ Full-screen permissions successfully saved and applied!", MdSymbols.CheckCircle, Color.Parse("#4CAF50"));
        };

        fullDialog.Actions = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            Children = { cancelBtn, saveBtn }
        };

        BasicDialogHost.Dialog = fullDialog;
        BasicDialogHost.IsOpen = true;
    }

    private void SetFeedback(string message, string? iconGlyph, IBrush iconBrush)
    {
        if (FeedbackText is not null) FeedbackText.Text = message;
        if (FeedbackIcon is not null && iconGlyph is not null)
        {
            FeedbackIcon.Glyph = iconGlyph;
            FeedbackIcon.Foreground = iconBrush;
        }
    }

    private void SetFeedback(string message, string? iconGlyph, Color color) =>
        SetFeedback(message, iconGlyph, new SolidColorBrush(color));
}
