using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace StudentAccountTSTU.Views.Mobile {
    public partial class MarksView : UserControl {
        private ScrollViewer? _activeScrollViewer;
        private ScrollViewer? _parentScrollViewer;
        private Point _lastPointerPosition;
        private bool _isLeftButtonPressed;

        public MarksView() {
            InitializeComponent();
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e) {
            var properties = e.GetCurrentPoint(this).Properties;
            if (properties.IsLeftButtonPressed) {
                var clickedElement = e.Source as Control;

                var current = clickedElement;
                ScrollViewer? horizontalScroll = null;
                ScrollViewer? verticalScroll = null;

                while (current is not null) {
                    if (current is ScrollViewer scrollViewer) {
                        bool canScrollHorizontally = scrollViewer.Extent.Width > scrollViewer.Viewport.Width;
                        bool canScrollVertically = scrollViewer.Extent.Height > scrollViewer.Viewport.Height;

                        if (canScrollHorizontally && horizontalScroll is null)
                            horizontalScroll = scrollViewer;

                        if (canScrollVertically && verticalScroll is null)
                            verticalScroll = scrollViewer;

                        if (horizontalScroll is not null && verticalScroll is not null)
                            break;
                    }
                    current = current.Parent as Control;
                }

                if (horizontalScroll is not null || verticalScroll is not null) {
                    _activeScrollViewer = horizontalScroll ?? verticalScroll;
                    _parentScrollViewer = verticalScroll != horizontalScroll ? verticalScroll : null;
                    _lastPointerPosition = e.GetPosition(this);
                    _isLeftButtonPressed = true;
                    e.Handled = true;
                    return;
                }
            }
            base.OnPointerPressed(e);
        }

        protected override void OnPointerMoved(PointerEventArgs e) {
            if (_isLeftButtonPressed && _activeScrollViewer is not null) {
                var currentPosition = e.GetPosition(this);
                var delta = currentPosition - _lastPointerPosition;

                _activeScrollViewer.Offset = new Vector(
                    _activeScrollViewer.Offset.X - delta.X,
                    _activeScrollViewer.Offset.Y
                );

                if (_parentScrollViewer is not null) {
                    _parentScrollViewer.Offset = new Vector(
                        _parentScrollViewer.Offset.X,
                        _parentScrollViewer.Offset.Y - delta.Y
                    );
                }

                _lastPointerPosition = currentPosition;
                e.Handled = true;
                return;
            }
            base.OnPointerMoved(e);
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e) {
            var properties = e.GetCurrentPoint(this).Properties;
            if (!properties.IsLeftButtonPressed && _isLeftButtonPressed) {
                _isLeftButtonPressed = false;
                _activeScrollViewer = null;
                _parentScrollViewer = null;
                e.Handled = true;
                return;
            }
            base.OnPointerReleased(e);
        }
    }
}