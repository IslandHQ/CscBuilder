using System;

class HelloWorld
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello from CscBuilder!");
        Console.WriteLine("CscBuilder is working correctly!");

        if (args.Length > 0)
        {
            Console.WriteLine("Arguments: " + string.Join(", ", args));
        }

        // Test multiple source files
        int result = MathHelper.Add(10, 20);
        Console.WriteLine("MathHelper.Add(10, 20) = " + result);

        result = MathHelper.Multiply(5, 6);
        Console.WriteLine("MathHelper.Multiply(5, 6) = " + result);
    }
}
