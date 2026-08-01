using System;
using System.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;


namespace StudentAccountTSTU;

public partial class NavigationBar : UserControl {
    #region FIELDS
    public static readonly StyledProperty<Orientation> OrientationProperty = AvaloniaProperty.Register<NavigationBar, Orientation>(nameof(Orientation), Orientation.Horizontal);
    public static readonly StyledProperty<int> SelectedIndexProperty = AvaloniaProperty.Register<NavigationBar, int>(nameof(SelectedIndex), 0, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    private readonly TranslateTransform _selectionTransform = new(0, 0);
    #endregion

    public Orientation Orientation {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public int SelectedIndex {
        get => GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    public NavigationBar() {
        InitializeComponent();

        SelectionGrid.RenderTransform = _selectionTransform;

        Loaded += (sender, e) => {
            UpdateElementsSize();
            UpdateElementsPosition();
        };

        ButtonsGrid.SizeChanged += (sender, e) => {
            UpdateElementsSize();
            UpdateElementsPosition();
        };

        ButtonsGrid.AddHandler(PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel);

        SelectedIndexProperty.Changed.AddClassHandler<NavigationBar>((control, e) => control.OnSelectedIndexChanged(e));
    }

    private void OnSelectedIndexChanged(AvaloniaPropertyChangedEventArgs e) {
        var newIndex = (int)e.NewValue!;
        UpdateUIForIndex(newIndex);
    }

    private void UpdateUIForIndex(int index) {
        if (ButtonsGrid.Children.Count is 0)
            return;

        if (index < 0 || index >= ButtonsGrid.Children.Count)
            return;

        foreach (var button in ButtonsGrid.Children.OfType<RadioButton>())
            button.IsChecked = false;

        if (ButtonsGrid.Children[index] is RadioButton targetButton)
            targetButton.IsChecked = true;

        if (ButtonsGrid.Bounds.Width is 0 || ButtonsGrid.Bounds.Height == 0)
            return;

        var count = ButtonsGrid.Children.Count;
        var cellWidth = Orientation is Orientation.Horizontal ? ButtonsGrid.Bounds.Width / count : ButtonsGrid.Bounds.Width;
        var cellHeight = Orientation is Orientation.Vertical ? ButtonsGrid.Bounds.Height / count : ButtonsGrid.Bounds.Height;

        SnapToTarget(index, cellWidth, cellHeight);
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e) {
        base.OnSizeChanged(e);
        UpdateElementsSize();
        UpdateElementsPosition();
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e) {
        var count = ButtonsGrid.Children.Count;
        if (count is 0)
            return;

        var point = e.GetPosition(ButtonsGrid);

        var cellWidth = Orientation is Orientation.Horizontal ? ButtonsGrid.Bounds.Width / count : ButtonsGrid.Bounds.Width;
        var cellHeight = Orientation is Orientation.Vertical ? ButtonsGrid.Bounds.Height / count : ButtonsGrid.Bounds.Height;

        var targetIndex = 0;
        if (Orientation is Orientation.Horizontal && cellWidth > 0)
            targetIndex = (int)(point.X / cellWidth);
        else if (Orientation is Orientation.Vertical && cellHeight > 0)
            targetIndex = (int)(point.Y / cellHeight);

        targetIndex = Math.Clamp(targetIndex, 0, count - 1);

        SelectedIndex = targetIndex;
    }

    private void UpdateElementsSize() {
        var count = ButtonsGrid.Children.Count;
        if (count is 0 || ButtonsGrid.Bounds.Width is 0 || ButtonsGrid.Bounds.Height is 0)
            return;

        var cellWidth = Orientation is Orientation.Horizontal ? ButtonsGrid.Bounds.Width / count : ButtonsGrid.Bounds.Width;
        var cellHeight = Orientation is Orientation.Vertical ? ButtonsGrid.Bounds.Height / count : ButtonsGrid.Bounds.Height;

        SelectionGrid.Width = cellWidth;
        SelectionGrid.Height = cellHeight;

        if (Orientation is Orientation.Horizontal) {
            Pill.Width = cellWidth * 0.3;
            Pill.Height = 2;
            Pill.VerticalAlignment = VerticalAlignment.Bottom;
            Pill.HorizontalAlignment = HorizontalAlignment.Center;
        }
        else {
            Pill.Width = 2;
            Pill.Height = cellHeight * 0.3;
            Pill.HorizontalAlignment = HorizontalAlignment.Left;
            Pill.VerticalAlignment = VerticalAlignment.Center;
        }
    }

    private void UpdateElementsPosition() {
        var checkedButton = ButtonsGrid.Children.OfType<RadioButton>().FirstOrDefault(b => b.IsChecked is true);
        
        if (checkedButton is null)
            return;

        var count = ButtonsGrid.Children.Count;
        var index = ButtonsGrid.Children.IndexOf(checkedButton);

        var cellWidth = Orientation is Orientation.Horizontal ? ButtonsGrid.Bounds.Width / count: ButtonsGrid.Bounds.Width;
        var cellHeight = Orientation is Orientation.Vertical ? ButtonsGrid.Bounds.Height / count: ButtonsGrid.Bounds.Height;

        SnapToTarget(index, cellWidth, cellHeight);
    }


    private void SnapToTarget(int index, double cellWidth, double cellHeight) {
        double targetX = Orientation is Orientation.Horizontal ? index * cellWidth : 0;
        double targetY = Orientation is Orientation.Vertical ? index * cellHeight : 0;

        _selectionTransform.X = targetX;
        _selectionTransform.Y = targetY;
    }
}