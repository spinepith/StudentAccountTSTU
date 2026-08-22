using System;
using System.Linq;

using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
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
    public static readonly StyledProperty<System.Windows.Input.ICommand?> TabClickedCommandProperty = AvaloniaProperty.Register<NavigationBar, System.Windows.Input.ICommand?>(nameof(TabClickedCommand));

    private readonly TranslateTransform _selectionTransform = new(0, 0);
    private readonly ScaleTransform _scaleTransform = new(1, 1);
    private readonly ScaleTransform _pillScaleTransform = new(1, 1);
    private readonly double SelectorCornerRadius = 24;
    private bool _isDragging = false;
    private bool _hasMoved = false;
    private Point _dragStartPoint;
    private Transitions? _animationTransitions;
    #endregion

    public Orientation Orientation {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public int SelectedIndex {
        get => GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    public System.Windows.Input.ICommand? TabClickedCommand {
        get => GetValue(TabClickedCommandProperty);
        set => SetValue(TabClickedCommandProperty, value);
    }

    public NavigationBar() {
        InitializeComponent();

        SelectionGrid.RenderTransform = _selectionTransform;
        Selector.RenderTransform = _scaleTransform;
        Selector.RenderTransformOrigin = RelativePoint.Center;
        Pill.RenderTransform = _pillScaleTransform;
        Pill.RenderTransformOrigin = RelativePoint.Center;

        _animationTransitions = new Transitions
        {
            new DoubleTransition
            {
                Property = TranslateTransform.XProperty,
                Duration = TimeSpan.FromMilliseconds(150),
                Easing = new CubicEaseOut()
            },
            new DoubleTransition
            {
                Property = TranslateTransform.YProperty,
                Duration = TimeSpan.FromMilliseconds(150),
                Easing = new CubicEaseOut()
            },
            new DoubleTransition
            {
                Property = ScaleTransform.ScaleXProperty,
                Duration = TimeSpan.FromMilliseconds(150),
                Easing = new CubicEaseOut()
            },
            new DoubleTransition
            {
                Property = ScaleTransform.ScaleYProperty,
                Duration = TimeSpan.FromMilliseconds(150),
                Easing = new CubicEaseOut()
            }
        };
        _selectionTransform.Transitions = _animationTransitions;
        _scaleTransform.Transitions = _animationTransitions;
        _pillScaleTransform.Transitions = _animationTransitions;

        Loaded += (sender, e) => {
            UpdateElementsSize();
            UpdateElementsPosition();
        };

        ButtonsGrid.SizeChanged += (sender, e) => {
            UpdateElementsSize();
            UpdateElementsPosition();
        };

        ButtonsGrid.AddHandler(PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel);
        ButtonsGrid.AddHandler(PointerMovedEvent, OnPointerMoved, RoutingStrategies.Tunnel);
        ButtonsGrid.AddHandler(PointerReleasedEvent, OnPointerReleased, RoutingStrategies.Tunnel);
        ButtonsGrid.AddHandler(PointerCaptureLostEvent, OnPointerCaptureLost, RoutingStrategies.Tunnel);

    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
        base.OnPropertyChanged(change);
        if (change.Property == SelectedIndexProperty) {
            OnSelectedIndexChanged(change);
        }
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

        if (ButtonsGrid.Bounds.Width is 0 || ButtonsGrid.Bounds.Height is 0)
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

        _isDragging = true;
        _hasMoved = false;
        _dragStartPoint = e.GetPosition(ButtonsGrid);

        var cellWidth = Orientation is Orientation.Horizontal ? ButtonsGrid.Bounds.Width / count : ButtonsGrid.Bounds.Width;
        var cellHeight = Orientation is Orientation.Vertical ? ButtonsGrid.Bounds.Height / count : ButtonsGrid.Bounds.Height;

        if (Orientation is Orientation.Horizontal) {
            var newX = Math.Clamp(_dragStartPoint.X - SelectionGrid.Width / 2, 0, ButtonsGrid.Bounds.Width - SelectionGrid.Width);
            _selectionTransform.X = newX;
        }
        else {
            var newY = Math.Clamp(_dragStartPoint.Y - SelectionGrid.Height / 2, 0, ButtonsGrid.Bounds.Height - SelectionGrid.Height);
            _selectionTransform.Y = newY;
        }

        if (Orientation is Orientation.Horizontal) {
            _scaleTransform.ScaleX = 1.4;
            _scaleTransform.ScaleY = 1.4;
            _pillScaleTransform.ScaleX = 1.6;
        }
        else {
            _scaleTransform.ScaleX = 1.3;
            _scaleTransform.ScaleY = 1.2;
            _pillScaleTransform.ScaleY = 1.3;
        }

        Selector.CornerRadius = new CornerRadius(26);

        e.Handled = true;
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e) {
        if (!_isDragging)
            return;

        var count = ButtonsGrid.Children.Count;
        if (count is 0)
            return;

        var point = e.GetPosition(ButtonsGrid);

        if (!_hasMoved)
            _hasMoved = true;

        var cellWidth = Orientation is Orientation.Horizontal ? ButtonsGrid.Bounds.Width / count : ButtonsGrid.Bounds.Width;
        var cellHeight = Orientation is Orientation.Vertical ? ButtonsGrid.Bounds.Height / count : ButtonsGrid.Bounds.Height;

        if (Orientation is Orientation.Horizontal) {
            var newX = Math.Clamp(point.X - SelectionGrid.Width / 2, 0, ButtonsGrid.Bounds.Width - SelectionGrid.Width);
            _selectionTransform.X = newX;
        }
        else {
            var newY = Math.Clamp(point.Y - SelectionGrid.Height / 2, 0, ButtonsGrid.Bounds.Height - SelectionGrid.Height);
            _selectionTransform.Y = newY;
        }
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e) {
        if (!_isDragging)
            return;

        _scaleTransform.ScaleX = 1.0;
        _scaleTransform.ScaleY = 1.0;
        _pillScaleTransform.ScaleX = 1.0;
        _pillScaleTransform.ScaleY = 1.0;
        Selector.CornerRadius = new CornerRadius(SelectorCornerRadius);

        _isDragging = false;
        var wasMoved = _hasMoved;
        _hasMoved = false;

        var count = ButtonsGrid.Children.Count;
        if (count is 0)
            return;

        var cellWidth = Orientation is Orientation.Horizontal ? ButtonsGrid.Bounds.Width / count : ButtonsGrid.Bounds.Width;
        var cellHeight = Orientation is Orientation.Vertical ? ButtonsGrid.Bounds.Height / count : ButtonsGrid.Bounds.Height;

        int targetIndex;

        if (wasMoved) {
            if (Orientation is Orientation.Horizontal && cellWidth > 0) {
                var selectorCenterX = _selectionTransform.X + SelectionGrid.Width / 2;
                targetIndex = (int)(selectorCenterX / cellWidth);
            }
            else if (Orientation is Orientation.Vertical && cellHeight > 0) {
                var selectorCenterY = _selectionTransform.Y + SelectionGrid.Height / 2;
                targetIndex = (int)(selectorCenterY / cellHeight);
            }
            else
                targetIndex = 0;
        }
        else {
            var point = e.GetPosition(ButtonsGrid);
            if (Orientation is Orientation.Horizontal && cellWidth > 0)
                targetIndex = (int)(point.X / cellWidth);
            else if (Orientation is Orientation.Vertical && cellHeight > 0)
                targetIndex = (int)(point.Y / cellHeight);
            else
                targetIndex = 0;
        }

        targetIndex = Math.Clamp(targetIndex, 0, count - 1);

        if (targetIndex == SelectedIndex)
            SnapToTarget(targetIndex, cellWidth, cellHeight);
        else
            SelectedIndex = targetIndex;

        if (TabClickedCommand?.CanExecute(targetIndex) is true) {
            System.Threading.Tasks.Task.Delay(150).ContinueWith(_ => {
                Avalonia.Threading.Dispatcher.UIThread.Post(() => {
                    TabClickedCommand.Execute(targetIndex);
                });
            });
        }
    }

    private void OnPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e) {
        _scaleTransform.ScaleX = 1.0;
        _scaleTransform.ScaleY = 1.0;
        _pillScaleTransform.ScaleX = 1.0;
        _pillScaleTransform.ScaleY = 1.0;
        _isDragging = false;
        _hasMoved = false;
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

        var cellWidth = Orientation is Orientation.Horizontal ? ButtonsGrid.Bounds.Width / count : ButtonsGrid.Bounds.Width;
        var cellHeight = Orientation is Orientation.Vertical ? ButtonsGrid.Bounds.Height / count : ButtonsGrid.Bounds.Height;

        SnapToTarget(index, cellWidth, cellHeight);
    }


    private void SnapToTarget(int index, double cellWidth, double cellHeight) {
        double targetX = Orientation is Orientation.Horizontal ? index * cellWidth : 0;
        double targetY = Orientation is Orientation.Vertical ? index * cellHeight : 0;

        _selectionTransform.X = targetX;
        _selectionTransform.Y = targetY;
    }
}
