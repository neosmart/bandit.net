using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Bandit.Stochastic;

namespace NeoSmart.Bandit
{
    [Serializable]
    public class Bandit<T>
    {
        public readonly Dictionary<string, Choice<T>> Choices = new Dictionary<string,Choice<T>>();
        public readonly Dictionary<int, string> IndexIndexer = new Dictionary<int, string>();
        private int _lastIndex = 0;

        private readonly EpsilonDecreasingGambler _gambler;

        public Bandit()
        {
            _gambler = new EpsilonDecreasingGambler(100);
        }

        public Choice<T> AddChoice(T choice)
        {
            lock (Choices)
            {
                var temp = new Choice<T>(choice, _gambler, _lastIndex);
                IndexIndexer.Add(_lastIndex++, temp.Guid);
                Choices.Add(temp.Guid, temp);
                _gambler.LeverCount = _lastIndex;
                _gambler.Reset();

                temp.Tally.Success = 0;
                temp.Tally.Total = 0;

                return temp;
            }
        }

        public void RemoveChoice(Choice<T> choice)
        {
            lock (Choices)
            {
                Choices.Remove(choice.Guid);
            }
        }

        public Choice<T> GetNext()
        {
            int index = _gambler.Play(1);
            _gambler.Observe(index, 0);

            return Choices[IndexIndexer[index]];
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
            //if (!File.Exists(path))
                return new Bandit<T>();

            using (Stream stream = File.OpenRead(path))
            {
                var bandit = (Bandit<T>) new BinaryFormatter().Deserialize(stream);
                return bandit;
            }
        }
    }
}
