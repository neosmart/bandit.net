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
using System.Collections;
using MathNet.Numerics.Distributions;

namespace BanditCore.Stochastic
{
	/// <summary>"Price of knowledge" player.</summary>
	[Serializable]
	public class PokerGambler : GamblerBase
	{
		/// <summary>Index of the last pulled lever.</summary>
		private int lastPulledLever = 0;

		/// <summary>Creates a new POKER gambler.</summary>
		public PokerGambler() {}

		/// <summary>Returns the index of the pulled lever.</summary>
		public override int Play(int horizon)
		{
			// initialization: observing at least two levers twice
			if(observedLeverCount < 1
				|| leverSigmaSum == 0) // FIX: the default sigma must not be null
			{
				// playing twice the same lever
				if(observationCounts[lastPulledLever] == 1)
				{
					return lastPulledLever;
				}

				lastPulledLever = random.Next(this.LeverCount);
				return lastPulledLever;
			}

			// computing the delta
			ArrayList means = new ArrayList(observedLeverCount);
			for(int i = 0; i < this.LeverCount; i++)
			{
				if(observationCounts[i] > 0)
					means.Add(rewardSums[i] / observationCounts[i]);
			}
			means.Sort();
			int k = (int) Math.Ceiling(Math.Sqrt(means.Count));
			double delta = (double) means[means.Count - 1] - 
				(double) means[means.Count - k];

			double maxMean = (double) means[means.Count - 1];

			// if k equals 1, then just play randomly (delta could not be estimated)
			if(k <= 1)
			{
				return random.Next(this.LeverCount);
			}

			delta /= (k - 1);

			// computing the prices of the observed levers
			double maxPrice = double.NegativeInfinity;
			int maxPriceIndex = -1; // dummy initialization

			for(int i = 0; i < this.LeverCount; i++)
			{
				if(observationCounts[i] > 0)
				{
					double mean = rewardSums[i] / observationCounts[i];
					
					// empirical estimate of the standard deviation is avaiblable
					double sigma = 0;
					if(observationCounts[i] > 1)
					{
						sigma = LeverSigma(i);

						// FIX: sigma must not be null
						if(sigma == 0) 
						{
							sigma = leverSigmaSum / twiceObservedLeverCount;
						}
					}
						// using the avg standard deviation amoung the levers
					else
					{
						sigma = leverSigmaSum / twiceObservedLeverCount;
					}

					// computing an estimate of the lever optimality probability
//					double proba = 
//						Gaussian(mean, maxMean + delta, sigma / Math.Sqrt(observationCounts[i]))
//						/ Gaussian(mean, mean, sigma / Math.Sqrt(observationCounts[i]));

				    var proba = Normal.CDF(mean, sigma / Math.Sqrt(observationCounts[i]), maxMean + delta);
					
					// price = empirical mean + estimated long term gain
					double price = mean + horizon * delta * proba;

					if(maxPrice < price)
					{
						maxPrice = price;
						maxPriceIndex = i;
					}
				}
			}

			// computing the price for the unobserved levers
			if(observedLeverCount < this.LeverCount)
			{
				double unobservedPrice = leverMeanSum / observedLeverCount
					+ horizon * delta / observedLeverCount;

				if(unobservedPrice > maxPrice)
				{
					maxPrice = unobservedPrice;

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

		/// <param name="x">Value</param>
		/// <param name="mean">Gaussian mean</param>
		/// <param name="sigma">Gaussian sigma</param>
		/// <returns>Probability density</returns>
		private static double Gaussian(double x, double mean, double sigma)
		{
			//Debug.Assert(sigma > 0.0);
			return 1 / (Math.Sqrt(2 * Math.PI) * sigma) * Math.Exp(- Math.Pow((x - mean) / sigma, 2.0) / 2); 
		}
	}
}
