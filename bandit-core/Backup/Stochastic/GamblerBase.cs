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
	/// <summary>Base stochastic gambler.</summary>
	/// <remarks>
	/// <p>The abstract class <c>GamblerBase</c> provides various utilities
	/// to implement stochastic bandit <see cref="Gambler"/>.</p>
	/// </remarks>
	[Serializable]
	public abstract class GamblerBase : Gambler
	{
		/// <summary>Random generator utility.</summary>
		protected static Random random = new Random();

		/// <summary>
		/// Contains the sum of the collected rewards for each lever.
		/// The sums are indexed by the lever indices.
		/// </summary>
		protected double[] rewardSums;

		/// <summary>
		/// Contains the sum of the squared of the collected rewards for 
		/// each lever. The sums are indexed by the lever indices.
		/// </summary>
		protected double[] rewardSquareSums;

		/// <summary>
		/// Contains the number of observations for each lever.
		/// The observation counts are indexed by the lever indices.
		/// </summary>
		protected int[] observationCounts;

		/// <summary>
		/// Index of the round currently played when
		/// the method <c>Play</c> is called.
		/// </summary>
		protected int roundIndex;

		/// <summary>
		/// The sum of the mean of the already observed levers.
		/// </summary>
		protected double leverMeanSum;

		/// <summary>
		/// The sum of the square mean of the already observed levers.
		/// </summary>
		protected double leverSquareMeanSum;

		/// <summary>
		/// Number of already observed levers.
		/// </summary>
		protected int observedLeverCount;

		/// <summary>
		/// The sum of the sigma of the levers already observed twice.
		/// </summary>
		protected double leverSigmaSum;
		
		/// <summary>
		/// Number of levers already observed twice.
		/// </summary>
		protected int twiceObservedLeverCount;

		/// <summary>Reset the counters of the <c>GamblerBase</c>.</summary>
		public override void Reset()
		{
			base.Reset();

			// zero initialization
			rewardSums = new double[this.LeverCount];
			rewardSquareSums = new double[this.LeverCount];
			observationCounts = new int[this.LeverCount];
			roundIndex = 0;

			leverMeanSum = 0d;
			leverSquareMeanSum = 0d;
			observedLeverCount = 0;

			leverSigmaSum = 0d;
			twiceObservedLeverCount = 0;
		}

		/// <summary>Records of the reward brough by the specified lever.</summary>
		public override void Observe(int index, double reward)
		{
			// base method MUST be called
			base.Observe (index, reward);

			// recording the number of observed lever
			if(observationCounts[index] == 0) observedLeverCount++;
			if(observationCounts[index] == 1) twiceObservedLeverCount++;

			// Updating the lever distribution estimates
			if(observationCounts[index] > 0) 
			{
				leverMeanSum -= LeverMean(index);
				leverSquareMeanSum -= LeverMean(index) * LeverMean(index);
			}
			if(observationCounts[index] > 1) leverSigmaSum -= LeverSigma(index);

			// Updating the lever estimates
			rewardSums[index] += reward;
			rewardSquareSums[index] += reward * reward;
			observationCounts[index] += 1;

			roundIndex++;

			// Updating the lever distribution estimates
			if(observationCounts[index] > 0) 
			{
				leverMeanSum += LeverMean(index);
				leverSquareMeanSum += LeverMean(index) * LeverMean(index);
			}
			if(observationCounts[index] > 1) leverSigmaSum += LeverSigma(index);
		}

		/// <summary>Gets the reward mean associated to the specified lever.</summary>
		/// <param name="index">Lever index.</param>
		protected double LeverMean(int index)
		{
			Debug.Assert(observationCounts[index] > 0);
			return rewardSums[index] / observationCounts[index];
		}

		/// <summary>
		/// Gets the reward standard deviation associated to the specified lever.
		/// </summary>
		/// <param name="index">Lever index.</param>
		protected double LeverSigma(int index)
		{
			Debug.Assert(observationCounts[index] > 1);

			double mean = rewardSums[index] / observationCounts[index];
			double variance = rewardSquareSums[index] / observationCounts[index] - mean * mean;

			return Math.Sqrt(variance);	
		}
	}
}
