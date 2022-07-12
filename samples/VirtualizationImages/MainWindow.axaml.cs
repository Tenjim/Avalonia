using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.VisualTree;
using VirtualizationImages.ViewModels;

namespace VirtualizationImages
{
    public class MainWindow : Window
    {
        private MainWindowViewModel _mainWindowViewModel;
        public MainWindow()
        {
            this.InitializeComponent();

            _lstSingleScrolling = this.FindControl<ListBox>("listBox");
            _lstSingleScrolling.AddHandler(PointerWheelChangedEvent, ScrollViewer_OnPreviewMouseWheel, RoutingStrategies.Tunnel);
            this.AttachDevTools();
            DataContext = new MainWindowViewModel();
            _mainWindowViewModel = (MainWindowViewModel)DataContext;
            _mainWindowViewModel.ZoomEventHandler += ZoomEventHandler;

            ZoomChanged();
        }

        private void ZoomEventHandler(object sender, EventArgs e)
        {
            ZoomChanged();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            
        }

        private static bool _isLoaded;
        private static object _LockObjet = new();
        private void CtlImage_OnLayoutUpdated(object? sender, EventArgs e)
        {
            lock (_LockObjet)
            {
                if (!_isLoaded)
                {
                    OnLoaded();
                    _isLoaded = true;
                }
            }
        }

        void OnLoaded()
        {
            ZoomChanged();
        }

        private ListBox _lstSingleScrolling;
        private ItemsPresenter _itemsPresenter;

        private ItemsPresenter ItemsPresenter
        {
            get
            {
                if (_itemsPresenter == null)
                {
                    _itemsPresenter = _lstSingleScrolling.FindDescendantOfType<ItemsPresenter>();
                }
                return _itemsPresenter;
            }
        }

        /// <summary>
        /// Creates a matrix that is scaling from a specified center.
        /// </summary>
        /// <param name="scaleX">Scaling factor that is applied along the x-axis.</param>
        /// <param name="scaleY">Scaling factor that is applied along the y-axis.</param>
        /// <param name="centerX">The center X-coordinate of the scaling.</param>
        /// <param name="centerY">The center Y-coordinate of the scaling.</param>
        /// <returns>The created scaling matrix.</returns>
        private Matrix ScaleAt(double scaleX, double scaleY, double centerX, double centerY)
        {
            return new Matrix(scaleX, 0, 0, scaleY, centerX - (scaleX * centerX), centerY - (scaleY * centerY));
        }

        private void ZoomChanged()
        {
            var allLayoutTransform = this.GetVisualDescendants().OfType<LayoutTransformControl>();

            if (ItemsPresenter != null)
            {
                var zoom = _mainWindowViewModel.Zoom;
                var matrix = new Matrix();


                var center = new Point((_lstSingleScrolling.Bounds.Width / 2) - ItemsPresenter.Bounds.Left,
                    (_lstSingleScrolling.Bounds.Height / 2) - ItemsPresenter.Bounds.Top);

                matrix = ScaleAt(zoom, zoom, center.X, center.Y);
                foreach (LayoutTransformControl layoutTransform in allLayoutTransform)
                {
                    layoutTransform.LayoutTransform = new MatrixTransform(matrix);
                }
                
            }

        }

        private void ScrollViewer_OnPreviewMouseWheel(object sender, PointerWheelEventArgs e)
        {
            if (e.KeyModifiers != KeyModifiers.Control)
                return;

            if (e.Delta.Y > 0)
            {
                _mainWindowViewModel.ZoomIn();
            }

            if (e.Delta.Y < 0)
            {
                _mainWindowViewModel.ZoomOut();
            }


            e.Handled = true;
        }

    }
}
