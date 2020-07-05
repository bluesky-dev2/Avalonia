﻿using System.Linq;
using Avalonia.Rendering;
using Avalonia.VisualTree;

#nullable enable

namespace Avalonia.Controls.Primitives
{
    public class ChromeOverlayLayer : Panel, ICustomSimpleHitTest
    {
        public static ChromeOverlayLayer? GetOverlayLayer(IVisual visual)
        {
            foreach (var v in visual.GetVisualAncestors())
                if (v is VisualLayerManager vlm)
                    if (vlm.OverlayLayer != null)
                        return vlm.ChromeOverlayLayer;

            if (visual is TopLevel tl)
            {
                var layers = tl.GetVisualDescendants().OfType<VisualLayerManager>().FirstOrDefault();
                return layers?.ChromeOverlayLayer;
            }

            return null;
        }

        public bool HitTest(Point point) => Children.HitTestCustom(point);
    }
}
