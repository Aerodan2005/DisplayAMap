using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Esri.ArcGISRuntime.Mapping;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Esri.ArcGISRuntime.Portal;

using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.Geometry;

using Esri.ArcGISRuntime.Tasks.Offline;

using System.Windows;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.UI.Controls;
using Windows.Devices.Radios;
using Newtonsoft.Json.Linq;
using MetisDB;


//doronnnnn
namespace DisplayAMap
{
    public partial class MapViewModel : INotifyPropertyChanged
    {

        private MainWindow _mainWindow;

        public MapViewModel(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            InitializeMap();
        }
        //private string _downloadLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "OfflineMapMED_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"));
        public string _downloadLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "OfflineMap");
        public MapViewModel()
        {
            // Use a basic imagery basemap instead of trying to load files right away
            // This prevents errors during application startup
            try
            {
                // Create a simple basemap without trying to access files
                Map = new Map(BasemapStyle.ArcGISImagery);
                
                // Initialize GraphicsOverlays collection
                if (_graphicsOverlays == null)
                {
                    _graphicsOverlays = new GraphicsOverlayCollection();
                }
                
                // We'll initialize the full map later once the application is ready
                Console.WriteLine("Created basic MapViewModel with imagery basemap");
                
                // IMPORTANT: Don't try to load any files in the constructor
                // Just create a basic map that will work no matter what
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in MapViewModel constructor: {ex.Message}");
            }
        }

        private async void InitializeMap()
        {
            try
            {
                // Delay initialization until everything is ready
                await Task.Delay(1000);
                
                // Create a basic online map that doesn't require any local files
                // This is the safest approach since we don't know if files exist
                Map = new Map(BasemapStyle.ArcGISImagery);
                
                Console.WriteLine("Map initialized with imagery basemap (online version)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in InitializeMap: {ex.Message}");
            }
        }
        
        // New method that safely loads local map files if they're available
        public async Task TryLoadLocalMapFiles(string baseDataFolder)
        {
            try
            {
                // Return immediately if the path is invalid
                if (string.IsNullOrEmpty(baseDataFolder) || !Directory.Exists(baseDataFolder))
                {
                    Console.WriteLine($"Warning: baseDataFolder doesn't exist: {baseDataFolder}");
                    return;
                }
                
                string mapLocation = System.IO.Path.Combine(baseDataFolder, "Maps", "MapNew.mmpk");
                
                if (!System.IO.File.Exists(mapLocation))
                {
                    Console.WriteLine($"Warning: Map file not found at {mapLocation}, keeping default imagery basemap");
                    return; // Keep existing online basemap
                }
                
                Console.WriteLine($"Loading local map package from: {mapLocation}");
                var mobileMapPackage = await MobileMapPackage.OpenAsync(mapLocation);
                await mobileMapPackage.LoadAsync();
                this.Map = mobileMapPackage.Maps.First();
                Console.WriteLine("Successfully loaded local map package");
                
                // Load TPK files only if they exist
                // First TPK file
                string tpkPath1 = System.IO.Path.Combine(baseDataFolder, "Maps", "world_imagery_tpk.tpk");
                if (File.Exists(tpkPath1))
                {
                    Console.WriteLine($"Loading TPK file: {tpkPath1}");
                    var tiledLayer1 = new ArcGISTiledLayer(new Uri(tpkPath1));
                    await tiledLayer1.LoadAsync();
                    Console.WriteLine("Successfully loaded first TPK file");
                }
                
                // Second TPK file
                string tpkPath2 = System.IO.Path.Combine(baseDataFolder, "Maps", "world_boundaries_and_places_4-11.tpk");
                if (File.Exists(tpkPath2))
                {
                    Console.WriteLine($"Loading TPK file: {tpkPath2}");
                    var tiledLayer2 = new ArcGISTiledLayer(new Uri(tpkPath2));
                    await tiledLayer2.LoadAsync();
                    Console.WriteLine("Successfully loaded second TPK file");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading local map files: {ex.Message}");
            }
        }

        private async Task SetupMap()
        {
            ArcGISPortal portal = await ArcGISPortal.CreateAsync();
            //string webMapId = "3bc3a553c2a640c5bace2f3065751dc8";
            string webMapId = "9d289b6059b14615b97b14e149a25eeb";

            PortalItem mapItem = await PortalItem.CreateAsync(portal, webMapId);
            Map map = new Map(mapItem);
            this.Map = map;
        }

        private async Task GetOfflinePreplannedMap()
        {
            var portal = await ArcGISPortal.CreateAsync();
            //var portalItem = await PortalItem.CreateAsync(portal, "3bc3a553c2a640c5bace2f3065751dc8");
            var portalItem = await PortalItem.CreateAsync(portal, "9d289b6059b14615b97b14e149a25eeb");
            var map = new Map(portalItem);

            OfflineMapTask offlineMapTask = await OfflineMapTask.CreateAsync(map);
            IReadOnlyList<PreplannedMapArea> availableAreas = await offlineMapTask.GetPreplannedMapAreasAsync();


            if (availableAreas?.FirstOrDefault() is PreplannedMapArea area)
            {
                DownloadPreplannedOfflineMapParameters downloadParameters = await offlineMapTask.CreateDefaultDownloadPreplannedOfflineMapParametersAsync(area);
                DownloadPreplannedOfflineMapJob job = offlineMapTask.DownloadPreplannedOfflineMap(downloadParameters, _downloadLocation);
                DownloadPreplannedOfflineMapResult result = await job.GetResultAsync();
                if (result.HasErrors)
                {
                    MessageBox.Show("Error downloading map");
                }
                else if (result?.OfflineMap is Map offlineMap)
                {
                    this.Map = offlineMap;
                }
            }
        }
        public event EventHandler MapAccessed;
        public async Task AccessMap()
        {
            try
            {
                // First check if AppConfigSvc.appCfg is properly initialized
                if (AppConfigSvc.appCfg == null)
                {
                    Console.WriteLine("Warning: AppConfigSvc.appCfg is null, using default imagery basemap");
                    this.Map = new Map(BasemapStyle.ArcGISImagery);
                    return;
                }
                
                if (string.IsNullOrEmpty(AppConfigSvc.appCfg.baseDataFolder))
                {
                    Console.WriteLine("Warning: baseDataFolder is not set, using default imagery basemap");
                    this.Map = new Map(BasemapStyle.ArcGISImagery);
                    return;
                }
                
                // Now try to access the map files
                string mapLocation = System.IO.Path.Combine(AppConfigSvc.appCfg.baseDataFolder, "Maps", "MapNew.mmpk");
                
                if (!System.IO.File.Exists(mapLocation))
                {
                    Console.WriteLine($"Warning: Map file not found at {mapLocation}, using default imagery basemap");
                    this.Map = new Map(BasemapStyle.ArcGISImagery);
                    return;
                }
                
                var mobileMapPackage = await MobileMapPackage.OpenAsync(mapLocation);
                await mobileMapPackage.LoadAsync();
                this.Map = mobileMapPackage.Maps.First();

                // Check for TPK files but don't fail if they don't exist
                // Load the first TPK file
                string tpkPath1 = System.IO.Path.Combine(AppConfigSvc.appCfg.baseDataFolder, "Maps","world_imagery_tpk.tpk");
                if (File.Exists(tpkPath1))
                {
                    var tiledLayer1 = new ArcGISTiledLayer(new Uri(tpkPath1));
                    await tiledLayer1.LoadAsync();
                }
                else
                {
                    Console.WriteLine($"Warning: TPK file not found at {tpkPath1}");
                }

                // Load the second TPK file
                string tpkPath2 = System.IO.Path.Combine(AppConfigSvc.appCfg.baseDataFolder, "Maps", "world_boundaries_and_places_4-11.tpk");
                if (File.Exists(tpkPath2))
                {
                    var tiledLayer2 = new ArcGISTiledLayer(new Uri(tpkPath2));
                    await tiledLayer2.LoadAsync();
                }
                else
                {
                    Console.WriteLine($"Warning: TPK file not found at {tpkPath2}");
                }
                
                // Successfully loaded the map
                Console.WriteLine("Map successfully loaded");
            }
            catch (Exception ex)
            {
                // If anything goes wrong, just use a default basemap
                Console.WriteLine($"Error loading map: {ex.Message}");
                this.Map = new Map(BasemapStyle.ArcGISImagery);
            }
        }
        protected virtual void OnMapAccessed()
        {
            MapAccessed?.Invoke(this, EventArgs.Empty);
        }

        private bool OfflineMapExists()
        {
            return Directory.Exists(_downloadLocation);
        }
        // Field to store the polyline builder
        public PolylineBuilder polylineBuilder;

        // Method to initialize the measuring tool
        public void InitializeMeasuringTool()
        {
            // Set the initial polyline builder with a spatial reference
            polylineBuilder = new PolylineBuilder(SpatialReferences.Wgs84);

            //this.GeoViewTapped += MapView_GeoViewTapped;


        }
        public bool show1000;
        public bool show500;

        public void AddTrajectoryToMap(Esri.ArcGISRuntime.Geometry.MapPoint newPoint)
        {
            // Check if the polylineBuilder is initialized, if not, initialize it.
            if (polylineBuilder == null)
            {
                polylineBuilder = new PolylineBuilder(SpatialReferences.Wgs84);
            }

            // Add the new point to the polyline builder
            polylineBuilder.AddPoint(newPoint);

            // Create a simple line symbol for the trajectory
            SimpleLineSymbol lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Red, 2);

            // Create a graphic for the polyline
            var graphic = new Graphic(polylineBuilder.ToGeometry(), lineSymbol);

            // Add the graphic to the map view
            CreateGraphics(graphic);
        }
        // Event handler for map view taps
        public void MapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // Project e.Location to WGS84
            var wgs84Point = GeometryEngine.Project(e.Location, SpatialReferences.Wgs84) as MapPoint;
            var point = new MapPoint(wgs84Point.X, wgs84Point.Y, SpatialReferences.Wgs84);
            // Add the tapped point to the polyline builder
            polylineBuilder.AddPoint(point.X, point.Y);
            // AddPointToMap(32, 32, 1000, "TA");
            if (false)
            {
                DrawCircleOnMap(point.Y, point.X, 1000.0);
                show1000 = false;
                return;
            }
            // Check if the polyline builder has 2 points
            if (polylineBuilder.Parts.Count > 0 && polylineBuilder.Parts[0].PointCount == 2)
            {
                // Create a polyline from the builder
                Polyline polyline = polylineBuilder.ToGeometry();

                // Create a composite symbol with a semi-transparent border
                var borderSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.FromArgb(64, 255, 0, 0), 40); // Semi-transparent red border
                var coreSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Red, 4); // Solid red core
                var circleSymbol1 = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.FromArgb(64, 255, 0, 0), 10); // Circle symbol with the same color as the border
                var circleSymbol2 = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.FromArgb(64, 255, 0, 0), 40); // Circle symbol with the same color as the border

                var compositeSymbol1 = new CompositeSymbol(new Symbol[] { borderSymbol, coreSymbol, circleSymbol1 });
                var compositeSymbol2 = new CompositeSymbol(new Symbol[] { borderSymbol, coreSymbol, circleSymbol2 });

                // Create a graphic with the composite symbol
                var polylineGraphic = new Graphic(polyline, compositeSymbol2);
                polylineGraphic.IsVisible = true;

                // Measure the distance of the polyline
                double distance = GeometryEngine.LengthGeodetic(polyline, LinearUnits.Kilometers, GeodeticCurveType.Geodesic);

                // Optionally, display the distance
                // MessageBox.Show($"Distance: {distance:F1} km");

                // Add the graphic to the map view
                CreateGraphics(polylineGraphic);
                // Display the distance (you might want to display this in the UI instead)
                //MessageBox.Show($"Distance: {distance :F1} km");
                AddPointToMap(point.Y, point.X, $"{distance:F1} km");

                // Reset the polyline builder for the next line
                polylineBuilder = new PolylineBuilder(SpatialReferences.Wgs84);

            }
        }
        private void AddPointToMap(double latitude, double longitude, string input)
        {
            // Create a point geometry
            MapPoint point = new MapPoint(longitude, latitude, SpatialReferences.Wgs84);

            // Create a symbol for the point
            SimpleMarkerSymbol pointSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.DarkRed, 10);

            // Create a graphic for the point
            Graphic pointGraphic = new Graphic(point, pointSymbol);

            // Add the point graphic to the graphics overlay
            CreateGraphics(pointGraphic);

            // Display the input above the point
            if (!string.IsNullOrEmpty(input))
            {
                TextSymbol textSymbol = new TextSymbol(input, System.Drawing.Color.DarkRed, 24, Esri.ArcGISRuntime.Symbology.HorizontalAlignment.Left, Esri.ArcGISRuntime.Symbology.VerticalAlignment.Top);
                Graphic textGraphic = new Graphic(point, textSymbol);
                CreateGraphics(textGraphic);
            }
        }
        public void DrawCircleOnMap(double latitude, double longitude, double radius)
        {
            // Create a point geometry
            MapPoint centerPoint = new MapPoint(longitude, latitude, SpatialReferences.Wgs84);

            // Create a buffer around the center point to represent the circle
            Geometry bufferGeometry = GeometryEngine.BufferGeodetic(centerPoint, radius, LinearUnits.Kilometers);

            // Create a symbol for the circle
            SimpleFillSymbol circleSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, System.Drawing.Color.Transparent, new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Red, 2));

            // Create a graphic for the circle
            Graphic circleGraphic = new Graphic(bufferGeometry, circleSymbol);

            // Add the circle graphic to the graphics overlay
            CreateGraphics(circleGraphic);
        }

        private PolylineBuilder GetPolylineBuilder()
        {
            return polylineBuilder;
        }

        // Modified CreateGraphics to accept a Graphic parameter
        private void CreateGraphics(Graphic polylineGraphic)
        {
            // Check if the GraphicsOverlays collection is initialized, if not, initialize it.
            if (GraphicsOverlays == null)
            {
                GraphicsOverlays = new GraphicsOverlayCollection();
            }

            // Check if there is already a GraphicsOverlay to add the Graphic to, if not, create a new one.
            GraphicsOverlay TAGraphicsOverlay;
            if (GraphicsOverlays.Count >= 0)
            {
                TAGraphicsOverlay = new GraphicsOverlay();
                GraphicsOverlays.Add(TAGraphicsOverlay);
            }
            else
            {
                // Assuming you want to add the new graphic to the first overlay in the collection
                TAGraphicsOverlay = GraphicsOverlays.First();
            }

            // Add the polylineGraphic to the selected or new GraphicsOverlay
            if (polylineGraphic != null)
            {
                TAGraphicsOverlay.Graphics.Add(polylineGraphic);
                TAGraphicsOverlay.IsVisible = true;
                OnPropertyChanged();
            }

        }


        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private Map? _map;
        public Map? Map
        {
            get { return _map; }
            set
            {
                _map = value;
                OnPropertyChanged();
            }
        }

        private GraphicsOverlayCollection? _graphicsOverlays = new GraphicsOverlayCollection();
        public GraphicsOverlayCollection? GraphicsOverlays
        {
            get
            {
                return _graphicsOverlays;
            }
            set
            {
                _graphicsOverlays = value;
                OnPropertyChanged();
            }
        }

    }
}
