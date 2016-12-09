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
	/// <summary>Greedy epsilon strategy.</summary>
	/// <remarks>
	/// <p>The <i>epsilon greedy</i> strategy is certainly the most simple 
	/// strategy for the bandit problem. Intuitively, it consists of always
	/// pulling the lever of highest estimated mean, except when a random
	/// lever is pulled with an <c>epsilon</c> frequency.</p>
	/// <p>The epsilon greedy strategy seems to appear first in 
	/// <i>Learning from Delayed Rewards</i> by Watkins (1989), Phd Thesis,
	/// Cambridge University. This strategy is so simple that earlier use
	/// are likely.</p>
	/// </remarks>
	[Serializable]
	public class EpsilonGreedyGambler : GamblerBase
	{
		/// <seealso cref="Epsilon"/>
		private double epsilon;

		/// <summary>
		/// Creates a new epsilon-greedy gambler.
		/// </summary>
		public EpsilonGreedyGambler(double epsilon)
		{
			this.epsilon = epsilon;
		}

		/// <summary>
		/// Gets the <i>random pull</i> frequency.
		/// </summary>
		public double Epsilon
		{
			get { return epsilon; }
		}

		/// <summary>Returns the index of the pulled lever.</summary>
		public override int Play(int horizon)
		{
			// randomom lever with epsilon frequency
			if(random.NextDouble() < epsilon || roundIndex == 0)
			{
				return random.Next(this.LeverCount);
			}

			// greedy optimal lever
			else
			{
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

				return maxIndex;
			}
		}
	}
}
