﻿// Copyright (c) The Avalonia Project. All rights reserved.
// Licensed under the MIT license. See licence.md file in the project root for full license information.

using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Rendering;
using Avalonia.UnitTests;
using Avalonia.VisualTree;
using Xunit;

namespace Avalonia.Visuals.UnitTests.Rendering
{
    public class ImmediateRendererTests_HitTesting
    {
        [Fact]
        public void HitTest_Should_Find_Controls_At_Point()
        {
            using (TestApplication())
            {
                var root = new TestRoot
                {
                    Width = 200,
                    Height = 200,
                    Child = new Border
                    {
                        Width = 100,
                        Height = 100,
                        Background = Brushes.Red,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                };

                root.Renderer = new ImmediateRenderer(root);
                root.Measure(Size.Infinity);
                root.Arrange(new Rect(root.DesiredSize));
                root.Renderer.Paint(new Rect(root.ClientSize));

                var result = root.Renderer.HitTest(new Point(100, 100), root, null);

                Assert.Equal(new[] { root.Child, root }, result);
            }
        }

        [Fact]
        public void HitTest_Should_Not_Find_Invisible_Controls_At_Point()
        {
            using (TestApplication())
            {
                Border visible;
                var root = new TestRoot
                {
                    Width = 200,
                    Height = 200,
                    Child = new Border
                    {
                        Width = 100,
                        Height = 100,
                        Background = Brushes.Red,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        IsVisible = false,
                        Child = visible = new Border
                        {
                            Background = Brushes.Red,
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            VerticalAlignment = VerticalAlignment.Stretch,
                        }
                    }
                };

                root.Renderer = new ImmediateRenderer(root);
                root.Measure(Size.Infinity);
                root.Arrange(new Rect(root.DesiredSize));
                root.Renderer.Paint(new Rect(root.ClientSize));

                var result = root.Renderer.HitTest(new Point(100, 100), root, null);

                Assert.Equal(new[] { root }, result);
            }
        }

        [Fact]
        public void HitTest_Should_Not_Find_Control_Outside_Point()
        {
            using (TestApplication())
            {
                var root = new TestRoot
                {
                    Width = 200,
                    Height = 200,
                    Child = new Border
                    {
                        Width = 100,
                        Height = 100,
                        Background = Brushes.Red,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                };

                root.Renderer = new ImmediateRenderer(root);
                root.Measure(Size.Infinity);
                root.Arrange(new Rect(root.DesiredSize));
                root.Renderer.Paint(new Rect(root.ClientSize));

                var result = root.Renderer.HitTest(new Point(10, 10), root, null);

                Assert.Equal(new[] { root }, result);
            }
        }

        [Fact]
        public void HitTest_Should_Return_Top_Controls_First()
        {
            using (TestApplication())
            {
                Panel container;
                var root = new TestRoot
                {
                    Child = container = new Panel
                    {
                        Width = 200,
                        Height = 200,
                        Children =
                        {
                            new Border
                            {
                                Width = 100,
                                Height = 100,
                                Background = Brushes.Red,
                                HorizontalAlignment = HorizontalAlignment.Center,
                                VerticalAlignment = VerticalAlignment.Center
                            },
                            new Border
                            {
                                Width = 50,
                                Height = 50,
                                Background = Brushes.Red,
                                HorizontalAlignment = HorizontalAlignment.Center,
                                VerticalAlignment = VerticalAlignment.Center
                            }
                        }
                    }
                };

                root.Renderer = new ImmediateRenderer(root);
                root.Measure(Size.Infinity);
                root.Arrange(new Rect(container.DesiredSize));
                root.Renderer.Paint(new Rect(root.ClientSize));

                var result = root.Renderer.HitTest(new Point(100, 100), root, null);

                Assert.Equal(new[] { container.Children[1], container.Children[0], container, root }, result);
            }
        }

        [Fact]
        public void HitTest_Should_Return_Top_Controls_First_With_ZIndex()
        {
            using (TestApplication())
            {
                Panel container;
                var root = new TestRoot
                {
                    Child = container = new Panel
                    {
                        Width = 200,
                        Height = 200,
                        Children =
                        {
                            new Border
                            {
                                Width = 100,
                                Height = 100,
                                ZIndex = 1,
                                Background = Brushes.Red,
                                HorizontalAlignment = HorizontalAlignment.Center,
                                VerticalAlignment = VerticalAlignment.Center
                            },
                            new Border
                            {
                                Width = 50,
                                Height = 50,
                                Background = Brushes.Red,
                                HorizontalAlignment = HorizontalAlignment.Center,
                                VerticalAlignment = VerticalAlignment.Center
                            },
                            new Border
                            {
                                Width = 75,
                                Height = 75,
                                ZIndex = 2,
                                Background = Brushes.Red,
                                HorizontalAlignment = HorizontalAlignment.Center,
                                VerticalAlignment = VerticalAlignment.Center
                            }
                        }
                    }
                };

                root.Renderer = new ImmediateRenderer(root);
                root.Measure(Size.Infinity);
                root.Arrange(new Rect(container.DesiredSize));
                root.Renderer.Paint(new Rect(root.ClientSize));

                var result = root.Renderer.HitTest(new Point(100, 100), root, null);

                Assert.Equal(
                    new[] 
                    {
                        container.Children[2],
                        container.Children[0],
                        container.Children[1],
                        container,
                        root
                    }, 
                    result);
            }
        }

        [Fact]
        public void HitTest_Should_Find_Control_Translated_Outside_Parent_Bounds()
        {
            using (TestApplication())
            {
                Border target;
                Panel container;
                var root = new TestRoot
                {
                    Child = container = new Panel
                    {
                        Width = 200,
                        Height = 200,
                        Background = Brushes.Red,
                        ClipToBounds = false,
                        Children =
                        {
                            new Border
                            {
                                Width = 100,
                                Height = 100,
                                ZIndex = 1,
                                Background = Brushes.Red,
                                HorizontalAlignment = HorizontalAlignment.Left,
                                VerticalAlignment = VerticalAlignment.Top,
                                Child = target = new Border
                                {
                                    Width = 50,
                                    Height = 50,
                                    Background = Brushes.Red,
                                    HorizontalAlignment = HorizontalAlignment.Left,
                                    VerticalAlignment = VerticalAlignment.Top,
                                    RenderTransform = new TranslateTransform(110, 110),
                                }
                            },
                        }
                    }
                };

                root.Renderer = new ImmediateRenderer(root);
                container.Measure(Size.Infinity);
                container.Arrange(new Rect(container.DesiredSize));
                root.Renderer.Paint(new Rect(root.ClientSize));

                var result = root.Renderer.HitTest(new Point(120, 120), root, null);

                Assert.Equal(new IVisual[] { target, container }, result);
            }
        }

        [Fact]
        public void HitTest_Should_Not_Find_Control_Outside_Parent_Bounds_When_Clipped()
        {
            using (TestApplication())
            {
                Border target;
                Panel container;
                var root = new TestRoot
                {
                    Child = container = new Panel
                    {
                        Width = 100,
                        Height = 200,
                        Background = Brushes.Red,
                        Children =
                        {
                            new Panel()
                            {
                                Width = 100,
                                Height = 100,
                                Background = Brushes.Red,
                                Margin = new Thickness(0, 100, 0, 0),
                                ClipToBounds = true,
                                Children =
                                {
                                    (target = new Border()
                                    {
                                        Width = 100,
                                        Height = 100,
                                        Background = Brushes.Red,
                                        Margin = new Thickness(0, -100, 0, 0)
                                    })
                                }
                            }
                        }
                    }
                };

                root.Renderer = new ImmediateRenderer(root);
                root.Measure(Size.Infinity);
                root.Arrange(new Rect(container.DesiredSize));
                root.Renderer.Paint(new Rect(root.ClientSize));

                var result = root.Renderer.HitTest(new Point(50, 50), root, null);

                Assert.Equal(new IVisual[] { container, root }, result);
            }
        }

        [Fact]
        public void HitTest_Should_Not_Find_Control_Outside_Scroll_Viewport()
        {
            using (TestApplication())
            {
                Border target;
                Border item1;
                Border item2;
                ScrollContentPresenter scroll;
                Panel container;
                var root = new TestRoot
                {
                    Child = container = new Panel
                    {
                        Width = 100,
                        Height = 200,
                        Background = Brushes.Red,
                        Children =
                        {
                            (target = new Border()
                            {
                                Name = "b1",
                                Width = 100,
                                Height = 100,
                                Background = Brushes.Red,
                            }),
                            new Border()
                            {
                                Name = "b2",
                                Width = 100,
                                Height = 100,
                                Background = Brushes.Red,
                                Margin = new Thickness(0, 100, 0, 0),
                                Child = scroll = new ScrollContentPresenter()
                                {
                                    CanHorizontallyScroll = true,
                                    CanVerticallyScroll = true,
                                    Content = new StackPanel()
                                    {
                                        Children =
                                        {
                                            (item1 = new Border()
                                            {
                                                Name = "b3",
                                                Width = 100,
                                                Height = 100,
                                                Background = Brushes.Red,
                                            }),
                                            (item2 = new Border()
                                            {
                                                Name = "b4",
                                                Width = 100,
                                                Height = 100,
                                                Background = Brushes.Red,
                                            }),
                                        }
                                    }
                                }
                            }
                        }
                    }
                };

                scroll.UpdateChild();

                root.Renderer = new ImmediateRenderer(root);
                root.Measure(Size.Infinity);
                root.Arrange(new Rect(container.DesiredSize));
                root.Renderer.Paint(new Rect(root.ClientSize));

                var result = root.Renderer.HitTest(new Point(50, 150), root, null).First();

                Assert.Equal(item1, result);

                result = root.Renderer.HitTest(new Point(50, 50), root, null).First();

                Assert.Equal(target, result);

                scroll.Offset = new Vector(0, 100);

                // We don't have LayoutManager set up so do the layout pass manually.
                scroll.Parent.InvalidateArrange();
                container.InvalidateArrange();
                container.Arrange(new Rect(container.DesiredSize));
                root.Renderer.Paint(new Rect(root.ClientSize));

                result = root.Renderer.HitTest(new Point(50, 150), root, null).First();
                Assert.Equal(item2, result);

                result = root.Renderer.HitTest(new Point(50, 50), root, null).First();
                Assert.Equal(target, result);
            }
        }

        private IDisposable TestApplication()
        {
            return UnitTestApplication.Start(TestServices.MockPlatformRenderInterface);
        }
    }
}
