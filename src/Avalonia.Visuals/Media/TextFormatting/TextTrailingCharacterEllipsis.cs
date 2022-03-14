﻿using System.Collections.Generic;
using Avalonia.Utilities;

namespace Avalonia.Media.TextFormatting
{
    /// <summary>
    /// A collapsing properties to collapse whole line toward the end
    /// at character granularity and with ellipsis being the collapsing symbol
    /// </summary>
    public class TextTrailingCharacterEllipsis : TextCollapsingProperties
    {
        /// <summary>
        /// Construct a text trailing character ellipsis collapsing properties
        /// </summary>
        /// <param name="width">width in which collapsing is constrained to</param>
        /// <param name="textRunProperties">text run properties of ellispis symbol</param>
        public TextTrailingCharacterEllipsis(ReadOnlySlice<char> ellipsis, double width, TextRunProperties textRunProperties)
        {
            Width = width;
            Symbol = new TextCharacters(ellipsis, textRunProperties);
        }

        /// <inheritdoc/>
        public sealed override double Width { get; }

        /// <inheritdoc/>
        public sealed override TextRun Symbol { get; }

        public override IReadOnlyList<TextRun>? Collapse(TextLine textLine, FlowDirection flowDirection)
        {
            return TextEllipsisHelper.Collapse(textLine, flowDirection, this, false);
        }
    }
}
