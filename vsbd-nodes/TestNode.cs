using vsbd_core;
using System;

[NodeInput(new Type[] { typeof(int), typeof(int) })]
[NodeOutput(new Type[] { typeof(string) })]
public class TestNode : NodeBase
{
    public override int Invoke()
    {
        Console.WriteLine("Invoked");

        return 0;
    }
}