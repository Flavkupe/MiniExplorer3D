using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class CoordPair<T,R>
{
    public CoordPair(T x, R y)
    {
        this.X = x;
        this.Y = y;
    }

    public T X { get; set; }
    public R Y { get; set; }
}

