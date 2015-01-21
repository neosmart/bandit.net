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

            if (b.Choices.Count > 2)
            {
                b = new Bandit<string>();
            }
            if (b.Choices.Count == 0)
            {
                b.AddChoice("hello");
                b.AddChoice("hi");
            }

            var choice1 = b.Choices[0];
            var choice2 = b.Choices[1];

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

                //var input = Console.ReadLine();

                //if (input == shown.Value)
                var r = new Random((Int32)DateTime.Now.Ticks);
                if (r.Next(100) > 50)
                {
                    shown.Succeeded();
                }
            }

            Console.WriteLine("Choice 1 - Success: {0}, Failure: {1}, Total: {2}, Ratio {3}", choice1.Success, choice1.Failure, choice1.Total, choice1.Ratio);
            Console.WriteLine("Choice 2 - Success: {0}, Failure: {1}, Total: {2}, Ratio {3}", choice2.Success, choice2.Failure, choice2.Total, choice2.Ratio);

            Console.WriteLine(" p= {0}", b.P);

            b.Save("output.bin");
            b = Bandit<string>.Load("output.bin");
        }
    }
}
