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

namespace BanditCore.Stochastic
{
	/// <summary>Epsilon decreasing gambler.</summary>
	/// <remarks>
	/// <p>The <i>epsilon decreasing</i> strategy is similar to the 
	/// <see cref="EpsilonGreedyGambler"/> but the epsilon value
	/// decreases over time. This implementation provides a decreasing
	/// factor of <c>e0 / t</c> where <c>e0</c> is a positive tuning
	/// parameter and <c>t</c> the current round index.</p>
	/// <p>The epsilon decreasing strategy is analysed in <i>Finite time
	/// analysis of the multiarmed bandit problem.</i> by Auer, Cesa-Bianchi
	/// and Fisher in <i>Machine Learning</i> (2002).</p>
	/// </remarks>
	[Serializable]
	public class EpsilonDecreasingGambler : GamblerBase
	{
		/// <seealso cref="EpsilonDecreasingGambler"/>
		private double epsilonZero;

		/// <summary>Creates a new epsilon decreasing gambler.</summary>
		public EpsilonDecreasingGambler(double epsilonZero)
		{
			if(epsilonZero <= 0.0)
				throw new ArgumentOutOfRangeException(
					"epsilonZero", epsilonZero, "epsilonZero must be in the interval (0, +infinity)."); 

			this.epsilonZero = epsilonZero;
		}

		/// <summary>Gets the <i>epsilon zero</i> tuning parameter.</summary>
		/// <remarks>
		/// See the <see cref="EpsilonDecreasingGambler"/> for details about this
		/// tuning parameter.
		/// </remarks>
		public double EpsilonZero
		{
			get { return epsilonZero; }
		}

		/// <summary>Returns the index of the pulled lever.</summary>
		public override int Play(int horizon)
		{
			// computing the max mean (and the 2nd mean)
			double maxMean = double.NegativeInfinity;
			int maxIndex = -1; // dummy initialization

			for(int i = 0; i < this.LeverCount; i++)
			{
				double mean = rewardSums[i] / Math.Max(observationCounts[i], 1);

				if(mean > maxMean)
				{
					maxMean = mean;
					maxIndex = i;
				}
			}
 
			// Computing the lever epsilonZero value
			double epsilonZeroT = Math.Min(1.0, epsilonZero / Math.Max(roundIndex, 1));

			if(random.NextDouble() < epsilonZeroT)
			{
				// randomom lever with epsilonZero frequency
				return random.Next(this.LeverCount);
			}
			else
			{
				// greedy optimal lever
				return maxIndex;
			}
		}
	}
}
