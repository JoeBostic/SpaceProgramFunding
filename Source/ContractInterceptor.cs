// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Project: SpaceProgramFunding -- Transforms KSP funding model to play like a governmental space program.
// Source:  https://github.com/JoeBostic/SpaceProgramFunding
// License: https://github.com/JoeBostic/SpaceProgramFunding/wiki/MIT-License
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Contracts;
using JetBrains.Annotations;
using UnityEngine;

namespace SpaceProgramFunding.Source
{
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary> Handles converting funds rewards into reputation rewards for contracts. Doing so fits
	/// 		  with the whole concept of this mod.</summary>
	[KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
	internal class ContractInterceptor : MonoBehaviour
	{
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The blacklisted agencies for whose contracts will be left intact. Typically, these
		/// 		  are agency contracts specifically designed to raise money rather than reputation.</summary>
		private readonly List<string> _blacklistedAgencies = new List<string>();


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Reference to the single existing object of this type.</summary>
		private static ContractInterceptor _instance;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Awakes this object and sets appropriate event callbacks so the contracts can be
		/// 		  intercepted and altered to reward reputation instead of funds.</summary>
		[UsedImplicitly]
		public void Awake()
		{
			if (_instance != null && _instance != this) {
				Destroy(this);
				return;
			}

			DontDestroyOnLoad(this);
			_instance = this;

			LoadBlacklist();

			GameEvents.Contract.onOffered.Add(OnOffered);
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Executes the destroy action and removes the event callback registered in the Awake()
		/// 		  method.</summary>
		[UsedImplicitly]
		public void OnDestroy()
		{
			if (_instance == this) {
				GameEvents.Contract.onOffered.Remove(OnOffered);
			}
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Loads the blacklisted space agencies so it knows not to alter the contracts offered
		/// 		  from them.</summary>
		private void LoadBlacklist()
		{
			try {
				var node = ConfigNode.Load(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/Config/Blacklist.cfg");

				foreach (var blacklisted_agency in node.GetValues("BLACKLISTED")) {
					if (!_blacklistedAgencies.Contains(blacklisted_agency)) {
						_blacklistedAgencies.Add(blacklisted_agency);
					}
				}
			} catch (Exception e) {
				Debug.LogError("[SPF] LoadBlacklist(): " + e);
			}
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Query if 'agent' is agent blacklisted. A blacklisted agent will not have any
		/// 		  contracts from it altered. The intent is that contracts for passenger
		/// 		  transportation will be unmodified so that funds will be rewarded. This accounts
		/// 		  for ticket prices which should not be converted into reputation.</summary>
		///
		/// <param name="agent"> The agent to check if blacklisted.</param>
		///
		/// <returns> True if agent blacklisted, false if not.</returns>
		private bool IsAgentBlacklisted(string agent)
		{
			if (agent == null) {
				return false;
			}
			foreach (var ss in _blacklistedAgencies)
				if (ss == agent)
					return true;

			return false;
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Handles when a contract has been offered. It will alter the contract rewards by
		/// 		  converting funds rewards into reputation rewards at the standard conversion rate.
		/// 		  The same is true for funds penalties for contract failure.</summary>
		///
		/// <param name="contract"> The contract that is being offered.</param>
		private void OnOffered(Contract contract)
		{
			// If the game is not career mode or contract modification has been specifically turned off, bail.
			if (HighLogic.CurrentGame.Mode != Game.Modes.CAREER || !SpaceProgramFunding.Instance.settings.isContractInterceptor) {
				return;
			}

			// If the contract doesn't reward any funds, then nothing can be done. Bail.
			if (contract.FundsCompletion <= 0) {
				return;
			}

			// A blacklisted agent will not have any of its contracts modified. Bail.
			if (IsAgentBlacklisted(contract.Agent.Name)) {
				return;
			}

			var funds_per_rep = SpaceProgramFunding.Instance.settings.fundsPerRep;

			/*
			 * Take the funds advance and funds on failure and combine them to get the net-cost-of-failure. Just convert
			 * that to reputation and modify the failure reputation to reflect that. Typically, the two values tend to
			 * cancel out mostly, but apply the reputation effect on failure in any case.
			 */
			var failure_funds = (float) (contract.FundsAdvance + contract.FundsFailure);
			contract.ReputationFailure += failure_funds / funds_per_rep;

			/*
			 * Figure success fund rewards (for main contract, not contract parameters).
			 */
			var success_funds = (float) (contract.FundsAdvance + contract.FundsCompletion);
			contract.ReputationCompletion += success_funds / funds_per_rep;

			contract.FundsFailure = 0;
			contract.FundsAdvance = 0;
			contract.FundsCompletion = 0;


			for (var i = 0; i < contract.AllParameters.Count(); i++) {
				var p = contract.AllParameters.ElementAt(i);

				var param_success_funds = p.FundsCompletion;
				p.ReputationCompletion += (float) (param_success_funds / funds_per_rep);
				p.FundsCompletion = 0;

				var param_failure_funds = p.FundsFailure;
				p.ReputationFailure += (float) (param_failure_funds / funds_per_rep);
				p.FundsFailure = 0;
			}

			// Every contract should reward at least 1 reputation point.
			if (contract.ReputationCompletion < 1) contract.ReputationCompletion = 1;
		}
	}
}