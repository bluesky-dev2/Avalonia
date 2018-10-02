using System;
using Avalonia.Logging;

namespace Avalonia.OpenGL
{
    public class EglGlPlatformFeature : IWindowingPlatformGlFeature
    {
        public IGlDisplay Display { get; set; }
        public IGlContext ImmediateContext { get; set; }
        public IGlContext DeferredContext { get; set; }

        public static void TryInitialize()
        {
            var feature = TryCreate();
            if (feature != null)
                AvaloniaLocator.CurrentMutable.Bind<IWindowingPlatformGlFeature>().ToConstant(feature);
        }
        
        public static EglGlPlatformFeature TryCreate()
        {
            try
            {
                var disp = new EglDisplay();
                var ctx = disp.CreateContext(null);
                return new EglGlPlatformFeature
                {
                    Display = disp,
                    ImmediateContext = ctx,
                    DeferredContext = disp.CreateContext(ctx)
                };
            }
            catch(Exception e)
            {
                Logger.Error("OpenGL", null, "Unable to initialize EGL-based rendering: {0}", e);
                return null;
            }
        }
    }
}
