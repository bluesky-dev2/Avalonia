using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Avalonia.Controls.Platform.Surfaces;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.OpenGL;
using Avalonia.Platform;
using Avalonia.Visuals.Media.Imaging;
using SkiaSharp;

namespace Avalonia.Skia
{
    /// <summary>
    /// Skia platform render interface.
    /// </summary>
    internal class PlatformRenderInterface : IPlatformRenderInterface
    {
        private readonly ICustomSkiaGpu _customSkiaGpu;

        private GRContext GrContext { get; }

        public PlatformRenderInterface(ICustomSkiaGpu customSkiaGpu, long maxResourceBytes = 100663296)
        {
            if (customSkiaGpu != null)
            {
                _customSkiaGpu = customSkiaGpu;

                GrContext = _customSkiaGpu.GrContext;

                GrContext.GetResourceCacheLimits(out var maxResources, out _);

                GrContext.SetResourceCacheLimits(maxResources, maxResourceBytes);

                return;
            }

            var gl = AvaloniaLocator.Current.GetService<IWindowingPlatformGlFeature>();
            if (gl != null)
            {
                var display = gl.ImmediateContext.Display;
                gl.ImmediateContext.MakeCurrent();
                using (var iface = display.Type == GlDisplayType.OpenGL2
                    ? GRGlInterface.AssembleGlInterface((_, proc) => display.GlInterface.GetProcAddress(proc))
                    : GRGlInterface.AssembleGlesInterface((_, proc) => display.GlInterface.GetProcAddress(proc)))
                {
                    GrContext = GRContext.Create(GRBackend.OpenGL, iface);

                    GrContext.GetResourceCacheLimits(out var maxResources, out _);

                    GrContext.SetResourceCacheLimits(maxResources, maxResourceBytes);
                }
                display.ClearContext();
            }
        }

        /// <inheritdoc />
        public IFormattedTextImpl CreateFormattedText(
            string text,
            Typeface typeface,
            double fontSize,
            TextAlignment textAlignment,
            TextWrapping wrapping,
            Size constraint,
            IReadOnlyList<FormattedTextStyleSpan> spans)
        {
            return new FormattedTextImpl(text, typeface, fontSize, textAlignment, wrapping, constraint, spans);
        }

        public IGeometryImpl CreateEllipseGeometry(Rect rect) => new EllipseGeometryImpl(rect);

        public IGeometryImpl CreateLineGeometry(Point p1, Point p2) => new LineGeometryImpl(p1, p2);

        public IGeometryImpl CreateRectangleGeometry(Rect rect) => new RectangleGeometryImpl(rect);

        /// <inheritdoc />
        public IStreamGeometryImpl CreateStreamGeometry()
        {
            return new StreamGeometryImpl();
        }

        /// <inheritdoc />
        public IBitmapImpl LoadBitmap(string fileName)
        {
            using (var stream = File.OpenRead(fileName))
            {
                return LoadBitmap(stream);
            }
        }

        /// <inheritdoc />
        public IBitmapImpl LoadBitmap(Stream stream)
        {
            return new ImmutableBitmap(stream);
        }

        /// <inheritdoc />
        public IBitmapImpl LoadBitmap(PixelFormat format, IntPtr data, PixelSize size, Vector dpi, int stride)
        {
            return new ImmutableBitmap(size, dpi, stride, format, data);
        }

        /// <inheritdoc />
        public IBitmapImpl LoadBitmapToWidth(Stream stream, int width, BitmapInterpolationMode interpolationMode = BitmapInterpolationMode.HighQuality)
        {
            return new ImmutableBitmap(stream, width, true, interpolationMode);
        }

        /// <inheritdoc />
        public IBitmapImpl LoadBitmapToHeight(Stream stream, int height, BitmapInterpolationMode interpolationMode = BitmapInterpolationMode.HighQuality)
        {
            return new ImmutableBitmap(stream, height, false, interpolationMode);
        }

        /// <inheritdoc />
        public IBitmapImpl ResizeBitmap(IBitmapImpl bitmapImpl, PixelSize destinationSize, BitmapInterpolationMode interpolationMode = BitmapInterpolationMode.HighQuality)
        {
            if (bitmapImpl is ImmutableBitmap ibmp)
            {
                return new ImmutableBitmap(ibmp, destinationSize, interpolationMode);
            }
            else
            {
                throw new Exception("Invalid source bitmap type.");
            }
        }

        /// <inheritdoc />
        public IRenderTargetBitmapImpl CreateRenderTargetBitmap(PixelSize size, Vector dpi)
        {
            if (size.Width < 1)
            {
                throw new ArgumentException("Width can't be less than 1", nameof(size));
            }

            if (size.Height < 1)
            {
                throw new ArgumentException("Height can't be less than 1", nameof(size));
            }

            var createInfo = new SurfaceRenderTarget.CreateInfo
            {
                Width = size.Width,
                Height = size.Height,
                Dpi = dpi,
                DisableTextLcdRendering = false
            };

            return new SurfaceRenderTarget(createInfo);
        }

        /// <inheritdoc />
        public IRenderTarget CreateRenderTarget(IEnumerable<object> surfaces)
        {
            if (_customSkiaGpu != null)
            {
                ICustomSkiaRenderTarget customRenderTarget = _customSkiaGpu.TryCreateRenderTarget(surfaces);

                if (customRenderTarget != null)
                {
                    return new CustomRenderTarget(customRenderTarget);
                }
            }

            foreach (var surface in surfaces)
            {
                if (surface is IGlPlatformSurface glSurface && GrContext != null)
                {
                    return new GlRenderTarget(GrContext, glSurface);
                }
                if (surface is IFramebufferPlatformSurface framebufferSurface)
                {
                    return new FramebufferRenderTarget(framebufferSurface);
                }
            }

            throw new NotSupportedException(
                "Don't know how to create a Skia render target from any of provided surfaces");
        }

        /// <inheritdoc />
        public IWriteableBitmapImpl CreateWriteableBitmap(PixelSize size, Vector dpi, PixelFormat? format = null)
        {
            return new WriteableBitmapImpl(size, dpi, format);
        }

        private static readonly SKPaint s_paint = new SKPaint
        {
            TextEncoding = SKTextEncoding.GlyphId,
            IsAntialias = true,
            IsStroke = false,
            SubpixelText = true
        };

        private static readonly SKTextBlobBuilder s_textBlobBuilder = new SKTextBlobBuilder();

        /// <inheritdoc />
        public IGlyphRunImpl CreateGlyphRun(GlyphRun glyphRun, out double width)
        {
            var count = glyphRun.GlyphIndices.Length;

            var glyphTypeface = (GlyphTypefaceImpl)glyphRun.GlyphTypeface.PlatformImpl;

            var typeface = glyphTypeface.Typeface;

            s_paint.TextSize = (float)glyphRun.FontRenderingEmSize;
            s_paint.Typeface = typeface;


            SKTextBlob textBlob;

            width = 0;

            var scale = (float)(glyphRun.FontRenderingEmSize / glyphTypeface.DesignEmHeight);

            if (glyphRun.GlyphOffsets.IsEmpty)
            {
                if (glyphTypeface.IsFixedPitch)
                {
                    s_textBlobBuilder.AddRun(s_paint, 0, 0, glyphRun.GlyphIndices.Buffer.Span);

                    textBlob = s_textBlobBuilder.Build();

                    width = glyphTypeface.GetGlyphAdvance(glyphRun.GlyphIndices[0]) * scale * glyphRun.GlyphIndices.Length;
                }
                else
                {
                    var buffer = s_textBlobBuilder.AllocateHorizontalRun(s_paint, count, 0);

                    var positions = buffer.GetPositionSpan();

                    for (var i = 0; i < count; i++)
                    {
                        positions[i] = (float)width;

                        if (glyphRun.GlyphAdvances.IsEmpty)
                        {
                            width += glyphTypeface.GetGlyphAdvance(glyphRun.GlyphIndices[i]) * scale;
                        }
                        else
                        {
                            width += glyphRun.GlyphAdvances[i];
                        }
                    }

                    buffer.SetGlyphs(glyphRun.GlyphIndices.Buffer.Span);

                    textBlob = s_textBlobBuilder.Build();
                }
            }
            else
            {
                var buffer = s_textBlobBuilder.AllocatePositionedRun(s_paint, count);

                var glyphPositions = buffer.GetPositionSpan();

                var currentX = 0.0;

                for (var i = 0; i < count; i++)
                {
                    var glyphOffset = glyphRun.GlyphOffsets[i];

                    glyphPositions[i] = new SKPoint((float)(currentX + glyphOffset.X), (float)glyphOffset.Y);

                    if (glyphRun.GlyphAdvances.IsEmpty)
                    {
                        currentX += glyphTypeface.GetGlyphAdvance(glyphRun.GlyphIndices[i]) * scale;
                    }
                    else
                    {
                        currentX += glyphRun.GlyphAdvances[i];
                    }
                }

                buffer.SetGlyphs(glyphRun.GlyphIndices.Buffer.Span);

                width = currentX;

                textBlob = s_textBlobBuilder.Build();
            }

            return new GlyphRunImpl(textBlob);

        }

        public bool SupportsIndividualRoundRects => true;
    }
}
