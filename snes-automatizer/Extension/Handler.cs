namespace snes_automatizer.Extension
{
    public delegate void SimpleEventHandler<T>(T sender);
    public delegate void SimpleEventHandler<T, T1>(T sender, T1 arg1);
    public delegate void SimpleEventHandler<T, T1, T2>(T sender, T1 arg1, T2 arg2);
}
