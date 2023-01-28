// See https://aka.ms/new-console-template for more information


using System;
using System.Threading.Tasks;
using DSharpPlus;

namespace MKEHCBot // Note: actual namespace depends on the project name.
{
    internal class Program
    {

        static void Main(string[] args)
        {
            var bot = new Bot();
            bot.RunAsync().GetAwaiter().GetResult();
        }

    }
}