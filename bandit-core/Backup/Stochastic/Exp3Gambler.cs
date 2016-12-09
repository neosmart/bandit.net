#region Bandit, Copyright ©2004 Joannes Vermorel

// Bandit project
//
// Project url: http://bandit.sourceforge.net
// Copyright (c) 2004,	Joannes Vermorel, http://www.vermorel.com
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published 
// by the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public 
// License along with this program; if not, write to the Free Software
// Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.

#endregion

using System;
using System.Diagnostics;

namespace Bandit.Stochastic
{
	/// <summary>
	/// The <c>Exp3Gambler</c> implements the <i>exponential-weight
	/// for exploration and exploitation</i>.
	/// </summary>
	/// <remarks>See the paper <i>The NonStochastic multiarmed bandit problem</i>
	/// of Peter Auer, Nicolo Cesa-Bianchi, Yoav Freund and Robert E. Shapire.</remarks>
	[Serializable]
	public class Exp3Gambler : Gambler
	{
		private static Random random = new Random();

		private double gamma;

		/// <summary>Cumulated weights associated to each variable.</summary>
		private double[] w;

		/// <summary>Sum of the cumulated weights.</summary>
		private double wSum;

		/// <summary>Action probabilities in the last play.</summary>
		private double[] p;

		/// <summary>Creates a new exp3 gambler.</summary>
		public Exp3Gambler(double gamma) : base()
		{
			if(gamma <= 0.0 || gamma > 1.0)
				throw new ArgumentOutOfRangeException("gamma", gamma,
					"The gamma parameter should be in (0, 1]");

			this.gamma = gamma;
		}

		/// <summary>Resets the counters.</summary>
		public override void Reset()
		{
			base.Reset();

			// Wieights initialization
			w = new double[this.LeverCount];
			for(int i = 0; i < w.Length; i++) w[i] = 1.0;

			// Weight sum
			wSum = (double) this.LeverCount;

			// Just any non-initialized array for now
			p = new double[this.LeverCount];
		}

		/// <summary>Returns the index of the pulled lever.</summary>
		public override int Play(int horizon)
		{
			// Computing the probabilities
			for(int i = 0; i < this.LeverCount; i++)
                p[i] = (1.0 - gamma) * w[i] / wSum + gamma / (double) this.LeverCount;

			// Transformation into cumulated probabilities
			double[] cumP = new double[this.LeverCount];
			cumP[0] = p[0];
			for(int i = 1; i < this.LeverCount; i++) cumP[i] = p[i] + cumP[i - 1];

			Debug.Assert(Math.Abs(cumP[this.LeverCount - 1] - 1.0) < 0.0000001,
				"The sum of the cumulated probabilities should be equal to one (value = "
				+ cumP[this.LeverCount - 1].ToString() + ").");

			// selecting the variable
			double threshold = random.NextDouble();
			int index = 0;
			while(cumP[index] < threshold) index++;

			return index;
		}

		/// <summary>Records of the reward brough by the specified lever.</summary>
		public override void Observe(int index, double reward)
		{
			base.Observe(index, reward);

			wSum -= w[index];
			w[index] *= Math.Exp(gamma * reward / (p[index] * (double) this.LeverCount));

			// brutal handling of numeric overflow
			if(double.IsNaN(w[index]) || double.IsInfinity(w[index]) || Math.Abs(w[index]) > 10e+9)
			{
				w[index] = 1.0; // dummy arbitrary weight
			}

			wSum += w[index];
		}

	}
}
