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

namespace Bandit.Stochastic
{
	/// <summary>Softmax strategy with Gibs distribution.</summary>
	/// <remarks>
	/// <p>The <i>Softmax</i> seems to have been described first by
	/// D. Luce in <i>Individual Choice Behavior</i> (Wiley, 1959).</p>
	/// </remarks>
	[Serializable]
	public class SoftmaxGambler : GamblerBase
	{
		private double temperature;

		/// <summary>Creates a new SoftMax gambler.</summary>
		public SoftmaxGambler(double temperature)
		{
			this.temperature = temperature;
		}

		/// <summary>Returns the index of the pulled lever.</summary>
		public override int Play(int horizon)
		{
			double[] weights = new double[this.LeverCount];

			// Generating the Gibs cumulated probabilities
			for(int i = 0; i < this.LeverCount; i++)
			{
                weights[i] = Math.Exp(rewardSums[i] 
					/ ((double) Math.Max(observationCounts[i], 1) * temperature));
			}

			return (new DiscreteGenerator(weights)).Next();
		}
	}
}
