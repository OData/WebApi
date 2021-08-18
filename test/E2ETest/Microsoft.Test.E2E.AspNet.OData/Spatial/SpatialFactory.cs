//-----------------------------------------------------------------------------
// <copyright file="SpatialFactory.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Spatial;

namespace Microsoft.Test.E2E.AspNet.OData.Spatial
{
    /// <summary>
    /// Geography Factory
    /// </summary>
    public static class GeographyFactory
    {
        #region Point and MultiPoint

        /// <summary>
        /// Create a Geography Point
        /// </summary>
        /// <param name="coordinateSystem">The CoordinateSystem</param>
        /// <param name="latitude">The latitude value</param>
        /// <param name="longitude">The longitude value</param>
        /// <param name="z">The Z value</param>
        /// <param name="m">The M value</param>
        /// <returns>A Geography Point Factory</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704", Justification = "x, y, z and m are meaningful")]
        public static GeographyFactory<GeographyPoint> Point(CoordinateSystem coordinateSystem, double latitude, double longitude, double? z, double? m)
        {
            return new GeographyFactory<GeographyPoint>(coordinateSystem).Point(latitude, longitude, z, m);
        }

        /// <summary>
        /// Create a Geography Point
        /// </summary>
        /// <param name="latitude">The latitude value</param>
        /// <param name="longitude">The longitude value</param>
        /// <param name="z">The Z value</param>
        /// <param name="m">The M value</param>
        /// <returns>A Geography Point Factory</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704", Justification = "x, y, z and m are meaningful")]
        public static GeographyFactory<GeographyPoint> Point(double latitude, double longitude, double? z, double? m)
        {
            return Point(CoordinateSystem.DefaultGeography, latitude, longitude, z, m);
        }

        /// <summary>
        /// Create a Geography Point
        /// </summary>
        /// <param name="coordinateSystem">The CoordinateSystem</param>
        /// <param name="latitude">The latitude value</param>
        /// <param name="longitude">The longitude value</param>
        /// <returns>A Geography Point Factory</returns>
        public static GeographyFactory<GeographyPoint> Point(CoordinateSystem coordinateSystem, double latitude, double longitude)
        {
            return Point(coordinateSystem, latitude, longitude, null, null);
        }

        /// <summary>
        /// Create a Geography Point
        /// </summary>
        /// <param name="latitude">The latitude value</param>
        /// <param name="longitude">The longitude value</param>
        /// <returns>A Geography Point Factory</returns>
        public static GeographyFactory<GeographyPoint> Point(double latitude, double longitude)
        {
            return Point(CoordinateSystem.DefaultGeography, latitude, longitude, null, null);
        }

        /// <summary>
        /// Create a factory with an empty Geography Point
        /// </summary>
        /// <param name="coordinateSystem">The CoordinateSystem</param>
        /// <returns>A Geography Point Factory</returns>
        public static GeographyFactory<GeographyPoint> Point(CoordinateSystem coordinateSystem)
        {
            return new GeographyFactory<GeographyPoint>(coordinateSystem).Point();
        }

        /// <summary>
        /// Create a factory with an empty Geography Point
        /// </summary>
        /// <returns>A Geography Point Factory</returns>
        public static GeographyFactory<GeographyPoint> Point()
        {
            return Point(CoordinateSystem.DefaultGeography);
        }

        /// <summary>
        /// Create a Geography MultiPoint
        /// </summary>
        /// <param name="coordinateSystem">The CoordinateSystem</param>
        /// <returns>A Geography MultiPoint Factory</returns>
        public static GeographyFactory<GeographyMultiPoint> MultiPoint(CoordinateSystem coordinateSystem)
        {
            return new GeographyFactory<GeographyMultiPoint>(coordinateSystem).MultiPoint();
        }

        /// <summary>
        /// Create a Geography MultiPoint
        /// </summary>
        /// <returns>A Geography MultiPoint Factory</returns>
        public static GeographyFactory<GeographyMultiPoint> MultiPoint()
        {
            return MultiPoint(CoordinateSystem.DefaultGeography);
        }

        #endregion

        #region LineString and MultiLineString

        /// <summary>
        /// Create a Geography LineString with a starting position
        /// </summary>
        /// <param name="coordinateSystem">The CoordinateSystem</param>
        /// <param name="latitude">The latitude value</param>
        /// <param name="longitude">The longitude value</param>
        /// <param name="z">The Z value</param>
        /// <param name="m">The M value</param>
        /// <returns>A Geography LineString Factory</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704", Justification = "x, y, z and m are meaningful")]
        public static GeographyFactory<GeographyLineString> LineString(CoordinateSystem coordinateSystem, double latitude, double longitude, double? z, double? m)
        {
            return new GeographyFactory<GeographyLineString>(coordinateSystem).LineString(latitude, longitude, z, m);
        }

        /// <summary>
        /// Create a Geography LineString with a starting position
        /// </summary>
        /// <param name="latitude">The latitude value</param>
        /// <param name="longitude">The longitude value</param>
        /// <param name="z">The Z value</param>
        /// <param name="m">The M value</param>
        /// <returns>A Geography LineString Factory</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704", Justification = "x, y, z and m are meaningful")]
        public static GeographyFactory<GeographyLineString> LineString(double latitude, double longitude, double? z, double? m)
        {
            return LineString(CoordinateSystem.DefaultGeography, latitude, longitude, z, m);
        }

        /// <summary>
        /// Create a Geography LineString with a starting position
        /// </summary>
        /// <param name="coordinateSystem">The CoordinateSystem</param>
        /// <param name="latitude">The latitude value</param>
        /// <param name="longitude">The longitude value</param>
        /// <returns>A Geography LineString Factory</returns>
        public static GeographyFactory<GeographyLineString> LineString(CoordinateSystem coordinateSystem, double latitude, double longitude)
        {
            return LineString(coordinateSystem, latitude, longitude, null, null);
        }

        /// <summary>
        /// Create a Geography LineString with a starting position
        /// </summary>
        /// <param name="latitude">The latitude value</param>
        /// <param name="longitude">The longitude value</param>
        /// <returns>A Geography LineString Factory</returns>
        public static GeographyFactory<GeographyLineString> LineString(double latitude, double longitude)
        {
            return LineString(CoordinateSystem.DefaultGeography, latitude, longitude, null, null);
        }

        /// <summary>
        /// Create an empty Geography LineString
        /// </summary>
        /// <param name="coordinateSystem">The CoordinateSystem</param>
        /// <returns>A Geography LineString Factory</returns>
        public static GeographyFactory<GeographyLineString> LineString(CoordinateSystem coordinateSystem)
        {
            return new GeographyFactory<GeographyLineString>(coordinateSystem).LineString();
        }

        /// <summary>
        /// Create an empty Geography LineString
        /// </summary>
        /// <returns>A Geography LineString Factory</returns>
        public static GeographyFactory<GeographyLineString> LineString()
        {
            return LineString(CoordinateSystem.DefaultGeography);
        }

        /// <summary>
        /// Create a Geography MultiLineString
        /// </summary>
        /// <param name="coordinateSystem">The CoordinateSystem</param>
        /// <returns>A Geography MultiLineString Factory</returns>
        public static GeographyFactory<GeographyMultiLineString> MultiLineString(CoordinateSystem coordinateSystem)
        {
            return new GeographyFactory<GeographyMultiLineString>(coordinateSystem).MultiLineString();
        }

        /// <summary>
        /// Create a Geography MultiLineString
        /// </summary>
        /// <returns>A Geography MultiLineString Factory</returns>
        public static GeographyFactory<GeographyMultiLineString> MultiLineString()
        {
            return MultiLineString(CoordinateSystem.DefaultGeography);
        }

        #endregion

        #region Polygon

        /// <summary>
        /// Create a Geography Polygon
        /// </summary>
        /// <param name="coordinateSystem">The CoordinateSystem</param>
        /// <returns>A Geography Polygon Factory</returns>
        public static GeographyFactory<GeographyPolygon> Polygon(CoordinateSystem coordinateSystem)
        {
            return new GeographyFactory<GeographyPolygon>(coordinateSystem).Polygon();
        }

        /// <summary>
        /// Create a Geography Polygon
        /// </summary>
        /// <returns>A Geography Polygon Factory</returns>
        public static GeographyFactory<GeographyPolygon> Polygon()
        {
            return Polygon(CoordinateSystem.DefaultGeography);
        }

        /// <summary>
        /// Create a Geography MultiPolygon
        /// </summary>
        /// <param name="coordinateSystem">The CoordinateSystem</param>
        /// <returns>A Geography MultiPolygon Factory</returns>
        public static GeographyFactory<GeographyMultiPolygon> MultiPolygon(CoordinateSystem coordinateSystem)
        {
            return new GeographyFactory<GeographyMultiPolygon>(coordinateSystem).MultiPolygon();
        }

        /// <summary>
        /// Create a Geography MultiPolygon
        /// </summary>
        /// <returns>A Geography MultiPolygon Factory</returns>
        public static GeographyFactory<GeographyMultiPolygon> MultiPolygon()
        {
            return MultiPolygon(CoordinateSystem.DefaultGeography);
        }

        #endregion

        #region Collection

        /// <summary>
        /// Create a Geography Collection
        /// </summary>
        /// <param name="coordinateSystem">The CoordinateSystem</param>
        /// <returns>A Geography Collection Factory</returns>
        public static GeographyFactory<GeographyCollection> Collection(CoordinateSystem coordinateSystem)
        {
            return new GeographyFactory<GeographyCollection>(coordinateSystem).Collection();
        }

        /// <summary>
        /// Create a Geography Collection
        /// </summary>
        /// <returns>A Geography Collection Factory</returns>
        public static GeographyFactory<GeographyCollection> Collection()
        {
            return Collection(CoordinateSystem.DefaultGeography);
        }

        #endregion
    }

    /// <summary>
    /// Geometry Factory
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Required for this file.")]
    public static class GeometryFactory
    {
        #region Point and MultiPoint

        /// <summary>
        /// Create a Geometry Point
        /// </summary>
        /// <param name="coordinateSystem">The CoordinateSystem</param>
        /// <param name="x">The X value</param>
        /// <param name="y">The Y value</param>
        /// <param name="z">The Z value</param>
        /// <param name="m">The M value</param>
        /// <returns>A Geometry Point Factory</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704", Justification = "x, y, z and m are meaningful")]
        public static GeometryFactory<GeometryPoint> Point(CoordinateSystem coordinateSystem, double x, double y, double? z, double? m)
        {
            return new GeometryFactory<GeometryPoint>(coordinateSystem).Point(x, y, z, m);
        }

        /// <summary>
        /// Create a Geometry Point
        /// </summary>
        /// <param name="x">The X value</param>
        /// <param name="y">The Y value</param>
        /// <param name="z">The Z value</param>
        /// <param name="m">The M value</param>
        /// <returns>A Geometry Point Factory</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704", Justification = "x, y, z and m are meaningful")]
        public static GeometryFactory<GeometryPoint> Point(double x, double y, double? z, double? m)
        {
            return Point(CoordinateSystem.DefaultGeometry, x, y, z, m);
        }

        /// <summary>
        /// Create a Geometry Point
        /// </summary>
        /// <param name="coordinateSystem">The CoordinateSystem</param>
        /// <param name="x">The X value</param>
        /// <param name="y">The Y value</param>
        /// <returns>A Geometry Point Factory</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704", Justification = "x, y, z and m are meaningful")]
        public static GeometryFactory<GeometryPoint> Point(CoordinateSystem coordinateSystem, double x, double y)
        {
            return Point(coordinateSystem, x, y, null, null);
        }

        /// <summary>
        /// Create a Geometry Point
        /// </summary>
        /// <param name="x">The X value</param>
        /// <param name="y">The Y value</param>
        /// <returns>A Geometry Point Factory</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704", Justification = "x, y, z and m are meaningful")]
        public static GeometryFactory<GeometryPoint> Point(double x, double y)
        {
            return Point(CoordinateSystem.DefaultGeometry, x, y, null, null);
        }

        /// <summary>
        /// Create a factory with an empty Geometry Point
        /// </summary>
        /// <param name="coordinateSystem">The CoordinateSystem</param>
        /// <returns>A Geometry Point Factory</returns>
        public static GeometryFactory<GeometryPoint> Point(CoordinateSystem coordinateSystem)
        {
            return new GeometryFactory<GeometryPoint>(coordinateSystem).Point();
        }

        /// <summary>
        /// Create a factory with an empty Geometry Point
        /// </summary>
        /// <returns>A Geometry Point Factory</returns>
        public static GeometryFactory<GeometryPoint> Point()
        {
            return Point(CoordinateSystem.DefaultGeometry);
        }

        /// <summary>
        /// Create a Geometry MultiPoint
        /// </summary>
        /// <param name="coordinateSystem">The CoordinateSystem</param>
        /// <returns>A Geometry MultiPoint Factory</returns>
        public static GeometryFactory<GeometryMultiPoint> MultiPoint(CoordinateSystem coordinateSystem)
        {
            return new GeometryFactory<GeometryMultiPoint>(coordinateSystem).MultiPoint();
        }

        /// <summary>
        /// Create a Geometry MultiPoint
        /// </summary>
        /// <returns>A Geometry MultiPoint Factory</returns>
        public static GeometryFactory<GeometryMultiPoint> MultiPoint()
        {
            return MultiPoint(CoordinateSystem.DefaultGeometry);
        }

        #endregion

        #region LineString and MultiLineString

        /// <summary>
        /// Create a Geometry LineString with a starting position
        /// </summary>
        /// <param name="coordinateSystem">The CoordinateSystem</param>
        /// <param name="x">The X value</param>
        /// <param name="y">The Y value</param>
        /// <param name="z">The Z value</param>
        /// <param name="m">The M value</param>
        /// <returns>A Geometry LineString Factory</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704", Justification = "x, y, z and m are meaningful")]
        public static GeometryFactory<GeometryLineString> LineString(CoordinateSystem coordinateSystem, double x, double y, double? z, double? m)
        {
            return new GeometryFactory<GeometryLineString>(coordinateSystem).LineString(x, y, z, m);
        }

        /// <summary>
        /// Create a Geometry LineString with a starting position
        /// </summary>
        /// <param name="x">The X value</param>
        /// <param name="y">The Y value</param>
        /// <param name="z">The Z value</param>
        /// <param name="m">The M value</param>
        /// <returns>A Geometry LineString Factory</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704", Justification = "x, y, z and m are meaningful")]
        public static GeometryFactory<GeometryLineString> LineString(double x, double y, double? z, double? m)
        {
            return LineString(CoordinateSystem.DefaultGeometry, x, y, z, m);
        }

        /// <summary>
        /// Create a Geometry LineString with a starting position
        /// </summary>
        /// <param name="coordinateSystem">The coordinate system</param>
        /// <param name="x">The X value</param>
        /// <param name="y">The Y value</param>
        /// <returns>A Geometry LineString Factory</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704", Justification = "x, y, z and m are meaningful")]
        public static GeometryFactory<GeometryLineString> LineString(CoordinateSystem coordinateSystem, double x, double y)
        {
            return LineString(coordinateSystem, x, y, null, null);
        }

        /// <summary>
        /// Create a Geometry LineString with a starting position
        /// </summary>
        /// <param name="x">The X value</param>
        /// <param name="y">The Y value</param>
        /// <returns>A Geometry LineString Factory</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704", Justification = "x, y, z and m are meaningful")]
        public static GeometryFactory<GeometryLineString> LineString(double x, double y)
        {
            return LineString(CoordinateSystem.DefaultGeometry, x, y, null, null);
        }

        /// <summary>
        /// Create an empty Geometry LineString
        /// </summary>
        /// <param name="coordinateSystem">The CoordinateSystem</param>
        /// <returns>A Geometry LineString Factory</returns>
        public static GeometryFactory<GeometryLineString> LineString(CoordinateSystem coordinateSystem)
        {
            return new GeometryFactory<GeometryLineString>(coordinateSystem).LineString();
        }

        /// <summary>
        /// Create an empty Geometry LineString
        /// </summary>
        /// <returns>A Geometry LineString Factory</returns>
        public static GeometryFactory<GeometryLineString> LineString()
        {
            return LineString(CoordinateSystem.DefaultGeometry);
        }

        /// <summary>
        /// Create a Geometry MultiLineString
        /// </summary>
        /// <param name="coordinateSystem">The CoordinateSystem</param>
        /// <returns>A Geometry MultiLineString Factory</returns>
        public static GeometryFactory<GeometryMultiLineString> MultiLineString(CoordinateSystem coordinateSystem)
        {
            return new GeometryFactory<GeometryMultiLineString>(coordinateSystem).MultiLineString();
        }

        /// <summary>
        /// Create a Geometry MultiLineString
        /// </summary>
        /// <returns>A Geometry MultiLineString Factory</returns>
        public static GeometryFactory<GeometryMultiLineString> MultiLineString()
        {
            return MultiLineString(CoordinateSystem.DefaultGeometry);
        }

        #endregion

        #region Polygon

        /// <summary>
        /// Create a Geometry Polygon
        /// </summary>
        /// <param name="coordinateSystem">The CoordinateSystem</param>
        /// <returns>A Geometry Polygon Factory</returns>
        public static GeometryFactory<GeometryPolygon> Polygon(CoordinateSystem coordinateSystem)
        {
            return new GeometryFactory<GeometryPolygon>(coordinateSystem).Polygon();
        }

        /// <summary>
        /// Create a Geometry Polygon
        /// </summary>
        /// <returns>A Geometry Polygon Factory</returns>
        public static GeometryFactory<GeometryPolygon> Polygon()
        {
            return Polygon(CoordinateSystem.DefaultGeometry);
        }

        /// <summary>
        /// Create a Geometry MultiPolygon
        /// </summary>
        /// <param name="coordinateSystem">The CoordinateSystem</param>
        /// <returns>A Geometry MultiPolygon Factory</returns>
        public static GeometryFactory<GeometryMultiPolygon> MultiPolygon(CoordinateSystem coordinateSystem)
        {
            return new GeometryFactory<GeometryMultiPolygon>(coordinateSystem).MultiPolygon();
        }

        /// <summary>
        /// Create a Geometry MultiPolygon
        /// </summary>
        /// <returns>A Geometry MultiPolygon Factory</returns>
        public static GeometryFactory<GeometryMultiPolygon> MultiPolygon()
        {
            return MultiPolygon(CoordinateSystem.DefaultGeometry);
        }

        #endregion

        #region Collection

        /// <summary>
        /// Create a Geometry Collection
        /// </summary>
        /// <param name="coordinateSystem">The CoordinateSystem</param>
        /// <returns>A Geometry Collection Factory</returns>
        public static GeometryFactory<GeometryCollection> Collection(CoordinateSystem coordinateSystem)
        {
            return new GeometryFactory<GeometryCollection>(coordinateSystem).Collection();
        }

        /// <summary>
        /// Create a Geometry Collection
        /// </summary>
        /// <returns>A Geometry Collection Factory</returns>
        public static GeometryFactory<GeometryCollection> Collection()
        {
            return Collection(CoordinateSystem.DefaultGeometry);
        }

        #endregion
    }

    /// <summary>
    /// Base Spatial Factory
    /// </summary>
    public abstract class BaseSpatialFactory
    {
        /// <summary>
        /// Stack of Containers
        /// </summary>
        private Stack<SpatialType> containers;

        /// <summary>
        /// Whether a figure has been started
        /// </summary>
        private bool figureDrawn;

        /// <summary>
        /// Inside a Polygon Ring
        /// </summary>
        private bool inRing;

        /// <summary>
        /// Current polygon ring has been closed
        /// </summary>
        private bool ringClosed;

        /// <summary>
        /// X coordinate of the current ring's starting position
        /// </summary>
        private double ringStartX;

        /// <summary>
        /// Y coordinate of the current ring's starting position
        /// </summary>
        private double ringStartY;

        /// <summary>
        /// Z coordinate of the current ring's starting position
        /// </summary>
        private double? ringStartZ;

        /// <summary>
        /// M coordinate of the current ring's starting position
        /// </summary>
        private double? ringStartM;

        /// <summary>
        /// Initializes a new instance of the BaseSpatialFactory class
        /// </summary>
        internal BaseSpatialFactory()
        {
            this.containers = new Stack<SpatialType>();
        }

        /// <summary>
        /// Gets the current container Definition
        /// </summary>
        private SpatialType CurrentType
        {
            get
            {
                if (this.containers.Count == 0)
                {
                    return SpatialType.Unknown;
                }
                else
                {
                    return this.containers.Peek();
                }
            }
        }

        /// <summary>
        /// Begin Geo
        /// </summary>
        /// <param name="type">The spatial type</param>
        protected virtual void BeginGeo(SpatialType type)
        {
            // close on nesting types until we find a container suitable for the current type
            while (!this.CanContain(type))
            {
                this.EndGeo();
            }

            this.containers.Push(type);
        }

        /// <summary>
        /// Begin drawing a figure
        /// </summary>
        /// <param name="x">X or Latitude Coordinate</param>
        /// <param name="y">Y or Longitude Coordinate</param>
        /// <param name="z">Z Coordinate</param>
        /// <param name="m">M Coordinate</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704", Justification = "x, y, z and m are meaningful")]
        protected virtual void BeginFigure(double x, double y, double? z, double? m)
        {
            Debug.Assert(!this.figureDrawn, "Figure already started");
            this.figureDrawn = true;
        }

        /// <summary>
        /// Draw a point in the specified coordinate
        /// </summary>
        /// <param name="x">X or Latitude Coordinate</param>
        /// <param name="y">Y or Longitude Coordinate</param>
        /// <param name="z">Z Coordinate</param>
        /// <param name="m">M Coordinate</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704", Justification = "x, y, z and m are meaningful")]
        protected virtual void AddLine(double x, double y, double? z, double? m)
        {
            Debug.Assert(this.figureDrawn, "Figure not yet started");

            if (this.inRing)
            {
                this.ringClosed = x == this.ringStartX && y == this.ringStartY;
            }
        }

        /// <summary>
        /// Ends the figure set on the current node
        /// </summary>
        protected virtual void EndFigure()
        {
            Debug.Assert(this.figureDrawn, "Figure not yet started");
            if (this.inRing)
            {
                if (!this.ringClosed)
                {
                    this.AddLine(this.ringStartX, this.ringStartY, this.ringStartZ, this.ringStartM);
                }

                this.inRing = false;
                this.ringClosed = true;
            }

            this.figureDrawn = false;
        }

        /// <summary>
        /// Ends the current spatial object
        /// </summary>
        protected virtual void EndGeo()
        {
            if (this.figureDrawn)
            {
                this.EndFigure();
            }

            this.containers.Pop();
        }

        /// <summary>
        /// Finish the current instance
        /// </summary>
        protected virtual void Finish()
        {
            while (this.containers.Count > 0)
            {
                this.EndGeo();
            }
        }

        /// <summary>
        /// Add a new position to the current line figure
        /// </summary>
        /// <param name="x">The X value</param>
        /// <param name="y">The Y value</param>
        /// <param name="z">The Z value</param>
        /// <param name="m">The M value</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704", Justification = "x, y, z and m are meaningful")]
        protected virtual void AddPos(double x, double y, double? z, double? m)
        {
            if (!this.figureDrawn)
            {
                this.BeginFigure(x, y, z, m);
            }
            else
            {
                this.AddLine(x, y, z, m);
            }
        }

        /// <summary>
        /// Start a new polygon ring
        /// </summary>
        /// <param name="x">The X value</param>
        /// <param name="y">The Y value</param>
        /// <param name="z">The Z value</param>
        /// <param name="m">The M value</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704", Justification = "x, y, z and m are meaningful")]
        protected virtual void StartRing(double x, double y, double? z, double? m)
        {
            if (this.figureDrawn)
            {
                this.EndFigure();
            }

            this.BeginFigure(x, y, z, m);

            this.ringStartX = x;
            this.ringStartY = y;
            this.ringStartM = m;
            this.ringStartZ = z;
            this.inRing = true;
            this.ringClosed = false;
        }

        /// <summary>
        /// Can the current container contain the spatial type
        /// </summary>
        /// <param name="type">The spatial type to test</param>
        /// <returns>A boolean value indicating whether the current container can contain the spatial type</returns>
        private bool CanContain(SpatialType type)
        {
            switch (this.CurrentType)
            {
                case SpatialType.Unknown:
                case SpatialType.Collection:
                    // top level or collection
                    return true;
                case SpatialType.MultiPoint:
                    return type == SpatialType.Point;
                case SpatialType.MultiLineString:
                    return type == SpatialType.LineString;
                case SpatialType.MultiPolygon:
                    return type == SpatialType.Polygon;
                default:
                    return false;
            }
        }
    }

    /// <summary>
    /// Spatial Factory
    /// </summary>
    /// <typeparam name="T">The target type</typeparam>
    public class GeographyFactory<T> : BaseSpatialFactory where T : Geography
    {
        /// <summary>
        /// The provider of the built type
        /// </summary>
        private IGeographyProvider provider;

        /// <summary>
        /// The chain to build through
        /// </summary>
        private GeographyPipeline buildChain;

        /// <summary>
        /// Initializes a new instance of the GeographyFactory class
        /// </summary>
        /// <param name="coordinateSystem">The coordinate system</param>
        internal GeographyFactory(CoordinateSystem coordinateSystem)
        {
            var builder = SpatialBuilder.Create();
            this.provider = builder;
            this.buildChain = SpatialValidator.Create().ChainTo(builder).StartingLink;
            this.buildChain.SetCoordinateSystem(coordinateSystem);
        }

        /// <summary>
        /// Using implicit cast to trigger the Finalize call
        /// </summary>
        /// <param name="factory">The factory</param>
        /// <returns>The built instance of the target type</returns>
        public static implicit operator T(GeographyFactory<T> factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }

            return factory.Build();
        }

        #region Public Construction Calls

        /// <summary>
        /// Start a new Point
        /// </summary>
        /// <param name="latitude">The latitude value</param>
        /// <param name="longitude">The longitude value</param>
        /// <param name="z">The Z value</param>
        /// <param name="m">The M Value</param>
        /// <returns>The current instance of GeographyFactory</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704", Justification = "x, y, z and m are meaningful")]
        public GeographyFactory<T> Point(double latitude, double longitude, double? z, double? m)
        {
            this.BeginGeo(SpatialType.Point);
            this.LineTo(latitude, longitude, z, m);
            return this;
        }

        /// <summary>
        /// Start a new Point
        /// </summary>
        /// <param name="latitude">The latitude value</param>
        /// <param name="longitude">The longitude value</param>
        /// <returns>The current instance of GeographyFactory</returns>
        public GeographyFactory<T> Point(double latitude, double longitude)
        {
            return this.Point(latitude, longitude, null, null);
        }

        /// <summary>
        /// Start a new empty Point
        /// </summary>
        /// <returns>The current instance of GeographyFactory</returns>
        public GeographyFactory<T> Point()
        {
            this.BeginGeo(SpatialType.Point);
            return this;
        }

        /// <summary>
        /// Start a new LineString
        /// </summary>
        /// <param name="latitude">The latitude value</param>
        /// <param name="longitude">The longitude value</param>
        /// <param name="z">The Z value</param>
        /// <param name="m">The M Value</param>
        /// <returns>The current instance of GeographyFactory</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704", Justification = "x, y, z and m are meaningful")]
        public GeographyFactory<T> LineString(double latitude, double longitude, double? z, double? m)
        {
            this.BeginGeo(SpatialType.LineString);
            this.LineTo(latitude, longitude, z, m);
            return this;
        }

        /// <summary>
        /// Start a new LineString
        /// </summary>
        /// <param name="latitude">The latitude value</param>
        /// <param name="longitude">The longitude value</param>
        /// <returns>The current instance of GeographyFactory</returns>
        public GeographyFactory<T> LineString(double latitude, double longitude)
        {
            return this.LineString(latitude, longitude, null, null);
        }

        /// <summary>
        /// Start a new empty LineString
        /// </summary>
        /// <returns>The current instance of GeographyFactory</returns>
        public GeographyFactory<T> LineString()
        {
            this.BeginGeo(SpatialType.LineString);
            return this;
        }

        /// <summary>
        /// Start a new Polygon
        /// </summary>
        /// <returns>The current instance of GeographyFactory</returns>
        public GeographyFactory<T> Polygon()
        {
            this.BeginGeo(SpatialType.Polygon);
            return this;
        }

        /// <summary>
        /// Start a new MultiPoint
        /// </summary>
        /// <returns>The current instance of GeographyFactory</returns>
        public GeographyFactory<T> MultiPoint()
        {
            this.BeginGeo(SpatialType.MultiPoint);
            return this;
        }

        /// <summary>
        /// Start a new MultiLineString
        /// </summary>
        /// <returns>The current instance of GeographyFactory</returns>
        public GeographyFactory<T> MultiLineString()
        {
            this.BeginGeo(SpatialType.MultiLineString);
            return this;
        }

        /// <summary>
        /// Start a new MultiPolygon
        /// </summary>
        /// <returns>The current instance of GeographyFactory</returns>
        public GeographyFactory<T> MultiPolygon()
        {
            this.BeginGeo(SpatialType.MultiPolygon);
            return this;
        }

        /// <summary>
        /// Start a new Collection
        /// </summary>
        /// <returns>The current instance of GeographyFactory</returns>
        public GeographyFactory<T> Collection()
        {
            this.BeginGeo(SpatialType.Collection);
            return this;
        }

        /// <summary>
        /// Start a new Polygon Ring
        /// </summary>
        /// <param name="latitude">The latitude value</param>
        /// <param name="longitude">The longitude value</param>
        /// <param name="z">The Z value</param>
        /// <param name="m">The M Value</param>
        /// <returns>The current instance of GeographyFactory</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704", Justification = "x, y, z and m are meaningful")]
        public GeographyFactory<T> Ring(double latitude, double longitude, double? z, double? m)
        {
            this.StartRing(latitude, longitude, z, m);
            return this;
        }

        /// <summary>
        /// Start a new Polygon Ring
        /// </summary>
        /// <param name="latitude">The latitude value</param>
        /// <param name="longitude">The longitude value</param>
        /// <returns>The current instance of GeographyFactory</returns>
        public GeographyFactory<T> Ring(double latitude, double longitude)
        {
            return this.Ring(latitude, longitude, null, null);
        }

        /// <summary>
        /// Add a new point in the current line figure
        /// </summary>
        /// <param name="latitude">The latitude value</param>
        /// <param name="longitude">The longitude value</param>
        /// <param name="z">The Z value</param>
        /// <param name="m">The M Value</param>
        /// <returns>The current instance of GeographyFactory</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704", Justification = "x, y, z and m are meaningful")]
        public GeographyFactory<T> LineTo(double latitude, double longitude, double? z, double? m)
        {
            this.AddPos(latitude, longitude, z, m);
            return this;
        }

        /// <summary>
        /// Add a new point in the current line figure
        /// </summary>
        /// <param name="latitude">The latitude value</param>
        /// <param name="longitude">The longitude value</param>
        /// <returns>The current instance of GeographyFactory</returns>
        public GeographyFactory<T> LineTo(double latitude, double longitude)
        {
            return this.LineTo(latitude, longitude, null, null);
        }

        #endregion

        /// <summary>
        /// Finish the current geography
        /// </summary>
        /// <returns>The constructed instance</returns>
        public T Build()
        {
            this.Finish();
            return (T)this.provider.ConstructedGeography;
        }

        #region GeoDataPipeline overrides

        /// <summary>
        /// Begin a new geography
        /// </summary>
        /// <param name="type">The spatial type</param>
        protected override void BeginGeo(SpatialType type)
        {
            base.BeginGeo(type);
            this.buildChain.BeginGeography(type);
        }

        /// <summary>
        /// Begin drawing a figure
        /// TODO: longitude and latitude should be swapped !!! per ABNF.
        /// </summary>
        /// <param name="latitude">X or Latitude Coordinate</param>
        /// <param name="longitude">Y or Longitude Coordinate</param>
        /// <param name="z">Z Coordinate</param>
        /// <param name="m">M Coordinate</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704", Justification = "x, y, z and m are meaningful")]
        protected override void BeginFigure(double latitude, double longitude, double? z, double? m)
        {
            base.BeginFigure(latitude, longitude, z, m);
            this.buildChain.BeginFigure(new GeographyPosition(latitude, longitude, z, m));
        }

        /// <summary>
        /// Draw a point in the specified coordinate
        /// </summary>
        /// <param name="latitude">X or Latitude Coordinate</param>
        /// <param name="longitude">Y or Longitude Coordinate</param>
        /// <param name="z">Z Coordinate</param>
        /// <param name="m">M Coordinate</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704", Justification = "x, y, z and m are meaningful")]
        protected override void AddLine(double latitude, double longitude, double? z, double? m)
        {
            base.AddLine(latitude, longitude, z, m);
            this.buildChain.LineTo(new GeographyPosition(latitude, longitude, z, m));
        }

        /// <summary>
        /// Ends the figure set on the current node
        /// </summary>
        protected override void EndFigure()
        {
            base.EndFigure();
            this.buildChain.EndFigure();
        }

        /// <summary>
        /// Ends the current spatial object
        /// </summary>
        protected override void EndGeo()
        {
            base.EndGeo();
            this.buildChain.EndGeography();
        }

        #endregion
    }

    /// <summary>
    /// Spatial Factory
    /// </summary>
    /// <typeparam name="T">The target type</typeparam>
    public class GeometryFactory<T> : BaseSpatialFactory where T : Geometry
    {
        /// <summary>
        /// The provider of the built type
        /// </summary>
        private IGeometryProvider provider;

        /// <summary>
        /// The chain to build through
        /// </summary>
        private GeometryPipeline buildChain;

        /// <summary>
        /// Initializes a new instance of the GeometryFactory class
        /// </summary>
        /// <param name="coordinateSystem">The coordinate system</param>
        internal GeometryFactory(CoordinateSystem coordinateSystem)
        {
            var builder = SpatialBuilder.Create();
            this.provider = builder;
            this.buildChain = SpatialValidator.Create().ChainTo(builder).StartingLink;
            this.buildChain.SetCoordinateSystem(coordinateSystem);
        }

        /// <summary>
        /// Cast a factory to the target type
        /// </summary>
        /// <param name="factory">The factory</param>
        /// <returns>The built instance of the target type</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Justification = "Operator used to build")]
        public static implicit operator T(GeometryFactory<T> factory)
        {
            if (factory != null)
            {
                return factory.Build();
            }

            return null;
        }

        #region Public Construction Calls

        /// <summary>
        /// Start a new Point
        /// </summary>
        /// <param name="x">The X value</param>
        /// <param name="y">The Y value</param>
        /// <param name="z">The Z value</param>
        /// <param name="m">The M value</param>
        /// <returns>The current instance of GeometryFactory</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704", Justification = "x, y, z and m are meaningful")]
        public GeometryFactory<T> Point(double x, double y, double? z, double? m)
        {
            this.BeginGeo(SpatialType.Point);
            this.LineTo(x, y, z, m);
            return this;
        }

        /// <summary>
        /// Start a new Point
        /// </summary>
        /// <param name="x">The X value</param>
        /// <param name="y">The Y value</param>
        /// <returns>The current instance of GeometryFactory</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704", Justification = "x, y, z and m are meaningful")]
        public GeometryFactory<T> Point(double x, double y)
        {
            return this.Point(x, y, null, null);
        }

        /// <summary>
        /// Start a new empty Point
        /// </summary>  
        /// <returns>The current instance of GeometryFactory</returns>
        public GeometryFactory<T> Point()
        {
            this.BeginGeo(SpatialType.Point);
            return this;
        }

        /// <summary>
        /// Start a new LineString
        /// </summary>
        /// <param name="x">The X value</param>
        /// <param name="y">The Y value</param>
        /// <param name="z">The Z value</param>
        /// <param name="m">The M value</param>
        /// <returns>The current instance of GeometryFactory</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704", Justification = "x, y, z and m are meaningful")]
        public GeometryFactory<T> LineString(double x, double y, double? z, double? m)
        {
            this.BeginGeo(SpatialType.LineString);
            this.LineTo(x, y, z, m);
            return this;
        }

        /// <summary>
        /// Start a new LineString
        /// </summary>
        /// <param name="x">The X value</param>
        /// <param name="y">The Y value</param>
        /// <returns>The current instance of GeometryFactory</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704", Justification = "x, y, z and m are meaningful")]
        public GeometryFactory<T> LineString(double x, double y)
        {
            return this.LineString(x, y, null, null);
        }

        /// <summary>
        /// Start a new empty LineString
        /// </summary>
        /// <returns>The current instance of GeometryFactory</returns>
        public GeometryFactory<T> LineString()
        {
            this.BeginGeo(SpatialType.LineString);
            return this;
        }

        /// <summary>
        /// Start a new Polygon
        /// </summary>
        /// <returns>The current instance of GeometryFactory</returns>
        public GeometryFactory<T> Polygon()
        {
            this.BeginGeo(SpatialType.Polygon);
            return this;
        }

        /// <summary>
        /// Start a new MultiPoint
        /// </summary>
        /// <returns>The current instance of GeometryFactory</returns>
        public GeometryFactory<T> MultiPoint()
        {
            this.BeginGeo(SpatialType.MultiPoint);
            return this;
        }

        /// <summary>
        /// Start a new MultiLineString
        /// </summary>
        /// <returns>The current instance of GeometryFactory</returns>
        public GeometryFactory<T> MultiLineString()
        {
            this.BeginGeo(SpatialType.MultiLineString);
            return this;
        }

        /// <summary>
        /// Start a new MultiPolygon
        /// </summary>
        /// <returns>The current instance of GeometryFactory</returns>
        public GeometryFactory<T> MultiPolygon()
        {
            this.BeginGeo(SpatialType.MultiPolygon);
            return this;
        }

        /// <summary>
        /// Start a new Collection
        /// </summary>
        /// <returns>The current instance of GeometryFactory</returns>
        public GeometryFactory<T> Collection()
        {
            this.BeginGeo(SpatialType.Collection);
            return this;
        }

        /// <summary>
        /// Start a new Polygon Ring
        /// </summary>
        /// <param name="x">The X value</param>
        /// <param name="y">The Y value</param>
        /// <param name="z">The Z value</param>
        /// <param name="m">The M value</param>
        /// <returns>The current instance of GeometryFactory</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704", Justification = "x, y, z and m are meaningful")]
        public GeometryFactory<T> Ring(double x, double y, double? z, double? m)
        {
            this.StartRing(x, y, z, m);
            return this;
        }

        /// <summary>
        /// Start a new Polygon Ring
        /// </summary>
        /// <param name="x">The X value</param>
        /// <param name="y">The Y value</param>
        /// <returns>The current instance of GeometryFactory</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704", Justification = "x, y, z and m are meaningful")]
        public GeometryFactory<T> Ring(double x, double y)
        {
            return this.Ring(x, y, null, null);
        }

        /// <summary>
        /// Add a new point in the current line figure
        /// </summary>
        /// <param name="x">The X value</param>
        /// <param name="y">The Y value</param>
        /// <param name="z">The Z value</param>
        /// <param name="m">The M value</param>
        /// <returns>The current instance of GeometryFactory</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704", Justification = "x, y, z and m are meaningful")]
        public GeometryFactory<T> LineTo(double x, double y, double? z, double? m)
        {
            this.AddPos(x, y, z, m);
            return this;
        }

        /// <summary>
        /// Add a new point in the current line figure
        /// </summary>
        /// <param name="x">The X value</param>
        /// <param name="y">The Y value</param>
        /// <returns>The current instance of GeometryFactory</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704", Justification = "x, y, z and m are meaningful")]
        public GeometryFactory<T> LineTo(double x, double y)
        {
            return this.LineTo(x, y, null, null);
        }

        #endregion

        /// <summary>
        /// Finish the current Geometry
        /// </summary>
        /// <returns>The constructed instance</returns>
        public T Build()
        {
            this.Finish();
            return (T)this.provider.ConstructedGeometry;
        }

        #region GeoDataPipeline overrides

        /// <summary>
        /// Begin a new geometry
        /// </summary>
        /// <param name="type">The spatial type</param>
        protected override void BeginGeo(SpatialType type)
        {
            base.BeginGeo(type);
            this.buildChain.BeginGeometry(type);
        }

        /// <summary>
        /// Begin drawing a figure
        /// </summary>
        /// <param name="x">X or Latitude Coordinate</param>
        /// <param name="y">Y or Longitude Coordinate</param>
        /// <param name="z">Z Coordinate</param>
        /// <param name="m">M Coordinate</param>
        protected override void BeginFigure(double x, double y, double? z, double? m)
        {
            base.BeginFigure(x, y, z, m);
            this.buildChain.BeginFigure(new GeometryPosition(x, y, z, m));
        }

        /// <summary>
        /// Draw a point in the specified coordinate
        /// </summary>
        /// <param name="x">X or Latitude Coordinate</param>
        /// <param name="y">Y or Longitude Coordinate</param>
        /// <param name="z">Z Coordinate</param>
        /// <param name="m">M Coordinate</param>
        protected override void AddLine(double x, double y, double? z, double? m)
        {
            base.AddLine(x, y, z, m);
            this.buildChain.LineTo(new GeometryPosition(x, y, z, m));
        }

        /// <summary>
        /// Ends the figure set on the current node
        /// </summary>
        protected override void EndFigure()
        {
            base.EndFigure();
            this.buildChain.EndFigure();
        }

        /// <summary>
        /// Ends the current spatial object
        /// </summary>
        protected override void EndGeo()
        {
            base.EndGeo();
            this.buildChain.EndGeometry();
        }

        #endregion
    }
}
