using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Controls;
using Avalonia.Controls.Platform.Surfaces;
using Avalonia.Direct2D1.Media;
using Avalonia.Direct2D1.Media.Imaging;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SharpDX.DirectWrite;
using GlyphRun = Avalonia.Media.GlyphRun;
using TextAlignment = Avalonia.Media.TextAlignment;
using SharpDX.Mathematics.Interop;
using System.Runtime.InteropServices;
using System.Drawing;

namespace Avalonia
{
    public static class Direct2DApplicationExtensions
    {
        public static T UseDirect2D1<T>(this T builder) where T : AppBuilderBase<T>, new()
        {
            builder.UseRenderingSubsystem(Direct2D1.Direct2D1Platform.Initialize, "Direct2D1");
            return builder;
        }
    }
}

namespace Avalonia.Direct2D1
{
    public class Direct2D1Platform : IPlatformRenderInterface
    {
        private static readonly Direct2D1Platform s_instance = new Direct2D1Platform();

        public static SharpDX.Direct3D11.Device Direct3D11Device { get; private set; }

        public static SharpDX.Direct2D1.Factory1 Direct2D1Factory { get; private set; }

        public static SharpDX.Direct2D1.Device Direct2D1Device { get; private set; }

        public static SharpDX.DirectWrite.Factory1 DirectWriteFactory { get; private set; }

        public static SharpDX.WIC.ImagingFactory ImagingFactory { get; private set; }

        public static SharpDX.DXGI.Device1 DxgiDevice { get; private set; }

        private static readonly object s_initLock = new object();
        private static bool s_initialized = false;

        internal static void InitializeDirect2D()
        {
            lock (s_initLock)
            {
                if (s_initialized)
                {
                    return;
                }
#if DEBUG
                try
                {
                    Direct2D1Factory = new SharpDX.Direct2D1.Factory1(
                        SharpDX.Direct2D1.FactoryType.MultiThreaded,
                            SharpDX.Direct2D1.DebugLevel.Error);
                }
                catch
                {
                    //
                }
#endif
                if (Direct2D1Factory == null)
                {
                    Direct2D1Factory = new SharpDX.Direct2D1.Factory1(
                        SharpDX.Direct2D1.FactoryType.MultiThreaded,
                        SharpDX.Direct2D1.DebugLevel.None);
                }

                using (var factory = new SharpDX.DirectWrite.Factory())
                {
                    DirectWriteFactory = factory.QueryInterface<SharpDX.DirectWrite.Factory1>();
                }

                ImagingFactory = new SharpDX.WIC.ImagingFactory();

                var featureLevels = new[]
                {
                    SharpDX.Direct3D.FeatureLevel.Level_11_1,
                    SharpDX.Direct3D.FeatureLevel.Level_11_0,
                    SharpDX.Direct3D.FeatureLevel.Level_10_1,
                    SharpDX.Direct3D.FeatureLevel.Level_10_0,
                    SharpDX.Direct3D.FeatureLevel.Level_9_3,
                    SharpDX.Direct3D.FeatureLevel.Level_9_2,
                    SharpDX.Direct3D.FeatureLevel.Level_9_1,
                };

                Direct3D11Device = new SharpDX.Direct3D11.Device(
                    SharpDX.Direct3D.DriverType.Hardware,
                    SharpDX.Direct3D11.DeviceCreationFlags.BgraSupport | SharpDX.Direct3D11.DeviceCreationFlags.VideoSupport,
                    featureLevels);

                DxgiDevice = Direct3D11Device.QueryInterface<SharpDX.DXGI.Device1>();

                Direct2D1Device = new SharpDX.Direct2D1.Device(Direct2D1Factory, DxgiDevice);

                s_initialized = true;
            }
        }

        public static void Initialize()
        {
            InitializeDirect2D();
            AvaloniaLocator.CurrentMutable
                .Bind<IPlatformRenderInterface>().ToConstant(s_instance)
                .Bind<IFontManagerImpl>().ToConstant(new FontManagerImpl())
                .Bind<ITextShaperImpl>().ToConstant(new TextShaperImpl());
            SharpDX.Configuration.EnableReleaseOnFinalizer = true;
        }

        public IRenderTarget CreateRenderTarget(IEnumerable<object> surfaces)
        {
            foreach (var s in surfaces)
            {
                if (s is IPlatformHandle nativeWindow)
                {
                    if (nativeWindow.HandleDescriptor != "HWND")
                    {
                        throw new NotSupportedException("Don't know how to create a Direct2D1 renderer from " +
                                                        nativeWindow.HandleDescriptor);
                    }

                    return new HwndRenderTarget(nativeWindow);
                }
                if (s is IExternalDirect2DRenderTargetSurface external)
                {
                    return new ExternalRenderTarget(external);
                }

                if (s is IFramebufferPlatformSurface fb)
                {
                    return new FramebufferShimRenderTarget(fb);
                }
            }
            throw new NotSupportedException("Don't know how to create a Direct2D1 renderer from any of provided surfaces");
        }

        public IRenderTargetBitmapImpl CreateRenderTargetBitmap(PixelSize size, Vector dpi)
        {
            return new WicRenderTargetBitmapImpl(size, dpi);
        }

        public IWriteableBitmapImpl CreateWriteableBitmap(PixelSize size, Vector dpi, PixelFormat format, AlphaFormat alphaFormat)
        {
            return new WriteableWicBitmapImpl(size, dpi, format, alphaFormat);
        }

        public IGeometryImpl CreateEllipseGeometry(Rect rect) => new EllipseGeometryImpl(rect);
        public IGeometryImpl CreateLineGeometry(Point p1, Point p2) => new LineGeometryImpl(p1, p2);
        public IGeometryImpl CreateRectangleGeometry(Rect rect) => new RectangleGeometryImpl(rect);
        public IStreamGeometryImpl CreateStreamGeometry() => new StreamGeometryImpl();
        public IGeometryImpl CreateGeometryGroup(FillRule fillRule, IReadOnlyList<Geometry> children) => new GeometryGroupImpl(fillRule, children);
        public IGeometryImpl CreateCombinedGeometry(GeometryCombineMode combineMode, Geometry g1, Geometry g2) => new CombinedGeometryImpl(combineMode, g1, g2);

        public IGeometryImpl BuildGlyphRunGeometry(GlyphRun glyphRun)
        {
            if (glyphRun.GlyphTypeface is not GlyphTypefaceImpl glyphTypeface)
            {
                throw new InvalidOperationException("PlatformImpl can't be null.");
            }

            var pathGeometry = new SharpDX.Direct2D1.PathGeometry(Direct2D1Factory);

            using (var sink = pathGeometry.Open())
            {
                var glyphs = new short[glyphRun.GlyphIndices.Count];

                for (int i = 0; i < glyphRun.GlyphIndices.Count; i++)
                {
                    glyphs[i] = (short)glyphRun.GlyphIndices[i];
                }

                glyphTypeface.FontFace.GetGlyphRunOutline((float)glyphRun.FontRenderingEmSize, glyphs, null, null, false, !glyphRun.IsLeftToRight, sink);

                sink.Close();
            }

            var (baselineOriginX, baselineOriginY) = glyphRun.BaselineOrigin;

            var transformedGeometry = new SharpDX.Direct2D1.TransformedGeometry(
                Direct2D1Factory,
                pathGeometry,
                new RawMatrix3x2(1.0f, 0.0f, 0.0f, 1.0f, (float)baselineOriginX, (float)baselineOriginY));

            return new TransformedGeometryWrapper(transformedGeometry);
        }

        private class TransformedGeometryWrapper : GeometryImpl
        {
            public TransformedGeometryWrapper(SharpDX.Direct2D1.TransformedGeometry geometry) : base(geometry)
            {

            }
        }

        /// <inheritdoc />
        public IBitmapImpl LoadBitmap(string fileName)
        {
            return new WicBitmapImpl(fileName);
        }

        /// <inheritdoc />
        public IBitmapImpl LoadBitmap(Stream stream)
        {
            return new WicBitmapImpl(stream);
        }

        public IWriteableBitmapImpl LoadWriteableBitmapToWidth(Stream stream, int width,
            BitmapInterpolationMode interpolationMode = BitmapInterpolationMode.HighQuality)
        {
            return new WriteableWicBitmapImpl(stream, width, true, interpolationMode);
        }

        public IWriteableBitmapImpl LoadWriteableBitmapToHeight(Stream stream, int height,
            BitmapInterpolationMode interpolationMode = BitmapInterpolationMode.HighQuality)
        {
            return new WriteableWicBitmapImpl(stream, height, false, interpolationMode);
        }

        public IWriteableBitmapImpl LoadWriteableBitmap(string fileName)
        {
            return new WriteableWicBitmapImpl(fileName);
        }

        public IWriteableBitmapImpl LoadWriteableBitmap(Stream stream)
        {
            return new WriteableWicBitmapImpl(stream);
        }

        /// <inheritdoc />
        public IBitmapImpl LoadBitmapToWidth(Stream stream, int width, BitmapInterpolationMode interpolationMode = BitmapInterpolationMode.HighQuality)
        {
            return new WicBitmapImpl(stream, width, true, interpolationMode);
        }

        /// <inheritdoc />
        public IBitmapImpl LoadBitmapToHeight(Stream stream, int height, BitmapInterpolationMode interpolationMode = BitmapInterpolationMode.HighQuality)
        {
            return new WicBitmapImpl(stream, height, false, interpolationMode);
        }

        /// <inheritdoc />
        public IBitmapImpl ResizeBitmap(IBitmapImpl bitmapImpl, PixelSize destinationSize, BitmapInterpolationMode interpolationMode = BitmapInterpolationMode.HighQuality)
        {
            // https://github.com/sharpdx/SharpDX/issues/959 blocks implementation.
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IBitmapImpl LoadBitmap(PixelFormat format, AlphaFormat alphaFormat, IntPtr data, PixelSize size, Vector dpi, int stride)
        {
            return new WicBitmapImpl(format, alphaFormat, data, size, dpi, stride);
        }

        private class DWGlyphRunBuffer : IGlyphRunBuffer
        {
            protected readonly SharpDX.DirectWrite.GlyphRun _dwRun;

            public DWGlyphRunBuffer(IGlyphTypeface glyphTypeface, float fontRenderingEmSize, int length)
            {
                var glyphTypefaceImpl = (GlyphTypefaceImpl)glyphTypeface;

                _dwRun = new SharpDX.DirectWrite.GlyphRun
                {
                    FontFace = glyphTypefaceImpl.FontFace,
                    FontSize = fontRenderingEmSize,
                    Indices = new short[length]
                };
            }

            public Span<ushort> GlyphIndices => MemoryMarshal.Cast<short, ushort>(_dwRun.Indices.AsSpan());

            public IGlyphRunImpl Build()
            {
                return new GlyphRunImpl(_dwRun);
            }
        }

        private class DWHorizontalGlyphRunBuffer : DWGlyphRunBuffer, IHorizontalGlyphRunBuffer
        {
            public DWHorizontalGlyphRunBuffer(IGlyphTypeface glyphTypeface, float fontRenderingEmSize, int length) 
                : base(glyphTypeface, fontRenderingEmSize, length)
            {
                _dwRun.Advances = new float[length];
            }

            public Span<float> GlyphPositions => _dwRun.Advances.AsSpan();
        }

        private class DWPositionedGlyphRunBuffer : DWGlyphRunBuffer, IPositionedGlyphRunBuffer
        {
            public DWPositionedGlyphRunBuffer(IGlyphTypeface glyphTypeface, float fontRenderingEmSize, int length)
                : base(glyphTypeface, fontRenderingEmSize, length)
            {
                _dwRun.Advances = new float[length];
                _dwRun.Offsets = new GlyphOffset[length];
            }

            public Span<PointF> GlyphPositions => MemoryMarshal.Cast<GlyphOffset, PointF>(_dwRun.Offsets.AsSpan());
        }

        public IGlyphRunBuffer AllocateGlyphRun(IGlyphTypeface glyphTypeface, float fontRenderingEmSize, int length)
        {
            return new DWGlyphRunBuffer(glyphTypeface, fontRenderingEmSize, length);
        }

        public IHorizontalGlyphRunBuffer AllocateHorizontalGlyphRun(IGlyphTypeface glyphTypeface, float fontRenderingEmSize, int length)
        {
            return new DWHorizontalGlyphRunBuffer(glyphTypeface, fontRenderingEmSize, length);
        }

        public IPositionedGlyphRunBuffer AllocatePositionedGlyphRun(IGlyphTypeface glyphTypeface, float fontRenderingEmSize, int length)
        {
            return new DWPositionedGlyphRunBuffer(glyphTypeface, fontRenderingEmSize, length);
        }

        public bool SupportsIndividualRoundRects => false;

        public AlphaFormat DefaultAlphaFormat => AlphaFormat.Premul;

        public PixelFormat DefaultPixelFormat => PixelFormat.Bgra8888;
    }
}
