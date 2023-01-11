﻿using System;
using System.Buffers;
using System.Collections.Generic;
using Avalonia.Utilities;

namespace Avalonia.Media.TextFormatting
{
    public sealed class ShapedBuffer : IList<GlyphInfo>, IDisposable
    {
        private static readonly IComparer<GlyphInfo> s_clusterComparer = new CompareClusters();
        private bool _rented;
        
        public ShapedBuffer(CharacterBufferRange characterBufferRange, int bufferLength, IGlyphTypeface glyphTypeface, double fontRenderingEmSize, sbyte bidiLevel) : 
            this(characterBufferRange, 
                new ArraySlice<GlyphInfo>(ArrayPool<GlyphInfo>.Shared.Rent(bufferLength), 0, bufferLength), 
                glyphTypeface,  
                fontRenderingEmSize,  
                bidiLevel)
        {
            _rented = true;
            Length = bufferLength;
        }

        internal ShapedBuffer(CharacterBufferRange characterBufferRange, ArraySlice<GlyphInfo> glyphInfos, IGlyphTypeface glyphTypeface, double fontRenderingEmSize, sbyte bidiLevel)
        {
            CharacterBufferRange = characterBufferRange;
            GlyphInfos = glyphInfos;
            GlyphTypeface = glyphTypeface;
            FontRenderingEmSize = fontRenderingEmSize;
            BidiLevel = bidiLevel;
            Length = GlyphInfos.Length;
        }

        internal ArraySlice<GlyphInfo> GlyphInfos { get; }
        
        public int Length { get; }

        public IGlyphTypeface GlyphTypeface { get; }

        public double FontRenderingEmSize { get; }

        public sbyte BidiLevel { get; }

        public bool IsLeftToRight => (BidiLevel & 1) == 0;
        
        public IReadOnlyList<ushort> GlyphIndices => new GlyphIndexList(GlyphInfos);

        public IReadOnlyList<int> GlyphClusters => new GlyphClusterList(GlyphInfos);

        public IReadOnlyList<double> GlyphAdvances => new GlyphAdvanceList(GlyphInfos);

        public IReadOnlyList<Vector> GlyphOffsets => new GlyphOffsetList(GlyphInfos);

        public CharacterBufferRange CharacterBufferRange { get; }
        
        /// <summary>
        /// Finds a glyph index for given character index.
        /// </summary>
        /// <param name="characterIndex">The character index.</param>
        /// <returns>
        /// The glyph index.
        /// </returns>
        private int FindGlyphIndex(int characterIndex)
        {
            if (characterIndex < GlyphInfos[0].GlyphCluster)
            {
                return 0;
            }

            if (characterIndex > GlyphInfos[GlyphInfos.Length - 1].GlyphCluster)
            {
                return GlyphInfos.Length - 1;
            }


            var comparer = s_clusterComparer;

            var clusters = GlyphInfos.Span;

            var searchValue = new GlyphInfo(0, characterIndex);

            var start = clusters.BinarySearch(searchValue, comparer);

            if (start < 0)
            {
                while (characterIndex > 0 && start < 0)
                {
                    characterIndex--;

                    searchValue = new GlyphInfo(0, characterIndex);

                    start = clusters.BinarySearch(searchValue, comparer);
                }

                if (start < 0)
                {
                    return -1;
                }
            }

            while (start > 0 && clusters[start - 1].GlyphCluster == clusters[start].GlyphCluster)
            {
                start--;
            }

            return start;
        }

        /// <summary>
        /// Splits the <see cref="TextRun"/> at specified length.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <returns>The split result.</returns>
        internal SplitResult<ShapedBuffer> Split(int length)
        {
            if (CharacterBufferRange.Length == length)
            {
                return new SplitResult<ShapedBuffer>(this, null);
            }

            var firstCluster = GlyphClusters[0];
            var lastCluster = GlyphClusters[GlyphClusters.Count - 1];

            var start = firstCluster < lastCluster ? firstCluster : lastCluster;

            var glyphCount = FindGlyphIndex(start + length);

            var first = new ShapedBuffer(CharacterBufferRange.Take(length), 
                GlyphInfos.Take(glyphCount), GlyphTypeface, FontRenderingEmSize, BidiLevel);

            var second = new ShapedBuffer(CharacterBufferRange.Skip(length),
                GlyphInfos.Skip(glyphCount), GlyphTypeface, FontRenderingEmSize, BidiLevel);

            return new SplitResult<ShapedBuffer>(first, second);
        }

        int ICollection<GlyphInfo>.Count => throw new NotImplementedException();

        bool ICollection<GlyphInfo>.IsReadOnly => true;

        public GlyphInfo this[int index]
        {
            get => GlyphInfos[index];
            set => GlyphInfos[index] = value;
        }

        int IList<GlyphInfo>.IndexOf(GlyphInfo item)
        {
            throw new NotImplementedException();
        }

        void IList<GlyphInfo>.Insert(int index, GlyphInfo item)
        {
            throw new NotImplementedException();
        }

        void IList<GlyphInfo>.RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        void ICollection<GlyphInfo>.Add(GlyphInfo item)
        {
            throw new NotImplementedException();
        }

        void ICollection<GlyphInfo>.Clear()
        {
            throw new NotImplementedException();
        }

        bool ICollection<GlyphInfo>.Contains(GlyphInfo item)
        {
            throw new NotImplementedException();
        }

        void ICollection<GlyphInfo>.CopyTo(GlyphInfo[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        bool ICollection<GlyphInfo>.Remove(GlyphInfo item)
        {
            throw new NotImplementedException();
        }
        public IEnumerator<GlyphInfo> GetEnumerator() => GlyphInfos.GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        private class CompareClusters : IComparer<GlyphInfo>
        {
            private static readonly Comparer<int> s_intClusterComparer = Comparer<int>.Default;

            public int Compare(GlyphInfo x, GlyphInfo y)
            {
                return s_intClusterComparer.Compare(x.GlyphCluster, y.GlyphCluster);
            }
        }

        private readonly struct GlyphAdvanceList : IReadOnlyList<double>
        {
            private readonly ArraySlice<GlyphInfo> _glyphInfos;

            public GlyphAdvanceList(ArraySlice<GlyphInfo> glyphInfos)
            {
                _glyphInfos = glyphInfos;
            }

            public double this[int index] => _glyphInfos[index].GlyphAdvance;

            public int Count => _glyphInfos.Length;

            public IEnumerator<double> GetEnumerator() => new ImmutableReadOnlyListStructEnumerator<double>(this);

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private readonly struct GlyphIndexList : IReadOnlyList<ushort>
        {
            private readonly ArraySlice<GlyphInfo> _glyphInfos;

            public GlyphIndexList(ArraySlice<GlyphInfo> glyphInfos)
            {
                _glyphInfos = glyphInfos;
            }

            public ushort this[int index] => _glyphInfos[index].GlyphIndex;

            public int Count => _glyphInfos.Length;

            public IEnumerator<ushort> GetEnumerator() => new ImmutableReadOnlyListStructEnumerator<ushort>(this);

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private readonly struct GlyphClusterList : IReadOnlyList<int>
        {
            private readonly ArraySlice<GlyphInfo> _glyphInfos;

            public GlyphClusterList(ArraySlice<GlyphInfo> glyphInfos)
            {
                _glyphInfos = glyphInfos;
            }

            public int this[int index] => _glyphInfos[index].GlyphCluster;

            public int Count => _glyphInfos.Length;

            public IEnumerator<int> GetEnumerator() => new ImmutableReadOnlyListStructEnumerator<int>(this);

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private readonly struct GlyphOffsetList : IReadOnlyList<Vector>
        {
            private readonly ArraySlice<GlyphInfo> _glyphInfos;

            public GlyphOffsetList(ArraySlice<GlyphInfo> glyphInfos)
            {
                _glyphInfos = glyphInfos;
            }

            public Vector this[int index] => _glyphInfos[index].GlyphOffset;

            public int Count => _glyphInfos.Length;

            public IEnumerator<Vector> GetEnumerator() => new ImmutableReadOnlyListStructEnumerator<Vector>(this);

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            if (_rented)
            {
                GlyphInfos.ReturnRent();
            }
        }

        ~ShapedBuffer()
        {
            if (_rented)
            {
                GlyphInfos.ReturnRent();
            }
        }
    }

    public readonly record struct GlyphInfo
    {
        public GlyphInfo(ushort glyphIndex, int glyphCluster, double glyphAdvance = 0, Vector glyphOffset = default)
        {
            GlyphIndex = glyphIndex;
            GlyphAdvance = glyphAdvance;
            GlyphCluster = glyphCluster;
            GlyphOffset = glyphOffset;
        }

        /// <summary>
        /// Get the glyph index.
        /// </summary>
        public ushort GlyphIndex { get; }

        /// <summary>
        /// Get the glyph cluster.
        /// </summary>
        public int GlyphCluster { get; }

        /// <summary>
        /// Get the glyph advance.
        /// </summary>
        public double GlyphAdvance { get; }

        /// <summary>
        /// Get the glyph offset.
        /// </summary>
        public Vector GlyphOffset { get; }
    }
}
