﻿using System;
using Avalonia.Media.TextFormatting.Unicode;
using Xunit;

namespace Avalonia.Visuals.UnitTests.Media.TextFormatting
{
    public class UnicodeDataGeneratorTests
    {
        /// <summary>
        ///     This test is used to generate all Unicode related types.
        ///     We only need to run this when the Unicode spec changes.
        /// </summary>
        [Fact/*(Skip = "Only run when the Unicode spec changes.")*/]
        public void Should_Generate_Data()
        {
            UnicodeDataGenerator.Execute();
        }
        [Theory/*(Skip = "Only run when we update the trie.")*/]
        [ClassData(typeof(LineBreakTestDataGenerator))]

        public void Should_Enumerate_LineBreaks(string text, int expectedLength)
        {
            var textMemory = text.AsMemory();

            var enumerator = new LineBreakEnumerator(textMemory);

            Assert.True(enumerator.MoveNext());

            Assert.Equal(expectedLength, enumerator.Current.PositionWrap);
        }

        private class LineBreakTestDataGenerator : TestDataGenerator
        {
            public LineBreakTestDataGenerator()
                : base("auxiliary/LineBreakTest.txt")
            {
            }
        }
    }
}
