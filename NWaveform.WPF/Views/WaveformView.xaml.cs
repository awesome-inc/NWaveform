using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using NWaveform.ViewModels;

namespace NWaveform.Views
{
    public partial class WaveformView : IHaveWaveformImage
    {
        private Point _mouseDownPoint;
        private Point _currentPoint;
        private IWaveformViewModel _viewModel;
        private bool _isMouseDown;

        public ImageBrush WaveformImageBrush => WaveformImage;

        public WaveformView()
        {
            InitializeComponent();

            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));

            _viewModel = e.NewValue as IWaveformViewModel;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));

            if (_viewModel == null) return;
            _isMouseDown = true;
            _mouseDownPoint = e.GetPosition(WaveformCanvas);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));

            if (!_isMouseDown || _viewModel == null) return;
            _currentPoint = e.GetPosition(WaveformCanvas);
            
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                _viewModel.Selection.Top = _currentPoint.Y < 0 ? -1 : 0;
                _viewModel.Selection.Height = 1;
            }
            else
            {
                _viewModel.Selection.Top = -1;
                _viewModel.Selection.Height = 2;
            }

            _viewModel.Selection.Start = Math.Min(_currentPoint.X, _mouseDownPoint.X);
            _viewModel.Selection.End = Math.Max(_currentPoint.X, _mouseDownPoint.X);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));

            if (!_isMouseDown || _viewModel == null) return;
            _currentPoint = e.GetPosition(WaveformCanvas);

            // if selection almost empty, click to position
            var delta = Math.Abs(_currentPoint.X - _mouseDownPoint.X);
            var pixelDelta = delta * (WaveformCanvas.ActualWidth/_viewModel.Duration);
            if (pixelDelta < 3)
                _viewModel.Position = _currentPoint.X;
            else
            {
                _viewModel.Selection.Start = Math.Min(_currentPoint.X, _mouseDownPoint.X);
                _viewModel.Selection.End = Math.Max(_currentPoint.X, _mouseDownPoint.X);
                // HACK: raise "property changed"
                _viewModel.Selection = _viewModel.Selection; 
            }

            _isMouseDown = false;
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));
            if (_viewModel == null) return;

            // check if right-clicked the selection
            var pt = e.GetPosition(SelectionRectangle);
            var result = VisualTreeHelper.HitTest(SelectionRectangle, pt);
            if (result == null) return;
            
            var cm = SelectionRectangle.ContextMenu;
            if (cm == null) return;

            // suppress empty context menu
            if (_viewModel.SelectionMenu.IsEmpty(_viewModel.Selection)) return;

            cm.PlacementTarget = SelectionRectangle;
            cm.IsOpen = true;
        }
    }
}
