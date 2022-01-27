using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Media;
using Avalonia.Styling;

namespace Avalonia.Animation;

public class Rotate3DTransition: PageSlide
{

    /// <summary>
    ///  Creates a new instance of the <see cref="Rotate3DTransition"/>
    /// </summary>
    /// <param name="duration">How long the rotation should take place</param>
    /// <param name="orientation">The orientation of the rotation</param>
    public Rotate3DTransition(TimeSpan duration, SlideAxis orientation = SlideAxis.Horizontal, double? depth = null)
        : base(duration, orientation)
    {
        Depth = depth;
    }
    
    /// <summary>
    ///  Defines the depth of the 3D Effect. If null, depth will be calculated automatically from the width or height
    ///  of the common parent of the visual being rotated.
    /// </summary>
    public double? Depth { get; set; }

    /// <summary>
    ///  Creates a new instance of the <see cref="Rotate3DTransition"/>
    /// </summary>
    public Rotate3DTransition() { }

    /// <inheritdoc />
    public override async Task Start(Visual? @from, Visual? to, bool forward, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        var tasks = new List<Task>();
        var parent = GetVisualParent(from, to);
        var (rotateProperty, center) = Orientation switch
        {
            SlideAxis.Vertical => (Rotate3DTransform.AngleXProperty, parent.Bounds.Height),
            SlideAxis.Horizontal => (Rotate3DTransform.AngleYProperty, parent.Bounds.Width),
            _ => throw new ArgumentOutOfRangeException()
        };

        var depthSetter = new Setter {Property = Rotate3DTransform.DepthProperty, Value = Depth ?? center};
        var centerZSetter = new Setter {Property = Rotate3DTransform.CenterZProperty, Value = -center / 2};

        KeyFrame CreateKeyFrame(double cue, double rotation, int zIndex) => 
            new() {
                Setters =
                {
                    new Setter { Property = rotateProperty, Value = rotation },
                    new Setter { Property = Visual.ZIndexProperty, Value = zIndex },
                    centerZSetter,
                    depthSetter
                },
                Cue = new Cue(cue)
            };

        if (from != null)
        {
            var animation = new Animation
            {
                Easing = SlideOutEasing,
                Duration = Duration,
                Children =
                {
                    CreateKeyFrame(0d, 0d, 2),
                    CreateKeyFrame(0.5d, 45d * (forward ? -1 : 1), 1),
                    CreateKeyFrame(1d, 90d * (forward ? -1 : 1), 1)
                }
            };

            tasks.Add(animation.RunAsync(from, null, cancellationToken));
        }

        if (to != null)
        {
            to.IsVisible = true;
            var animation = new Animation
            {
                Easing = SlideInEasing,
                Duration = Duration,
                Children =
                {
                    CreateKeyFrame(0d, 90d * (forward ? 1 : -1), 1),
                    CreateKeyFrame(0.5d, 45d * (forward ? 1 : -1), 1),
                    CreateKeyFrame(1d, 0d, 2)
                }
            };

            tasks.Add(animation.RunAsync(to, null, cancellationToken));
        }

        await Task.WhenAll(tasks);

        if (!cancellationToken.IsCancellationRequested)
        {
            if (from != null)
            {
                from.IsVisible = false;
                from.ZIndex = 1;
            }

            if (to != null)
            {
                to.ZIndex = 2;
            }
        }
    }
}
