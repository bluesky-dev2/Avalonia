﻿using Avalonia.Controls.Shapes;
using Xunit;

namespace Avalonia.Controls.UnitTests
{
    public class RelativePanelTests
    {
        [Fact]
        public void Lays_Out_1_Child_Below_the_other()
        {
            var rect1 = new Rectangle { Height = 20, Width = 20 };
            var rect2 = new Rectangle { Height = 20, Width = 20 };

            var target = new RelativePanel
            {
                Children =
                {
                    rect1, rect2
                }
            };

            RelativePanel.SetAlignLeftWithPanel(rect1 , true);
            RelativePanel.SetBelow(rect2, rect1);
            target.Measure(new Size(400, 400));
            target.Arrange(new Rect(target.DesiredSize));

            Assert.Equal(new Size(20, 40), target.Bounds.Size);
            Assert.Equal(new Rect(0, 0, 20, 20), target.Children[0].Bounds);
            Assert.Equal(new Rect(0, 20, 20, 20), target.Children[1].Bounds);
        }
    }
}
