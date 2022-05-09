using System.Numerics;
using Avalonia.Media.Immutable;
using Avalonia.Platform;

namespace Avalonia.Rendering.Composition.Server
{
    internal partial class ServerCompositionSolidColorVisual
    {
        protected override void RenderCore(CompositorDrawingContextProxy canvas, Matrix4x4 transform)
        {
            canvas.Transform = canvas.CutTransform(transform);
            canvas.DrawRectangle(new ImmutableSolidColorBrush(Color), null, new RoundedRect(new Rect(new Size(Size))));
            base.RenderCore(canvas, transform);
        }
    }
}