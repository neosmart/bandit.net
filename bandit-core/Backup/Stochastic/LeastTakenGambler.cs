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

	[Serializable]
	public class LeastTakenGambler : GamblerBase
	{
		/// <seealso cref="LeastTakenGambler"/>
		private double epsilonZero;

		/// <summary>Creates a new epsilon decreasing gambler.</summary>
		public LeastTakenGambler(double epsilonZero)
		{
			if(epsilonZero <= 0.0)
				throw new ArgumentOutOfRangeException(
					"epsilonZero", epsilonZero, "epsilonZero must be in the interval (0, +infinity)."); 

			this.epsilonZero = epsilonZero;
		}

		/// <summary>Gets the <i>epsilon zero</i> tuning parameter.</summary>
		/// <remarks>
		/// See the <see cref="LeastTakenGambler"/> for details about this
		/// tuning parameter.
		/// </remarks>
		public double EpsilonZero
		{
			get { return epsilonZero; }
		}

		/// <summary>Returns the index of the pulled lever.</summary>
		public override int Play(int horizon)
		{
			// computing the max mean
			double maxMean = double.NegativeInfinity;
			int maxMeanIndex = -1;
			int minObsCount = int.MaxValue;
			int minObsIndex = -1;

			for(int i = 0; i < this.LeverCount; i++)
			{
				double mean = rewardSums[i] / Math.Max(observationCounts[i], 1);

				if(mean > maxMean)
				{
					maxMean = mean;
					maxMeanIndex = i;
				}

				if(observationCounts[i] < minObsCount)
				{
					minObsCount = observationCounts[i];
					minObsIndex = i;
				}
			}
 
			// Computing the lever epsilonZero value
			double epsilonZeroT = epsilonZero * 4 / (4 + minObsCount * minObsCount);

			if(random.NextDouble() < epsilonZeroT)
			{
				return minObsIndex;
			}
			else
			{
				// greedy optimal lever
				return maxMeanIndex;
			}
		}
	}
}
