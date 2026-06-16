using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;

namespace AruaRoseLoginManager.Controls
{
    public partial class PositionPickerWindow : Window
    {
        private bool _isDragging;
        private Point _dragStart;
        private double _boxLeft;
        private double _boxTop;
        private double _dpiScaleX = 1.0;
        private double _dpiScaleY = 1.0;
        private readonly int _boxWidthPx;
        private readonly int _boxHeightPx;
        private readonly int _initialXPx;
        private readonly int _initialYPx;

        public int ResultX { get; private set; }
        public int ResultY { get; private set; }

        public PositionPickerWindow(int boxWidth, int boxHeight, int initialX, int initialY)
        {
            _boxWidthPx = boxWidth;
            _boxHeightPx = boxHeight;
            _initialXPx = initialX;
            _initialYPx = initialY;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var source = PresentationSource.FromVisual(this);
            if (source?.CompositionTarget != null)
            {
                _dpiScaleX = source.CompositionTarget.TransformToDevice.M11;
                _dpiScaleY = source.CompositionTarget.TransformToDevice.M22;
            }

            // Cover all screens combined
            var bounds = SystemInformation.VirtualScreen;
            Left = bounds.Left / _dpiScaleX;
            Top = bounds.Top / _dpiScaleY;
            Width = bounds.Width / _dpiScaleX;
            Height = bounds.Height / _dpiScaleY;

            // Position the red box (convert screen pixels to DIPs, offset by window origin)
            _boxLeft = (_initialXPx - bounds.Left) / _dpiScaleX;
            _boxTop = (_initialYPx - bounds.Top) / _dpiScaleY;

            _dragBox.Width = _boxWidthPx / _dpiScaleX;
            _dragBox.Height = _boxHeightPx / _dpiScaleY;
            Canvas.SetLeft(_dragBox, _boxLeft);
            Canvas.SetTop(_dragBox, _boxTop);
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Return || e.Key == Key.Enter)
            {
                e.Handled = true;
                var bounds = SystemInformation.VirtualScreen;
                ResultX = (int)(_boxLeft * _dpiScaleX) + bounds.Left;
                ResultY = (int)(_boxTop * _dpiScaleY) + bounds.Top;
                DialogResult = true;
            }
            else if (e.Key == Key.Escape)
            {
                e.Handled = true;
                DialogResult = false;
            }
        }

        private void DragBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = true;
            _dragStart = e.GetPosition(_canvas);
            _dragBox.CaptureMouse();
            e.Handled = true;
        }

        private void DragBox_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!_isDragging) return;
            Point current = e.GetPosition(_canvas);
            _boxLeft += current.X - _dragStart.X;
            _boxTop += current.Y - _dragStart.Y;
            Canvas.SetLeft(_dragBox, _boxLeft);
            Canvas.SetTop(_dragBox, _boxTop);
            _dragStart = current;
        }

        private void DragBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            _dragBox.ReleaseMouseCapture();
        }
    }
}
