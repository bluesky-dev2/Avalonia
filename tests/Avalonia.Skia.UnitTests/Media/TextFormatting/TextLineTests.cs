﻿using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.UnitTests;
using Xunit;

namespace Avalonia.Skia.UnitTests.Media.TextFormatting
{
    public class TextLineTests
    {
        [InlineData("𐐷𐐷𐐷𐐷𐐷")]
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

                var clusters = textLine.TextRuns.Cast<ShapedTextCharacters>().SelectMany(x => x.GlyphRun.GlyphClusters)
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

                for (var i = 0; i < clusters.Length; i++)
                {
                    Assert.Equal(clusters[i], nextCharacterHit.FirstCharacterIndex);

                    nextCharacterHit = textLine.GetNextCaretCharacterHit(nextCharacterHit);
                }

                lastCharacterHit = nextCharacterHit;

                nextCharacterHit = textLine.GetNextCaretCharacterHit(lastCharacterHit);

                Assert.Equal(lastCharacterHit.FirstCharacterIndex, nextCharacterHit.FirstCharacterIndex);

                Assert.Equal(lastCharacterHit.TrailingLength, nextCharacterHit.TrailingLength);
            }
        }

        [InlineData("𐐷𐐷𐐷𐐷𐐷")]
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

                var clusters = textLine.TextRuns.Cast<ShapedTextCharacters>().SelectMany(x => x.GlyphRun.GlyphClusters)
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

                var textSource = new MultiBufferTextSource(defaultProperties);

                var formatter = new TextFormatterImpl();

                var textLine =
                    formatter.FormatLine(textSource, 0, double.PositiveInfinity,
                        new GenericTextParagraphProperties(defaultProperties));

                var currentDistance = 0.0;

                foreach (var run in textLine.TextRuns)
                {
                    var textRun = (ShapedTextCharacters)run;

                    var glyphRun = textRun.GlyphRun;

                    for (var i = 0; i < glyphRun.GlyphClusters.Length; i++)
                    {
                        var cluster = glyphRun.GlyphClusters[i];

                        var glyph = glyphRun.GlyphIndices[i];

                        var advance = glyphRun.GlyphTypeface.GetGlyphAdvance(glyph) * glyphRun.Scale;

                        var distance = textLine.GetDistanceFromCharacterHit(new CharacterHit(cluster));

                        Assert.Equal(currentDistance, distance);

                        currentDistance += advance;
                    }
                }

                Assert.Equal(currentDistance,
                    textLine.GetDistanceFromCharacterHit(new CharacterHit(MultiBufferTextSource.TextRange.Length)));
            }
        }

        [Fact]
        public void Should_Get_CharacterHit_From_Distance()
        {
            using (Start())
            {
                var defaultProperties = new GenericTextRunProperties(Typeface.Default);

                var textSource = new MultiBufferTextSource(defaultProperties);

                var formatter = new TextFormatterImpl();

                var textLine =
                    formatter.FormatLine(textSource, 0, double.PositiveInfinity,
                        new GenericTextParagraphProperties(defaultProperties));

                var currentDistance = 0.0;

                CharacterHit characterHit;

                foreach (var run in textLine.TextRuns)
                {
                    var textRun = (ShapedTextCharacters)run;

                    var glyphRun = textRun.GlyphRun;

                    for (var i = 0; i < glyphRun.GlyphClusters.Length; i++)
                    {
                        var cluster = glyphRun.GlyphClusters[i];

                        var glyph = glyphRun.GlyphIndices[i];

                        var advance = glyphRun.GlyphTypeface.GetGlyphAdvance(glyph) * glyphRun.Scale;

                        characterHit = textLine.GetCharacterHitFromDistance(currentDistance);

                        Assert.Equal(cluster, characterHit.FirstCharacterIndex + characterHit.TrailingLength);

                        currentDistance += advance;
                    }
                }

                characterHit = textLine.GetCharacterHitFromDistance(textLine.LineMetrics.Size.Width);

                Assert.Equal(MultiBufferTextSource.TextRange.End, characterHit.FirstCharacterIndex);
            }
        }

        [InlineData("01234 01234", 8, TextCollapsingStyle.TrailingCharacter, "01234 0\u2026")]
        [InlineData("01234 01234", 8, TextCollapsingStyle.TrailingWord, "01234 \u2026")]
        [Theory]
        public void Should_Collapse_Line(string text, int numberOfCharacters, TextCollapsingStyle style, string expected)
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

                var glyphTypeface = Typeface.Default.GlyphTypeface;

                var scale = defaultProperties.FontRenderingEmSize / glyphTypeface.DesignEmHeight;

                var width = 1.0;

                for (var i = 0; i < numberOfCharacters; i++)
                {
                    var glyph = glyphTypeface.GetGlyph(text[i]);

                    width += glyphTypeface.GetGlyphAdvance(glyph) * scale;
                }

                TextCollapsingProperties collapsingProperties;

                if (style == TextCollapsingStyle.TrailingCharacter)
                {
                    collapsingProperties = new TextTrailingCharacterEllipsis(width, defaultProperties);
                }
                else
                {
                    collapsingProperties = new TextTrailingWordEllipsis(width, defaultProperties);
                }

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
        public void TestNext()
        {
            using (Start())
            {
                var defaultProperties = new GenericTextRunProperties(Typeface.Default);

                var textSource = new SingleBufferTextSource("Text from memory", defaultProperties);

                var formatter = new TextFormatterImpl();

                var textLine =
                    formatter.FormatLine(textSource, 0, double.PositiveInfinity,
                        new GenericTextParagraphProperties(defaultProperties));

                var characterHits = new List<CharacterHit>();

                var currentCharacterHit = new CharacterHit(0);

                characterHits.Add(currentCharacterHit);

                var nextCharacterHit = textLine.GetNextCaretCharacterHit(currentCharacterHit);

                while (nextCharacterHit != currentCharacterHit)
                {
                    currentCharacterHit = nextCharacterHit;

                    characterHits.Add(currentCharacterHit);

                    nextCharacterHit = textLine.GetNextCaretCharacterHit(currentCharacterHit);
                }
            }
        }

        [Fact]
        public void TestPrevious()
        {
            using (Start())
            {
                var defaultProperties = new GenericTextRunProperties(Typeface.Default);

                var text = "Text from memory";

                var textSource = new SingleBufferTextSource(text, defaultProperties);

                var formatter = new TextFormatterImpl();

                var textLine =
                    formatter.FormatLine(textSource, 0, double.PositiveInfinity,
                        new GenericTextParagraphProperties(defaultProperties));

                var characterHits = new List<CharacterHit>();

                var currentCharacterHit = new CharacterHit(text.Length);

                characterHits.Add(currentCharacterHit);

                var nextCharacterHit = textLine.GetPreviousCaretCharacterHit(currentCharacterHit);

                while (nextCharacterHit != currentCharacterHit)
                {
                    currentCharacterHit = nextCharacterHit;

                    characterHits.Add(currentCharacterHit);

                    nextCharacterHit = textLine.GetPreviousCaretCharacterHit(currentCharacterHit);
                }
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
