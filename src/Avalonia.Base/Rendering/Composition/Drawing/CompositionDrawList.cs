using System;
using Avalonia.Collections.Pooled;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Utilities;

namespace Avalonia.Rendering.Composition.Drawing;

internal class CompositionDrawList : PooledList<IRef<IDrawOperation>>
{
    public CompositionDrawList()
    {
        
    }

    public CompositionDrawList(int capacity) : base(capacity)
    {
        
    }
    
    public override void Dispose()
    {
        foreach(var item in this)
            item.Dispose();
        base.Dispose();
    }

    public CompositionDrawList Clone()
    {
        var clone = new CompositionDrawList(Count);
        foreach (var r in this)
            clone.Add(r.Clone());
        return clone;
    }
}

internal class CompositionDrawListBuilder
{
    private CompositionDrawList? _operations;
    private bool _owns;

    public void Reset(CompositionDrawList? previousOperations)
    {
        _operations = previousOperations;
        _owns = false;
    }

    public CompositionDrawList DrawOperations => _operations ?? new CompositionDrawList();

    void MakeWritable(int atIndex)
    {
        if(_owns)
            return;
        _owns = true;
        var newOps = new CompositionDrawList(_operations?.Count ?? Math.Max(1, atIndex));
        if (_operations != null)
        {
            for (var c = 0; c < atIndex; c++)
                newOps.Add(_operations[c].Clone());
        }

        _operations = newOps;
    }

    public void ReplaceDrawOperation(int index, IDrawOperation node)
    {
        MakeWritable(index);
        DrawOperations.Add(RefCountable.Create(node));
    }

    public void AddDrawOperation(IDrawOperation node)
    {
        MakeWritable(DrawOperations.Count);
        DrawOperations.Add(RefCountable.Create(node));
    }

    public void TrimTo(int count)
    {
        if (count < DrawOperations.Count)
            DrawOperations.RemoveRange(count, DrawOperations.Count - count);
    }
}