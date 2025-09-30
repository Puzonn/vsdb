using vsbd_core;
using System;

public class A : NodeBase
{
    public override int Invoke()
    {
        Console.WriteLine("Invoked");

        return 0;
    }
}