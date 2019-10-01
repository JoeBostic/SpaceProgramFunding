using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SpaceProgramFunding.Source
{
	internal class MaintenanceParameters : GameParameters.CustomParameterNode
	{

		public override GameParameters.GameMode GameMode => GameParameters.GameMode.CAREER;

		public override bool HasPresets => true;

		public override string Section => "Space Program Funding";

		public override string DisplaySection => "Space Program Funding";

		public override int SectionOrder => 2;

		public override string Title => "Maintenance";



		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> If all active vessels require a maintenance cost, then this will be true.</summary>
		[GameParameters.CustomParameterUI("Active vessel costs enabled", autoPersistance = true)]
		public bool isActiveVesselCost = true;



		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Should Kerbals be paid wages? Wages are a non-discretionary expenditure that is taken
		/// 		  out of the funding first.</summary>
		[GameParameters.CustomParameterUI("Kerbals have wages", autoPersistance = true)]
		public bool isKerbalWages = true;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Should costs for each launch (to cover wear-n-tear on launch facility) be charged
		/// 		  whenever a vessel is launched? Heavy vessels, and particularly with the launch-
		/// 		  pad, cause the launch costs to increase. This cost is a one-time charge.</summary>
		[GameParameters.CustomParameterUI("Launch Costs Enabled", autoPersistance = true)]
		public bool isLaunchCostsEnabled = true;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The launch costs per 100t on the launch-pad. Essentially this is the rocket launch
		/// 		  cost. This is the cost per level of the launch-pad where the initial level equals
		/// 		  zero. This represents the wear-n-tear of the launch-pad where heavier rockets
		/// 		  cause more damage.</summary>
		[GameParameters.CustomIntParameterUI("Launchpad Launch Cost", minValue = 0, maxValue = 5000, autoPersistance = true)]
		public int launchCostsLaunchPad = 1000;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The launch costs per 100t on the runway. Essentially this is the space-plane launch
		/// 		  cost which should be pretty low. This is the cost per level of the runway where
		/// 		  the initial runway level equals zero. This number should be small (or even zero)
		/// 		  to encourage space-plane use.</summary>
		[GameParameters.CustomIntParameterUI("Runway Launch Cost", minValue = 0, maxValue = 5000, autoPersistance = true)]
		public int launchCostsRunway;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Should maintenance costs be applied for the Kerbal Space Center? This makes upgrading
		/// 		  the space center have a tradeoff due to higher maintenance costs. Maintenance
		/// 		  costs are a non-discretionary expenditure that is taken out of the funding first.</summary>
		[GameParameters.CustomParameterUI("Facility costs enabled", autoPersistance = true)]
		public bool isBuildingCostsEnabled = true;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Administration Structure. </summary>
		[GameParameters.CustomIntParameterUI("Administration Cost", minValue = 1000, maxValue = 10000, autoPersistance = true)]
		public int structureCostAdministration = 4000;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Astronaut Complex. </summary>
		[GameParameters.CustomIntParameterUI("Astronaut Complex Cost", minValue = 1000, maxValue = 10000, autoPersistance = true)]
		public int structureCostAstronautComplex = 2000;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Mission Control Structure. </summary>
		[GameParameters.CustomIntParameterUI("Mission Control Cost", minValue = 1000, maxValue = 10000, autoPersistance = true)]
		public int structureCostMissionControl = 6000;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Research & Development Structure. </summary>
		[GameParameters.CustomIntParameterUI("R&D Cost", minValue = 1000, maxValue = 10000, autoPersistance = true)]
		public int structureCostRnD = 8000;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Tracking-Station. </summary>
		[GameParameters.CustomIntParameterUI("Tracking Station Cost", minValue = 1000, maxValue = 10000, autoPersistance = true)]
		public int structureCostTrackingStation = 4000;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Space-Plane Hangar. </summary>
		[GameParameters.CustomIntParameterUI("SPH Cost", minValue = 1000, maxValue = 10000, autoPersistance = true)]
		public int structureCostSph = 8000;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Vehicle Assembly Building. </summary>
		[GameParameters.CustomIntParameterUI("VAB Cost", minValue = 1000, maxValue = 10000, autoPersistance = true)]
		public int structureCostVab = 8000;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Other structures (added by mods) </summary>
		[GameParameters.CustomParameterUI("Other Facility Cost", autoPersistance = true)]
		public int structureCostOtherFacility = 5000;



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


		public override bool Enabled(MemberInfo member, GameParameters parameters)
		{
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

				/*
				 * Launch cost sliders only appear if launch costs are enabled.
				 */
				case "launchCostsLaunchPad":
				case "launchCostsRunway":
					return isLaunchCostsEnabled;

				default:
					break;
			}

			return base.Enabled(member, parameters);
		}










	}
}
