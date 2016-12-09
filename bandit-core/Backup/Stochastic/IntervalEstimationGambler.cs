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

using MathNet.Numerics.Distributions;

namespace Bandit.Stochastic
{
	/// <summary>Interval estimation gambler.</summary>
	/// <remarks>
	/// <p>Intuively, the <i>Interval Estimation</i> algorithm
	/// makes an optimistic reward estimation with <c>100 (1 - alpha) %</c>
	/// confidence interval for each lever. Then the lever of highest estimated
	/// upper bound is pulled. Notice that a smaller <c>alpha</c> leads to
	/// more exploration. In order to compute the upper bound of the mean
	/// estimate, the current implementation relies on the assumption
	/// that the lever mean estimate is normally distributed.</p>
	/// <p>The <i>Interval Estimation</i> is due to KaelBling (1993) in 
	/// <i>Learning in Embedded Systems</i> (MIT Press).</p>
	/// </remarks>
	[Serializable]
	public class IntervalEstimationGambler : GamblerBase
	{
		private double alpha;

		/// <summary>Index of the last pulled lever.</summary>
		private int lastPulledLever = 0;

		/// <param name="alpha">Upper confidence parameter.</param>
		public IntervalEstimationGambler(double alpha) 
		{
			if(alpha <= 0.0 || alpha >= 1.0)
				throw new ArgumentOutOfRangeException("alpha", alpha, 
					"alpha should be in the interval (0,1).");

			this.alpha = alpha;
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

			double maxPrice = double.NegativeInfinity;
			int maxPriceIndex = -1; // dummy initialization
			
			// computing the prices of the observed levers
			for(int i = 0; i < this.LeverCount; i++)
			{
				if(observationCounts[i] > 0)
				{
					double price = 0;

					// two or more observations
					if(observationCounts[i] > 1)
					{
						price = Price(LeverMean(i), LeverSigma(i), observationCounts[i]);
					}
					// only one observation
					else
					{
						price = Price(LeverMean(i), leverSigmaSum / twiceObservedLeverCount, 1);
					}

					if(price > maxPrice)
					{
						maxPrice = price;
						maxPriceIndex = i;
					}
				}
			}

			// computing the price of an unobserved lever
			if(observedLeverCount < this.LeverCount)
			{
				double uPrice = Price(leverMeanSum / observedLeverCount, 
										leverSigmaSum / twiceObservedLeverCount, 1);

				if(uPrice > maxPrice)
				{
					maxPrice = uPrice;

					// Choosing randomly an unobserved lever
					int uIndex = random.Next(this.LeverCount - observedLeverCount);
					int uCount = 0;
					for(int i = 0; i < this.LeverCount; i++)
					{
						if(observationCounts[i] == 0)
						{
							if(uCount == uIndex)
							{ 
								maxPriceIndex = i;
								break;
							}
							else uCount++;
						}
					}
				}
			}

			lastPulledLever = maxPriceIndex;
			return maxPriceIndex;
		}

		private double Price(double mean, double sigma, int count)
		{
			InvCumulativeNormalDistribution icdf =
				new InvCumulativeNormalDistribution(mean, sigma / Math.Sqrt(Math.Max(count, 1)));

			return icdf.ValueOf(1 - alpha);
		}
	}
}
