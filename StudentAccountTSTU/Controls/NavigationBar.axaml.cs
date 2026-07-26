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
    #region PUBLIC
    public static readonly StyledProperty<Orientation> OrientationProperty = AvaloniaProperty.Register<NavigationBar, Orientation>(nameof(Orientation), defaultValue: Orientation.Horizontal);
    public Orientation Orientation {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public event EventHandler<int?>? PageChanged;
    #endregion

    #region PRIVATE
    private readonly TranslateTransform _selectionTransform = new(0, 0);
    #endregion
    #endregion

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
        if (ButtonsGrid.Children[targetIndex] is RadioButton targetButton)
            targetButton.IsChecked = true;

        SnapToTarget(targetIndex, cellWidth, cellHeight);
        PageChanged?.Invoke(this, targetIndex);
    }

    private void UpdateElementsSize() {
        var count = ButtonsGrid.Children.Count;
        if (count == 0 || ButtonsGrid.Bounds.Width is 0 || ButtonsGrid.Bounds.Height is 0)
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