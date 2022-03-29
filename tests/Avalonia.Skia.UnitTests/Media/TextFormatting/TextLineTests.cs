﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.UnitTests;
using Avalonia.Utilities;
using Xunit;

namespace Avalonia.Skia.UnitTests.Media.TextFormatting
{
    public class TextLineTests
    {
        private static readonly string s_multiLineText = "012345678\r\r0123456789";

        [Fact]
        public void Should_Get_First_CharacterHit()
        {
            using (Start())
            {
                var defaultProperties = new GenericTextRunProperties(Typeface.Default);

                var textSource = new SingleBufferTextSource(s_multiLineText, defaultProperties);

                var formatter = new TextFormatterImpl();

                var currentIndex = 0;

                while (currentIndex < s_multiLineText.Length)
                {
                    var textLine =
                        formatter.FormatLine(textSource, currentIndex, double.PositiveInfinity,
                            new GenericTextParagraphProperties(defaultProperties));

                    var firstCharacterHit = textLine.GetPreviousCaretCharacterHit(new CharacterHit(int.MinValue));

                    Assert.Equal(textLine.TextRange.Start, firstCharacterHit.FirstCharacterIndex);

                    currentIndex += textLine.TextRange.Length;
                }
            }
        }

        [Fact]
        public void Should_Get_Last_CharacterHit()
        {
            using (Start())
            {
                var defaultProperties = new GenericTextRunProperties(Typeface.Default);

                var textSource = new SingleBufferTextSource(s_multiLineText, defaultProperties);

                var formatter = new TextFormatterImpl();

                var currentIndex = 0;

                while (currentIndex < s_multiLineText.Length)
                {
                    var textLine =
                        formatter.FormatLine(textSource, currentIndex, double.PositiveInfinity,
                            new GenericTextParagraphProperties(defaultProperties));

                    var lastCharacterHit = textLine.GetNextCaretCharacterHit(new CharacterHit(int.MaxValue));

                    Assert.Equal(textLine.TextRange.Start + textLine.TextRange.Length,
                        lastCharacterHit.FirstCharacterIndex + lastCharacterHit.TrailingLength);

                    currentIndex += textLine.TextRange.Length;
                }
            }
        }
        
        [Fact]
        public void Should_Get_Next_Caret_CharacterHit_Bidi()
        {
            const string text = "אבג 1 ABC";
            
            using (Start())
            {
                var defaultProperties = new GenericTextRunProperties(Typeface.Default);

                var textSource = new SingleBufferTextSource(text, defaultProperties);

                var formatter = new TextFormatterImpl();

                var textLine =
                    formatter.FormatLine(textSource, 0, double.PositiveInfinity,
                        new GenericTextParagraphProperties(defaultProperties));

                var clusters = new List<int>();

                foreach (var textRun in textLine.TextRuns.OrderBy(x=> x.Text.Start))
                {
                    var shapedRun = (ShapedTextCharacters)textRun;

                    clusters.AddRange(shapedRun.IsReversed ?
                        shapedRun.ShapedBuffer.GlyphClusters.Reverse() :
                        shapedRun.ShapedBuffer.GlyphClusters);
                }
                
                var nextCharacterHit = new CharacterHit(0, clusters[1] - clusters[0]);

                foreach (var cluster in clusters)
                {
                    Assert.Equal(cluster, nextCharacterHit.FirstCharacterIndex);

                    nextCharacterHit = textLine.GetNextCaretCharacterHit(nextCharacterHit);
                }

                var lastCharacterHit = nextCharacterHit;

                nextCharacterHit = textLine.GetNextCaretCharacterHit(lastCharacterHit);

                Assert.Equal(lastCharacterHit.FirstCharacterIndex, nextCharacterHit.FirstCharacterIndex);

                Assert.Equal(lastCharacterHit.TrailingLength, nextCharacterHit.TrailingLength);
            }
        }

        [Fact]
        public void Should_Get_Previous_Caret_CharacterHit_Bidi()
        {
            const string text = "אבג 1 ABC";
            
            using (Start())
            {
                var defaultProperties = new GenericTextRunProperties(Typeface.Default);

                var textSource = new SingleBufferTextSource(text, defaultProperties);

                var formatter = new TextFormatterImpl();

                var textLine =
                    formatter.FormatLine(textSource, 0, double.PositiveInfinity,
                        new GenericTextParagraphProperties(defaultProperties));

                var clusters = new List<int>();

                foreach (var textRun in textLine.TextRuns.OrderBy(x=> x.Text.Start))
                {
                    var shapedRun = (ShapedTextCharacters)textRun;

                    clusters.AddRange(shapedRun.IsReversed ?
                        shapedRun.ShapedBuffer.GlyphClusters.Reverse() :
                        shapedRun.ShapedBuffer.GlyphClusters);
                }

                clusters.Reverse();
                
                var nextCharacterHit = new CharacterHit(text.Length - 1);

                foreach (var cluster in clusters)
                {
                    var currentCaretIndex = nextCharacterHit.FirstCharacterIndex + nextCharacterHit.TrailingLength;
                    
                    Assert.Equal(cluster, currentCaretIndex);

                    nextCharacterHit = textLine.GetPreviousCaretCharacterHit(nextCharacterHit);
                }

                var lastCharacterHit = nextCharacterHit;

                nextCharacterHit = textLine.GetPreviousCaretCharacterHit(lastCharacterHit);

                Assert.Equal(lastCharacterHit.FirstCharacterIndex, nextCharacterHit.FirstCharacterIndex);

                Assert.Equal(lastCharacterHit.TrailingLength, nextCharacterHit.TrailingLength);
            }
        }
        
        [InlineData("𐐷𐐷𐐷𐐷𐐷")]
        [InlineData("01234567🎉\n")]
        [InlineData("𐐷1234")]
        [Theory]
        public void Should_Get_Next_Caret_CharacterHit(string text)
        {
            using (Start())
            {
                var defaultProperties = new GenericTextRunProperties(Typeface.Default);

                var textSource = new SingleBufferTextSource(text, defaultProperties);

                var formatter = new TextFormatterImpl();

                var textLine =
                    formatter.FormatLine(textSource, 0, double.PositiveInfinity,
                        new GenericTextParagraphProperties(defaultProperties));

                var clusters = textLine.TextRuns.Cast<ShapedTextCharacters>().SelectMany(x => x.ShapedBuffer.GlyphClusters)
                    .ToArray();

                var nextCharacterHit = new CharacterHit(0);

                for (var i = 0; i < clusters.Length; i++)
                {
                    Assert.Equal(clusters[i], nextCharacterHit.FirstCharacterIndex);

                    nextCharacterHit = textLine.GetNextCaretCharacterHit(nextCharacterHit);
                }

                var lastCharacterHit = nextCharacterHit;

                nextCharacterHit = textLine.GetNextCaretCharacterHit(lastCharacterHit);

                Assert.Equal(lastCharacterHit.FirstCharacterIndex, nextCharacterHit.FirstCharacterIndex);

                Assert.Equal(lastCharacterHit.TrailingLength, nextCharacterHit.TrailingLength);

                nextCharacterHit = new CharacterHit(0, clusters[1] - clusters[0]);

                foreach (var cluster in clusters)
                {
                    Assert.Equal(cluster, nextCharacterHit.FirstCharacterIndex);

                    nextCharacterHit = textLine.GetNextCaretCharacterHit(nextCharacterHit);
                }

                lastCharacterHit = nextCharacterHit;

                nextCharacterHit = textLine.GetNextCaretCharacterHit(lastCharacterHit);

                Assert.Equal(lastCharacterHit.FirstCharacterIndex, nextCharacterHit.FirstCharacterIndex);

                Assert.Equal(lastCharacterHit.TrailingLength, nextCharacterHit.TrailingLength);
            }
        }

        [InlineData("𐐷𐐷𐐷𐐷𐐷")]
        [InlineData("01234567🎉\n")]
        [InlineData("𐐷1234")]
        [Theory]
        public void Should_Get_Previous_Caret_CharacterHit(string text)
        {
            using (Start())
            {
                var defaultProperties = new GenericTextRunProperties(Typeface.Default);

                var textSource = new SingleBufferTextSource(text, defaultProperties);

                var formatter = new TextFormatterImpl();

                var textLine =
                    formatter.FormatLine(textSource, 0, double.PositiveInfinity,
                        new GenericTextParagraphProperties(defaultProperties));

                var clusters = textLine.TextRuns.Cast<ShapedTextCharacters>().SelectMany(x => x.ShapedBuffer.GlyphClusters)
                    .ToArray();

                var previousCharacterHit = new CharacterHit(text.Length);

                for (var i = clusters.Length - 1; i >= 0; i--)
                {
                    previousCharacterHit = textLine.GetPreviousCaretCharacterHit(previousCharacterHit);

                    Assert.Equal(clusters[i],
                        previousCharacterHit.FirstCharacterIndex + previousCharacterHit.TrailingLength);
                }

                var firstCharacterHit = previousCharacterHit;

                previousCharacterHit = textLine.GetPreviousCaretCharacterHit(firstCharacterHit);

                Assert.Equal(firstCharacterHit.FirstCharacterIndex, previousCharacterHit.FirstCharacterIndex);

                Assert.Equal(0, previousCharacterHit.TrailingLength);

                previousCharacterHit = new CharacterHit(clusters[^1], text.Length - clusters[^1]);

                for (var i = clusters.Length - 1; i > 0; i--)
                {
                    previousCharacterHit = textLine.GetPreviousCaretCharacterHit(previousCharacterHit);

                    Assert.Equal(clusters[i],
                        previousCharacterHit.FirstCharacterIndex + previousCharacterHit.TrailingLength);
                }

                firstCharacterHit = previousCharacterHit;

                firstCharacterHit = textLine.GetPreviousCaretCharacterHit(firstCharacterHit);

                previousCharacterHit = textLine.GetPreviousCaretCharacterHit(firstCharacterHit);

                Assert.Equal(firstCharacterHit.FirstCharacterIndex, previousCharacterHit.FirstCharacterIndex);

                Assert.Equal(0, previousCharacterHit.TrailingLength);
            }
        }

        [Fact]
        public void Should_Get_Distance_From_CharacterHit()
        {
            using (Start())
            {
                var defaultProperties = new GenericTextRunProperties(Typeface.Default);

                var textSource = new SingleBufferTextSource(s_multiLineText, defaultProperties);

                var formatter = new TextFormatterImpl();

                var textLine =
                    formatter.FormatLine(textSource, 0, double.PositiveInfinity,
                        new GenericTextParagraphProperties(defaultProperties));

                var currentDistance = 0.0;

                foreach (var run in textLine.TextRuns)
                {
                    var textRun = (ShapedTextCharacters)run;

                    var glyphRun = textRun.GlyphRun;

                    for (var i = 0; i < glyphRun.GlyphClusters!.Count; i++)
                    {
                        var cluster = glyphRun.GlyphClusters[i];

                        var advance = glyphRun.GlyphAdvances[i];

                        var distance = textLine.GetDistanceFromCharacterHit(new CharacterHit(cluster));

                        Assert.Equal(currentDistance, distance);

                        currentDistance += advance;
                    }
                }

                Assert.Equal(currentDistance,textLine.GetDistanceFromCharacterHit(new CharacterHit(s_multiLineText.Length)));
            }
        }

        [InlineData("ABC012345")] //LeftToRight
        [InlineData("זה כיף סתם לשמוע איך תנצח קרפד עץ טוב בגן")] //RightToLeft
        [Theory]
        public void Should_Get_CharacterHit_From_Distance(string text)
        {
            using (Start())
            {
                var defaultProperties = new GenericTextRunProperties(Typeface.Default);

                var textSource = new SingleBufferTextSource(text, defaultProperties);

                var formatter = new TextFormatterImpl();

                var textLine =
                    formatter.FormatLine(textSource, 0, double.PositiveInfinity,
                        new GenericTextParagraphProperties(defaultProperties));

                var isRightToLeft = IsRightToLeft(textLine);
                var rects = BuildRects(textLine);
                var glyphClusters = BuildGlyphClusters(textLine);

                for (var i = 0; i < rects.Count; i++)
                {
                    var cluster = glyphClusters[i];
                    var rect = rects[i];

                    var characterHit = textLine.GetCharacterHitFromDistance(rect.Left);

                    Assert.Equal(isRightToLeft ? cluster + 1 : cluster,
                        characterHit.FirstCharacterIndex + characterHit.TrailingLength);
                }
            }
        }

        public static IEnumerable<object[]> CollapsingData
        {
            get
            {
                yield return CreateData("01234 01234 01234", 120, TextTrimming.PrefixCharacterEllipsis, "01234 01\u20264 01234");
                yield return CreateData("01234 01234", 58, TextTrimming.CharacterEllipsis, "01234 0\u2026");
                yield return CreateData("01234 01234", 58, TextTrimming.WordEllipsis, "01234\u2026");
                yield return CreateData("01234", 9, TextTrimming.CharacterEllipsis, "\u2026");
                yield return CreateData("01234", 2, TextTrimming.CharacterEllipsis, "");
                
                object[] CreateData(string text, double width, TextTrimming mode, string expected)
                {
                    return new object[]
                    {
                        text, width, mode, expected
                    };
                }
            }
        }

        [MemberData(nameof(CollapsingData))]
        [Theory]
        public void Should_Collapse_Line(string text, double width, TextTrimming trimming, string expected)
        {
            using (Start())
            {
                var defaultProperties = new GenericTextRunProperties(Typeface.Default);

                var textSource = new SingleBufferTextSource(text, defaultProperties);

                var formatter = new TextFormatterImpl();

                var textLine =
                    formatter.FormatLine(textSource, 0, double.PositiveInfinity,
                        new GenericTextParagraphProperties(defaultProperties));

                Assert.False(textLine.HasCollapsed);

                TextCollapsingProperties collapsingProperties = trimming.CreateCollapsingProperties(new TextCollapsingCreateInfo(width, defaultProperties));

                var collapsedLine = textLine.Collapse(collapsingProperties);

                Assert.True(collapsedLine.HasCollapsed);

                var trimmedText = collapsedLine.TextRuns.SelectMany(x => x.Text).ToArray();

                Assert.Equal(expected.Length, trimmedText.Length);

                for (var i = 0; i < expected.Length; i++)
                {
                    Assert.Equal(expected[i], trimmedText[i]);
                }
            }
        }

        [Fact]
        public void Should_Get_Next_CharacterHit_For_Drawable_Runs()
        {
            using (Start())
            {
                var defaultProperties = new GenericTextRunProperties(Typeface.Default);
                var textSource = new DrawableRunTextSource();
                
                var formatter = new TextFormatterImpl();

                var textLine =
                    formatter.FormatLine(textSource, 0, double.PositiveInfinity,
                        new GenericTextParagraphProperties(defaultProperties));

                Assert.Equal(4, textLine.TextRuns.Count);

                var currentHit = textLine.GetNextCaretCharacterHit(new CharacterHit(0));

                Assert.Equal(1, currentHit.FirstCharacterIndex);
                Assert.Equal(0, currentHit.TrailingLength);

                currentHit = textLine.GetNextCaretCharacterHit(currentHit);

                Assert.Equal(2, currentHit.FirstCharacterIndex);
                Assert.Equal(0, currentHit.TrailingLength);

                currentHit = textLine.GetNextCaretCharacterHit(currentHit);

                Assert.Equal(3, currentHit.FirstCharacterIndex);
                Assert.Equal(0, currentHit.TrailingLength);

                currentHit = textLine.GetNextCaretCharacterHit(currentHit);

                Assert.Equal(3, currentHit.FirstCharacterIndex);
                Assert.Equal(1, currentHit.TrailingLength);
            }
        }

        [Fact]
        public void Should_Get_Previous_CharacterHit_For_Drawable_Runs()
        {
            using (Start())
            {
                var defaultProperties = new GenericTextRunProperties(Typeface.Default);
                var textSource = new DrawableRunTextSource();

                var formatter = new TextFormatterImpl();

                var textLine =
                    formatter.FormatLine(textSource, 0, double.PositiveInfinity,
                        new GenericTextParagraphProperties(defaultProperties));

                Assert.Equal(4, textLine.TextRuns.Count);

                var currentHit = textLine.GetPreviousCaretCharacterHit(new CharacterHit(3,1));

                Assert.Equal(3, currentHit.FirstCharacterIndex);
                Assert.Equal(0, currentHit.TrailingLength);

                currentHit = textLine.GetPreviousCaretCharacterHit(currentHit);

                Assert.Equal(2, currentHit.FirstCharacterIndex);
                Assert.Equal(0, currentHit.TrailingLength);

                currentHit = textLine.GetPreviousCaretCharacterHit(currentHit);

                Assert.Equal(1, currentHit.FirstCharacterIndex);
                Assert.Equal(0, currentHit.TrailingLength);

                currentHit = textLine.GetPreviousCaretCharacterHit(currentHit);

                Assert.Equal(0, currentHit.FirstCharacterIndex);
                Assert.Equal(0, currentHit.TrailingLength);
            }
        }

        [Fact]
        public void Should_Get_CharacterHit_From_Distance_For_Drawable_Runs()
        {
            using (Start())
            {
                var defaultProperties = new GenericTextRunProperties(Typeface.Default);
                var textSource = new DrawableRunTextSource();

                var formatter = new TextFormatterImpl();

                var textLine =
                    formatter.FormatLine(textSource, 0, double.PositiveInfinity,
                        new GenericTextParagraphProperties(defaultProperties));

                var characterHit = textLine.GetCharacterHitFromDistance(50);

                Assert.Equal(3, characterHit.FirstCharacterIndex);
                Assert.Equal(1, characterHit.TrailingLength);

                characterHit = textLine.GetCharacterHitFromDistance(32);


                Assert.Equal(3, characterHit.FirstCharacterIndex);
                Assert.Equal(0, characterHit.TrailingLength);
            }
        }

        private class DrawableRunTextSource : ITextSource
        {
            const string Text = "A_A_";

            public TextRun? GetTextRun(int textSourceIndex)
            {
                switch (textSourceIndex)
                {
                    case 0:
                        return new TextCharacters(new ReadOnlySlice<char>(Text.AsMemory(), 0, 1), new GenericTextRunProperties(Typeface.Default));
                    case 1:
                        return new CustomDrawableRun(1);
                    case 2:
                        return new TextCharacters(new ReadOnlySlice<char>(Text.AsMemory(), 2, 1, 2), new GenericTextRunProperties(Typeface.Default));
                    case 3:
                        return new CustomDrawableRun(3);
                    default:
                        return null;
                }
            }
        }
        
        private class CustomDrawableRun : DrawableTextRun
        {
            public CustomDrawableRun(int start)
            {
                Text = new ReadOnlySlice<char>(new char[1], start, DefaultTextSourceLength);
            }

            public override ReadOnlySlice<char> Text { get; }

            public override Size Size => new(14, 14);
            public override double Baseline => 14;
            public override void Draw(DrawingContext drawingContext, Point origin)
            {
               
            }
        }

        private static bool IsRightToLeft(TextLine textLine)
        {
            return textLine.TextRuns.Cast<ShapedTextCharacters>().Any(x => !x.ShapedBuffer.IsLeftToRight);
        }

        private static List<int> BuildGlyphClusters(TextLine textLine)
        {
            var glyphClusters = new List<int>();

            var shapedTextRuns = textLine.TextRuns.Cast<ShapedTextCharacters>().ToList();

            var lastCluster = -1;
            
            foreach (var textRun in shapedTextRuns)
            {
                var shapedBuffer = textRun.ShapedBuffer;

                var currentClusters = shapedBuffer.GlyphClusters.ToList();

                foreach (var currentCluster in currentClusters) 
                {
                    if (lastCluster == currentCluster)
                    {
                        continue;
                    }
                    
                    glyphClusters.Add(currentCluster);

                    lastCluster = currentCluster;
                }
            }
            
            return glyphClusters;
        }
        
        private static List<Rect> BuildRects(TextLine textLine)
        {
            var rects = new List<Rect>();
            var height = textLine.Height;

            var currentX = 0d;

            var lastCluster = -1;

            var shapedTextRuns = textLine.TextRuns.Cast<ShapedTextCharacters>().ToList();

            foreach (var textRun in shapedTextRuns)
            {
                var shapedBuffer = textRun.ShapedBuffer;
            
                for (var index = 0; index < shapedBuffer.GlyphAdvances.Count; index++)
                {
                    var currentCluster = shapedBuffer.GlyphClusters[index];
                
                    var advance = shapedBuffer.GlyphAdvances[index];

                    if (lastCluster != currentCluster)
                    {
                        rects.Add(new Rect(currentX, 0, advance, height));
                    }
                    else
                    {
                        var rect = rects[index - 1];

                        rects.Remove(rect);

                        rect = rect.WithWidth(rect.Width + advance);
                    
                        rects.Add(rect);
                    }
                    
                    currentX += advance;

                    lastCluster = currentCluster;
                }
            }

            return rects;
        }

        [Fact]
        public void Should_Get_TextBounds()
        {
            using (Start())
            {
                var defaultProperties = new GenericTextRunProperties(Typeface.Default);
                var text = "0123".AsMemory();
                var ltrOptions = new TextShaperOptions(Typeface.Default.GlyphTypeface, 10, 0, CultureInfo.CurrentCulture);
                var rtlOptions = new TextShaperOptions(Typeface.Default.GlyphTypeface, 10, 1, CultureInfo.CurrentCulture);

                var textRuns = new List<TextRun>
                {
                    new ShapedTextCharacters(TextShaper.Current.ShapeText(new ReadOnlySlice<char>(text), ltrOptions), defaultProperties),
                    new ShapedTextCharacters(TextShaper.Current.ShapeText(new ReadOnlySlice<char>(text, text.Length, text.Length), ltrOptions), defaultProperties),
                    new ShapedTextCharacters(TextShaper.Current.ShapeText(new ReadOnlySlice<char>(text, text.Length * 2, text.Length), rtlOptions), defaultProperties),
                    new ShapedTextCharacters(TextShaper.Current.ShapeText(new ReadOnlySlice<char>(text, text.Length * 3, text.Length), ltrOptions), defaultProperties)
                };

             
                var textSource = new FixedRunsTextSource(textRuns);

                var formatter = new TextFormatterImpl();

                var textLine =
                    formatter.FormatLine(textSource, 0, double.PositiveInfinity,
                        new GenericTextParagraphProperties(defaultProperties));

                var textBounds = textLine.GetTextBounds(0, text.Length * 4);

                Assert.Equal(3, textBounds.Count);
                Assert.Equal(textLine.WidthIncludingTrailingWhitespace, textBounds.Sum(x => x.Rectangle.Width));
            }
        }

        private class FixedRunsTextSource : ITextSource
        {
            private readonly IReadOnlyList<TextRun> _textRuns;

            public FixedRunsTextSource(IReadOnlyList<TextRun> textRuns)
            {
                _textRuns = textRuns;
            }

            public TextRun? GetTextRun(int textSourceIndex)
            {
                foreach (var textRun in _textRuns)
                {
                    if(textRun.Text.Start == textSourceIndex)
                    {
                        return textRun;
                    }
                }

                return null;
            }
        }

        private static IDisposable Start()
        {
            var disposable = UnitTestApplication.Start(TestServices.MockPlatformRenderInterface
                .With(renderInterface: new PlatformRenderInterface(null),
                    textShaperImpl: new TextShaperImpl(),
                    fontManagerImpl: new CustomFontManagerImpl()));

            return disposable;
        }
    }
}
