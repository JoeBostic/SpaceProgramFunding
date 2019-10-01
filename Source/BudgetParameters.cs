// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Project: SpaceProgramFunding -- Transforms KSP funding model to play like a governmental space program.
// Source:  https://github.com/JoeBostic/SpaceProgramFunding
// License: https://github.com/JoeBostic/SpaceProgramFunding/wiki/MIT-License
// --------------------------------------------------------------------------------------------------------------------

using System.Reflection;

namespace SpaceProgramFunding.Source
{
	internal class BudgetParameters : GameParameters.CustomParameterNode
	{


		public override GameParameters.GameMode GameMode => GameParameters.GameMode.CAREER;

		public override bool HasPresets => true;

		public override string Section => "Space Program Funding";

		public override string DisplaySection => "Space Program Funding";

		public override int SectionOrder => 1;

		public override string Title => "Funding";





		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The big-project is capped at a multiple of the gross funding. This prevents the
		/// 		  exploit of letting the big-project accumulate indefinitely. The value is the
		/// 		  number of reputation points per multiple. For example, reputation of 150 would be
		/// 		  3x multiple for a value of 50.</summary>
		[GameParameters.CustomParameterUI("Big-Project Multiple", autoPersistance = true)]
		public int bigProjectMultiple = 50;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Fee cost to moving money into the big-project fund. This needs to be large
		/// 		  enough to discourage keeping maximum funds flowing into big-project. The
		/// 		  big-project account should just be used for big-projects.</summary>
		[GameParameters.CustomParameterUI("Big-Project Fee", autoPersistance = true)]
		public int bigProjectFee = 20;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Mission reward funds are converted to reputation at the follow rate./summary></summary>
		[GameParameters.CustomParameterUI("Funds per rep", autoPersistance = true)]
		public int fundsPerRep = 10000;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Should contracts reward reputation instead of funds? Typically, this is what you want
		/// 		  to do to fit with the philosophy of this mod.</summary>
		[GameParameters.CustomParameterUI("Contracts pay rep instead of funds", autoPersistance = true)]
		public bool isContractInterceptor = true;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Should non-discretionary costs that happen to exceed the current funding be forgiven
		/// 		  rather than charged to the player's current bank account? A responsible Kerbal
		/// 		  government would take care of these costs and this flag would be true. A more
		/// 		  mercenary government would set this flag to false and make the player pay these
		/// 		  costs regardless.</summary>
		[GameParameters.CustomParameterUI("Excess costs covered", autoPersistance = true)]
		public bool isCostsCovered;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Should a hit to reputation occur if a Kerbal is killed? As with all reputation hits,
		/// 		  it hurts the most when the reputation level is highest since gaining reputation
		/// 		  at high levels is extremely difficult.</summary>
		[GameParameters.CustomParameterUI("Dead Kerbals affect rep", autoPersistance = true)]
		public bool isKerbalDeathPenalty = true;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Is reputation decay per funding period enabled? Reputation decay means the player must
		/// 		  always pat attention to reputation and perform missions as necessary to keep the
		/// 		  reputation level sustained.</summary>
		[GameParameters.CustomParameterUI("Rep Decay Enabled", autoPersistance = true)]
		public bool isRepDecayEnabled;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The number of reputation points (per Kerbal XP level) to reduce when a Kerbal is
		/// 		  killed.</summary>
		[GameParameters.CustomParameterUI("Kerbal Death Penalty", autoPersistance = true)]
		public int kerbalDeathPenalty = 15;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The minimum reputation to use when calculating gross funding. There is always a loyal
		/// 		  cadre within the Kerbal government that ensures a minimum funding.</summary>
		[GameParameters.CustomParameterUI("Minimum Rep", autoPersistance = true)]
		public int minimumRep = 20;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The number of reputation points deducted per funding period if reputation decay has
		/// 		  been enabled.</summary>
		[GameParameters.CustomParameterUI("Rep Decay Rate", autoPersistance = true)]
		public int repDecayRate = 5;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> When diverting funds to create science points, this is the number of credits it
		/// 		  takes to create one science point.</summary>
		[GameParameters.CustomParameterUI("Science Point Cost", autoPersistance = true)]
		public int sciencePointCost = 10000;



//		[GameParameters.CustomParameterUI("Test parameter", autoPersistance = true)]
//		public bool test_parameter = true;

		public override void SetDifficultyPreset(GameParameters.Preset preset)
		{
#if false
			switch (preset) {
				case GameParameters.Preset.Easy:
					transmissionBoost = true;
					requireMPLForBoost = false;
					requireMPL = false;
					requireRelay = false;
					transmissionPenalty = 0;
					break;
				case GameParameters.Preset.Normal:
					transmissionBoost = true;
					requireMPLForBoost = false;
					requireMPL = false;
					requireRelay = false;
					transmissionPenalty = 0.25f;
					break;
				case GameParameters.Preset.Moderate:
					transmissionBoost = true;
					requireMPLForBoost = true;
					requireMPL = false;
					requireRelay = true;
					transmissionPenalty = 0.5f;
					break;
				case GameParameters.Preset.Hard:
					transmissionBoost = true;
					requireMPLForBoost = true;
					requireMPL = true;
					requireRelay = true;
					transmissionPenalty = 0.75f;
					break;
				case GameParameters.Preset.Custom:
					break;
			}
#endif
		}

#if false

		public override bool Enabled(MemberInfo member, GameParameters parameters)
		{
			if (member.Name == "requireMPLForBoost") {
				return !requireMPL && transmissionBoost;
			}

			if (member.Name == "transmissionPenalty") {
				return transmissionBoost;
			}

			return base.Enabled(member, parameters);
		}
#endif

	}
}