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
using MathNet.Numerics.Generators;

namespace BanditCore.Stochastic
{
	/// <summary>Gaussian match strategy.</summary>
	/// <remarks>
	/// <p>The <i>gaussian match</i> strategy is a variant of the
	/// <see cref="IntervalEstimationGambler"/> strategy. Intuitively
	/// the idea is to pull to every lever with a probability
	/// equal to the lever probability of being the <i>best lever</i>.</p>
	/// <p>The interval draw method is inspired from the paper
	/// <i>Simulation Results for a new two-armed bandit heuristic</i>
	/// from R. L. Rivest and Y. Yin (1993).</p>
	/// </remarks>
	[Serializable]
	public class GaussMatchGambler : GamblerBase
	{
		private double skew;

		/// <summary>Index of the last pulled lever.</summary>
		private int lastPulledLever = 0;

		private NormalGenerator generator = new NormalGenerator();

		/// <summary>Creates a new <i>interval draw</i> gambler.</summary>
		public GaussMatchGambler(double skew)
		{
			this.skew = skew;
		}

		/// <summary>Returns the index of the pulled lever.</summary>
		public override int Play(int horizon)
		{
			// initialization: observing at least two levers twice
			if(observedLeverCount < 1 || twiceObservedLeverCount < 1)
			{
				// playing twice the same lever
				if(observationCounts[lastPulledLever] == 1)
				{
					return lastPulledLever;
				}

				return random.Next(this.LeverCount);
			}

			// draw a deviate from each lever
			double maxDeviate = double.NegativeInfinity;
			int maxDeviateIndex = -1;

			for(int i = 0; i < this.LeverCount; i++)
			{
                double mean = 0;
				if(observationCounts[i] > 0) mean = LeverMean(i);
				else mean = leverMeanSum / observedLeverCount;

				double sigma = 0;
				if(observationCounts[i] > 1) sigma = LeverSigma(i);
				else sigma = leverSigmaSum / twiceObservedLeverCount;

				// recycling the normal generator instead of creating a new one each time
				generator.Mean = mean;
				generator.Sigma = sigma * skew // notice the "skew" factor
					/ Math.Sqrt(Math.Max(observationCounts[i], 1)); 
				double deviate = generator.Next();

				if(deviate > maxDeviate)
				{
					maxDeviate = deviate;
					maxDeviateIndex = i;
				}
			}

			// pulling the lever of highest deviate
			lastPulledLever = maxDeviateIndex;
			return maxDeviateIndex;
		}
	}
}
