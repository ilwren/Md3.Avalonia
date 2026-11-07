using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Sortable.Avalonia;
using Xunit;
using SortableBehavior = Sortable.Avalonia.Sortable;

namespace Md3.Avalonia.HeadlessTests;

/// <summary>Characterization of a test-only vendor candidate, not production conformance tests.</summary>
public sealed class SortableVendorProbeTests
{
    [AvaloniaFact]
    public void Restored_Assembly_And_Explicit_Mutation_Contracts()
    {
        Assert.Equal(new Version(2, 3, 1, 0), typeof(SortableBehavior).Assembly.GetName().Version);
        var source = new ObservableCollection<string> { "A", "B" };
        var target = new ObservableCollection<string> { "C" };
        var rejected = new SortableDropEventArgs
        {
            Item = "A", SourceCollection = source, TargetCollection = target,
            OldIndex = 0, NewIndex = 0, IsAccepted = false
        };
        Assert.False(rejected.ApplyDropMutation());
        Assert.Equal(new[] { "A", "B" }, source);
        Assert.Equal(new[] { "C" }, target);
        rejected.IsAccepted = true;
        Assert.True(rejected.ApplyDropMutation());
        Assert.Equal(new[] { "B" }, source);
        Assert.Equal(new[] { "A", "C" }, target);
    }

    [AvaloniaFact]
    public void Stale_Source_Index_Can_Change_Target_Even_When_Move_Returns_False()
    {
        var source = new ObservableCollection<string> { "A" };
        var target = new ObservableCollection<string> { "B" };
        var move = new SortableDropEventArgs
        {
            Item = "A", SourceCollection = source, TargetCollection = target,
            OldIndex = -1, NewIndex = 0
        };
        // Characterize the version under review: false is NOT an atomic rollback.
        Assert.False(move.ApplyDropMutation());
        Assert.Equal(new[] { "A" }, source);
        Assert.Equal(new[] { "A", "B" }, target);
    }

    [AvaloniaFact]
    public void Tracking_Strongly_Retains_Unattached_Controls_Until_Disabled()
    {
        var weak = CreateTrackedControl();
        try
        {
            DrainAndCollect();
            Assert.True(weak.IsAlive); // regression documented upstream as issue #25
            DisableTracking(weak);
            DrainAndCollect();
            Assert.False(weak.IsAlive);
        }
        finally { DisableTracking(weak); }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference CreateTrackedControl()
    {
        var list = new ItemsControl { ItemsSource = new ObservableCollection<string> { "A" } };
        SortableBehavior.SetSortable(list, true);
        return new WeakReference(list);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void DisableTracking(WeakReference weak)
    {
        if (weak.Target is not ItemsControl list) return;
        SortableBehavior.SetSortable(list, false);
        SortableBehavior.SetDroppable(list, false);
    }

    private static void DrainAndCollect()
    {
        for (var i = 0; i < 4; i++)
        {
            Dispatcher.UIThread.RunJobs();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }

    [AvaloniaFact]
    public void Real_Pointer_Drag_Can_Transfer_Between_Same_Group_Lists()
    {
        var source = new ObservableCollection<string> { "A", "B" };
        var target = new ObservableCollection<string> { "C" };
        var first = List(source);
        var second = List(target);
        var drops = 0;
        SortableBehavior.SetDropCommand(second, new CallbackCommand(value =>
        {
            var drop = Assert.IsType<SortableDropEventArgs>(value);
            drops++;
            Assert.True(drop.ApplyDropMutation());
        }));
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 30, Children = { first, second } };
        var window = new Window { Width = 600, Height = 300, Content = panel };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        try
        {
            var card = first.GetVisualDescendants().OfType<Border>().Single(b => b.Name == "ProbeCard" && Equals(b.DataContext, "A"));
            var from = card.TranslatePoint(new Point(20, 20), window)!.Value;
            var to = second.TranslatePoint(new Point(40, 28), window)!.Value;
            window.MouseMove(from, RawInputModifiers.None);
            window.MouseDown(from, MouseButton.Left, RawInputModifiers.None);
            window.MouseMove(new Point(from.X + 12, from.Y), RawInputModifiers.LeftMouseButton);
            window.MouseMove(to, RawInputModifiers.LeftMouseButton);
            window.MouseUp(to, MouseButton.Left, RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(1, drops);
            Assert.Equal(new[] { "B" }, source);
            Assert.Contains("A", target);
        }
        finally
        {
            SortableBehavior.SetSortable(first, false);
            SortableBehavior.SetDroppable(first, false);
            SortableBehavior.SetSortable(second, false);
            SortableBehavior.SetDroppable(second, false);
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static ItemsControl List(ObservableCollection<string> source)
    {
        var list = new ItemsControl
        {
            Width = 200, Height = 200, VerticalAlignment = VerticalAlignment.Top, ItemsSource = source,
            ItemTemplate = new FuncDataTemplate<string>((value, _) =>
            {
                var card = new Border
                {
                    Name = "ProbeCard", Height = 52, Background = Brushes.LightGray,
                    Child = new TextBlock { Text = value, Margin = new Thickness(12) }
                };
                SortableBehavior.SetIsDroppable(card, true);
                return card;
            })
        };
        SortableBehavior.SetGroup(list, "probe");
        SortableBehavior.SetSortable(list, true);
        SortableBehavior.SetDroppable(list, true);
        return list;
    }

    private sealed class CallbackCommand(Action<object?> callback) : ICommand
    {
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => callback(parameter);
        public event EventHandler? CanExecuteChanged { add { } remove { } }
    }
}
