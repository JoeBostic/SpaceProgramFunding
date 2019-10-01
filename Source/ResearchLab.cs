// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Project: SpaceProgramFunding -- Transforms KSP funding model to play like a governmental space program.
// Source:  https://github.com/JoeBostic/SpaceProgramFunding
// License: https://github.com/JoeBostic/SpaceProgramFunding/wiki/MIT-License
// --------------------------------------------------------------------------------------------------------------------

using System;

namespace SpaceProgramFunding.Source
{
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary> The Research Lab is used to convert funds into science points. The cost is high and
	/// 		  there is a reputation hit when doing so, but can be useful in some circumstances.</summary>
	public class ResearchLab
	{
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Should some portion of the available funds be diverted to creating science points?</summary>
		public bool isRNDEnabled;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The percentage of available funds that should be diverted to creating science points.
		/// 		  This is a value from 1..100.</summary>
		public float scienceDivertPercentage;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Saves the state to the saved game file.</summary>
		///
		/// <param name="savedNode"> The file node.</param>
		public void OnSave(ConfigNode savedNode)
		{
			savedNode.SetValue("RnD", scienceDivertPercentage, true);
			savedNode.SetValue("RnDEnabled", isRNDEnabled, true);
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Loads the state from the saved game file.</summary>
		///
		/// <param name="node"> The file node.</param>
		public void OnLoad(ConfigNode node)
		{
			node.TryGetValue("RnD", ref scienceDivertPercentage);
			node.TryGetValue("RnDEnabled", ref isRNDEnabled);
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Siphon funds from available funds in order to create science points.</summary>
		///
		/// <param name="funds"> The funds.</param>
		///
		/// <returns> The funds left over after the siphoning operation.</returns>
		public double SiphonFunds(double funds)
		{
			if (funds <= 0) return funds;
			if (!isRNDEnabled) return funds;
			if (scienceDivertPercentage <= 0) return funds;
			if (SpaceProgramFunding.Instance == null) return funds;

			var percent_diverted_to_science = scienceDivertPercentage / 100;
			var max_science_points = (float) (funds / SpaceProgramFunding.Instance._misc.sciencePointCost);
			var desired_science_points = (float) Math.Round(max_science_points * percent_diverted_to_science, 1);

			// Add the science.
			ResearchAndDevelopment.Instance.AddScience(desired_science_points, TransactionReasons.RnDs);
			funds -= desired_science_points * SpaceProgramFunding.Instance._misc.sciencePointCost;

			// Apply reputation penalty.
			var max_decay = Reputation.CurrentRep - SpaceProgramFunding.Instance._settings.minimumRep;
			var amount_to_decay = Math.Min(desired_science_points, max_decay);
			Reputation.Instance.addReputation_discrete(-amount_to_decay, TransactionReasons.RnDs);

			// Let the player know what happened.
			ScreenMessages.PostScreenMessage("R&D Department generated " + Math.Round(desired_science_points, 1) +
			                                 " science");
			return funds;
		}
	}
}