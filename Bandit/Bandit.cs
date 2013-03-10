using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace NeoSmart.Bandit
{
    [Serializable]
    public class Bandit<T>
    {
        public readonly Dictionary<string, Choice<T>> Choices = new Dictionary<string,Choice<T>>();

        public Choice<T> AddChoice(T choice, double initialConfidence, long multiplier)
        {
            lock (Choices)
            {
                var temp = new Choice<T>(choice);
                Choices.Add(temp.Guid, temp);

                temp.Tally.Success = (int) (multiplier*initialConfidence);
                temp.Tally.Total = multiplier;

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
            var best = Choices.Values.Aggregate((agg, next) => next.Ratio > agg.Ratio ? next : agg);
            best.Displayed();

            return best;
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
