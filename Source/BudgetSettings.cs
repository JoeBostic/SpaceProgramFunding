// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Project: SpaceProgramFunding -- Transforms KSP funding model to play like a governmental space program.
// Source:  https://github.com/JoeBostic/SpaceProgramFunding
// License: https://github.com/JoeBostic/SpaceProgramFunding/wiki/MIT-License
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.IO;
using JetBrains.Annotations;
using UnityEngine;

namespace SpaceProgramFunding.Source
{
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary> Manager for the settings that likely will not change. These settings are editable
	/// 		  through the Settings Pop-up Dialog or by editing the .cfg files directly.</summary>
	[KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
	internal class BudgetSettings : MonoBehaviour
	{
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Values that represent difficulty settings that the player can use.</summary>
		public enum DifficultyEnum
		{
			/// <summary> Minimal penalties... just free income. </summary>
			Easy,

			/// <summary> Balanced game-play. </summary>
			Normal,

			/// <summary> Hard game-play... necessary to keep reputation up. </summary>
			Hard
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The active vessel cost is determined from the mass of the vessel expressed as cost
		/// 		  per 100 tons. This represents the Mission Control staff and equipment expenses
		/// 		  that all ongoing missions require. Small vessels (such as tiny relay satellites)
		/// 		  imply low-maintenance missions so have less maintenance cost, as they should.</summary>
		public float activeVesselCost = 500.0f;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The assigned kerbal wage is paid to each Kerbal that is on a mission (not in the
		/// 		  Astronaut Complex). The wage is multiplied by the XP level of the Kerbal.</summary>
		public int assignedKerbalWage = 2000;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The base kerbal wage is paid to each Kerbal that is sitting around in the Astronaut
		/// 		  Complex. The wage is multiplied by the XP level of the Kerbal.</summary>
		public int baseKerbalWage = 1000;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The emergency budget is capped at a multiple of the gross budget. This prevents the
		/// 		  exploit of letting the emergency budget accumulate indefinitely. The value is the
		/// 		  number of reputation points per multiple. For example, reputation of 150 would be
		/// 		  3x multiple for a value of 50.</summary>
		public int bigProjectMultiple = 50;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The budget is run every month (typically). This specifies the number of days in a
		/// 		  budget period.</summary>
		public float budgetIntervalDays = 30;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The budget funds to grant is based on a multiple of the current reputation value. To
		/// 		  get more budget, get a higher reputation.</summary>
		public int budgetRepMultiplier = 2200;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Fee cost to moving money into the emergency budget fund. This needs to be large
		/// 		  enough to discourage keeping maximum funds flowing into emergency budget. The
		/// 		  emergency budget should just be used for big projects.</summary>
		public int emergencyBudgetFee = 20;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Mission reward funds are converted to reputation at the follow rate./summary></summary>
		public int fundsPerRep = 10000;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> If all active vessels require a maintenance cost, then this will be true.</summary>
		public bool isActiveVesselCost = true;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Should the budget period be logged in Kerbal Alarm Clock? If true, the player will be
		/// 		  aware when the budget period will end.</summary>
		public bool isAlarmClockPerBudget = true;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Should maintenance costs be applied for the Kerbal Space Center? This makes upgrading
		/// 		  the space center have a tradeoff due to higher maintenance costs. Maintenance
		/// 		  costs are a non-discretionary expenditure that is taken out of the budget first.</summary>
		public bool isBuildingCostsEnabled = true;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Should contracts reward reputation instead of funds? Typically, this is what you want
		/// 		  to do to fit with the philosophy of this mod.</summary>
		public bool isContractInterceptor = true;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Should non-discretionary costs that happen to exceed the current budget be forgiven
		/// 		  rather than charged to the player's current bank account? A responsible Kerbal
		/// 		  government would take care of these costs and this flag would be true. A more
		/// 		  mercenary government would set this flag to false and make the player pay these
		/// 		  costs regardless.</summary>
		public bool isCostsCovered;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Records if this mod has not yet been run. Some initial setups are necessary in such a
		/// 		  case. This value should start as 'true'.</summary>
		public bool isFirstRun = true;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Should a hit to reputation occur if a Kerbal is killed? As with all reputation hits,
		/// 		  it hurts the most when the reputation level is highest since gaining reputation
		/// 		  at high levels is extremely difficult.</summary>
		public bool isKerbalDeathPenalty = true;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Should Kerbals be paid wages? Wages are a non-discretionary expenditure that is taken
		/// 		  out of the budget first.</summary>
		public bool isKerbalWages = true;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Should costs for each launch (to cover wear-n-tear on launch facility) be charged
		/// 		  whenever a vessel is launched? Heavy vessels, and particularly with the launch-
		/// 		  pad, cause the launch costs to increase. This cost is a one-time charge.</summary>
		public bool isLaunchCostsEnabled = true;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Is reputation decay per budget period enabled? Reputation decay means the player must
		/// 		  always pat attention to reputation and perform missions as necessary to keep the
		/// 		  reputation level sustained.</summary>
		public bool isRepDecayEnabled;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The number of reputation points (per Kerbal XP level) to reduce when a Kerbal is
		/// 		  killed.</summary>
		public int kerbalDeathPenalty = 15;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The launch costs per 100t on the launch-pad. Essentially this is the rocket launch
		/// 		  cost. This is the cost per level of the launch-pad where the initial level equals
		/// 		  zero. This represents the wear-n-tear of the launch-pad where heavier rockets
		/// 		  cause more damage.</summary>
		public int launchCostsLaunchPad = 1000;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The launch costs per 100t on the runway. Essentially this is the space-plane launch
		/// 		  cost which should be pretty low. This is the cost per level of the runway where
		/// 		  the initial runway level equals zero. This number should be small (or even zero)
		/// 		  to encourage space-plane use.</summary>
		public int launchCostsRunway;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The minimum reputation to use when calculating gross budget. There is always a loyal
		/// 		  cadre within the Kerbal government that ensures a minimum budget.</summary>
		public int minimumRep = 20;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The number of reputation points deducted per budget period if reputation decay has
		/// 		  been enabled.</summary>
		public int repDecayRate = 5;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> When diverting budget to create science points, this is the number of credits it
		/// 		  takes to create one science point.</summary>
		public int sciencePointCost = 10000;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Administration Structure. </summary>
		public int structureCostAdministration = 4000;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Astronaut Complex. </summary>
		public int structureCostAstronautComplex = 2000;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Mission Control Structure. </summary>
		public int structureCostMissionControl = 6000;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Other structures (added by mods) </summary>
		public int structureCostOtherFacility = 5000;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Research & Development Structure. </summary>
		public int structureCostRnD = 8000;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Space-Plane Hangar. </summary>
		public int structureCostSph = 8000;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Tracking-Station. </summary>
		public int structureCostTrackingStation = 4000;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Vehicle Assembly Building. </summary>
		public int structureCostVab = 8000;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Reference to the singleton instance of this class object. </summary>
		/// <value> The instance. This may be null. </value>
		[CanBeNull]
		public static BudgetSettings Instance { get; private set; }


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Awakes this object.</summary>
		[UsedImplicitly]
		private void Awake()
		{
			if (Instance != null && Instance != this) {
				Destroy(this);
				return;
			}

			if (Instance == null) {
				Instance = this;
				DontDestroyOnLoad(this);
			}
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Executes the save action which saves all the current settings to the save-game file. </summary>
		/// <param name="savedNode"> The saved node. </param>
		public void OnSave(ConfigNode savedNode)
		{
			savedNode.SetValue("EmergencyFundMultiple", bigProjectMultiple, true);
			savedNode.SetValue("EmergencyFundFee", emergencyBudgetFee, true);
			savedNode.SetValue("sciencePointCost", sciencePointCost, true);
			savedNode.SetValue("ContractInterceptor", isContractInterceptor, true);
			savedNode.SetValue("FundsPerRep", fundsPerRep, true);
			savedNode.SetValue("CoverCosts", isCostsCovered, true);
			savedNode.SetValue("StopTimeWarp", isAlarmClockPerBudget, true);
			savedNode.SetValue("KerbalDeathPenaltyActive", isKerbalDeathPenalty, true);
			savedNode.SetValue("DecayEnabled", isRepDecayEnabled, true);
			savedNode.SetValue("MinimumRep", minimumRep, true);
			savedNode.SetValue("RepDecay", repDecayRate, true);
			savedNode.SetValue("Multiplier", budgetRepMultiplier, true);
			savedNode.SetValue("FriendlyInterval", budgetIntervalDays, true);
			savedNode.SetValue("KerbalWageActive", isKerbalWages, true);
			savedNode.SetValue("AvailableWages", baseKerbalWage, true);
			savedNode.SetValue("AssignedWages", assignedKerbalWage, true);
			savedNode.SetValue("VesselCostEnabled", isActiveVesselCost, true);
			savedNode.SetValue("VesselCost", activeVesselCost, true);
			savedNode.SetValue("FirstRun", isFirstRun, true);
			savedNode.SetValue("BuildingCostsEnabled", isBuildingCostsEnabled, true);
			savedNode.SetValue("sphCost", structureCostSph, true);
			savedNode.SetValue("missionControlCost", structureCostMissionControl, true);
			savedNode.SetValue("astronautComplexCost", structureCostAstronautComplex, true);
			savedNode.SetValue("administrationCost", structureCostAdministration, true);
			savedNode.SetValue("vabCost", structureCostVab, true);
			savedNode.SetValue("trackingStationCost", structureCostTrackingStation, true);
			savedNode.SetValue("rndCost", structureCostRnD, true);
			savedNode.SetValue("otherFacilityCost", structureCostOtherFacility, true);
			savedNode.SetValue("LaunchCostsEnabled", isLaunchCostsEnabled, true);
			savedNode.SetValue("LaunchCostsVAB", launchCostsLaunchPad, true);
			savedNode.SetValue("LaunchCostsSPH", launchCostsRunway, true);
			savedNode.SetValue("kerbalDeathPenalty", kerbalDeathPenalty, true);
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Executes the load action which loads the settings from the saved-game file. </summary>
		/// <param name="node"> The node. </param>
		public void OnLoad(ConfigNode node)
		{
			//masterSwitch = true;

			node.TryGetValue("EmergencyFundMultiple", ref bigProjectMultiple);
			node.TryGetValue("EmergencyFundFee", ref emergencyBudgetFee);
			node.TryGetValue("sciencePointCost", ref sciencePointCost);
			node.TryGetValue("ContractInterceptor", ref isContractInterceptor);
			node.TryGetValue("FundsPerRep", ref fundsPerRep);
			node.TryGetValue("CoverCosts", ref isCostsCovered);
			node.TryGetValue("StopTimeWarp", ref isAlarmClockPerBudget);
			node.TryGetValue("KerbalDeathPenaltyActive", ref isKerbalDeathPenalty);
			node.TryGetValue("DecayEnabled", ref isRepDecayEnabled);
			node.TryGetValue("RepDecay", ref repDecayRate);
			node.TryGetValue("MinimumRep", ref minimumRep);
			node.TryGetValue("Multiplier", ref budgetRepMultiplier);
			node.TryGetValue("FriendlyInterval", ref budgetIntervalDays);
			node.TryGetValue("KerbalWageActive", ref isKerbalWages);
			node.TryGetValue("AvailableWages", ref baseKerbalWage);
			node.TryGetValue("AssignedWages", ref assignedKerbalWage);
			node.TryGetValue("VesselCostEnabled", ref isActiveVesselCost);
			node.TryGetValue("VesselCost", ref activeVesselCost);
			node.TryGetValue("FirstRun", ref isFirstRun);
			node.TryGetValue("BuildingCostsEnabled", ref isBuildingCostsEnabled);
			node.TryGetValue("sphCost", ref structureCostSph);
			node.TryGetValue("missionControlCost", ref structureCostMissionControl);
			node.TryGetValue("astronautComplexCost", ref structureCostAstronautComplex);
			node.TryGetValue("administrationCost", ref structureCostAdministration);
			node.TryGetValue("vabCost", ref structureCostVab);
			node.TryGetValue("trackingStationCost", ref structureCostTrackingStation);
			node.TryGetValue("rndCost", ref structureCostRnD);
			node.TryGetValue("otherFacilityCost", ref structureCostOtherFacility);
			node.TryGetValue("LaunchCostsEnabled", ref isLaunchCostsEnabled);
			node.TryGetValue("LaunchCostsVAB", ref launchCostsLaunchPad);
			node.TryGetValue("LaunchCostsSPH", ref launchCostsRunway);
			node.TryGetValue("kerbalDeathPenalty", ref kerbalDeathPenalty);
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Fetches the filename to use for settings according to the difficulty level specified.</summary>
		///
		/// <exception cref="ArgumentOutOfRangeException"> Thrown when one or more arguments are outside
		/// 											   the required range.</exception>
		///
		/// <param name="diff"> The difficulty level to load recommended settings for.</param>
		///
		/// <returns> A string that is the filename to fetch settings from.</returns>
		private string SettingsFilename(DifficultyEnum diff)
		{
			string filename;
			switch (diff) {
				case DifficultyEnum.Easy:
					filename = "EasyDefaults.cfg";
					break;
				case DifficultyEnum.Normal:
					filename = "NormalDefaults.cfg";
					break;
				case DifficultyEnum.Hard:
					filename = "HardDefaults.cfg";
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(diff), diff, null);
			}

			return KSPUtil.ApplicationRootPath + "/GameData/SpaceProgramFunding/" + filename;
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Called when the mod is run for the first time. It will default to the normal
		/// 		  difficulty settings.</summary>
		public void FirstRun()
		{
			isFirstRun = false;

			LoadSettings(DifficultyEnum.Normal);
			//SpawnSettingsDialog();
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Loads the settings from a file. The settings loaded are the recommended settings for
		/// 		  the specified difficulty level.</summary>
		///
		/// <param name="diff"> The difficulty level to load recommended settings for.</param>
		public void LoadSettings(DifficultyEnum diff)
		{
			var filename = SettingsFilename(diff);

			if (!File.Exists(filename)) return;

			var settings = ConfigNode.Load(filename);


			bool.TryParse(settings.GetValue("contractInterceptor"), out isContractInterceptor);
			int.TryParse(settings.GetValue("FundsPerRep"), out fundsPerRep);
			bool.TryParse(settings.GetValue("coverCosts"), out isCostsCovered);
			bool.TryParse(settings.GetValue("stopTimewarp"), out isAlarmClockPerBudget);
			bool.TryParse(settings.GetValue("decayEnabled"), out isRepDecayEnabled);
			float.TryParse(settings.GetValue("friendlyInterval"), out budgetIntervalDays);
			int.TryParse(settings.GetValue("repDecay"), out repDecayRate);
			int.TryParse(settings.GetValue("minimumRep"), out minimumRep);
			int.TryParse(settings.GetValue("multiplier"), out budgetRepMultiplier);
			int.TryParse(settings.GetValue("availableWages"), out baseKerbalWage);
			int.TryParse(settings.GetValue("assignedWages"), out assignedKerbalWage);
			float.TryParse(settings.GetValue("activeVesselCost"), out activeVesselCost);
			bool.TryParse(settings.GetValue("VesselCostsEnabled"), out isActiveVesselCost);
			bool.TryParse(settings.GetValue("buildingCostsEnabled"), out isBuildingCostsEnabled);
			bool.TryParse(settings.GetValue("launchCostsEnabled"), out isLaunchCostsEnabled);
			int.TryParse(settings.GetValue("launchCostsVAB"), out launchCostsLaunchPad);
			int.TryParse(settings.GetValue("launchCostsSPH"), out launchCostsRunway);
			int.TryParse(settings.GetValue("sphCost"), out structureCostSph);
			int.TryParse(settings.GetValue("missionControlCost"), out structureCostMissionControl);
			int.TryParse(settings.GetValue("astronautComplexCost"), out structureCostAstronautComplex);
			int.TryParse(settings.GetValue("administrationCost"), out structureCostAdministration);
			int.TryParse(settings.GetValue("vabCost"), out structureCostVab);
			int.TryParse(settings.GetValue("trackingStationCost"), out structureCostTrackingStation);
			int.TryParse(settings.GetValue("rndCost"), out structureCostRnD);
			int.TryParse(settings.GetValue("otherFacilityCost"), out structureCostOtherFacility);
			bool.TryParse(settings.GetValue("kerbalDeathPenaltyActive"), out isKerbalDeathPenalty);
			int.TryParse(settings.GetValue("kerbalDeathPenalty"), out kerbalDeathPenalty);
			int.TryParse(settings.GetValue("sciencePointCost"), out sciencePointCost);
			int.TryParse(settings.GetValue("emergencyBudgetMultiple"), out bigProjectMultiple);
			int.TryParse(settings.GetValue("emergencyBudgetFee"), out emergencyBudgetFee);
		}
	}
}