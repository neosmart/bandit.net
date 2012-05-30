using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoSmart.Bandit
{
    [Serializable]
    public class Choice<T>
    {
        public readonly T Value;
        internal Tally Tally = new Tally();

        public readonly string Guid;

        public long Success
        {
            get { return Tally.Success; }
        }

        public long Failure
        {
            get { return Tally.Total - Tally.Success; }
        }

        public long Total
        {
            get { return Tally.Total; }
        }

        public double Ratio
        {
            get
            {
                return ((Double) Tally.Success/Tally.Total);
            }
        }

        public Choice(T t)
        {
            Value = t;
            Guid = System.Guid.NewGuid().ToString();
        }

        public void Displayed()
        {
            System.Threading.Interlocked.Increment(ref Tally.Total);
        }

        public void Succeeded()
        {
            System.Threading.Interlocked.Increment(ref Tally.Success);
        }
    }
}
