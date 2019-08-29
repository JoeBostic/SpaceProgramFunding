// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Project: SpaceProgramFunding -- DeadKerbalPenalizer.cs
// 
// Summary: Transforms KSP funding model to play like a governmental space program rather than a commercial business.
// -------------------------------------------------------------------------------------------------------------------------

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
		/// <summary>
		///     Record of the singleton object. There needs to be only one of these since we don't want a dead
		///     Kerbal to be registered as dying more than once.
		/// </summary>
		private DeadKerbalPenalizer _instance;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		///     Awakes this object and sets the event callback method for handling a dead Kerbal. This method
		///     also ensures that only one of these objects exists at a time.
		/// </summary>
		[UsedImplicitly]
		public void Awake()
		{
			if (_instance != null && _instance != this) {
				Destroy(this);
				return;
			}

			DontDestroyOnLoad(this);
			_instance = this;

			GameEvents.onKerbalStatusChange.Add(OnKerbalStatusChange);
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		///     Executes the Kerbal status change action and if the Kerbal is found to be dead or MIA then apply
		///     the reputation penalty.
		/// </summary>
		/// <param name="p">		  A ProtoCrewMember to process.</param>
		/// <param name="statusFrom"> The status from.</param>
		/// <param name="statusTo">   The status to.</param>
		private void OnKerbalStatusChange(ProtoCrewMember p, ProtoCrewMember.RosterStatus statusFrom, ProtoCrewMember.RosterStatus statusTo)
		{
			if (BudgetSettings.Instance == null) return;

			if (!BudgetSettings.Instance.isKerbalDeathPenalty) return;

			if (statusTo != ProtoCrewMember.RosterStatus.Missing) return;

			var max_penalty = Reputation.CurrentRep - BudgetSettings.Instance.minimumRep;

			var actual_penalty = Math.Min(BudgetSettings.Instance.kerbalDeathPenalty * (p.experienceLevel + 1), max_penalty);
			actual_penalty = Math.Max(actual_penalty, 0);
			if (actual_penalty > 0) Reputation.Instance.AddReputation(-actual_penalty, TransactionReasons.VesselLoss);
		}
	}
}