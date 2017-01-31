﻿using Esri.ArcGISRuntime.Layers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BikeTouringGISLibrary.Model;
using BikeTouringGISLibrary;
using BikeTouringGISLibrary.Enumerations;
using System.Collections.Specialized;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Geometry;
using MoreLinq;
namespace BikeTouringGIS.Controls
{
    public class BikeTouringGISLayer : GraphicsLayer
    {
        private IEnumerable<IRoute> _routes;
        private BikeTouringGISLayer _splitLayer;
        private Dictionary<GraphicType, object> _symbols;
        private bool _isInEditMode;

        private BikeTouringGISLayer()
        {
            Graphics.CollectionChanged += SetVisibility;
        }
        public BikeTouringGISLayer(string name) : this()
        {
            DisplayName = name;
            Type = LayerType.PointsOfInterest;
        }
        public BikeTouringGISLayer(string fileName, IEnumerable<IRoute> routes) : this(fileName)
        {
            _routes = routes;
            foreach (var route in routes)
            {
                Graphics.Add(route.StartLocation);
                Graphics.Add(route.EndLocation);
                Graphics.Add(route.RouteGeometry);
            }
            Type = LayerType.GPXRoutes;
        }
        public BikeTouringGISLayer SplitLayer
        {
            get { return _splitLayer; }
            set
            {
                if(value != _splitLayer)
                {
                    _splitLayer = value;
                    OnPropertyChanged("SplitLayer");
                }
            }
        }
        public bool IsInEditMode
        {
            get { return _isInEditMode; }
            set
            {
                if (value != _isInEditMode)
                {
                    _isInEditMode = value;
                    OnPropertyChanged("IsInEditMode");
                }
            }
        }

        private void SetVisibility(object sender, NotifyCollectionChangedEventArgs e)
        {
           ShowLegend = Graphics.Count > 0;
        }

        public LayerType Type { get;private set;}

        internal void FlipDirection()
        {
            Graphics.Clear();
            foreach (var route in _routes)
            {
                Graphics.Add(route.EndLocation);
                Graphics.Add(route.StartLocation);
                route.Flip();
                Graphics.Add(route.RouteGeometry);
            }
            SetSymbols();
            IsInEditMode = true;
        }

        public void SetSymbols(Dictionary<GraphicType, object> symbols)
        {
            _symbols = symbols;
            SetSymbols();
            SplitLayer = new BikeTouringGISLayer() { Type = LayerType.SplitRoutes, IsVisible = false };
        }

        private void SetSymbols()
        {
            foreach (BikeTouringGISGraphic graphic in Graphics)
            {
                var symbol = _symbols[graphic.Type];
                if (symbol is Symbol)
                {
                    graphic.Symbol = (Symbol)_symbols[graphic.Type];
                }
            }
        }
        public Envelope Extent
        {
            get
            {
                Envelope initialExtent = null;
                foreach (var graphic in Graphics)
                {
                    var graphicExtent = graphic.Geometry.Extent;
                    initialExtent = initialExtent == null ? initialExtent = graphicExtent : initialExtent = initialExtent.Union(graphicExtent);
                }
                return initialExtent;
            }
        }
        public int TotalLength
        {
            get
            {
                var length = 0;
                foreach (BikeTouringGISGraphic graphic in Graphics)
                {
                    var distanceAnalyzer = new DistanceAnalyzer();
                    var distance = distanceAnalyzer.CalculateDistance(graphic.Geometry);
                    length += distance;
                }
                return length;
            }
        }

        public void SplitRoutes(int splitDistance)
        {
            var routeSplitter = new RouteSplitter(splitDistance);
            SplitLayer.DisplayName = $"{splitDistance} km";
            SplitLayer.Graphics.Clear();
            foreach (var route in _routes)
            {
                routeSplitter.SplitRoute(route.Points);
                var splitPoints = routeSplitter.GetSplitPoints();
                splitPoints.ForEach(x => x.Symbol = (Symbol)_symbols[GraphicType.SplitPoint]);
                var splitRoutes = routeSplitter.GetSplittedRoutes();
                splitRoutes.ForEach(x => x.Symbol = (Symbol)_symbols[GraphicType.SplitRoute]);
                SplitLayer.Graphics.AddRange(splitPoints);
                SplitLayer.Graphics.AddRange(splitRoutes);
            }
        }
    }
}