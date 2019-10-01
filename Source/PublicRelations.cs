// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Project: SpaceProgramFunding -- Transforms KSP funding model to play like a governmental space program.
// Source:  https://github.com/JoeBostic/SpaceProgramFunding
// License: https://github.com/JoeBostic/SpaceProgramFunding/wiki/MIT-License
// --------------------------------------------------------------------------------------------------------------------

using System;

namespace SpaceProgramFunding.Source
{
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary> The public relations department handles diverting funds toward improving the
	/// 		  reputation of the Kerbal Space Program.</summary>
	public class PublicRelations
	{
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Should some portion of funds be diverted to Public Relations in an effort to increase
		/// 		  reputation?</summary>
		public bool isPREnabled;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The percentage of available funds that should be diverted to public relations in
		/// 		  order to increase reputation. This is a value from 1..100.</summary>
		public float reputationDivertPercentage;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Siphon funds off to public relations in order to raise the reputation of the Kerbal
		/// 		  Space Program.</summary>
		///
		/// <param name="funds"> The maximum funds available to the player.</param>
		///
		/// <returns> Returns with the funds remaining after the siphoning.</returns>
		public double SiphonFunds(double funds)
		{
			if (funds <= 0) return funds;
			if (!isPREnabled) return funds;
			if (reputationDivertPercentage <= 0) return funds;

			var percent_diverted_to_pr = reputationDivertPercentage / 100;
			var max_reputation_points = (float) (funds / SpaceProgramFunding.Instance._settings.fundsPerRep);
			var desired_reputation_points = (float) Math.Round(max_reputation_points * percent_diverted_to_pr, 1);

			// Add the reputation.
			Reputation.Instance.AddReputation(desired_reputation_points, TransactionReasons.None);
			funds -= desired_reputation_points * SpaceProgramFunding.Instance._settings.fundsPerRep;

			// Let the player know what happened.
			ScreenMessages.PostScreenMessage("Public Relations generated " + Math.Round(desired_reputation_points, 1) + " reputation");

			return funds;
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Executes the save action.</summary>
		///
		/// <param name="node"> The node.</param>
		public void OnSave(ConfigNode node)
		{
			node.SetValue("PRPercent", reputationDivertPercentage, true);
			node.SetValue("PREnabled", isPREnabled, true);
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Executes the load action.</summary>
		///
		/// <param name="node"> The node.</param>
		public void OnLoad(ConfigNode node)
		{
			node.TryGetValue("PRPercent", ref reputationDivertPercentage);
			node.TryGetValue("PREnabled", ref isPREnabled);
		}
	}
}