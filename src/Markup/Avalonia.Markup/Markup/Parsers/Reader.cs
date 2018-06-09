// Copyright (c) The Avalonia Project. All rights reserved.
// Licensed under the MIT license. See licence.md file in the project root for full license information.

using System;
using System.Text;

namespace Avalonia.Markup.Parsers
{
    internal class Reader
    {
        private readonly string _s;
        private int _i;

        public Reader(string s)
        {
            _s = s;
        }

        public bool End => _i == _s.Length;
        public char Peek => _s[_i];
        public int Position => _i;
        public char Take() => _s[_i++];

        public void SkipWhitespace()
        {
            while (!End && char.IsWhiteSpace(Peek))
            {
                Take();
            }
        }

        public bool TakeIf(char c)
        {
            if (Peek == c)
            {
                Take();
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool TakeIf(Func<char, bool> condition)
        {
            if (condition(Peek))
            {
                Take();
                return true;
            }
            return false;
        }

        public string TakeUntil(char c)
        {
            var builder = new StringBuilder();
            while (!End && Peek != c)
            {
                builder.Append(Take());
            }
            return builder.ToString();
        }
    }
}
