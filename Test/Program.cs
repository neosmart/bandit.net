using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeoSmart.Bandit;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            //var buttonBandit = Bandit<string>.Load("c:/users/mahmoud/desktop/buttonbandit.bin");

            var _lastSave = DateTime.MinValue;


            var b = Bandit<string>.Load("output.bin");
            b.Save("output.bin");
            b = Bandit<string>.Load("output.bin");

            var choice1 = b.AddChoice("hello");
            var choice2 = b.AddChoice("hi");

            for (int i = 0; i < 6; ++i)
            {
                if (DateTime.UtcNow > (_lastSave + new TimeSpan(0, 0, 0, 5)))
                {
                    lock (b)
                    {
                        if (DateTime.UtcNow > (_lastSave + new TimeSpan(0, 0, 0, 5)))
                        {
                            b.Save("output.bin");
                            _lastSave = DateTime.UtcNow;
                        }
                    }
                }

                var shown = b.GetNext();
                Console.WriteLine("Result: " + shown.Value);

                var input = Console.ReadLine();

                if (input == shown.Value)
                {
                    shown.Succeeded();
                }
            }

            Console.WriteLine("Choice 1 - Success: {0}, Failure: {1}, Total: {2}, Ratio {3}", choice1.Success, choice1.Failure, choice1.Total, choice1.Ratio);
            Console.WriteLine("Choice 2 - Success: {0}, Failure: {1}, Total: {2}, Ratio {3}", choice2.Success, choice2.Failure, choice2.Total, choice2.Ratio);

            b.Save("output.bin");
            b = Bandit<string>.Load("output.bin");
        }
    }
}
