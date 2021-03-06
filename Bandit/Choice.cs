﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BanditCore.Stochastic;

namespace NeoSmart.Bandit
{
    [Serializable]
    public class Choice<T>
    {
        public readonly T Value;
        internal Tally Tally = new Tally();

        public string Guid { get; private set; }

        private readonly int _gamblerIndex;
        private readonly GamblerBase _gambler;

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
            get { return ((Double) Tally.Success/Tally.Total); }
        }

        public string ValueString
        {
            get { return Value.ToString(); }
        }

        public Choice(T t, GamblerBase gambler, int index)
        {
            _gamblerIndex = index;
            Value = t;
            Guid = System.Guid.NewGuid().ToString();
            _gambler = gambler;
        }

        public void Reset()
        {
            System.Threading.Interlocked.Exchange(ref Tally.Total, 0);
            System.Threading.Interlocked.Exchange(ref Tally.Success, 0);
            Guid = System.Guid.NewGuid().ToString();
        }

        public void Displayed()
        {
            System.Threading.Interlocked.Increment(ref Tally.Total);
            lock (_gambler)
            {
                _gambler.Observe(_gamblerIndex, 0);
            }
        }

        public void Succeeded()
        {
            System.Threading.Interlocked.Increment(ref Tally.Success);
            lock (_gambler)
            {
                _gambler.Observe(_gamblerIndex, 2);
            }
        }

        public bool Equals(Choice<T> choice2)
        {
            return Guid == choice2.Guid;
        }
    }
}
