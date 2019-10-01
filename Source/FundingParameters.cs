// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Project: SpaceProgramFunding -- Transforms KSP funding model to play like a governmental space program.
// Source:  https://github.com/JoeBostic/SpaceProgramFunding
// License: https://github.com/JoeBostic/SpaceProgramFunding/wiki/MIT-License
// --------------------------------------------------------------------------------------------------------------------

using System.IO;

namespace SpaceProgramFunding.Source
{
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary> Settings for the basic Space Program Funding algorithm. This is the first column of settings in the
	/// 		  settings dialog.</summary>
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
			toolTip = "Funds granted per funding period takes current reputation number multiplied by this value.", minValue = 0, maxValue = 5000, stepSize = 100, autoPersistance = true)]
		public int fundingRepMultiplier = 2200;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Should contracts reward reputation instead of funds? Typically, this is what you want
		/// 		  to do to fit with the philosophy of this mod.</summary>
		[GameParameters.CustomParameterUI("Contracts pay rep instead of funds", 
			toolTip = "Contract fund rewards converted into reputation rewards instead. Tourist contracts are left unchanged.", autoPersistance = true)]
		public bool isContractInterceptor = true;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Should non-discretionary costs that happen to exceed the current funding be forgiven rather than
		/// 		  charged to the player's current bank account? A responsible Kerbal government would take care
		/// 		  of these costs and this flag would be true. A more mercenary government would set this flag
		/// 		  to false and make the player pay these costs regardless.</summary>
		[GameParameters.CustomParameterUI("Excess costs covered", 
			toolTip = "Fees such as launch costs, Kerbal wages, and facility maintenance that exceed funding amount \n" +
			          "will be forgiven instead of being billed against the current funds on hand.", autoPersistance = true)]
		public bool isCostsCovered;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The number of reputation points deducted per funding period if reputation decay has
		/// 		  been enabled.</summary>
		[GameParameters.CustomIntParameterUI("Rep Decay Rate", 
			toolTip = "Reputation will decay by this rate every funding period. Reputation decay becomes larger \n" +
			          "the higher the current reputation level is.", minValue = 0, maxValue = 25, autoPersistance = true)]
		public int repDecayRate = 5;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The minimum reputation to use when calculating gross funding. There is always a loyal
		/// 		  cadre within the Kerbal government that ensures a minimum funding.</summary>
		[GameParameters.CustomIntParameterUI("Minimum Rep", 
			toolTip = "Reputation will be considered to be no lower than this value when calculating funding. \n" +
			          "Reputation decay will also not lower reputation below this value.", minValue = 0, maxValue = 500, stepSize = 10, autoPersistance = true)]
		public int minimumRep = 150;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The number of reputation points (per Kerbal XP level) to reduce when a Kerbal is
		/// 		  killed.</summary>
		[GameParameters.CustomIntParameterUI("Kerbal Death Penalty", 
			toolTip = "Reputation is lowered by this amount when a Kerbal dies. The penalty becomes larger \n" +
			          "the higher the current reputation level is.", minValue = 0, maxValue = 25, autoPersistance = true)]
		public int kerbalDeathPenalty = 15;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> True if reputation is allowed to be raised by diverting funds from the budget.</summary>
		[GameParameters.CustomParameterUI("Allow diverting funds to raise reputation",
			toolTip = "Allows diverting funds toward raising reputation level.", autoPersistance = true)]
		public bool isReputationAllowed = true;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Mission reward funds are converted to reputation at the follow rate./summary></summary>
		[GameParameters.CustomIntParameterUI("Funds per reputation", 
			toolTip = "The cost to create 1 reputation point. This is used when diverting funds toward \n" +
			          "reputation gain or when converting contract rewards from funds to reputation.", minValue = 0, maxValue = 10000, stepSize = 100, autoPersistance = true)]
		public int fundsPerRep = 10000;


		public override void SetDifficultyPreset(GameParameters.Preset preset)
		{
			string filename = SpaceProgramFunding.SettingsFilename(preset);
			if (!File.Exists(filename)) return;
			var settings = ConfigNode.Load(filename);

			int.TryParse(settings.GetValue(nameof(fundingIntervalDays)), out fundingIntervalDays);
			int.TryParse(settings.GetValue(nameof(fundingRepMultiplier)), out fundingRepMultiplier);
			bool.TryParse(settings.GetValue(nameof(isContractInterceptor)), out isContractInterceptor);
			int.TryParse(settings.GetValue(nameof(repDecayRate)), out repDecayRate);
			int.TryParse(settings.GetValue(nameof(minimumRep)), out minimumRep);
			int.TryParse(settings.GetValue(nameof(kerbalDeathPenalty)), out kerbalDeathPenalty);
			int.TryParse(settings.GetValue(nameof(fundsPerRep)), out fundsPerRep);
			bool.TryParse(settings.GetValue(nameof(isCostsCovered)), out isCostsCovered);
			bool.TryParse(settings.GetValue(nameof(isReputationAllowed)), out isReputationAllowed);
		}
	}
}