using System;
using System.Linq;
using Avalonia.OpenGL;
using static Avalonia.X11.Glx.GlxConsts;
using static Avalonia.X11.Glx.Glx;
namespace Avalonia.X11.Glx
{
    unsafe class GlxDisplay : IGlDisplay
    {
        private readonly X11Info _x11;
        private readonly IntPtr _fbconfig;
        private readonly XVisualInfo* _visual;
        public GlDisplayType Type => GlDisplayType.OpenGL2;
        public GlInterface GlInterface { get; }
        
        public XVisualInfo* VisualInfo => _visual;
        public int SampleCount { get; }
        public int StencilSize { get; }
        
        public GlxContext ImmediateContext { get; }
        public GlxContext DeferredContext { get; }
        
        public GlxDisplay(X11Info x11) 
        {
            _x11 = x11;

            var baseAttribs = new[]
            {
                GLX_X_RENDERABLE, 1,
                GLX_RENDER_TYPE, GLX_RGBA_BIT,
                GLX_DRAWABLE_TYPE, GLX_WINDOW_BIT,
                GLX_DOUBLEBUFFER, 1,
                GLX_RED_SIZE, 8,
                GLX_GREEN_SIZE, 8,
                GLX_BLUE_SIZE, 8,
                GLX_ALPHA_SIZE, 8,
                GLX_DEPTH_SIZE, 1,
                GLX_STENCIL_SIZE, 8,

            };

            foreach (var attribs in new[]
            {
                //baseAttribs.Concat(multiattribs),
                baseAttribs,
            })
            {
                var ptr = GlxChooseFBConfig(_x11.Display, x11.DefaultScreen,
                    attribs, out var count);
                for (var c = 0 ; c < count; c++)
                {
                    
                    var visual = GlxGetVisualFromFBConfig(_x11.Display, ptr[c]);
                    // We prefer 32 bit visuals
                    if (_fbconfig == IntPtr.Zero || visual->depth == 32)
                    {
                        _fbconfig = ptr[c];
                        _visual = visual;
                        if(visual->depth == 32)
                            break;
                    }
                }

                if (_fbconfig != IntPtr.Zero)
                    break;
            }

            if (_fbconfig == IntPtr.Zero)
                throw new OpenGlException("Unable to choose FBConfig");
            
            if (_visual == null)
                throw new OpenGlException("Unable to get visual info from FBConfig");
            if (GlxGetFBConfigAttrib(_x11.Display, _fbconfig, GLX_SAMPLES, out var samples) == 0)
                SampleCount = samples;
            if (GlxGetFBConfigAttrib(_x11.Display, _fbconfig, GLX_STENCIL_SIZE, out var stencil) == 0)
                StencilSize = stencil;

            ImmediateContext = CreateContext(null);
            DeferredContext = CreateContext(ImmediateContext);
            ImmediateContext.MakeCurrent();

            GlInterface = GlInterface.FromNativeUtf8GetProcAddress(p => GlxGetProcAddress(p));
        }
        
        public void ClearContext() => GlxMakeCurrent(_x11.Display, IntPtr.Zero, IntPtr.Zero);

        public GlxContext CreateContext(IGlContext share)
        {
            var sharelist = ((GlxContext)share)?.Handle ?? IntPtr.Zero;
            var h = GlxCreateContext(_x11.Display, _visual, sharelist, true);
            if (h == IntPtr.Zero)
                throw new OpenGlException("Unable to create direct GLX context");
            return new GlxContext(h, this, _x11);
        }

        public void SwapBuffers(IntPtr xid) => GlxSwapBuffers(_x11.Display, xid);
    }
}
