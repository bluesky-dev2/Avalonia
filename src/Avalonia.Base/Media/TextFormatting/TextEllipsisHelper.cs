﻿using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Media.TextFormatting.Unicode;
using Avalonia.Utilities;

namespace Avalonia.Media.TextFormatting
{
    internal static class TextEllipsisHelper
    {
        public static TextRun[]? Collapse(TextLine textLine, TextCollapsingProperties properties, bool isWordEllipsis)
        {
            var textRuns = textLine.TextRuns;

            if (textRuns.Count == 0)
            {
                return null;
            }

            var runIndex = 0;
            var currentWidth = 0.0;
            var collapsedLength = 0;
            var shapedSymbol = TextFormatterImpl.CreateSymbol(properties.Symbol, FlowDirection.LeftToRight);

            if (properties.Width < shapedSymbol.GlyphRun.Size.Width)
            {
                //Not enough space to fit in the symbol
                return Array.Empty<TextRun>();
            }

            var availableWidth = properties.Width - shapedSymbol.Size.Width;

            while (runIndex < textRuns.Count)
            {
                var currentRun = textRuns[runIndex];

                switch (currentRun)
                {
                    case ShapedTextRun shapedRun:
                        {
                            currentWidth += shapedRun.Size.Width;

                            if (currentWidth > availableWidth)
                            {
                                if (shapedRun.TryMeasureCharacters(availableWidth, out var measuredLength))
                                {
                                    if (isWordEllipsis && measuredLength < textLine.Length)
                                    {
                                        var currentBreakPosition = 0;

                                        var lineBreaker = new LineBreakEnumerator(currentRun.Text.Span);

                                        while (currentBreakPosition < measuredLength && lineBreaker.MoveNext())
                                        {
                                            var nextBreakPosition = lineBreaker.Current.PositionMeasure;

                                            if (nextBreakPosition == 0)
                                            {
                                                break;
                                            }

                                            if (nextBreakPosition >= measuredLength)
                                            {
                                                break;
                                            }

                                            currentBreakPosition = nextBreakPosition;
                                        }

                                        measuredLength = currentBreakPosition;
                                    }
                                }

                                collapsedLength += measuredLength;

                                return CreateCollapsedRuns(textRuns, collapsedLength, shapedSymbol);
                            }

                            availableWidth -= shapedRun.Size.Width;

                            break;
                        }

                    case DrawableTextRun drawableRun:
                        {
                            //The whole run needs to fit into available space
                            if (currentWidth + drawableRun.Size.Width > availableWidth)
                            {
                                return CreateCollapsedRuns(textRuns, collapsedLength, shapedSymbol);
                            }

                            availableWidth -= drawableRun.Size.Width;

                            break;
                        }
                }

                collapsedLength += currentRun.Length;

                runIndex++;
            }

            return null;
        }

        private static TextRun[] CreateCollapsedRuns(IReadOnlyList<TextRun> textRuns, int collapsedLength,
            TextRun shapedSymbol)
        {
            if (collapsedLength <= 0)
            {
                return new[] { shapedSymbol };
            }

            // perf note: the runs are very likely to come from TextLineImpl
            // which already uses an array: ToArray() won't ever be called in this case
            var textRunArray = textRuns as TextRun[] ?? textRuns.ToArray();

            var (preSplitRuns, _) = TextFormatterImpl.SplitTextRuns(textRunArray, collapsedLength);

            var collapsedRuns = new TextRun[preSplitRuns.Count + 1];

            for (var i = 0; i < preSplitRuns.Count; ++i)
            {
                collapsedRuns[i] = preSplitRuns[i];
            }

            collapsedRuns[collapsedRuns.Length - 1] = shapedSymbol;
            return collapsedRuns;
        }
    }
}
