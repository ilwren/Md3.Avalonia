# Core Controls API Reference (`Md3.Avalonia`)

The `Md3.Avalonia` package contains the standard Material Design 3 (Material You) component catalog.

---

## 1. Action Buttons

### `MdButton`
Material 3 common button supporting five standard visual variants and sizes.

| Property | Type | Default | Description |
| :--- | :--- | :--- | :--- |
| `Variant` | `MdButtonVariant` | `Filled` | `Filled`, `Elevated`, `Tonal`, `Outlined`, `Text` |
| `Size` | `MdButtonSize` | `Medium` | `ExtraSmall`, `Small`, `Medium`, `Large`, `ExtraLarge` |
| `Elevation` | `MdElevationLevel` | `Level0` | Container elevation shadow |

```xml
<md:MdButton Content="Submit" Variant="Filled" />
<md:MdButton Content="Details" Variant="Tonal" />
<md:MdButton Content="Cancel" Variant="Outlined" />
```

---

### `MdFloatingActionButton` & `MdExtendedFloatingActionButton`
Floating action buttons for primary screen actions.

| Property | Type | Default | Description |
| :--- | :--- | :--- | :--- |
| `Icon` | `object?` | `null` | Icon glyph or visual content |
| `Size` | `MdFabSize` | `Regular` | `Small`, `Regular`, `Medium`, `Large` |
| `ColorStyle` | `MdFabColor` | `PrimaryContainer` | `PrimaryContainer`, `SecondaryContainer`, `TertiaryContainer`, `Primary`, `Secondary`, `Tertiary` |
| `Alignment` | `MdFabAlignment` | `Right` | `Left` or `Right` alignment for menus and layout positioning |

```xml
<md:MdFloatingActionButton Icon="{x:Static md:MdSymbols.Add}" Size="Regular" />
<md:MdExtendedFloatingActionButton Content="Create" Icon="{x:Static md:MdSymbols.Add}" />
```

---

### `MdFabMenu` (Expandable FAB with Floating Actions)
Material 3 FAB menu that expands two to six labeled actions with reversible motion. Horizontal alignment and vertical expansion direction are independent APIs.

| Property | Type | Default | Description |
| :--- | :--- | :--- | :--- |
| `IsOpen` | `bool` | `false` | Two-way binding for expanded/collapsed state |
| `Alignment` | `MdFabAlignment` | `Right` | `Left` or `Right` alignment of actions and trigger |
| `ExpansionDirection` | `MdFabMenuExpansionDirection` | `Up` | Places actions above or below the anchored trigger without changing its parent-layout alignment |
| `ColorStyle` | `MdFabColor` | `PrimaryContainer` | Color container style for trigger |
| `OpenIcon` | `object?` | `MdSymbols.Add` | Icon when closed |
| `CloseIcon` | `object?` | `MdSymbols.Close` | Icon when opened |

```xml
<!-- Material default: actions expand above an anchor near the bottom edge. -->
<md:MdFabMenu Alignment="Right" ExpansionDirection="Up" VerticalAlignment="Bottom">
    <md:MdFabMenuItem Content="Photo" Icon="{x:Static md:MdSymbols.Photo}" />
    <md:MdFabMenuItem Content="Document" Icon="{x:Static md:MdSymbols.Description}" />
</md:MdFabMenu>

<!-- Explicit extension: actions expand below an anchor near the top edge. -->
<md:MdFabMenu Alignment="Left" ExpansionDirection="Down" VerticalAlignment="Top">
    <md:MdFabMenuItem Content="Scanner" Icon="{x:Static md:MdSymbols.QrCodeScanner}" />
    <md:MdFabMenuItem Content="Attachment" Icon="{x:Static md:MdSymbols.AttachFile}" />
</md:MdFabMenu>
```

---

## 2. Text Inputs & Search

### `MdTextBox`
Material 3 text field supporting Filled and Outlined container variants, floating labels, helper text, error state, and clear actions.

| Property | Type | Default | Description |
| :--- | :--- | :--- | :--- |
| `Variant` | `MdTextBoxVariant` | `Outlined` | `Outlined` or `Filled` |
| `Label` | `string?` | `null` | Floating label text |
| `PlaceholderText` | `string?` | `null` | Watermark text |
| `HelperText` | `string?` | `null` | Persistent helper text below field |
| `ErrorText` | `string?` | `null` | Validation error text |
| `ShowClearButton` | `bool` | `false` | Shows trailing clear button when text is non-empty |

```xml
<md:MdTextBox Label="Email" Variant="Outlined" ShowClearButton="True" />
```

### `MdSearchBar` & `MdSearchView`
Full-width Material 3 search bar with docking search view overlay.

```xml
<md:MdSearchBar PlaceholderText="Search settings and files..." />
```

---

## 3. Selection Controls

### `MdSwitch`, `MdCheckBox`, `MdRadioButton`, `MdSlider`
```xml
<md:MdSwitch Content="Dark Mode" IsChecked="{Binding IsDark}" />
<md:MdCheckBox Content="Remember password" IsChecked="True" />
<md:MdSlider Minimum="0" Maximum="100" Value="60" />
```

---

## 4. Settings Cards & Groups

### `MdSettingsCard` & `MdSettingsExpander`
Desktop and mobile settings row with icon, title, description, and action content.

```xml
<md:MdSettingsGroup Header="Personalization">
    <md:MdSettingsCard Header="App Theme" Description="Choose light, dark, or system default" Icon="{x:Static md:MdSymbols.Palette}">
        <md:MdSwitch IsChecked="True" />
    </md:MdSettingsCard>

    <md:MdSettingsExpander Header="Network Configuration" Description="Manage proxies and DNS" Icon="{x:Static md:MdSymbols.Wifi}">
        <StackPanel Spacing="8">
            <md:MdTextBox Label="Proxy Server" Variant="Outlined" />
        </StackPanel>
    </md:MdSettingsExpander>
</md:MdSettingsGroup>
```

---

## 5. Dialogs & Sheets

### `MdDialogHost` & `MdSheetHost`
Modal dialogs and bottom/side sheets with backdrop dismiss and keyboard escape support. A dialog host accepts controls directly or selects any number of predeclared `DataTemplate` views by the active model type.

```xml
<md:MdDialogHost x:Name="DialogHost">
    <md:MdDialogHost.DataTemplates>
        <DataTemplate DataType="{x:Type vm:DeleteDialogModel}">
            <md:MdDialog Headline="Delete item?">...</md:MdDialog>
        </DataTemplate>
        <DataTemplate DataType="{x:Type vm:RenameDialogModel}">
            <md:MdDialog Headline="Rename item">...</md:MdDialog>
        </DataTemplate>
    </md:MdDialogHost.DataTemplates>
</md:MdDialogHost>
```

```csharp
var result = await DialogHost.ShowAsync(new DeleteDialogModel(item));
DialogHost.Close(result: true);
```

### `MdSnackbarHost` & `IMdSnackbarService`

`MdSnackbar.Show()` changes the state of an existing visual control; it does not attach a newly constructed control to a window. For ViewModels, put an `MdSnackbarHost` in the application shell and share one `MdSnackbarService` instance between that host and the ViewModel.

```csharp
await snackbars.ShowAsync(new MdSnackbarMessage("Draft archived")
{
    ActionContent = "Undo",
    ActionCommand = UndoCommand,
    IsDismissible = true
});
```
