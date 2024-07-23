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

            //MapPoint mapCenterPoint = new MapPoint(32.805, 34.027, SpatialReferences.Wgs84);
            //MainMapView.SetViewpoint(new Viewpoint(mapCenterPoint, 100000));
            var viewModel = new MapViewModel();
            // Assuming MainMapView is the name of your MapView control in XAML
            MainMapView.GeoViewTapped += viewModel.MapView_GeoViewTapped;

            // Set the DataContext if it's not already set in XAML
            this.DataContext = viewModel;
        }
    }
}