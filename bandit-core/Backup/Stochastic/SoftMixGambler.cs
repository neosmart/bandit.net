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

namespace Bandit.Stochastic
{
	/// <summary>SoftMix gambler.</summary>
	/// <remarks>
	/// <p>The <i>SoftMix</i> strategy is a variant of the weel-known
	/// <see cref="SoftmaxGambler"/> with a decreasing temperature.</p>
	/// <p>See <i>Finite-time Regret Bounds for the Multiarmed Bandit Problem</i>
	/// by Nicolo Cesa-Bianchi and Paul Fisher.</p>
	/// </remarks>
	[Serializable]
	public class SoftMixGambler : Gambler
	{
		private static Random random = new Random();

		private double d;

		private double[] s;

		private int roundCount;

		/// <summary>
		/// Used as a cache for the <see cref="Observe"/> method implementation.
		/// </summary>
		private double gamma;

		/// <summary>
		/// Used as a cache for the <see cref="Observe"/> method implementation.
		/// </summary>
		private int maxIndex;

		/// <summary>Creates a new SoftMix gambler.</summary>
		public SoftMixGambler(double d)
		{
			if(d <= 0.0 || d >= 1.0) throw new ArgumentOutOfRangeException(
				"d", d, "#E00: d should be in the interval (0,1).");

			this.d = d;
		}

		/// <summary>Reset the counters of the SoftMix gambler.</summary>
		public override void Reset()
		{
			base.Reset();

			// zero initialization
			s = new double[this.LeverCount];
			roundCount = 0;
		}

		/// <summary>Returns the index of the pulled levers.</summary>
		public override int Play(int horizon)
		{
			// computing gamma_t
			gamma = 1.0;
			
			if(roundCount > 2)
			{
				gamma = Math.Min(
					1.0,
					5 * this.LeverCount * Math.Log(roundCount - 1) / (d * d * (roundCount - 1)));
			}

			// computing the maximal index
			double maxS = double.NegativeInfinity;
			maxIndex = -1; // dummy initialization

			for(int i = 0; i < this.LeverCount; i++)
			{
				if(s[i] > maxS)
				{
					maxS = s[i];
					maxIndex = i;
				}
			}

			// randomom lever with epsilon frequency
			if(random.NextDouble() < gamma)
			{
				return random.Next(this.LeverCount);
			}
			// greedy optimal lever
			else
			{
				return maxIndex;
			}
		}

		/// <summary>Records of the reward brough by the specified lever.</summary>
		public override void Observe(int index, double value)
		{
			// The base method MUST be called
			base.Observe (index, value);

			roundCount++;
			
			if(index == maxIndex)
			{
				s[index] += value / (1 - gamma + (gamma / this.LeverCount) );
			}
			else
			{
				s[index] += value / (gamma / this.LeverCount);
			}
		}
	}
}
