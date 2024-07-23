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

namespace DisplayAMap
{
    internal class MapViewModel : INotifyPropertyChanged
    {
        //private string _downloadLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "OfflineMapMED_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"));
        private string _downloadLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "OfflineMap");
        public MapViewModel()
        {
            InitializeMap();
        }

        private async void InitializeMap()
        {
            if (true)//OfflineMapExists())
            {
                await SetupMap();
                //                _downloadLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "OfflineMap");
                await AccessMap();
                InitializeMeasuringTool();
            }
            else
            {
                await SetupMap();
                await GetOfflinePreplannedMap();
            }
            CreateGraphics();
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

        private GraphicsOverlayCollection? _graphicsOverlays;
        public GraphicsOverlayCollection? GraphicsOverlays
        {
            get { return _graphicsOverlays; }
            set
            {
                _graphicsOverlays = value;
                OnPropertyChanged();
            }
        }

        private async Task SetupMap()
        {
            ArcGISPortal portal = await ArcGISPortal.CreateAsync();
            string webMapId = "3bc3a553c2a640c5bace2f3065751dc8";
            PortalItem mapItem = await PortalItem.CreateAsync(portal, webMapId);
            Map map = new Map(mapItem);
            this.Map = map;
        }

        private async Task GetOfflinePreplannedMap()
        {
            var portal = await ArcGISPortal.CreateAsync();
            var portalItem = await PortalItem.CreateAsync(portal, "3bc3a553c2a640c5bace2f3065751dc8");
            var map = new Map(portalItem);

            OfflineMapTask offlineMapTask = await OfflineMapTask.CreateAsync(map);
            IReadOnlyList<PreplannedMapArea> availableAreas = await offlineMapTask.GetPreplannedMapAreasAsync();

            //foreach (var areaoffline in availableAreas )
            //{
            //    // Define the download location for each area
            //    string areaDownloadLocation = Path.Combine(_downloadLocation, areaoffline.PortalItem.Title);

            //    // Ensure the directory exists
            //    Directory.CreateDirectory(areaDownloadLocation);

            //    // Create download parameters
            //    DownloadPreplannedOfflineMapParameters downloadParameters = await offlineMapTask.CreateDefaultDownloadPreplannedOfflineMapParametersAsync(areaoffline);

            //    // Create the job
            //    DownloadPreplannedOfflineMapJob job = offlineMapTask.DownloadPreplannedOfflineMap(downloadParameters, areaDownloadLocation);

            //    // Start the job and await its completion
            //    DownloadPreplannedOfflineMapResult result = await job.GetResultAsync();

            //    if (result.HasErrors)
            //    {
            //        // Handle errors (e.g., log or display a message)
            //        Debug.WriteLine($"Error downloading map for area {areaoffline.PortalItem.Title}");
            //    }
            //    else
            //    {
            //        // Success - the map is downloaded
            //        Debug.WriteLine($"Successfully downloaded map for area {areaoffline.PortalItem.Title}");
            //    }
            //}
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
            var mobileMapPackage = await MobileMapPackage.OpenAsync(_downloadLocation);

            await mobileMapPackage.LoadAsync();

            this.Map = mobileMapPackage.Maps.First();

        }
        private bool OfflineMapExists()
        {
            return Directory.Exists(_downloadLocation);
        }
        // Field to store the polyline builder
        private PolylineBuilder polylineBuilder;

        // Method to initialize the measuring tool
        private void InitializeMeasuringTool()
        {
            // Set the initial polyline builder with a spatial reference
            polylineBuilder = new PolylineBuilder(SpatialReferences.WebMercator);

            //this.GeoViewTapped += MapView_GeoViewTapped;


        }

        // Event handler for map view taps
        public async void MapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // Add the tapped point to the polyline builder
            polylineBuilder.AddPoint(e.Location);

            // Check if the polyline builder has 2 points
            if (polylineBuilder.Parts.Count > 0 && polylineBuilder.Parts[0].PointCount == 2)
            {
                // Create a polyline from the builder
                Polyline polyline = polylineBuilder.ToGeometry();

                // Measure the distance of the polyline
                double distance = GeometryEngine.LengthGeodetic(polyline, LinearUnits.Meters, GeodeticCurveType.Geodesic);

                // Display the distance (you might want to display this in the UI instead)
                Debug.WriteLine($"Distance: {distance / 1000} km");

                // Optionally, display the polyline on the map by adding it to a GraphicsOverlay
                var polylineGraphic = new Graphic(polyline);
                // Assuming you have a GraphicsOverlay named 'graphicsOverlay'
                CreateGraphics(polylineGraphic);

                // Reset the polyline builder for the next line
                polylineBuilder = new PolylineBuilder(SpatialReferences.WebMercator);
            }
        }

        private PolylineBuilder GetPolylineBuilder()
        {
            return polylineBuilder;
        }

        // Modified CreateGraphics to accept a Graphic parameter
        private void CreateGraphics(Graphic polylineGraphic = null)
        {
            var TAGraphicsOverlay = new GraphicsOverlay();
            if (GraphicsOverlays == null || GraphicsOverlays.Count == 0)
            {
                GraphicsOverlayCollection overlays = new GraphicsOverlayCollection { TAGraphicsOverlay };
                this.GraphicsOverlays = overlays;
            }
            else
            {
                TAGraphicsOverlay = GraphicsOverlays.FirstOrDefault();
            }

            if (polylineGraphic != null)
            {
                TAGraphicsOverlay.Graphics.Add(polylineGraphic);
            }
        }
    }
}
