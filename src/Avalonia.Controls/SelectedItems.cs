﻿// This source file is adapted from the WinUI project.
// (https://github.com/microsoft/microsoft-ui-xaml)
//
// Licensed to The Avalonia Project under MIT License, courtesy of The .NET Foundation.

using System;
using System.Collections;
using System.Collections.Generic;
using SelectedItemInfo = Avalonia.Controls.SelectionModel.SelectedItemInfo;

#nullable enable

namespace Avalonia.Controls
{
    internal class SelectedItems<T> : IReadOnlyList<T>
    {
        private readonly List<SelectedItemInfo> _infos;
        private readonly Func<List<SelectedItemInfo>, int, T> _getAtImpl;

        public SelectedItems(
            List<SelectedItemInfo> infos,
            Func<List<SelectedItemInfo>, int, T> getAtImpl)
        {
            _infos = infos;
            _getAtImpl = getAtImpl;

            foreach (var info in infos)
            {
                var node = info.Node;

                if (node != null)
                {
                    Count += node.SelectedCount;
                }
                else
                {
                    throw new InvalidOperationException("Selection changed after the SelectedIndices/Items property was read.");
                }
            }
        }

        public T this[int index] => _getAtImpl(_infos, index);

        public int Count { get; }

        public IEnumerator<T> GetEnumerator()
        {
            for (var i = 0; i < Count; ++i)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
