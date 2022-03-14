﻿using Avalonia.Media.TextFormatting;
using Avalonia.Utilities;

namespace Avalonia.Media
{
    public class TextLeadingPrefixTrimming : TextTrimming
    {
        private readonly ReadOnlySlice<char> _ellipsis;
        private readonly int _prefixLength;

        public TextLeadingPrefixTrimming(char ellipsis, int prefixLength) : this(new[] { ellipsis }, prefixLength)
        {
        }

        public TextLeadingPrefixTrimming(char[] ellipsis, int prefixLength)
        {
            _prefixLength = prefixLength;
            _ellipsis = new ReadOnlySlice<char>(ellipsis);
        }

        public override TextCollapsingProperties CreateCollapsingProperties(TextCollapsingCreateInfo createInfo)
        {
            return new TextLeadingPrefixEllipsis(_ellipsis, _prefixLength, createInfo.Width, createInfo.TextRunProperties);
        }

        public override string ToString()
        {
            return nameof(PrefixCharacterEllipsis);
        }
    }
}
