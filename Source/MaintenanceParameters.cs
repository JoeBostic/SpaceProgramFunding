using System.IO;
using System.Reflection;

namespace SpaceProgramFunding.Source
{
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary> The settings for space center facility maintenance. This is the third column of the settings dialog.</summary>
	public class MaintenanceParameters : GameParameters.CustomParameterNode
	{

		public override GameParameters.GameMode GameMode => GameParameters.GameMode.CAREER;
		public override bool HasPresets => true;
		public override string Section => "Space Program Funding";
		public override string DisplaySection => "Space Program Funding";
		public override int SectionOrder => 3;
		public override string Title => "Facilities";


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Should maintenance costs be applied for the Kerbal Space Center? This makes upgrading the space
		/// 		  center have a tradeoff due to higher maintenance costs. Maintenance costs are a non-
		/// 		  discretionary expenditure that is taken out of the funding first.</summary>
		[GameParameters.CustomParameterUI("Facility costs enabled", 
			toolTip = "Are facility maintenance costs enabled? Facility costs are applied each funding period \n" + 
			          "and multiplied by the specified value according to facility upgrade level. Level 1 multiplier \n" +
			          "is 0. Level 2 multiplier is 2. Level 3 multiplier 4.", autoPersistance = true)]
		public bool isBuildingCostsEnabled = true;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Administration Structure. </summary>
		[GameParameters.CustomIntParameterUI("Administration Cost", toolTip = "Administration building base maintenance cost.", minValue = 0, maxValue = 10000, stepSize = 100, autoPersistance = true)]
		public int structureCostAdministration = 4000;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Astronaut Complex. </summary>
		[GameParameters.CustomIntParameterUI("Astronaut Complex Cost", toolTip = "Astronaut complex base maintenance cost.", minValue = 0, maxValue = 10000, stepSize = 100, autoPersistance = true)]
		public int structureCostAstronautComplex = 2000;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Mission Control Structure. </summary>
		[GameParameters.CustomIntParameterUI("Mission Control Cost", toolTip = "Mission Control base maintenance cost.", minValue = 0, maxValue = 10000, stepSize = 100, autoPersistance = true)]
		public int structureCostMissionControl = 6000;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Research & Development Structure. </summary>
		[GameParameters.CustomIntParameterUI("R&D Cost", toolTip = "Research & Development facility base maintenance cost.", minValue = 0, maxValue = 10000, stepSize = 100, autoPersistance = true)]
		public int structureCostRnD = 7000;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Tracking-Station. </summary>
		[GameParameters.CustomIntParameterUI("Tracking Station Cost", toolTip = "Tracking Station base maintenance cost.", minValue = 0, maxValue = 10000, stepSize = 100, autoPersistance = true)]
		public int structureCostTrackingStation = 3000;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Space-Plane Hangar. </summary>
		[GameParameters.CustomIntParameterUI("Space-Plane Hangar Cost", toolTip = "Space-Plane Hanger (SPH) base maintenance cost.", minValue = 0, maxValue = 10000, stepSize = 100, autoPersistance = true)]
		public int structureCostSph = 6000;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Vehicle Assembly Building. </summary>
		[GameParameters.CustomIntParameterUI("Vehicle Assembly Building Cost", toolTip = "Vehicle Assembly Building (VAB) base maintenance cost.", minValue = 0, maxValue = 10000, stepSize = 100, autoPersistance = true)]
		public int structureCostVab = 6000;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Other structures (added by mods) </summary>
		[GameParameters.CustomIntParameterUI("Other Facility Cost", toolTip = "Other facilities (added by mods) base maintenance cost.", minValue = 0, maxValue = 10000, stepSize = 100, autoPersistance = true)]
		public int structureCostOtherFacility = 3000;


		public override void SetDifficultyPreset(GameParameters.Preset preset)
		{
			string filename = SpaceProgramFunding.SettingsFilename(preset);
			if (!File.Exists(filename)) return;
			var settings = ConfigNode.Load(filename);

			bool.TryParse(settings.GetValue(nameof(isBuildingCostsEnabled)), out isBuildingCostsEnabled);
			int.TryParse(settings.GetValue(nameof(structureCostSph)), out structureCostSph);
			int.TryParse(settings.GetValue(nameof(structureCostMissionControl)), out structureCostMissionControl);
			int.TryParse(settings.GetValue(nameof(structureCostAstronautComplex)), out structureCostAstronautComplex);
			int.TryParse(settings.GetValue(nameof(structureCostAdministration)), out structureCostAdministration);
			int.TryParse(settings.GetValue(nameof(structureCostVab)), out structureCostVab);
			int.TryParse(settings.GetValue(nameof(structureCostTrackingStation)), out structureCostTrackingStation);
			int.TryParse(settings.GetValue(nameof(structureCostRnD)), out structureCostRnD);
			int.TryParse(settings.GetValue(nameof(structureCostOtherFacility)), out structureCostOtherFacility);
		}


		public override bool Enabled(MemberInfo member, GameParameters parameters)
		{
			switch (member.Name) {

				/*
				 * Building maintenance cost sliders only appear if maintenance costs are
				 * enabled.
				 */
				case nameof(structureCostAdministration):
				case nameof(structureCostAstronautComplex):
				case nameof(structureCostMissionControl):
				case nameof(structureCostOtherFacility):
				case nameof(structureCostRnD):
				case nameof(structureCostTrackingStation):
				case nameof(structureCostSph):
				case nameof(structureCostVab):
					return isBuildingCostsEnabled;
			}

			return base.Enabled(member, parameters);
		}
	}
}
