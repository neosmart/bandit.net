using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using BanditCore.Stochastic;

namespace NeoSmart.Bandit
{
    [Serializable]
    public class Bandit<T>
    {
        public string Name { get; set; }
        public readonly List<Choice<T>> Choices = new List<Choice<T>>();

        private readonly GamblerBase _gambler = null;

        public Bandit(string name = null)
        {
            Name = name ?? Guid.NewGuid().ToString();

            //Choose the winner with a probability of (1-epsilon) 
            //otherwise, randomly select between choices
            //_gambler = new EpsilonGreedyGambler(0.70);
            _gambler = new PureLuckGambler();
        }

        public void ResetStats()
        {
            _gambler.Reset();

            lock (Choices)
            {
                foreach (var choice in Choices)
                {
                    choice.Reset();
                }
            }
        }

        public Choice<T> AddChoice(T choice)
        {
            lock (Choices)
            {
                var temp = new Choice<T>(choice, _gambler, Choices.Count);
                Choices.Add(temp);
                _gambler.LeverCount = Choices.Count;
                _gambler.Reset();

                temp.Tally.Total = 0;
                temp.Tally.Success = 0;

                return temp;
            }
        }

        public void RemoveChoice(Choice<T> choice)
        {
            lock (Choices)
            {
                Choices.RemoveAll(choice1 => choice1.Guid == choice.Guid);
            }
        }

        public Choice<T> GetNext()
        {
            int index = _gambler.Play(1);
            _gambler.Observe(index, 0);

            var next = Choices[index];
            next.Displayed();
            
            return next;
        }

        public bool Save(string path)
        {
            try
            {
                lock (Choices)
                {
                    using (Stream stream = File.Create(path))
                    {
                        new BinaryFormatter().Serialize(stream, this);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
            
            return true;
        }

        static public Bandit<T> Load(string path)
        {
            if (!File.Exists(path))
                return new Bandit<T>();

            using (Stream stream = File.OpenRead(path))
            {
                var bandit = (Bandit<T>) new BinaryFormatter().Deserialize(stream);
                return bandit;
            }
        }
    }
}
