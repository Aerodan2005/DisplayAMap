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

namespace DisplayAMap
{
    internal class MapViewModel : INotifyPropertyChanged
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
            InitializeMap();
        }

        private async void InitializeMap()
        {
            if (true)//OfflineMapExists())
            {
//                await SetupMap();
                //                _downloadLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "OfflineMap");
                await AccessMap();
                //InitializeMeasuringTool();
                //show1000 = false;
                //show500 = false;
            }
            else
            {
               // await SetupMap();
                await GetOfflinePreplannedMap();
            }
     //       CreateGraphics();
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
            get { 
                return _graphicsOverlays; 
            }
            set
            {
                _graphicsOverlays = value;
                OnPropertyChanged();
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
        //private async Task GetOfflinePreplannedMap()
        //{
        //    var portal = await ArcGISPortal.CreateAsync();
        //    var portalItem = await PortalItem.CreateAsync(portal, "42ed05cb907d4be69e74be788d1ec9df"); // Replace with your web map ID
        //    var map = new Map(portalItem);

        //    OfflineMapTask offlineMapTask = await OfflineMapTask.CreateAsync(map);
        //    IReadOnlyList<PreplannedMapArea> availableAreas = await offlineMapTask.GetPreplannedMapAreasAsync();

        //    if (availableAreas?.FirstOrDefault() is PreplannedMapArea area)
        //    {
        //        DownloadPreplannedOfflineMapParameters downloadParameters = await offlineMapTask.CreateDefaultDownloadPreplannedOfflineMapParametersAsync(area);

        //        string documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        //        _downloadLocation = System.IO.Path.Combine(documentsFolder, "OfflineMap");

        //        DownloadPreplannedOfflineMapJob job = offlineMapTask.DownloadPreplannedOfflineMap(downloadParameters, _downloadLocation);
        //        DownloadPreplannedOfflineMapResult result = await job.GetResultAsync();

        //        if (result?.OfflineMap is Map offlineMap)
        //        {
        //            this.Map = offlineMap;
        //        }
        //    }
        //}
        //private async Task AccessMap()
        //{
        //    // Assuming _downloadLocation points to the directory containing the .tpk file
        //    // and "YourTilePackage.tpk" is the name of your tile package file.
        //    string tpkFilePath = Path.Combine(_downloadLocation, "p13\\37xpi4a64lqgcycwijyijeaizf.tpk");

        //    // Create a new tile cache from the .tpk file
        //    TileCache tileCache = new TileCache(tpkFilePath);

        //    // Create a tiled layer from the tile cache
        //    ArcGISTiledLayer tiledLayer = new ArcGISTiledLayer(tileCache);

        //    // Wait for the tiled layer to load
        //    await tiledLayer.LoadAsync();

        //    // Check if the layer loaded successfully
        //    if (tiledLayer.LoadStatus == Esri.ArcGISRuntime.LoadStatus.Loaded)
        //    {
        //        // Create a new map and set the tiled layer as the basemap
        //        Map map = new Map();
        //        map.Basemap = new Basemap(tiledLayer);

        //        // Assign the map to the Map property to display it
        //        this.Map = map;
        //    }
        //    else
        //    {
        //        // Handle the case where the layer failed to load
        //        MessageBox.Show($"Failed to load the tile package: {tiledLayer.LoadError.Message}", "Error");
        //    }
        //}
        private async Task AccessMap()
        {
            //    string documentsFolder = Path.Combine(_downloadLocation, "MedMap3_MapArea_1");
            //string mapLocation = @"C:\Users\urika\OneDrive\מסמכים\ArcGIS\MapNew.mmpk";
            //string osmMapLocation = @"F:\asia-latest.osm\asia-latest.osm";
            //string osmMapLocation = @"F:\israel-and-palestine-latest.osm\israel-and-palestine-latest.osm";
            //string mapLocation = @"F:\med1qgis.png";

            //var mobileMapPackage = await MobileMapPackage.OpenAsync(mapLocation);

            //await mobileMapPackage.LoadAsync();

            //this.Map = mobileMapPackage.Maps.First();
            var myMap = new Map();
            string mbTilesPath = @"C:\Work\FromQGis\Mid1.mbtiles";
            var tileCache = new TileCache(mbTilesPath);
            var tiledLayer = new ArcGISTiledLayer(tileCache);
            await tiledLayer.LoadAsync();
            myMap.Basemap.BaseLayers.Add(tiledLayer);
            this.Map = myMap;

            try
            {
  //              await tiledLayer.LoadAsync();
                if (tiledLayer.LoadStatus == Esri.ArcGISRuntime.LoadStatus.Loaded)
                {
                    //await this.SetViewpointGeometryAsync(tiledLayer.FullExtent);
                    MessageBox.Show("Loaded the tiled layer.");
                }
                else
                {
                    MessageBox.Show("Failed to load the tiled layer.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading tiled layer: {ex.Message}");
            }

            //try
            //{
            //    // Create a new map
            //    var osmMap = new Map();

            //    // Create a feature table from your OSM data file
            //    var osmFeatureTable = new ShapefileFeatureTable(osmMapLocation);

            //    // Create a feature layer from the feature table
            //    var osmFeatureLayer;
            //    await osmFeatureLayer = new FeatureLayer(osmFeatureTable);
            //    // Add the layer to the map
            //    osmMap.OperationalLayers.Add(osmFeatureLayer);

            //    // Show the map in your MapView
            //    this.Map = osmMap;
            //}
            //catch (Exception ex)
            //{
            //    // Handle any exceptions (e.g., file not found, invalid data)
            //    Console.WriteLine($"Error loading OSM map: {ex.Message}");
            //}

        }
        private bool OfflineMapExists()
        {
            return Directory.Exists(_downloadLocation);
        }
        // Field to store the polyline builder
        public PolylineBuilder polylineBuilder;

        // Method to initialize the measuring tool
        private void InitializeMeasuringTool()
        {
            // Set the initial polyline builder with a spatial reference
            polylineBuilder = new PolylineBuilder(SpatialReferences.Wgs84);

            //this.GeoViewTapped += MapView_GeoViewTapped;


        }
        public  bool show1000 ;
        public  bool show500;
        // Event handler for map view taps
        public void MapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // Project e.Location to WGS84
            var wgs84Point = GeometryEngine.Project(e.Location, SpatialReferences.Wgs84) as MapPoint;
            var point = new MapPoint(wgs84Point.X, wgs84Point.Y, SpatialReferences.Wgs84);
            // Add the tapped point to the polyline builder
            polylineBuilder.AddPoint(point.X,point.Y);
            // AddPointToMap(32, 32, 1000, "TA");
           if(false)
            {
                DrawCircleOnMap( point.Y,point.X, 1000.0);
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
                
                var compositeSymbol1 = new CompositeSymbol(new Symbol[] { borderSymbol, coreSymbol , circleSymbol1 });
                var compositeSymbol2 = new CompositeSymbol(new Symbol[] { borderSymbol, coreSymbol , circleSymbol2 });

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
                AddPointToMap( point.Y, point.X, $"{distance :F1} km");

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

    
    }
}
