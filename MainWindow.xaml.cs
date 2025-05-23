using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;

namespace DisplayAMap
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var viewModel = new MapViewModel();
            // Assuming MainMapView is the name of your MapView control in XAML
            MainMapView.GraphicsOverlays = viewModel.GraphicsOverlays;
            MainMapView.GeoViewTapped += viewModel.MapView_GeoViewTapped;

            // Set the DataContext if it's not already set in XAML
            this.DataContext = viewModel;
        }


        private void Show1000(object sender, RoutedEventArgs e)
        {
            var viewModel = new MapViewModel();
            viewModel.show1000 = true;
        }
    }
}