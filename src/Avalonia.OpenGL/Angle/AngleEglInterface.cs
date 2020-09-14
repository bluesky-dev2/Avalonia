using System;
using System.Runtime.InteropServices;

namespace Avalonia.OpenGL.Angle
{
    public class AngleEglInterface : EglInterface
    {
        [DllImport("avangle.dll", CharSet = CharSet.Ansi)]
        static extern IntPtr EGL_GetProcAddress(string proc);

        public AngleEglInterface() : base(LoadAngle())
        {

        }

        static Func<string, IntPtr> LoadAngle()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var disp = EGL_GetProcAddress("eglGetPlatformDisplayEXT");

                if (disp == IntPtr.Zero)
                {
                    throw new OpenGlException("libegl.dll doesn't have eglGetPlatformDisplayEXT entry point");
                }

                return eglGetProcAddress;
            }
            
            throw new PlatformNotSupportedException();
        }

    }
}
