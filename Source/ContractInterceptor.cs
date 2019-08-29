// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Project: SpaceProgramFunding -- ContractInterceptor.cs
// 
// Summary: Transforms KSP funding model to play like a governmental space program rather than a commercial business.
// -------------------------------------------------------------------------------------------------------------------------

using System.IO;
using System.Linq;
using Contracts;
using JetBrains.Annotations;
using UnityEngine;

namespace SpaceProgramFunding.Source
{
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>
	///     Handles converting funds rewards into reputation rewards for contracts. Doing so fits with the whole
	///     concept of this mod.
	/// </summary>
	[KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
	internal class ContractInterceptor : MonoBehaviour
	{
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Reference to the single existing object of this type.</summary>
		private ContractInterceptor _instance;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The blacklisted agencies for whose contracts will be left intact. Typically, these
		/// 		  are agency contracts specifically designed to raise money rather than reputation.</summary>
		///------------------------------------------------------------------------------------------------------------
		private string[] _blacklistedAgencies;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		///     Awakes this object and sets appropriate event callbacks so the contracts can be intercepted and
		///     altered to reward reputation instead of funds.
		/// </summary>
		[UsedImplicitly]
		public void Awake()
		{
			if (_instance != null && _instance != this) {
				Destroy(this);
				return;
			}

			//if (!BudgetSettings.Instance.masterSwitch) Destroy(this);
			DontDestroyOnLoad(this);
			_instance = this;

			LoadBlacklist();

			GameEvents.Contract.onOffered.Add(OnOffered);
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Executes the destroy action and removes the event callback registered in the Awake()
		/// 		  method.</summary>
		///------------------------------------------------------------------------------------------------------------
		[UsedImplicitly]
		public void OnDestroy()
		{
			GameEvents.Contract.onOffered.Remove(OnOffered);
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Loads the blacklisted space agencies so it knows not to alter the contracts offered
		/// 		  from them.</summary>
		///------------------------------------------------------------------------------------------------------------
		void LoadBlacklist()
		{
			const string filename = "/GameData/SpaceProgramFunding/Blacklist.cfg";
			if (!File.Exists(filename)) return;

			_blacklistedAgencies = File.ReadAllLines(filename);
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		///     Query if 'agent' is agent blacklisted. A blacklisted agent will not have any contracts from it
		///     altered. The intent is that contracts for passenger transportation will be unmodified so that
		///     funds will be rewarded. This accounts for ticket prices which should not be converted into
		///     reputation.
		/// </summary>
		/// <param name="agent"> The agent to check if blacklisted.</param>
		/// <returns> True if agent blacklisted, false if not.</returns>
		private bool IsAgentBlacklisted(string agent)
		{
			foreach (var ss in _blacklistedAgencies) {
				if (ss == agent) return true;
			}

			return false;
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		///     Handles when a contract has been offered. It will alter the contract rewards by converting funds
		///     rewards into reputation rewards at the standard conversion rate. The same is true for funds
		///     penalties for contract failure.
		/// </summary>
		/// <param name="contract"> The contract that is being offered.</param>
		private void OnOffered(Contract contract)
		{
			if (BudgetSettings.Instance == null) return;

			// If the game is not career mode or contract modification has been specifically turned off, bail.
			if (HighLogic.CurrentGame.Mode != Game.Modes.CAREER || !BudgetSettings.Instance.isContractInterceptor) return;

			// If the contract doesn't reward any funds, then nothing can be done. Bail.
			if (contract.FundsCompletion <= 0) return;

			// A blacklisted agent will not have any of its contracts modified. Bail.
			if (IsAgentBlacklisted(contract.Agent.Name)) return;

			var funds_per_rep = BudgetSettings.Instance.fundsPerRep;


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