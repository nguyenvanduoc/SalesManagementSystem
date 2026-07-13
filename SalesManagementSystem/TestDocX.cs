using System;
using System.Reflection;
using Xceed.Words.NET;

class Program
{
    static void Main()
    {
        var methods = typeof(DocX).GetMethods();
        foreach(var m in methods)
        {
            Console.WriteLine(m.Name);
        }
    }
}
