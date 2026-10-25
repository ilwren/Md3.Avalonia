# Showing dialogs from a ViewModel: `IMdDialogService`

[English](DIALOG_SERVICE.md) | [简体中文](DIALOG_SERVICE.zh-CN.md)

The goal is to keep the ViewModel **free of control references**: it requests a dialog model and then `await`s a result. `MdDialogHost` resolves the view through its `DataTemplates`.

An executable reference implementation is in `gallery/Md3.Avalonia.Gallery/Pages/DialogGalleryPage.axaml(.cs)`; the examples below are based on it.

---

## 1. Composition root: one instance, two roles

`MdDialogHost.Service` requires the concrete `MdDialogService`, while the ViewModel receives the `IMdDialogService` interface. **They must be the same instance.**

```csharp
var dialogs = new MdDialogService();

//  Host (view side)
shell.DialogHost.Service = dialogs;

//  Injection (ViewModel side)
services.AddSingleton<IMdDialogService>(dialogs);
```

A DI container is optional; you can pass the instance directly to constructors:

```csharp
ServiceDialogHost.Service = dialogs;
ServiceDemoRoot.DataContext = new DialogServiceDemoViewModel(dialogs);
```

`MdDialogService` holds a **weak reference** to the host, so it will not keep a window alive. The host must therefore be kept alive by the visual tree. One instance for the whole shell is enough.

---

## 2. Host: place it at the shell root and register templates

`MdDialogHost` is a container with an overlay; **its content is the layer that it covers**. Put it at the outermost level of the application shell, not inside an individual page.

```xml
<md:MdDialogHost x:Name="DialogHost" ZIndex="100">

  <md:MdDialogHost.DataTemplates>
    <!--  The repository enables AvaloniaUseCompiledBindingsByDefault, so both DataType and x:DataType are required.-->
    <DataTemplate DataType="{x:Type vm:ConfirmDeleteDialogModel}"
                  x:DataType="vm:ConfirmDeleteDialogModel">
      <md:MdDialog Headline="{Binding Headline}" Variant="Basic">
        <TextBlock MaxWidth="340" TextWrapping="Wrap" Text="{Binding Message}" />
        <md:MdDialog.Actions>
          <StackPanel Orientation="Horizontal" Spacing="8">
            <md:MdButton Content="Keep"   Variant="Text" Command="{Binding CancelCommand}" />
            <md:MdButton Content="Delete" Variant="Text" Command="{Binding ConfirmCommand}" />
          </StackPanel>
        </md:MdDialog.Actions>
      </md:MdDialog>
    </DataTemplate>

    <!--  One template per dialog model; the host selects by runtime type.-->
    <DataTemplate DataType="{x:Type vm:RenameDialogModel}" x:DataType="vm:RenameDialogModel">
      ...
    </DataTemplate>
  </md:MdDialogHost.DataTemplates>

  <views:Shell />   <!--  Content covered by the overlay-->
</md:MdDialogHost>
```

Templates can also live in `App.axaml` under `DataTemplates`; they remain visible through the same inherited scope. Keeping them on the host simply makes them easier to find beside their use.

---

## 3. Calling ViewModel: depend only on the service

```csharp
public sealed class DialogServiceDemoViewModel
{
    private readonly IMdDialogService _dialogs;

    public DialogServiceDemoViewModel(IMdDialogService dialogs)
    {
        _dialogs = dialogs;
        DeleteCommand = new AsyncRelayCommand(DeleteAsync);
    }

    public AsyncRelayCommand DeleteCommand { get; }

    private async Task DeleteAsync()
    {
        // Overlay dismissal, Escape, Android back, or token cancellation returns default (false here).
        var confirmed = await _dialogs.ShowAsync<bool>(
            new ConfirmDeleteDialogModel(_dialogs, "Quarterly report"));

        Status = confirmed ? "Deleted." : "Kept.";
    }
}
```

---

## 4. How the dialog returns its answer

The dialog model also receives the service. Call `Close(result)` to provide the result returned by `ShowAsync`:

```csharp
public sealed class ConfirmDeleteDialogModel
{
    private readonly IMdDialogService _dialogs;

    public ConfirmDeleteDialogModel(IMdDialogService dialogs, string item)
    {
        _dialogs = dialogs;
        Headline = $"Delete {item}?";
        ConfirmCommand = new RelayCommand(() => _dialogs.Close(true));
        CancelCommand  = new RelayCommand(() => _dialogs.Close(false));
    }

    public string Headline { get; }
    public string Message => "This cannot be undone.";
    public RelayCommand ConfirmCommand { get; }
    public RelayCommand CancelCommand { get; }
}
```

---

## 5. Behaviors to know

| Situation | Behavior |
|---|---|
| Call `ShowAsync` while another dialog is open | **Queued**, not preemptive (Material permits only one modal dialog at a time). |
| Call `ShowAsync` before the host is attached | Queued as well; no exception, so startup timing does not need special handling. |
| Overlay click, Escape, Android back, or token cancellation | The pending `ShowAsync` completes with `null`; `ShowAsync<T>` returns `default`. |
| Move to the next step in the same flow (about → license, wizard step) | Use `ReplaceAsync` to **move it to the front**. `ShowAsync` queues it behind the current dialog, so it will not appear until that dialog closes. |
| Call from a background thread | The service marshals the call to the UI thread; the continuation of `ShowAsync<T>` also resumes on the UI thread. |
| Result is not needed | Use `Show(dialog)` or `Replace(dialog)` as fire-and-forget calls. |

`IsOpen` is readable and can be used for checks such as disabling selected commands while a dialog is open.

---

## 6. The three routes can be mixed

`MdDialogHost` supports three ways to drive dialogs. Choose by **call site**; the whole application does not need to use just one approach:

| Source | Usage | Result |
|---|---|---|
| View code-behind | `await DialogHost.ShowAsync(model)` | Return value |
| ViewModel | Inject `IMdDialogService`, then `await _dialogs.ShowAsync(model)` | Return value |
| Binding only | Two-way `IsOpen` together with `Dialog` | ViewModel state; no `await` |
