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
        private string _downloadLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "OfflineMap" );
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
            string webMapId = "8b39872525eb43b28a69fba290d16ac8";
            PortalItem mapItem = await PortalItem.CreateAsync(portal, webMapId);
            Map map = new Map(mapItem);
            this.Map = map;
        }

        private async Task GetOfflinePreplannedMap()
        {
            var portal = await ArcGISPortal.CreateAsync();
            var portalItem = await PortalItem.CreateAsync(portal, "8b39872525eb43b28a69fba290d16ac8");
            var map = new Map(portalItem);

            OfflineMapTask offlineMapTask = await OfflineMapTask.CreateAsync(map);
            IReadOnlyList<PreplannedMapArea> availableAreas = await offlineMapTask.GetPreplannedMapAreasAsync();

            if (availableAreas?.FirstOrDefault() is PreplannedMapArea area)
            {
                DownloadPreplannedOfflineMapParameters downloadParameters = await offlineMapTask.CreateDefaultDownloadPreplannedOfflineMapParametersAsync(area);
                DownloadPreplannedOfflineMapJob job = offlineMapTask.DownloadPreplannedOfflineMap(downloadParameters, _downloadLocation);
                // Start the job
             //   job.Start();

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
        //    var portalItem = await PortalItem.CreateAsync(portal, "8b39872525eb43b28a69fba290d16ac8"); // Replace with your web map ID
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
        private async Task AccessMap()
        {
            //var mobileMapPackage = await MobileMapPackage.OpenAsync(_downloadLocation);
            //await mobileMapPackage.LoadAsync();
            //this.Map = mobileMapPackage.Maps.First();
            // Assuming _downloadLocation is the path to your downloaded geodatabase
            // Assuming _downloadLocation is the path to your downloaded tile package
            var tileCache = new TileCache(_downloadLocation);
            var tiledLayer = new ArcGISTiledLayer(tileCache);

            // Create a new map and add the tiled layer to it
            var map = new Map();
            map.Basemap.BaseLayers.Add(tiledLayer);

            // Assign the map to your MapView
            this.Map = map;
        }

        private bool OfflineMapExists()
        {
            return Directory.Exists(_downloadLocation) ;
        }

        private void CreateGraphics()
        {
            var TAGraphicsOverlay = new GraphicsOverlay();
            GraphicsOverlayCollection overlays = new GraphicsOverlayCollection { TAGraphicsOverlay };
            this.GraphicsOverlays = overlays;

            var TAPoint = new MapPoint(34.7808, 32.0707, SpatialReferences.Wgs84);
            var pointSymbol = new SimpleMarkerSymbol
            {
                Style = SimpleMarkerSymbolStyle.Circle,
                Color = System.Drawing.Color.Orange,
                Size = 10.0,
                Outline = new SimpleLineSymbol
                {
                    Style = SimpleLineSymbolStyle.Solid,
                    Color = System.Drawing.Color.Blue,
                    Width = 2.0
                }
            };
            var pointGraphic = new Graphic(TAPoint, pointSymbol);
            TAGraphicsOverlay.Graphics.Add(pointGraphic);
        }
    }
}
