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
	/// <summary>Epsilon first gambler.</summary>
	/// <remarks>
	/// <p>The epsilon first strategy is similar to the <see cref="EpsilonGreedyGambler"/>
	/// but instead, all the exploration is performed at first (a pure exploration
	/// phase, followed by a pure exploitation phase).</p>
	/// <p>Note that, by design, the epsilon first strategy is not an <i>online</i>
	/// strategy. The number of rounds that will be played have to be known in advance
	/// otherwise it will be possible to decide the number of rounds to spend in the 
	/// exploration phase.</p>
	/// <p>This algorithm is analysed in <i>PAC bounds for Multi-Armed
	/// Bandit and Markov Decision Processes</i> by Even-Dar, Mannor and 
	/// Mansour in COLT 2002.</p>
	/// </remarks>
	[Serializable]
	public class EpsilonFirstGambler : GamblerBase
	{
		/// <seealso cref="Epsilon"/>
		private double epsilon;

		/// <summary>
		/// Gets the number of remaining exploration rounds.
		/// </summary>
		private int remainingExplorationRounds;

		/// <summary>
		/// Creates a new epsilon first gambler.
		/// </summary>
		public EpsilonFirstGambler(double epsilon)
		{
			this.epsilon = epsilon;
		}

		/// <summary>
		/// Gets the percentage of initial exploration.
		/// </summary>
		public double Epsilon
		{
			get { return epsilon; }
		}

		/// <summary>Returns the index of the pulled lever.</summary>
		public override int Play(int horizon)
		{
			// setting the number of initial random pulls
			if(roundIndex == 0)
			{
				remainingExplorationRounds = (int) (epsilon * horizon);
			}

			// initial exploration
			if(remainingExplorationRounds > 0)
			{
				remainingExplorationRounds--;
				return random.Next(this.LeverCount);
			}

			// greedy exploitation
			else
			{
				double maxMean = double.NegativeInfinity;
				int maxMeanIndex = -1; // dummy initialization

				for(int i = 0; i < this.LeverCount; i++)
				{
					double mean = rewardSums[i] / Math.Max(observationCounts[i], 1);

					if(mean > maxMean)
					{
						maxMean = mean;
						maxMeanIndex = i;
					}
				}

				return maxMeanIndex;
			}
		}
	}
}
