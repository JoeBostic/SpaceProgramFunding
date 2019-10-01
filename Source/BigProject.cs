// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Project: SpaceProgramFunding -- Transforms KSP funding model to play like a governmental space program.
// Source:  https://github.com/JoeBostic/SpaceProgramFunding
// License: https://github.com/JoeBostic/SpaceProgramFunding/wiki/MIT-License
// --------------------------------------------------------------------------------------------------------------------

using System;
using JetBrains.Annotations;
using UnityEngine;

namespace SpaceProgramFunding.Source
{
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary> The "big project" is like a savings account with constraints on withdrawal and
	/// 		  depositing. It is designed to allow accumulation of funds far in excess of normal
	/// 		  monthly funding so that big purchases can be made.</summary>
	public class BigProject
	{
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The percentage of funds to siphon off of the discretionary funding.</summary>
		public float divertPercentage;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The current amount of funds socked away in the Big Project savings-account.</summary>
		public double fundsAccumulator;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Should some money be siphoned off of the funding to store in the "savings account" for
		/// 		  a big project?</summary>
		public bool isEnabled;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Hack to fix a quirk with extracting funds in VAB or SPH. The values of this class get
		/// 		  restored when leaving the vessel editor, but changes to the global funds balance
		/// 		  does not. This means that one could extract the big-project funds in the VAB and
		/// 		  then when returning to the Space Center, the funds will have magically returned
		/// 		  yet the global funds balance would still reflect the withdrawal -- exploitable to
		/// 		  get infinite funds. If this flag is true, then the big-project funds will be
		/// 		  zeroed out as soon as we know we are no longer inside the vessel editor.</summary>
		public bool isHack;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		///     Makes sure that the big-project balance never exceeds the maximum. This is an issue when
		///     reputation drops and the player is at or near maximum big-project balance.
		/// </summary>
		[UsedImplicitly]
		private void Update()
		{
			if (fundsAccumulator > MaximumBigProject()) fundsAccumulator = MaximumBigProject();
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Withdraw funds from the big-project reserve account.</summary>
		public void WithdrawFunds()
		{
			Funding.Instance.AddFunds(fundsAccumulator, TransactionReasons.Strategies);
			if (HighLogic.LoadedScene == GameScenes.EDITOR) isHack = true;

			fundsAccumulator = 0;
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Siphon funds from available funds. It takes the available funds and returns with what
		/// 		  is left over after siphoning funds off and into the big-project fund accumulator.</summary>
		///
		/// <param name="funds"> The funds.</param>
		///
		/// <returns> The funds left over after the siphoning operation.</returns>
		public double SiphonFunds(double funds)
		{
			if (funds <= 0 || !isEnabled) return funds;

			var max_allowed = MaximumBigProject() - fundsAccumulator;
			var desired_amount = funds * (divertPercentage / 100.0);
			var actual_amount = Math.Min(desired_amount, max_allowed);

			funds -= actual_amount;
			if (SpaceProgramFunding.Instance != null) actual_amount -= actual_amount * SpaceProgramFunding.Instance._misc.bigProjectFee;
			fundsAccumulator += actual_amount;
			return funds;
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Maximum big-project allowed. This is based on funding and multiplier specified in
		/// 		  settings.</summary>
		///
		/// <returns> The maximum that the big-project can hold.</returns>
		public float MaximumBigProject()
		{
			if (SpaceProgramFunding.Instance == null) return 0;
			return SpaceProgramFunding.Instance.GrossFunding() * SpaceProgramFunding.Instance._misc.bigProjectMultiple;
//			return SpaceProgramFunding.Instance.GrossFunding() * (Reputation.CurrentRep / BudgetSettings.Instance.bigProjectMultiple);
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Saves the state to the saved game file.</summary>
		///
		/// <param name="savedNode"> The file node.</param>
		public void OnSave(ConfigNode savedNode)
		{
			savedNode.SetValue("EmergencyFundingEnabled", isEnabled, true);
			savedNode.SetValue("EmergencyFund", fundsAccumulator, true);
			savedNode.SetValue("EmergencyFundPercent", divertPercentage, true);
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Loads the state from the saved game file.</summary>
		///
		/// <param name="node"> The file node.</param>
		public void OnLoad(ConfigNode node)
		{
			node.TryGetValue("EmergencyFundingEnabled", ref isEnabled);
			node.TryGetValue("EmergencyFund", ref fundsAccumulator);
			node.TryGetValue("EmergencyFundPercent", ref divertPercentage);
		}
	}
}