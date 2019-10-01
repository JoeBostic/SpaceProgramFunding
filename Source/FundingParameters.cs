// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Project: SpaceProgramFunding -- Transforms KSP funding model to play like a governmental space program.
// Source:  https://github.com/JoeBostic/SpaceProgramFunding
// License: https://github.com/JoeBostic/SpaceProgramFunding/wiki/MIT-License
// --------------------------------------------------------------------------------------------------------------------

using System.Reflection;
using System.IO;

namespace SpaceProgramFunding.Source
{
	public class FundingParameters : GameParameters.CustomParameterNode
	{


		public override GameParameters.GameMode GameMode => GameParameters.GameMode.CAREER;

		public override bool HasPresets => true;

		public override string Section => "Space Program Funding";

		public override string DisplaySection => "Space Program Funding";

		public override int SectionOrder => 1;

		public override string Title => "Funding & Reputation";



		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The funding is run every month (typically). This specifies the number of days in a
		/// 		  funding period.</summary>
		[GameParameters.CustomIntParameterUI("Funding interval (days)", 
			toolTip = "Funding calculation occurs at regular interval of days specified.", minValue = 0, maxValue = 120, autoPersistance = true)]
		public int fundingIntervalDays = 30;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The funds to grant is based on a multiple of the current reputation value. To
		/// 		  get more funding, get a higher reputation.</summary>
		[GameParameters.CustomIntParameterUI("Funds per Reputation point awarded", 
			toolTip = "Funds granted per funding period takes current reputation number multiplied by this value.", minValue = 0, maxValue = 5000, autoPersistance = true)]
		public int fundingRepMultiplier = 2200;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Should contracts reward reputation instead of funds? Typically, this is what you want
		/// 		  to do to fit with the philosophy of this mod.</summary>
		[GameParameters.CustomParameterUI("Contracts pay rep instead of funds", 
			toolTip = "Contract fund rewards converted into reputation rewards instead. Tourist contracts are left unchanged.", autoPersistance = true)]
		public bool isContractInterceptor = true;

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Should non-discretionary costs that happen to exceed the current funding be forgiven
		/// 		  rather than charged to the player's current bank account? A responsible Kerbal
		/// 		  government would take care of these costs and this flag would be true. A more
		/// 		  mercenary government would set this flag to false and make the player pay these
		/// 		  costs regardless.</summary>
		[GameParameters.CustomParameterUI("Excess costs covered", 
			toolTip = "Fees such as launch costs, Kerbal wages, and facility maintenance that exceed funding amount \n" +
			          "will be forgiven instead of being billed against the current funds on hand.", autoPersistance = true)]
		public bool isCostsCovered = false;






		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Is reputation decay per funding period enabled? Reputation decay means the player must
		/// 		  always pat attention to reputation and perform missions as necessary to keep the
		/// 		  reputation level sustained.</summary>
		//		[GameParameters.CustomParameterUI("Rep Decay Enabled", autoPersistance = true)]
		//		public bool isRepDecayEnabled = true;

		// TODO: Rep decay of zero means no rep decay

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The number of reputation points deducted per funding period if reputation decay has
		/// 		  been enabled.</summary>
		[GameParameters.CustomIntParameterUI("Rep Decay Rate", 
			toolTip = "Reputation will decay by this rate every funding period. Reputation decay becomes larger \nthe higher the current reputation level is.", minValue = 0, maxValue = 25, autoPersistance = true)]
		public int repDecayRate = 5;



		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The minimum reputation to use when calculating gross funding. There is always a loyal
		/// 		  cadre within the Kerbal government that ensures a minimum funding.</summary>
		[GameParameters.CustomIntParameterUI("Minimum Rep", 
			toolTip = "Reputation will be considered to be no lower than this value when calculating funding. \nReputation decay will also not lower reputation below this value.", minValue = 0, maxValue = 500, autoPersistance = true)]
		public int minimumRep = 150;



		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Should a hit to reputation occur if a Kerbal is killed? As with all reputation hits,
		/// 		  it hurts the most when the reputation level is highest since gaining reputation
		/// 		  at high levels is extremely difficult.</summary>
		//		[GameParameters.CustomParameterUI("Dead Kerbals affect rep", autoPersistance = true)]
		//		public bool isKerbalDeathPenalty = true;

		// TODO: Death penalty of 0 means no death penalty
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The number of reputation points (per Kerbal XP level) to reduce when a Kerbal is
		/// 		  killed.</summary>
		[GameParameters.CustomIntParameterUI("Kerbal Death Penalty", 
			toolTip = "Reputation is lowered by this amount when a Kerbal dies. The penalty becomes larger \nthe higher the current reputation level is.", minValue = 0, maxValue = 25, autoPersistance = true)]
		public int kerbalDeathPenalty = 15;



		// TODO: Use value of 0 to mean no conversion allowed
		//		[GameParameters.CustomParameterUI("Convert funds to Reputation allowed", autoPersistance = true)]
		//		public bool isReputationAllowed = true;

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Mission reward funds are converted to reputation at the follow rate./summary></summary>
		[GameParameters.CustomIntParameterUI("Funds per reputation", 
			toolTip = "The cost to create 1 reputation point. This is used when diverting funds toward reputation gain.", minValue = 0, maxValue = 10000, autoPersistance = true)]
		public int fundsPerRep = 10000;


		// TODO: Use value of 0 to mean no conversion allowed
		//		[GameParameters.CustomParameterUI("Convert funds to Science allowed", autoPersistance = true)]
		//		public bool isScienceAllowed = true;

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> When diverting funds to create science points, this is the number of credits it
		/// 		  takes to create one science point.</summary>
		[GameParameters.CustomIntParameterUI("Funds per science point", 
			toolTip = "The cost to create 1 science point. This is used when diverting funds toward science point gain.", minValue = 0, maxValue = 10000, autoPersistance = true)]
		public int sciencePointCost = 10000;



		public override void SetDifficultyPreset(GameParameters.Preset preset)
		{
			string filename = SpaceProgramFunding.SettingsFilename(preset);
			if (!File.Exists(filename)) return;
			var settings = ConfigNode.Load(filename);

			int.TryParse(settings.GetValue("friendlyInterval"), out fundingIntervalDays);
			int.TryParse(settings.GetValue("multiplier"), out fundingRepMultiplier);
			bool.TryParse(settings.GetValue("contractInterceptor"), out isContractInterceptor);
			int.TryParse(settings.GetValue("repDecay"), out repDecayRate);
			int.TryParse(settings.GetValue("minimumRep"), out minimumRep);
			int.TryParse(settings.GetValue("kerbalDeathPenalty"), out kerbalDeathPenalty);
			int.TryParse(settings.GetValue("sciencePointCost"), out sciencePointCost);
			int.TryParse(settings.GetValue("FundsPerRep"), out fundsPerRep);
			bool.TryParse(settings.GetValue("coverCosts"), out isCostsCovered);
		}


#if true
		public override bool Enabled(MemberInfo member, GameParameters parameters)
		{
			if (member?.Name != null) {
				switch (member.Name) {
//					case "sciencePointCost":
//						return isScienceAllowed;

//					case "fundsPerRep":
//						return isReputationAllowed;

//					case "bigProjectMultiple":
//					case "bigProjectFee":
//						return isBigProjectAllowed;

//					case "kerbalDeathPenalty":
//						return isKerbalDeathPenalty;

//					case "repDecayRate":
//						return isRepDecayEnabled;

					default:
						break;
				}
			}

			return base.Enabled(member, parameters);
		}
#endif


	}
}