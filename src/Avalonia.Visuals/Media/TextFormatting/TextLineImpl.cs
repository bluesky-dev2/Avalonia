﻿using System;
using System.Collections.Generic;
using Avalonia.Media.TextFormatting.Unicode;
using Avalonia.Utilities;

namespace Avalonia.Media.TextFormatting
{
    internal class TextLineImpl : TextLine
    {
        private static readonly Comparer<int> s_compareStart = Comparer<int>.Default;

        private static readonly Comparison<DrawableTextRun> s_compareLogicalOrder =
            (a, b) =>
            {
                if (a is ShapedTextCharacters && b is ShapedTextCharacters)
                {
                    return s_compareStart.Compare(a.Text.Start, b.Text.Start);
                }

                return 0;
            };

        private readonly List<DrawableTextRun> _textRuns;
        private readonly double _paragraphWidth;
        private readonly TextParagraphProperties _paragraphProperties;
        private TextLineMetrics _textLineMetrics;
        private readonly FlowDirection _flowDirection;

        public TextLineImpl(List<DrawableTextRun> textRuns, TextRange textRange, double paragraphWidth,
            TextParagraphProperties paragraphProperties, FlowDirection flowDirection = FlowDirection.LeftToRight,
            TextLineBreak? lineBreak = null, bool hasCollapsed = false)
        {
            TextRange = textRange;
            TextLineBreak = lineBreak;
            HasCollapsed = hasCollapsed;

            _textRuns = textRuns;
            _paragraphWidth = paragraphWidth;
            _paragraphProperties = paragraphProperties;

            _flowDirection = flowDirection;
        }

        /// <inheritdoc/>
        public override IReadOnlyList<TextRun> TextRuns => _textRuns;

        /// <inheritdoc/>
        public override TextRange TextRange { get; }

        /// <inheritdoc/>
        public override TextLineBreak? TextLineBreak { get; }

        /// <inheritdoc/>
        public override bool HasCollapsed { get; }

        /// <inheritdoc/>
        public override bool HasOverflowed => _textLineMetrics.HasOverflowed;

        /// <inheritdoc/>
        public override double Baseline => _textLineMetrics.TextBaseline;

        /// <inheritdoc/>
        public override double Extent => _textLineMetrics.Height;

        /// <inheritdoc/>
        public override double Height => _textLineMetrics.Height;

        /// <inheritdoc/>
        public override int NewLineLength => _textLineMetrics.NewLineLength;

        /// <inheritdoc/>
        public override double OverhangAfter => 0;

        /// <inheritdoc/>
        public override double OverhangLeading => 0;

        /// <inheritdoc/>
        public override double OverhangTrailing => 0;

        /// <inheritdoc/>
        public override int TrailingWhitespaceLength => _textLineMetrics.TrailingWhitespaceLength;

        /// <inheritdoc/>
        public override double Start => _textLineMetrics.Start;

        /// <inheritdoc/>
        public override double Width => _textLineMetrics.Width;

        /// <inheritdoc/>
        public override double WidthIncludingTrailingWhitespace => _textLineMetrics.WidthIncludingTrailingWhitespace;

        /// <inheritdoc/>
        public override void Draw(DrawingContext drawingContext, Point lineOrigin)
        {
            var (currentX, currentY) = lineOrigin;

            foreach (var textRun in _textRuns)
            {
                var offsetY = GetBaselineOffset(this, textRun);

                textRun.Draw(drawingContext, new Point(currentX, currentY + offsetY));

                currentX += textRun.Size.Width;
            }
        }

        private static double GetBaselineOffset(TextLine textLine, DrawableTextRun textRun)
        {
            var baseline = textRun.Baseline;
            var baselineAlignment = textRun.Properties?.BaselineAlignment;

            switch (baselineAlignment)
            {
                case BaselineAlignment.Top:
                    return 0;
                case BaselineAlignment.Center:
                    return textLine.Height / 2 - textRun.Size.Height / 2;
                case BaselineAlignment.Bottom:
                    return textLine.Height - textRun.Size.Height;
                case BaselineAlignment.Baseline:
                case BaselineAlignment.TextTop:
                case BaselineAlignment.TextBottom:
                case BaselineAlignment.Subscript:
                case BaselineAlignment.Superscript:
                    return textLine.Baseline - baseline;
                default:
                    throw new ArgumentOutOfRangeException(nameof(baselineAlignment), baselineAlignment, null);
            }
        }

        /// <inheritdoc/>
        public override TextLine Collapse(params TextCollapsingProperties[] collapsingPropertiesList)
        {
            if (collapsingPropertiesList.Length == 0)
            {
                return this;
            }

            var collapsingProperties = collapsingPropertiesList[0];

            var collapsedRuns = collapsingProperties.Collapse(this);

            if (collapsedRuns is null)
            {
                return this;
            }

            var collapsedLine = new TextLineImpl(collapsedRuns, TextRange, _paragraphWidth, _paragraphProperties,
                _flowDirection, TextLineBreak, true);

            if (collapsedRuns.Count > 0)
            {
                collapsedLine.FinalizeLine();
            }

            return collapsedLine;

        }

        /// <inheritdoc/>
        public override CharacterHit GetCharacterHitFromDistance(double distance)
        {
            distance -= Start;

            if (distance <= 0)
            {
                if (_textRuns.Count == 0)
                {
                    return _flowDirection == FlowDirection.LeftToRight ?
                        new CharacterHit(TextRange.Start) :
                        new CharacterHit(TextRange.Start, TextRange.Length);
                }

                // hit happens before the line, return the first position
                var firstRun = _textRuns[0];

                if (firstRun is ShapedTextCharacters shapedTextCharacters)
                {
                    return shapedTextCharacters.GlyphRun.GetCharacterHitFromDistance(distance, out _);
                }

                return _flowDirection == FlowDirection.LeftToRight ?
                    new CharacterHit(TextRange.Start) :
                    new CharacterHit(TextRange.Start + TextRange.Length);
            }

            // process hit that happens within the line
            var characterHit = new CharacterHit();

            foreach (var currentRun in _textRuns)
            {
                switch (currentRun)
                {
                    case ShapedTextCharacters shapedRun:
                        {
                            characterHit = shapedRun.GlyphRun.GetCharacterHitFromDistance(distance, out _);
                            break;
                        }
                    default:
                        {
                            if(distance < currentRun.Size.Width / 2)
                            {
                                characterHit = new CharacterHit(currentRun.Text.Start);
                            }
                            else
                            {
                                characterHit = new CharacterHit(currentRun.Text.Start, currentRun.Text.Length);
                            }
                            break;
                        }
                }

                if (distance <= currentRun.Size.Width)
                {
                    break;
                }

                distance -= currentRun.Size.Width;
            }

            return characterHit;
        }

        /// <inheritdoc/>
        public override double GetDistanceFromCharacterHit(CharacterHit characterHit)
        {
            var characterIndex = characterHit.FirstCharacterIndex + (characterHit.TrailingLength != 0 ? 1 : 0);

            var currentDistance = Start;

            GlyphRun? lastRun = null;

            for (var index = 0; index < _textRuns.Count; index++)
            {
                var textRun = _textRuns[index];

                switch (textRun)
                {
                    case ShapedTextCharacters shapedTextCharacters:
                        {
                            var currentRun = shapedTextCharacters.GlyphRun;

                            if (lastRun != null)
                            {
                                if (!lastRun.IsLeftToRight && currentRun.IsLeftToRight &&
                                    currentRun.Characters.Start == characterHit.FirstCharacterIndex &&
                                    characterHit.TrailingLength == 0)
                                {
                                    return currentDistance;
                                }
                            }

                            //Look for a hit in within the current run
                            if (characterIndex >= textRun.Text.Start && characterIndex <= textRun.Text.End)
                            {
                                var distance = currentRun.GetDistanceFromCharacterHit(characterHit);

                                return currentDistance + distance;
                            }

                            //Look at the left and right edge of the current run
                            if (currentRun.IsLeftToRight)
                            {
                                if (lastRun == null || lastRun.IsLeftToRight)
                                {
                                    if (characterIndex <= textRun.Text.Start)
                                    {
                                        return currentDistance;
                                    }
                                }
                                else
                                {
                                    if (characterIndex == textRun.Text.Start)
                                    {
                                        return currentDistance;
                                    }
                                }

                                if (characterIndex == textRun.Text.Start + textRun.Text.Length &&
                                    characterHit.TrailingLength > 0)
                                {
                                    return currentDistance + currentRun.Size.Width;
                                }
                            }
                            else
                            {
                                if (characterIndex == textRun.Text.Start)
                                {
                                    return currentDistance + currentRun.Size.Width;
                                }

                                var nextRun = index + 1 < _textRuns.Count ?
                                    _textRuns[index + 1] as ShapedTextCharacters :
                                    null;

                                if (nextRun != null)
                                {
                                    if (characterHit.FirstCharacterIndex == textRun.Text.End &&
                                        nextRun.ShapedBuffer.IsLeftToRight)
                                    {
                                        return currentDistance;
                                    }

                                    if (characterIndex > textRun.Text.End && nextRun.Text.End < textRun.Text.End)
                                    {
                                        return currentDistance;
                                    }
                                }
                                else
                                {
                                    if (characterIndex > textRun.Text.End)
                                    {
                                        return currentDistance;
                                    }
                                }
                            }

                            lastRun = currentRun;

                            break;
                        }
                    default:
                        {
                            if(characterIndex == textRun.Text.Start)
                            {
                                return currentDistance;
                            }

                            break;
                        }
                }

                //No hit hit found so we add the full width
                currentDistance += textRun.Size.Width;
            }

            return currentDistance;
        }

        /// <inheritdoc/>
        public override CharacterHit GetNextCaretCharacterHit(CharacterHit characterHit)
        {
            if (_textRuns.Count == 0)
            {
                var textPosition = TextRange.Start + TextRange.Length;

                return characterHit.FirstCharacterIndex + characterHit.TrailingLength == textPosition ?
                    characterHit :
                    new CharacterHit(textPosition);
            }

            if (TryFindNextCharacterHit(characterHit, out var nextCharacterHit))
            {
                return nextCharacterHit;
            }

            // Can't move, we're after the last character
            var runIndex = GetRunIndexAtCharacterIndex(TextRange.End, LogicalDirection.Forward);

            var currentRun = _textRuns[runIndex];

            switch (currentRun)
            {
                case ShapedTextCharacters shapedRun:
                    {
                        characterHit = shapedRun.GlyphRun.GetNextCaretCharacterHit(characterHit);
                        break;
                    }
                default:
                    {
                        characterHit = new CharacterHit(currentRun.Text.Start, currentRun.Text.Length);
                        break;
                    }
            }

            return characterHit;
        }

        /// <inheritdoc/>
        public override CharacterHit GetPreviousCaretCharacterHit(CharacterHit characterHit)
        {
            if (TryFindPreviousCharacterHit(characterHit, out var previousCharacterHit))
            {
                return previousCharacterHit;
            }

            if (characterHit.FirstCharacterIndex <= TextRange.Start)
            {
                characterHit = new CharacterHit(TextRange.Start);
            }

            return characterHit; // Can't move, we're before the first character
        }

        /// <inheritdoc/>
        public override CharacterHit GetBackspaceCaretCharacterHit(CharacterHit characterHit)
        {
            // same operation as move-to-previous
            return GetPreviousCaretCharacterHit(characterHit);
        }

        public override IReadOnlyList<TextBounds> GetTextBounds(int firstTextSourceCharacterIndex, int textLength)
        {
            if (firstTextSourceCharacterIndex + textLength <= TextRange.Start)
            {
                return Array.Empty<TextBounds>();
            }

            var result = new List<TextBounds>(TextRuns.Count);
            var lastDirection = _flowDirection;
            var currentDirection = lastDirection;
            var currentPosition = 0;
            var currentRect = Rect.Empty;
            var startX = Start;

            //A portion of the line is covered.
            for (var index = 0; index < TextRuns.Count; index++)
            {
                var currentRun = TextRuns[index] as DrawableTextRun;

                if (currentRun is null)
                {
                    continue;
                }

                TextRun? nextRun = null;

                if (index + 1 < TextRuns.Count)
                {
                    nextRun = TextRuns[index + 1];
                }

                if (nextRun != null)
                {
                    if (nextRun.Text.Start < currentRun.Text.Start && firstTextSourceCharacterIndex + textLength < currentRun.Text.End)
                    {
                        goto skip;
                    }

                    if (currentRun.Text.Start >= firstTextSourceCharacterIndex + textLength)
                    {
                        goto skip;
                    }

                    if (currentRun.Text.Start > nextRun.Text.Start && currentRun.Text.Start < firstTextSourceCharacterIndex)
                    {
                        goto skip;
                    }

                    if (currentRun.Text.End < firstTextSourceCharacterIndex)
                    {
                        goto skip;
                    }

                    goto noop;

                skip:
                    {
                        startX += currentRun.Size.Width;
                    }

                    continue;

                noop:
                    {
                    }
                }


                var endX = startX;
                var endOffset = 0d;

                switch (currentRun)
                {
                    case ShapedTextCharacters shapedRun:
                        {
                            endOffset = shapedRun.GlyphRun.GetDistanceFromCharacterHit(
                                shapedRun.ShapedBuffer.IsLeftToRight ?
                                    new CharacterHit(firstTextSourceCharacterIndex + textLength) :
                                    new CharacterHit(firstTextSourceCharacterIndex));

                            endX += endOffset;

                            var startOffset = shapedRun.GlyphRun.GetDistanceFromCharacterHit(
                                shapedRun.ShapedBuffer.IsLeftToRight ?
                                    new CharacterHit(firstTextSourceCharacterIndex) :
                                    new CharacterHit(firstTextSourceCharacterIndex + textLength));

                            startX += startOffset;

                            var characterHit = shapedRun.GlyphRun.IsLeftToRight ?
                                shapedRun.GlyphRun.GetCharacterHitFromDistance(endOffset, out _) :
                                shapedRun.GlyphRun.GetCharacterHitFromDistance(startOffset, out _);

                            currentPosition = characterHit.FirstCharacterIndex + characterHit.TrailingLength;

                            currentDirection = shapedRun.ShapedBuffer.IsLeftToRight ?
                                FlowDirection.LeftToRight :
                                FlowDirection.RightToLeft;

                            if (nextRun is ShapedTextCharacters nextShaped)
                            {
                                if (shapedRun.ShapedBuffer.IsLeftToRight == nextShaped.ShapedBuffer.IsLeftToRight)
                                {
                                    endOffset = nextShaped.GlyphRun.GetDistanceFromCharacterHit(
                                        nextShaped.ShapedBuffer.IsLeftToRight ?
                                            new CharacterHit(firstTextSourceCharacterIndex + textLength) :
                                            new CharacterHit(firstTextSourceCharacterIndex));

                                    index++;

                                    endX += endOffset;

                                    currentRun = nextShaped;

                                    if (nextShaped.ShapedBuffer.IsLeftToRight)
                                    {
                                        characterHit = nextShaped.GlyphRun.GetCharacterHitFromDistance(endOffset, out _);

                                        currentPosition = characterHit.FirstCharacterIndex + characterHit.TrailingLength;
                                    }
                                }
                            }

                            break;
                        }
                    default:
                        {
                            if (firstTextSourceCharacterIndex + textLength >= currentRun.Text.Start + currentRun.Text.Length)
                            {
                                endX += currentRun.Size.Width;
                            }

                            break;
                        }
                }

                if (endX < startX)
                {
                    (endX, startX) = (startX, endX);
                }

                var width = endX - startX;

                if (lastDirection == currentDirection && result.Count > 0 && MathUtilities.AreClose(currentRect.Right, startX))
                {
                    var textBounds = new TextBounds(currentRect.WithWidth(currentRect.Width + width), currentDirection);

                    result[result.Count - 1] = textBounds;
                }
                else
                {
                    currentRect = new Rect(startX, 0, width, Height);

                    result.Add(new TextBounds(currentRect, currentDirection));
                }

                if (currentDirection == FlowDirection.LeftToRight)
                {
                    if (nextRun != null)
                    {
                        if (nextRun.Text.Start > currentRun.Text.Start && nextRun.Text.Start >= firstTextSourceCharacterIndex + textLength)
                        {
                            break;
                        }

                        currentPosition = nextRun.Text.End;
                    }
                    else
                    {
                        if (currentPosition >= firstTextSourceCharacterIndex + textLength)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    if (currentPosition <= firstTextSourceCharacterIndex)
                    {
                        break;
                    }

                    if (currentPosition != currentRun.Text.Start)
                    {
                        endX += currentRun.Size.Width - endOffset;
                    }
                }

                lastDirection = currentDirection;
                startX = endX;
            }

            return result;
        }

        public static void SortRuns(List<DrawableTextRun> textRuns)
        {
            textRuns.Sort(s_compareLogicalOrder);
        }

        public TextLineImpl FinalizeLine()
        {
            BidiReorder();

            _textLineMetrics = CreateLineMetrics();

            return this;
        }

        private static sbyte GetRunBidiLevel(DrawableTextRun run, FlowDirection flowDirection)
        {
            if (run is ShapedTextCharacters shapedTextCharacters)
            {
                return shapedTextCharacters.BidiLevel;
            }

            var defaultLevel = flowDirection == FlowDirection.LeftToRight ? 0 : 1;

            return (sbyte)defaultLevel;
        }

        private void BidiReorder()
        {
            if (_textRuns.Count == 0)
            {
                return;
            }

            // Build up the collection of ordered runs.
            var run = _textRuns[0];

            OrderedBidiRun orderedRun = new(run, GetRunBidiLevel(run, _flowDirection));

            var current = orderedRun;

            for (var i = 1; i < _textRuns.Count; i++)
            {
                run = _textRuns[i];

                current.Next = new OrderedBidiRun(run, GetRunBidiLevel(run, _flowDirection));

                current = current.Next;
            }

            // Reorder them into visual order.
            orderedRun = LinearReOrder(orderedRun);

            // Now perform a recursive reversal of each run.
            // From the highest level found in the text to the lowest odd level on each line, including intermediate levels
            // not actually present in the text, reverse any contiguous sequence of characters that are at that level or higher.
            // https://unicode.org/reports/tr9/#L2
            sbyte max = 0;
            var min = sbyte.MaxValue;

            for (var i = 0; i < _textRuns.Count; i++)
            {
                var currentRun = _textRuns[i];

                var level = GetRunBidiLevel(currentRun, _flowDirection);

                if (level > max)
                {
                    max = level;
                }

                if ((level & 1) != 0 && level < min)
                {
                    min = level;
                }
            }

            if (min > max)
            {
                min = max;
            }

            if (max == 0 || (min == max && (max & 1) == 0))
            {
                // Nothing to reverse.
                return;
            }

            // Now apply the reversal and replace the original contents.
            var minLevelToReverse = max;

            while (minLevelToReverse >= min)
            {
                current = orderedRun;

                while (current != null)
                {
                    if (current.Level >= minLevelToReverse && current.Level % 2 != 0)
                    {
                        if (current.Run is ShapedTextCharacters { IsReversed: false } shapedTextCharacters)
                        {
                            shapedTextCharacters.Reverse();
                        }
                    }

                    current = current.Next;
                }

                minLevelToReverse--;
            }

            _textRuns.Clear();

            current = orderedRun;

            while (current != null)
            {
                _textRuns.Add(current.Run);

                current = current.Next;
            }
        }

        /// <summary>
        /// Reorders a series of runs from logical to visual order, returning the left most run.
        /// <see href="https://github.com/fribidi/linear-reorder/blob/f2f872257d4d8b8e137fcf831f254d6d4db79d3c/linear-reorder.c"/>
        /// </summary>
        /// <param name="run">The ordered bidi run.</param>
        /// <returns>The <see cref="OrderedBidiRun"/>.</returns>
        private static OrderedBidiRun LinearReOrder(OrderedBidiRun? run)
        {
            BidiRange? range = null;

            while (run != null)
            {
                var next = run.Next;

                while (range != null && range.Level > run.Level
                    && range.Previous != null && range.Previous.Level >= run.Level)
                {
                    range = BidiRange.MergeWithPrevious(range);
                }

                if (range != null && range.Level >= run.Level)
                {
                    // Attach run to the range.
                    if ((run.Level & 1) != 0)
                    {
                        // Odd, range goes to the right of run.
                        run.Next = range.Left;
                        range.Left = run;
                    }
                    else
                    {
                        // Even, range goes to the left of run.
                        range.Right!.Next = run;
                        range.Right = run;
                    }

                    range.Level = run.Level;
                }
                else
                {
                    var r = new BidiRange();

                    r.Left = r.Right = run;
                    r.Level = run.Level;
                    r.Previous = range;

                    range = r;
                }

                run = next;
            }

            while (range?.Previous != null)
            {
                range = BidiRange.MergeWithPrevious(range);
            }

            // Terminate.
            range!.Right!.Next = null;

            return range.Left!;
        }

        /// <summary>
        /// Tries to find the next character hit.
        /// </summary>
        /// <param name="characterHit">The current character hit.</param>
        /// <param name="nextCharacterHit">The next character hit.</param>
        /// <returns></returns>
        private bool TryFindNextCharacterHit(CharacterHit characterHit, out CharacterHit nextCharacterHit)
        {
            nextCharacterHit = characterHit;

            var codepointIndex = characterHit.FirstCharacterIndex + characterHit.TrailingLength;

            if (codepointIndex >= TextRange.End)
            {
                return false; // Cannot go forward anymore
            }

            if (codepointIndex < TextRange.Start)
            {
                codepointIndex = TextRange.Start;
            }

            var runIndex = GetRunIndexAtCharacterIndex(codepointIndex, LogicalDirection.Forward);

            while (runIndex < _textRuns.Count)
            {
                var currentRun = _textRuns[runIndex];

                switch (currentRun)
                {
                    case ShapedTextCharacters shapedRun:
                        {
                            var foundCharacterHit = shapedRun.GlyphRun.FindNearestCharacterHit(characterHit.FirstCharacterIndex + characterHit.TrailingLength, out _);

                            var isAtEnd = foundCharacterHit.FirstCharacterIndex + foundCharacterHit.TrailingLength ==
                                          TextRange.Start + TextRange.Length;

                            if (isAtEnd && !shapedRun.GlyphRun.IsLeftToRight)
                            {
                                nextCharacterHit = foundCharacterHit;

                                return true;
                            }

                            var characterIndex = codepointIndex - shapedRun.Text.Start;

                            if (characterIndex < 0 && shapedRun.ShapedBuffer.IsLeftToRight)
                            {
                                foundCharacterHit = new CharacterHit(foundCharacterHit.FirstCharacterIndex);
                            }

                            nextCharacterHit = isAtEnd || characterHit.TrailingLength != 0 ?
                                foundCharacterHit :
                                new CharacterHit(foundCharacterHit.FirstCharacterIndex + foundCharacterHit.TrailingLength);

                            if (isAtEnd || nextCharacterHit.FirstCharacterIndex > characterHit.FirstCharacterIndex)
                            {
                                return true;
                            }

                            break;
                        }
                    default:
                        {
                            var textPosition = characterHit.FirstCharacterIndex + characterHit.TrailingLength;

                            if (textPosition == currentRun.Text.Start)
                            {
                                nextCharacterHit = new CharacterHit(currentRun.Text.Start + currentRun.Text.Length);

                                return true;
                            }

                            break;
                        }
                }

                runIndex++;
            }

            return false;
        }

        /// <summary>
        /// Tries to find the previous character hit.
        /// </summary>
        /// <param name="characterHit">The current character hit.</param>
        /// <param name="previousCharacterHit">The previous character hit.</param>
        /// <returns></returns>
        private bool TryFindPreviousCharacterHit(CharacterHit characterHit, out CharacterHit previousCharacterHit)
        {
            var characterIndex = characterHit.FirstCharacterIndex + characterHit.TrailingLength;

            if (characterIndex == TextRange.Start)
            {
                previousCharacterHit = new CharacterHit(TextRange.Start);

                return true;
            }

            previousCharacterHit = characterHit;

            if (characterIndex < TextRange.Start)
            {
                return false; // Cannot go backward anymore.
            }

            var runIndex = GetRunIndexAtCharacterIndex(characterIndex, LogicalDirection.Backward);

            while (runIndex >= 0)
            {
                var currentRun = _textRuns[runIndex];

                switch (currentRun)
                {
                    case ShapedTextCharacters shapedRun:
                        {
                            var foundCharacterHit = shapedRun.GlyphRun.FindNearestCharacterHit(characterHit.FirstCharacterIndex - 1, out _);

                            if (foundCharacterHit.FirstCharacterIndex + foundCharacterHit.TrailingLength < characterIndex)
                            {
                                previousCharacterHit = foundCharacterHit;

                                return true;
                            }

                            var previousPosition = foundCharacterHit.FirstCharacterIndex + foundCharacterHit.TrailingLength;

                            if(foundCharacterHit.TrailingLength > 0 && previousPosition == characterIndex)
                            {
                                previousCharacterHit = new CharacterHit(foundCharacterHit.FirstCharacterIndex);
                            }

                            if (previousCharacterHit != characterHit)
                            {
                                return true;
                            }

                            break;
                        }
                    default:
                        {
                            if(characterIndex == currentRun.Text.Start + currentRun.Text.Length)
                            {
                                previousCharacterHit = new CharacterHit(currentRun.Text.Start);

                                return true;
                            }

                            break;
                        }
                }

                runIndex--;
            }

            return false;
        }

        /// <summary>
        /// Gets the run index of the specified codepoint index.
        /// </summary>
        /// <param name="codepointIndex">The codepoint index.</param>
        /// <param name="direction">The logical direction.</param>
        /// <returns>The text run index.</returns>
        private int GetRunIndexAtCharacterIndex(int codepointIndex, LogicalDirection direction)
        {
            var runIndex = 0;
            DrawableTextRun? previousRun = null;

            while (runIndex < _textRuns.Count)
            {
                var currentRun = _textRuns[runIndex];

                switch (currentRun)
                {
                    case ShapedTextCharacters shapedRun:
                        {
                            if (previousRun is ShapedTextCharacters previousShaped && !previousShaped.ShapedBuffer.IsLeftToRight)
                            {
                                if (shapedRun.ShapedBuffer.IsLeftToRight)
                                {
                                    if (currentRun.Text.Start >= codepointIndex)
                                    {
                                        return --runIndex;
                                    }
                                }
                                else
                                {
                                    if (codepointIndex > currentRun.Text.Start + currentRun.Text.Length)
                                    {
                                        return --runIndex;
                                    }
                                }
                            }

                            if (direction == LogicalDirection.Forward)
                            {
                                if (codepointIndex >= currentRun.Text.Start && codepointIndex <= currentRun.Text.End)
                                {
                                    return runIndex;
                                }
                            }
                            else
                            {
                                if (codepointIndex > currentRun.Text.Start &&
                                    codepointIndex <= currentRun.Text.Start + currentRun.Text.Length)
                                {
                                    return runIndex;
                                }
                            }

                            if (runIndex + 1 < _textRuns.Count)
                            {
                                runIndex++;
                                previousRun = currentRun;
                            }
                            else
                            {
                                break;
                            }
                            break;
                        }

                    default:
                        {
                            if (codepointIndex == currentRun.Text.Start)
                            {
                                return runIndex;
                            }

                            if (runIndex + 1 < _textRuns.Count)
                            {
                                runIndex++;
                                previousRun = currentRun;
                            }
                            else
                            {
                                return runIndex;
                            }

                            break;
                        }
                }
            }

            return runIndex;
        }

        private TextLineMetrics CreateLineMetrics()
        {
            var start = 0d;
            var height = 0d;
            var width = 0d;
            var widthIncludingWhitespace = 0d;
            var trailingWhitespaceLength = 0;
            var newLineLength = 0;
            var ascent = 0d;
            var descent = 0d;
            var lineGap = 0d;
            var fontRenderingEmSize = 0d;

            var lineHeight = _paragraphProperties.LineHeight;

            if (_textRuns.Count == 0)
            {
                var glyphTypeface = _paragraphProperties.DefaultTextRunProperties.Typeface.GlyphTypeface;
                fontRenderingEmSize = _paragraphProperties.DefaultTextRunProperties.FontRenderingEmSize;
                var scale = fontRenderingEmSize / glyphTypeface.DesignEmHeight;
                ascent = glyphTypeface.Ascent * scale;
                height = double.IsNaN(lineHeight) || MathUtilities.IsZero(lineHeight) ?
                descent - ascent + lineGap :
                lineHeight;

                return new TextLineMetrics(false, height, 0, start, -ascent, 0, 0, 0);
            }

            for (var index = 0; index < _textRuns.Count; index++)
            {
                switch (_textRuns[index])
                {
                    case ShapedTextCharacters textRun:
                        {
                            var fontMetrics =
                                new FontMetrics(textRun.Properties.Typeface, textRun.Properties.FontRenderingEmSize);

                            if (fontRenderingEmSize < textRun.Properties.FontRenderingEmSize)
                            {
                                fontRenderingEmSize = textRun.Properties.FontRenderingEmSize;

                                if (ascent > fontMetrics.Ascent)
                                {
                                    ascent = fontMetrics.Ascent;
                                }

                                if (descent < fontMetrics.Descent)
                                {
                                    descent = fontMetrics.Descent;
                                }

                                if (lineGap < fontMetrics.LineGap)
                                {
                                    lineGap = fontMetrics.LineGap;
                                }
                            }

                            switch (_paragraphProperties.FlowDirection)
                            {
                                case FlowDirection.LeftToRight:
                                    {
                                        if (index == _textRuns.Count - 1)
                                        {
                                            width = widthIncludingWhitespace + textRun.GlyphRun.Metrics.Width;
                                            trailingWhitespaceLength = textRun.GlyphRun.Metrics.TrailingWhitespaceLength;
                                            newLineLength = textRun.GlyphRun.Metrics.NewlineLength;
                                        }

                                        break;
                                    }

                                case FlowDirection.RightToLeft:
                                    {
                                        if (index == _textRuns.Count - 1)
                                        {
                                            var firstRun = _textRuns[0];

                                            if (firstRun is ShapedTextCharacters shapedTextCharacters)
                                            {
                                                var offset = shapedTextCharacters.GlyphRun.Metrics.WidthIncludingTrailingWhitespace -
                                                             shapedTextCharacters.GlyphRun.Metrics.Width;

                                                width = widthIncludingWhitespace +
                                                    textRun.GlyphRun.Metrics.WidthIncludingTrailingWhitespace - offset;

                                                trailingWhitespaceLength = shapedTextCharacters.GlyphRun.Metrics.TrailingWhitespaceLength;
                                                newLineLength = shapedTextCharacters.GlyphRun.Metrics.NewlineLength;
                                            }
                                        }

                                        break;
                                    }
                            }

                            widthIncludingWhitespace += textRun.GlyphRun.Metrics.WidthIncludingTrailingWhitespace;

                            break;
                        }

                    case { } drawableTextRun:
                        {
                            widthIncludingWhitespace += drawableTextRun.Size.Width;

                            switch (_paragraphProperties.FlowDirection)
                            {
                                case FlowDirection.LeftToRight:
                                    {
                                        if (index == _textRuns.Count - 1)
                                        {
                                            width = widthIncludingWhitespace;
                                            trailingWhitespaceLength = 0;
                                            newLineLength = 0;
                                        }

                                        break;
                                    }

                                case FlowDirection.RightToLeft:
                                    {
                                        if (index == _textRuns.Count - 1)
                                        {
                                            width = widthIncludingWhitespace;
                                            trailingWhitespaceLength = 0;
                                            newLineLength = 0;
                                        }

                                        break;
                                    }
                            }

                            if (ascent > -drawableTextRun.Size.Height)
                            {
                                ascent = -drawableTextRun.Size.Height;
                            }

                            break;
                        }
                }
            }

            start = GetParagraphOffsetX(width, widthIncludingWhitespace, _paragraphWidth,
                _paragraphProperties.TextAlignment, _paragraphProperties.FlowDirection);

            height = double.IsNaN(lineHeight) || MathUtilities.IsZero(lineHeight) ?
                descent - ascent + lineGap :
                lineHeight;

            return new TextLineMetrics(widthIncludingWhitespace > _paragraphWidth, height, newLineLength, start,
                -ascent, trailingWhitespaceLength, width, widthIncludingWhitespace);
        }

        private sealed class OrderedBidiRun
        {
            public OrderedBidiRun(DrawableTextRun run, sbyte level)
            {
                Run = run;
                Level = level;
            }

            public sbyte Level { get; }

            public DrawableTextRun Run { get; }

            public OrderedBidiRun? Next { get; set; }
        }

        private sealed class BidiRange
        {
            public int Level { get; set; }

            public OrderedBidiRun? Left { get; set; }

            public OrderedBidiRun? Right { get; set; }

            public BidiRange? Previous { get; set; }

            public static BidiRange MergeWithPrevious(BidiRange range)
            {
                var previous = range.Previous;

                BidiRange left;
                BidiRange right;

                if ((previous!.Level & 1) != 0)
                {
                    // Odd, previous goes to the right of range.
                    left = range;
                    right = previous;
                }
                else
                {
                    // Even, previous goes to the left of range.
                    left = previous;
                    right = range;
                }

                // Stitch them
                left.Right!.Next = right.Left;
                previous.Left = left.Left;
                previous.Right = right.Right;

                return previous;
            }
        }
    }
}
