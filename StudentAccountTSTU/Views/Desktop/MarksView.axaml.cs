using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using System.Linq;

namespace StudentAccountTSTU.Views.Desktop {
    public partial class MarksView : UserControl {
        private ScrollViewer? _activeScrollViewer;
        private Point _lastPointerPosition;
        private bool _isLeftButtonPressed;

        public MarksView() {
            InitializeComponent();
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e) {
            var properties = e.GetCurrentPoint(this).Properties;
            if (properties.IsLeftButtonPressed) {
                var clickedElement = e.Source as Control;
                var scrollViewer = clickedElement?.FindAncestorOfType<ScrollViewer>();

                if (scrollViewer != null &&
                    scrollViewer.HorizontalScrollBarVisibility == Avalonia.Controls.Primitives.ScrollBarVisibility.Auto &&
                    scrollViewer.Extent.Width > scrollViewer.Viewport.Width) {
                    _activeScrollViewer = scrollViewer;
                    _lastPointerPosition = e.GetPosition(this);
                    _isLeftButtonPressed = true;
                    e.Handled = true;
                    return;
                }
            }
            base.OnPointerPressed(e);
        }

        protected override void OnPointerMoved(PointerEventArgs e) {
            if (_isLeftButtonPressed && _activeScrollViewer != null) {
                var currentPosition = e.GetPosition(this);
                var delta = currentPosition - _lastPointerPosition;

                _activeScrollViewer.Offset = _activeScrollViewer.Offset.WithX(_activeScrollViewer.Offset.X - delta.X);

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
                e.Handled = true;
                return;
            }
            base.OnPointerReleased(e);
        }
    }
}