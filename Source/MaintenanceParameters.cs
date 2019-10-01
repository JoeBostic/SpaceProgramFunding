using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SpaceProgramFunding.Source
{

	public class MaintenanceParameters : GameParameters.CustomParameterNode
	{

		public override GameParameters.GameMode GameMode => GameParameters.GameMode.CAREER;

		public override bool HasPresets => true;

		public override string Section => "Space Program Funding";

		public override string DisplaySection => "Space Program Funding";

		public override int SectionOrder => 3;

		public override string Title => "Facilities";





		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Should maintenance costs be applied for the Kerbal Space Center? This makes upgrading
		/// 		  the space center have a tradeoff due to higher maintenance costs. Maintenance
		/// 		  costs are a non-discretionary expenditure that is taken out of the funding first.</summary>
		[GameParameters.CustomParameterUI("Facility costs enabled", 
			toolTip = "Are facility maintenance costs enabled? Facility costs are applied each funding period \n" + 
			          "and multiplied by the specified value according to facility upgrade level. Level 1 multiplier \n" +
			          "is 0. Level 2 multiplier is 2. Level 3 multiplier 4.", autoPersistance = true)]
		public bool isBuildingCostsEnabled = true;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Administration Structure. </summary>
		[GameParameters.CustomIntParameterUI("Administration Cost", toolTip = "Administration building base maintenance cost.", minValue = 0, maxValue = 10000, stepSize = 1000, autoPersistance = true)]
		public int structureCostAdministration = 4000;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Astronaut Complex. </summary>
		[GameParameters.CustomIntParameterUI("Astronaut Complex Cost", toolTip = "Astronaut complex base maintenance cost.", minValue = 0, maxValue = 10000, stepSize = 1000, autoPersistance = true)]
		public int structureCostAstronautComplex = 2000;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Mission Control Structure. </summary>
		[GameParameters.CustomIntParameterUI("Mission Control Cost", toolTip = "Mission Control base maintenance cost.", minValue = 0, maxValue = 10000, stepSize = 1000, autoPersistance = true)]
		public int structureCostMissionControl = 6000;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Research & Development Structure. </summary>
		[GameParameters.CustomIntParameterUI("R&D Cost", toolTip = "Research & Development facility base maintenance cost.", minValue = 0, maxValue = 10000, stepSize = 1000, autoPersistance = true)]
		public int structureCostRnD = 8000;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Tracking-Station. </summary>
		[GameParameters.CustomIntParameterUI("Tracking Station Cost", toolTip = "Tracking Station base maintenance cost.", minValue = 0, maxValue = 10000, stepSize = 1000, autoPersistance = true)]
		public int structureCostTrackingStation = 4000;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Space-Plane Hangar. </summary>
		[GameParameters.CustomIntParameterUI("Space-Plane Hangar Cost", toolTip = "Space-Plane Hanger (SPH) base maintenance cost.", minValue = 0, maxValue = 10000, stepSize = 1000, autoPersistance = true)]
		public int structureCostSph = 8000;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Vehicle Assembly Building. </summary>
		[GameParameters.CustomIntParameterUI("Vehicle Assembly Building Cost", toolTip = "Vehicle Assembly Building (VAB) base maintenance cost.", minValue = 0, maxValue = 10000, stepSize = 1000, autoPersistance = true)]
		public int structureCostVab = 8000;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Other structures (added by mods) </summary>
		[GameParameters.CustomIntParameterUI("Other Facility Cost", toolTip = "Other facilities (added by mods) base maintenance cost.", minValue = 0, maxValue = 10000, stepSize = 1000, autoPersistance = true)]
		public int structureCostOtherFacility = 5000;


		public override void SetDifficultyPreset(GameParameters.Preset preset)
		{
			string filename = SpaceProgramFunding.SettingsFilename(preset);
			if (!File.Exists(filename)) return;
			var settings = ConfigNode.Load(filename);

			bool.TryParse(settings.GetValue("buildingCostsEnabled"), out isBuildingCostsEnabled);
			int.TryParse(settings.GetValue("sphCost"), out structureCostSph);
			int.TryParse(settings.GetValue("missionControlCost"), out structureCostMissionControl);
			int.TryParse(settings.GetValue("astronautComplexCost"), out structureCostAstronautComplex);
			int.TryParse(settings.GetValue("administrationCost"), out structureCostAdministration);
			int.TryParse(settings.GetValue("vabCost"), out structureCostVab);
			int.TryParse(settings.GetValue("trackingStationCost"), out structureCostTrackingStation);
			int.TryParse(settings.GetValue("rndCost"), out structureCostRnD);
			int.TryParse(settings.GetValue("otherFacilityCost"), out structureCostOtherFacility);
		}


		public override bool Enabled(MemberInfo member, GameParameters parameters)
		{
			if (member?.Name != null) {
				switch (member.Name) {

					/*
					 * Building maintenance cost sliders only appear if maintenance costs are
					 * enabled.
					 */
					case "structureCostAdministration":
					case "structureCostAstronautComplex":
					case "structureCostMissionControl":
					case "structureCostOtherFacility":
					case "structureCostRnD":
					case "structureCostTrackingStation":
					case "structureCostSph":
					case "structureCostVab":
						return isBuildingCostsEnabled;

					default:
						break;
				}
			}

			return base.Enabled(member, parameters);
		}
	}
}
