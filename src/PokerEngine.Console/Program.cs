using System;
using System.Linq;
using MSC = System.Console;

namespace PokerEngine.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args != null)
            {
                MSC.WriteLine($"Args {string.Join(',', args)} are ignored!");
            }
            
            MSC.WriteLine("Hello World! What's your name?");
            var name = MSC.ReadLine();
            MSC.WriteLine($"Ciao {name}!");
        }
    }
}
