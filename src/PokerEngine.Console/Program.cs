using System;
using MSC = System.Console;

namespace PokerEngine.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            MSC.WriteLine("Hello World! What's your name?");
            var name = MSC.ReadLine();
            MSC.WriteLine($"Ciao {name}!");
        }
    }
}
