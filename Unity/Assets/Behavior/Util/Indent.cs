using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public readonly struct Indent
{
    public int Depth { get; }

    public Indent(int depth)
    {
        Depth = depth;
    }

    public Indent Inc() => new Indent(Depth + 1);
    public Indent Dec() => new Indent(Depth - 1);

    public override string ToString()
    {
        StringBuilder b = new StringBuilder();
        for (int i = 0; i < Depth; i++)
            b.Append("  ");
        return b.ToString();
    }
}