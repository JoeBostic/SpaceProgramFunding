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
	/// <summary> Handles the reputation hit when a Kerbal has been detected as dying or going MIA.</summary>
	[KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
	public class DeadKerbalPenalizer : MonoBehaviour
	{
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary> Record of the singleton object. There needs to be only one of these since we don't
		/// 		  want a dead Kerbal to be registered as dying more than once.</summary>
		public DeadKerbalPenalizer Instance { get; private set; }


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Awakes this object and sets the event callback method for handling a dead Kerbal.
		/// 		  This method also ensures that only one of these objects exists at a time.</summary>
		[UsedImplicitly]
		public void Awake()
		{
			if (Instance != null && Instance != this) {
				Destroy(this);
				return;
			}

			DontDestroyOnLoad(this);
			Instance = this;

			GameEvents.onKerbalStatusChange.Add(OnKerbalStatusChange);
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Executes the Kerbal status change action and if the Kerbal is found to be dead or MIA
		/// 		  then apply the reputation penalty.</summary>
		///
		/// <param name="p">		  A ProtoCrewMember to process.</param>
		/// <param name="statusFrom"> The status from.</param>
		/// <param name="statusTo">   The status to.</param>
		private void OnKerbalStatusChange(ProtoCrewMember p, ProtoCrewMember.RosterStatus statusFrom, ProtoCrewMember.RosterStatus statusTo)
		{
			if (BudgetSettings.Instance == null) return;

			if (!BudgetSettings.Instance.isKerbalDeathPenalty) return;

			if (statusTo != ProtoCrewMember.RosterStatus.Dead && statusTo != ProtoCrewMember.RosterStatus.Missing) return;

			if (Reputation.CurrentRep < BudgetSettings.Instance.minimumRep) return;

			var actual_penalty = BudgetSettings.Instance.kerbalDeathPenalty * (p.experienceLevel + 1);
			if (actual_penalty > 0) Reputation.Instance.AddReputation(-actual_penalty, TransactionReasons.ContractPenalty);

			// Don't lower reputation below minimum
			if (Reputation.CurrentRep < BudgetSettings.Instance.minimumRep) {
				Reputation.Instance.SetReputation(BudgetSettings.Instance.minimumRep, TransactionReasons.Cheating);
			}
		}
	}
}