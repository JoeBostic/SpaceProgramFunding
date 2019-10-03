using System.IO;

namespace SpaceProgramFunding.Source
{
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary> Miscellaneous settings for the mod. This is the second column of the settings dialog.</summary>
	public class MiscParameters : GameParameters.CustomParameterNode
	{
		public override GameParameters.GameMode GameMode => GameParameters.GameMode.CAREER;
		public override bool HasPresets => true;
		public override string Section => "Space Program Funding";
		public override string DisplaySection => "Space Program Funding";
		public override int SectionOrder => 2;
		public override string Title => "Miscellaneous";


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The launch costs per 100t on the launch-pad. Essentially this is the rocket launch cost. This is
		/// 		  the cost per level of the launch-pad where the initial level equals zero. This represents the
		/// 		  wear-n-tear of the launch-pad where heavier rockets cause more damage.</summary>
		[GameParameters.CustomIntParameterUI("Launchpad Launch Cost", 
			toolTip = "Additional fee applied (to next funding period) for launches from the Launch Pad for \n" +
			          "each 100 tons of vehicle weight for vehicles over 100 tons.", minValue = 0, maxValue = 15000, stepSize = 1000, autoPersistance = true)]
		public int launchCostsLaunchPad = 5000;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The launch costs per 100t on the runway. Essentially this is the space-plane launch cost which
		/// 		  should be pretty low. This is the cost per level of the runway where the initial runway level
		/// 		  equals zero. This number should be small (or even zero)
		/// 		  to encourage space-plane use.</summary>
		[GameParameters.CustomIntParameterUI("Runway Launch Cost", 
			toolTip = "Additional fee applied (to next funding period) for launches from the Runway for \n" +
			"each 100 tons of vehicle weight for vehicles over 100 tons.", minValue = 0, maxValue = 15000, stepSize = 1000, autoPersistance = true)]
		public int launchCostsRunway;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The active vessel cost is determined from the mass of the vessel expressed as cost per 100 tons.
		/// 		  This represents the Mission Control staff and equipment expenses that all ongoing missions
		/// 		  require. Small vessels (such as tiny relay satellites)
		/// 		  imply low-maintenance missions so have less maintenance cost, as they should.</summary>
		[GameParameters.CustomIntParameterUI("Active vessel cost per 100 tons", 
			toolTip = "Vessels in flight have a maintenance cost (per 100 tons) to reflect ground personnel \n" +
			          "and equipment needed to maintain active missions.", minValue = 0, maxValue = 15000, stepSize = 1000, autoPersistance = true)]
		public int activeVesselCost = 5000;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The assigned kerbal wage is paid to each Kerbal that is on a mission (not in the Astronaut
		/// 		  Complex). The wage is multiplied by the XP level of the Kerbal.</summary>
		[GameParameters.CustomIntParameterUI("Assigned wage per XP level.", 
			toolTip = "Kerbals on active missions (not sitting in the Astronaut Complex) are paid \n" +
					  "this wage multiplied by their experience level (+1). This reflects \"Hazard Pay\".", minValue = 0, maxValue = 5000, stepSize = 100, autoPersistance = true)]
		public int assignedKerbalWage = 3000;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The base kerbal wage is paid to each Kerbal that is sitting around in the Astronaut Complex. The
		/// 		  wage is multiplied by the XP level of the Kerbal.</summary>
		[GameParameters.CustomIntParameterUI("Unassigned wage per XP level.", 
			toolTip = "Kerbals sitting in the Astronaut Complex are paid this wage multiplied by their experience level (+1).", minValue = 0, maxValue = 5000, stepSize = 100, autoPersistance = true)]
		public int baseKerbalWage = 1000;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The big-project is capped at a multiple of the gross funding. This prevents the exploit of
		/// 		  letting the big-project accumulate indefinitely. The value is the number of reputation points
		/// 		  per multiple. For example, reputation of 150 would be 3x multiple for a value of 50.</summary>
		[GameParameters.CustomIntParameterUI("Big-Project Multiple", 
			toolTip = "Funds diverted to the Big-Project savings account can accumulate to a maximum of \n" +
					  "no more than this multiple of the current gross funding level. Set to 0 to disable\n" +
					  "Big-Project savings account.", minValue = 0, maxValue = 30, autoPersistance = true)]
		public int bigProjectMultiple = 15;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Fee cost to moving money into the big-project fund. This needs to be large enough to discourage
		/// 		  keeping maximum funds flowing into big-project. The big-project account should just be used
		/// 		  for big-projects.</summary>
		[GameParameters.CustomFloatParameterUI("Big-Project Fee", 
			toolTip = "Funds transferred to the Big-Project savings account suffer a transaction fee of this percent.", asPercentage = true, minValue = 0, maxValue = 1, displayFormat = "N", stepCount = 1, autoPersistance = true)]
		public float bigProjectFee = 0.2f;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> True if science is allowed to be raised by diverting funds from the budget.</summary>
		[GameParameters.CustomParameterUI("Allow diverting funds to create science points",
			toolTip = "Allows diverting funds toward gaining science points.", autoPersistance = true)]
		public bool isScienceAllowed = true;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> When diverting funds to create science points, this is the number of credits it takes to create
		/// 		  one science point.</summary>
		[GameParameters.CustomIntParameterUI("Funds per science point",
			toolTip = "The cost to create 1 science point. This is used when diverting funds toward science point gain.", minValue = 0, maxValue = 15000, stepSize = 1000, autoPersistance = true)]
		public int sciencePointCost = 10000;


		public override void SetDifficultyPreset(GameParameters.Preset preset)
		{
			string filename = SpaceProgramFunding.SettingsFilename(preset);
			if (!File.Exists(filename)) return;
			var settings = ConfigNode.Load(filename);

			int.TryParse(settings.GetValue(nameof(launchCostsLaunchPad)), out launchCostsLaunchPad);
			int.TryParse(settings.GetValue(nameof(launchCostsRunway)), out launchCostsRunway);
			int.TryParse(settings.GetValue(nameof(activeVesselCost)), out activeVesselCost);
			int.TryParse(settings.GetValue(nameof(baseKerbalWage)), out baseKerbalWage);
			int.TryParse(settings.GetValue(nameof(assignedKerbalWage)), out assignedKerbalWage);
			int.TryParse(settings.GetValue(nameof(bigProjectMultiple)), out bigProjectMultiple);
			float.TryParse(settings.GetValue(nameof(bigProjectFee)), out bigProjectFee);
			bool.TryParse(settings.GetValue(nameof(isScienceAllowed)), out isScienceAllowed);
			int.TryParse(settings.GetValue(nameof(sciencePointCost)), out sciencePointCost);
		}
	}
}
