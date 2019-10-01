using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace SpaceProgramFunding.Source
{

	public class MiscParameters : GameParameters.CustomParameterNode
	{

		public override GameParameters.GameMode GameMode => GameParameters.GameMode.CAREER;

		public override bool HasPresets => true;

		public override string Section => "Space Program Funding";

		public override string DisplaySection => "Space Program Funding";

		public override int SectionOrder => 2;

		public override string Title => "Misc";


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Should costs for each launch (to cover wear-n-tear on launch facility) be charged
		/// 		  whenever a vessel is launched? Heavy vessels, and particularly with the launch-
		/// 		  pad, cause the launch costs to increase. This cost is a one-time charge.</summary>
		[GameParameters.CustomParameterUI("Launch Costs Enabled", 
			toolTip = "Are launch costs incurred (billed to next funding period)? This reflects the added \n" +
			          "personnel and equipment required when a vessel is launched.", autoPersistance = true)]
		public bool isLaunchCostsEnabled = true;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The launch costs per 100t on the launch-pad. Essentially this is the rocket launch
		/// 		  cost. This is the cost per level of the launch-pad where the initial level equals
		/// 		  zero. This represents the wear-n-tear of the launch-pad where heavier rockets
		/// 		  cause more damage.</summary>
		[GameParameters.CustomIntParameterUI("Launchpad Launch Cost", 
			toolTip = "Additional fee applied (to next funding period) for launches from the Launch Pad for \n" +
			          "each 100 tons of vehicle weight for vehicles over 100 tons.", minValue = 0, maxValue = 5000, autoPersistance = true)]
		public int launchCostsLaunchPad = 1000;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The launch costs per 100t on the runway. Essentially this is the space-plane launch
		/// 		  cost which should be pretty low. This is the cost per level of the runway where
		/// 		  the initial runway level equals zero. This number should be small (or even zero)
		/// 		  to encourage space-plane use.</summary>
		[GameParameters.CustomIntParameterUI("Runway Launch Cost", 
			toolTip = "Additional fee applied (to next funding period) for launches from the Runway for \n" +
			"each 100 tons of vehicle weight for vehicles over 100 tons.", minValue = 0, maxValue = 5000, autoPersistance = true)]
		public int launchCostsRunway = 0;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The active vessel cost is determined from the mass of the vessel expressed as cost
		/// 		  per 100 tons. This represents the Mission Control staff and equipment expenses
		/// 		  that all ongoing missions require. Small vessels (such as tiny relay satellites)
		/// 		  imply low-maintenance missions so have less maintenance cost, as they should.</summary>
		[GameParameters.CustomIntParameterUI("Active vessel cost per 100 tons", 
			toolTip = "Vessels in flight have a maintenance cost (per 100 tons) to reflect ground personnel \n" +
			          " and equipment needed to maintain active missions.", minValue = 0, maxValue = 1500, autoPersistance = true)]
		public int activeVesselCost = 500;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Should Kerbals be paid wages? Wages are a non-discretionary expenditure that is taken
		/// 		  out of the funding first.</summary>
		[GameParameters.CustomParameterUI("Kerbals have wages", 
			toolTip = "Are Kerbals paid wages scaled by their experience level?", autoPersistance = true)]
		public bool isKerbalWages = true;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The assigned kerbal wage is paid to each Kerbal that is on a mission (not in the
		/// 		  Astronaut Complex). The wage is multiplied by the XP level of the Kerbal.</summary>
		[GameParameters.CustomIntParameterUI("Assigned wage per XP level.", 
			toolTip = "Kerbals on active missions (not sitting in the Astronaut Complex) are paid \n" +
					  "this wage multiplied by their experience level (+1). This reflects \"Hazard Pay\".", minValue = 0, maxValue = 5000, autoPersistance = true)]
		public int assignedKerbalWage = 2000;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The base kerbal wage is paid to each Kerbal that is sitting around in the Astronaut
		/// 		  Complex. The wage is multiplied by the XP level of the Kerbal.</summary>
		[GameParameters.CustomIntParameterUI("Unassigned wage per XP level.", 
			toolTip = "Kerbals sitting in the Astronaut Complex are paid this wage multiplied by their experience level (+1).", minValue = 0, maxValue = 5000, autoPersistance = true)]
		public int baseKerbalWage = 1000;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The big-project is capped at a multiple of the gross funding. This prevents the
		/// 		  exploit of letting the big-project accumulate indefinitely. The value is the
		/// 		  number of reputation points per multiple. For example, reputation of 150 would be
		/// 		  3x multiple for a value of 50.</summary>
		[GameParameters.CustomIntParameterUI("Big-Project Multiple", 
			toolTip = "Funds diverted to the Big-Project savings account can accumulate to a maximum of \n" +
					  "no more than this multiple of the current gross funding level. Set to 0 to disable\n" +
					  "Big-Project savings account.", minValue = 0, maxValue = 50, autoPersistance = true)]
		public int bigProjectMultiple = 50;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Fee cost to moving money into the big-project fund. This needs to be large
		/// 		  enough to discourage keeping maximum funds flowing into big-project. The
		/// 		  big-project account should just be used for big-projects.</summary>
		[GameParameters.CustomFloatParameterUI("Big-Project Fee", 
			toolTip = "Funds transferred to the Big-Project savings account suffer a transaction fee of this percent.", asPercentage = true, minValue = 0, maxValue = 1, displayFormat = "N0", autoPersistance = true)]
		public float bigProjectFee = 0.2f;



		public override void SetDifficultyPreset(GameParameters.Preset preset)
		{
			string filename = SpaceProgramFunding.SettingsFilename(preset);
			if (!File.Exists(filename)) return;
			var settings = ConfigNode.Load(filename);

			bool.TryParse(settings.GetValue("launchCostsEnabled"), out isLaunchCostsEnabled);
			int.TryParse(settings.GetValue("launchCostsVAB"), out launchCostsLaunchPad);
			int.TryParse(settings.GetValue("launchCostsSPH"), out launchCostsRunway);
			int.TryParse(settings.GetValue("activeVesselCost"), out activeVesselCost);
			bool.TryParse(settings.GetValue("payWages"), out isKerbalWages);
			int.TryParse(settings.GetValue("availableWages"), out baseKerbalWage);
			int.TryParse(settings.GetValue("assignedWages"), out assignedKerbalWage);
			int.TryParse(settings.GetValue("emergencyBudgetMultiple"), out bigProjectMultiple);
			float.TryParse(settings.GetValue("emergencyBudgetFee"), out bigProjectFee);
		}


		public override bool Enabled(MemberInfo member, GameParameters parameters)
		{
			if (member?.Name != null) {
				switch (member.Name) {
					case "assignedKerbalWage":
					case "baseKerbalWage":
						return isKerbalWages;

					case "launchCostsLaunchPad":
					case "launchCostsRunway":
						return isLaunchCostsEnabled;

					default:
						break;
				}
			}

			return base.Enabled(member, parameters);
		}
	}
}
