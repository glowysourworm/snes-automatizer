using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace snes_automatizer.Extension
{
    public delegate TResult SimpleLinqKeySelector<T, TResult>(T sender);
    public delegate void SimpleLinqCallback<T>(T sender, int index);
    public delegate bool SimpleLinqPredicate<T>(T sender, int index);
}
