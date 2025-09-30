using vsbd_core;
using System;
using vsbd.Attributes;

[NodeOutput(typeof(string))]
[NodeInput(typeof(int), typeof(int))]
public class TestNode : NodeBase
{
    public override int Invoke()
    {
        Console.WriteLine("Invoked");

        return 6969;
    }
}