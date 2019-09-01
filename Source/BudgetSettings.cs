// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Project: SpaceProgramFunding -- Transforms KSP funding model to play like a governmental space program.
// Source:  https://github.com/JoeBostic/SpaceProgramFunding
// License: https://github.com/JoeBostic/SpaceProgramFunding/wiki/MIT-License
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Globalization;
using System.IO;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;

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

		public const float _settingsWidth = 500;
		public const float _settingsHeight = 600;
		private Vector2 _settingsScrollViewPosition = new Vector2(0, 0);


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
					filename = "BalancedDefaults.cfg";
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


			Boolean.TryParse(settings.GetValue("contractInterceptor"), out isContractInterceptor);
			Int32.TryParse(settings.GetValue("FundsPerRep"), out fundsPerRep);
			Boolean.TryParse(settings.GetValue("coverCosts"), out isCostsCovered);
			Boolean.TryParse(settings.GetValue("stopTimewarp"), out isAlarmClockPerBudget);
			Boolean.TryParse(settings.GetValue("decayEnabled"), out isRepDecayEnabled);
			Single.TryParse(settings.GetValue("friendlyInterval"), out budgetIntervalDays);
			Int32.TryParse(settings.GetValue("repDecay"), out repDecayRate);
			Int32.TryParse(settings.GetValue("minimumRep"), out minimumRep);
			Int32.TryParse(settings.GetValue("multiplier"), out budgetRepMultiplier);
			Int32.TryParse(settings.GetValue("availableWages"), out baseKerbalWage);
			Int32.TryParse(settings.GetValue("assignedWages"), out assignedKerbalWage);
			Single.TryParse(settings.GetValue("activeVesselCost"), out activeVesselCost);
			Boolean.TryParse(settings.GetValue("VesselCostsEnabled"), out isActiveVesselCost);
			Boolean.TryParse(settings.GetValue("buildingCostsEnabled"), out isBuildingCostsEnabled);
			Boolean.TryParse(settings.GetValue("launchCostsEnabled"), out isLaunchCostsEnabled);
			Int32.TryParse(settings.GetValue("launchCostsVAB"), out launchCostsLaunchPad);
			Int32.TryParse(settings.GetValue("launchCostsSPH"), out launchCostsRunway);
			Int32.TryParse(settings.GetValue("sphCost"), out structureCostSph);
			Int32.TryParse(settings.GetValue("missionControlCost"), out structureCostMissionControl);
			Int32.TryParse(settings.GetValue("astronautComplexCost"), out structureCostAstronautComplex);
			Int32.TryParse(settings.GetValue("administrationCost"), out structureCostAdministration);
			Int32.TryParse(settings.GetValue("vabCost"), out structureCostVab);
			Int32.TryParse(settings.GetValue("trackingStationCost"), out structureCostTrackingStation);
			Int32.TryParse(settings.GetValue("rndCost"), out structureCostRnD);
			Int32.TryParse(settings.GetValue("otherFacilityCost"), out structureCostOtherFacility);
			Boolean.TryParse(settings.GetValue("kerbalDeathPenaltyActive"), out isKerbalDeathPenalty);
			Int32.TryParse(settings.GetValue("kerbalDeathPenalty"), out kerbalDeathPenalty);
			Int32.TryParse(settings.GetValue("sciencePointCost"), out sciencePointCost);
			Int32.TryParse(settings.GetValue("emergencyBudgetMultiple"), out bigProjectMultiple);
			Int32.TryParse(settings.GetValue("emergencyBudgetFee"), out emergencyBudgetFee);
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Handles the layout of the settings window.</summary>
		///
		/// <param name="windowID"> Identifier for the window.</param>
		public void SettingsGUI(int windowID)
		{
			const int ledgerWidth = 185;
			const int labelWidth = 270;
			const int indentWidth = 35;
			const int modWidth = ledgerWidth + labelWidth;

			Assert.IsTrue(BudgetSettings.Instance != null);
			if (BudgetSettings.Instance == null) return;

			var label_style = new GUIStyle(GUI.skin.label);
			label_style.normal.textColor = label_style.normal.textColor = Color.white;

			GUILayout.BeginVertical(GUILayout.Width(_settingsWidth));

			_settingsScrollViewPosition = GUILayout.BeginScrollView(_settingsScrollViewPosition,
				GUILayout.Width(_settingsWidth), GUILayout.Height(_settingsHeight));

			GUILayout.BeginHorizontal(GUILayout.Width(modWidth));
			GUILayout.Label("Funding Interval: ", label_style);
			var day_string = GUILayout.TextField(budgetIntervalDays.ToString(CultureInfo.CurrentCulture), GUILayout.Width(50));
			if (Int32.TryParse(day_string, out var day_number)) {
				day_number = Math.Max(day_number, 1);
				budgetIntervalDays = day_number;
			}
			GUILayout.Label(" days", label_style);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal(GUILayout.Width(modWidth));
			GUILayout.Label(
				"Funding Multiplier times Rep: " + budgetRepMultiplier.ToString("n0"),
				label_style, GUILayout.MinWidth(labelWidth));
			budgetRepMultiplier =
				(int)GUILayout.HorizontalSlider(budgetRepMultiplier / 100.0f, 0, 50,
					GUILayout.MinWidth(ledgerWidth)) * 100;
			GUILayout.EndHorizontal();


			GUILayout.BeginHorizontal(GUILayout.Width(modWidth));
			GUILayout.Label("Minimum Reputation: " + minimumRep, label_style,
				GUILayout.MinWidth(labelWidth));
			minimumRep = (int)GUILayout.HorizontalSlider(minimumRep * 10, 0, 100, GUILayout.MinWidth(ledgerWidth)) / 10;
			GUILayout.EndHorizontal();


			isContractInterceptor = GUILayout.Toggle(
				isContractInterceptor, "Contracts pay rep instead of funds?",
				GUILayout.MaxWidth(modWidth));
			if (isContractInterceptor) {
				GUILayout.BeginHorizontal(GUILayout.Width(modWidth));
				GUILayout.Space(indentWidth);
				GUILayout.Label("Funds per Reputation Point: " + fundsPerRep.ToString("n0"),
					label_style, GUILayout.MinWidth(labelWidth - indentWidth));
				fundsPerRep =
					(int)GUILayout.HorizontalSlider(fundsPerRep / 1000.0f, 0, 50,
						GUILayout.MinWidth(ledgerWidth)) * 1000;
				GUILayout.EndHorizontal();
			}

			isCostsCovered = GUILayout.Toggle(isCostsCovered,
				"Fixed costs above funding level are forgiven?", GUILayout.MaxWidth(modWidth));

			isAlarmClockPerBudget = GUILayout.Toggle(
				isAlarmClockPerBudget, "Stop Time-warp / Set KAC Alarm on funding period?",
				GUILayout.MaxWidth(modWidth));

			isRepDecayEnabled = GUILayout.Toggle(isRepDecayEnabled,
				"Decay Reputation each funding period?", GUILayout.MaxWidth(modWidth));
			if (isRepDecayEnabled) {
				GUILayout.BeginHorizontal(GUILayout.Width(modWidth));
				GUILayout.Space(indentWidth);
				GUILayout.Label("Reputation Decay per period: " + repDecayRate, label_style,
					GUILayout.MinWidth(labelWidth - indentWidth));
				repDecayRate =
					(int)GUILayout.HorizontalSlider(repDecayRate, 0, 50,
						GUILayout.MinWidth(ledgerWidth));
				GUILayout.EndHorizontal();
			}


			isKerbalWages = GUILayout.Toggle(isKerbalWages,
				"Enable Kerbal wages (per Kerbal per XP level)?", GUILayout.MaxWidth(modWidth));
			if (isKerbalWages) {
				GUILayout.BeginHorizontal(GUILayout.Width(modWidth));
				GUILayout.Space(indentWidth);
				GUILayout.Label("Assigned Kerbal Wage: " + assignedKerbalWage.ToString("n0"),
					label_style, GUILayout.MinWidth(labelWidth - indentWidth));
				assignedKerbalWage =
					(int)GUILayout.HorizontalSlider(assignedKerbalWage / 100.0f, 0, 100,
						GUILayout.MinWidth(ledgerWidth)) * 100;
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal(GUILayout.Width(modWidth));
				GUILayout.Space(indentWidth);
				GUILayout.Label("Unassigned Kerbal Wage: " + baseKerbalWage.ToString("n0"),
					label_style, GUILayout.MinWidth(labelWidth - indentWidth));
				baseKerbalWage =
					(int)GUILayout.HorizontalSlider(baseKerbalWage / 100.0f, 0, 100,
						GUILayout.MinWidth(ledgerWidth)) * 100;
				GUILayout.EndHorizontal();
			}

			isKerbalDeathPenalty =
				GUILayout.Toggle(isKerbalDeathPenalty,
					"Enable Kerbal death penalty (Rep per XP level)?", GUILayout.MaxWidth(modWidth));
			if (isKerbalDeathPenalty) {
				GUILayout.BeginHorizontal(GUILayout.Width(modWidth));
				GUILayout.Space(indentWidth);
				GUILayout.Label("Reputation penalty: " + kerbalDeathPenalty, label_style,
					GUILayout.MinWidth(labelWidth - indentWidth));
				kerbalDeathPenalty =
					(int)GUILayout.HorizontalSlider(kerbalDeathPenalty, 0, 100,
						GUILayout.MinWidth(ledgerWidth));
				GUILayout.EndHorizontal();
			}

			// Big-Project multiple
			GUILayout.BeginHorizontal(GUILayout.Width(modWidth));
			GUILayout.Label("Big-Project multiple: " + bigProjectMultiple, label_style,
				GUILayout.MinWidth(labelWidth));
			bigProjectMultiple =
				(int)GUILayout.HorizontalSlider(bigProjectMultiple / 10.0f, 0, 25,
					GUILayout.MinWidth(ledgerWidth)) * 10;
			GUILayout.EndHorizontal();

			// Big-Project penalty
			GUILayout.BeginHorizontal(GUILayout.Width(modWidth));
			GUILayout.Label("Big-Project Fund Transfer Fee: " + emergencyBudgetFee + "%",
				label_style, GUILayout.MinWidth(labelWidth));
			emergencyBudgetFee =
				(int)GUILayout.HorizontalSlider(emergencyBudgetFee, 0, 50,
					GUILayout.MinWidth(ledgerWidth));
			GUILayout.EndHorizontal();

			// Cost per science point
			GUILayout.BeginHorizontal(GUILayout.Width(modWidth));
			GUILayout.Label("R&D cost per science point : " + sciencePointCost.ToString("n0"),
				label_style, GUILayout.MinWidth(labelWidth));
			sciencePointCost =
				(int)GUILayout.HorizontalSlider(sciencePointCost / 1000.0f, 0, 50,
					GUILayout.MinWidth(ledgerWidth)) * 1000;
			GUILayout.EndHorizontal();


			isBuildingCostsEnabled =
				GUILayout.Toggle(isBuildingCostsEnabled,
					"Structure maintenance costs (per Structure per level)", GUILayout.MaxWidth(modWidth));
			if (isBuildingCostsEnabled) {
				GUILayout.BeginHorizontal(GUILayout.Width(modWidth));
				GUILayout.Space(indentWidth);
				GUILayout.Label("Space-Plane Hangar: " + structureCostSph.ToString("n0"), label_style,
					GUILayout.MinWidth(labelWidth - indentWidth));
				structureCostSph =
					(int)GUILayout.HorizontalSlider(structureCostSph / 1000.0f, 0, 10,
						GUILayout.MinWidth(ledgerWidth)) * 1000;
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal(GUILayout.Width(modWidth));
				GUILayout.Space(indentWidth);
				GUILayout.Label("Mission Control: " + structureCostMissionControl.ToString("n0"),
					label_style, GUILayout.MinWidth(labelWidth - indentWidth));
				structureCostMissionControl =
					(int)GUILayout.HorizontalSlider(structureCostMissionControl / 1000.0f, 0, 10,
						GUILayout.MinWidth(ledgerWidth)) * 1000;
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal(GUILayout.Width(modWidth));
				GUILayout.Space(indentWidth);
				GUILayout.Label("Astronaut Complex: " + structureCostAstronautComplex.ToString("n0"),
					label_style, GUILayout.MinWidth(labelWidth - indentWidth));
				structureCostAstronautComplex =
					(int)GUILayout.HorizontalSlider(structureCostAstronautComplex / 1000.0f, 0, 10,
						GUILayout.MinWidth(ledgerWidth)) * 1000;
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal(GUILayout.Width(modWidth));
				GUILayout.Space(indentWidth);
				GUILayout.Label("Administration: " + structureCostAdministration.ToString("n0"),
					label_style, GUILayout.MinWidth(labelWidth - indentWidth));
				structureCostAdministration =
					(int)GUILayout.HorizontalSlider(structureCostAdministration / 1000.0f, 0, 10,
						GUILayout.MinWidth(ledgerWidth)) * 1000;
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal(GUILayout.Width(modWidth));
				GUILayout.Space(indentWidth);
				GUILayout.Label("Vehicle Assembly Building: " + structureCostVab.ToString("n0"),
					label_style, GUILayout.MinWidth(labelWidth - indentWidth));
				structureCostVab =
					(int)GUILayout.HorizontalSlider(structureCostVab / 1000.0f, 0, 10,
						GUILayout.MinWidth(ledgerWidth)) * 1000;
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal(GUILayout.Width(modWidth));
				GUILayout.Space(indentWidth);
				GUILayout.Label("Tracking Station: " + structureCostTrackingStation.ToString("n0"),
					label_style, GUILayout.MinWidth(labelWidth - indentWidth));
				structureCostTrackingStation =
					(int)GUILayout.HorizontalSlider(structureCostTrackingStation / 1000.0f, 0, 10,
						GUILayout.MinWidth(ledgerWidth)) * 1000;
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal(GUILayout.Width(modWidth));
				GUILayout.Space(indentWidth);
				GUILayout.Label("R&D Facility: " + structureCostRnD.ToString("n0"), label_style,
					GUILayout.MinWidth(labelWidth - indentWidth));
				structureCostRnD =
					(int)GUILayout.HorizontalSlider(structureCostRnD / 1000.0f, 0, 10,
						GUILayout.MinWidth(ledgerWidth)) * 1000;
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal(GUILayout.Width(modWidth));
				GUILayout.Space(indentWidth);
				GUILayout.Label(
					"Other Facilities (non-stock): " + structureCostOtherFacility.ToString("n0"),
					label_style, GUILayout.MinWidth(labelWidth - indentWidth));
				structureCostOtherFacility =
					(int)GUILayout.HorizontalSlider(structureCostOtherFacility / 1000.0f, 0, 10,
						GUILayout.MinWidth(ledgerWidth)) * 1000;
				GUILayout.EndHorizontal();
			}

			isLaunchCostsEnabled =
				GUILayout.Toggle(isLaunchCostsEnabled,
					"Launch costs (per launch per level per 100t)?", GUILayout.MaxWidth(modWidth));
			if (isLaunchCostsEnabled) {
				GUILayout.BeginHorizontal(GUILayout.Width(modWidth));
				GUILayout.Space(indentWidth);
				GUILayout.Label("Runway launch cost: " + launchCostsRunway, label_style,
					GUILayout.MinWidth(labelWidth - indentWidth));
				launchCostsRunway =
					(int)GUILayout.HorizontalSlider(launchCostsRunway / 10.0f, 0, 100,
						GUILayout.MinWidth(ledgerWidth)) * 10;
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal(GUILayout.Width(modWidth));
				GUILayout.Space(indentWidth);
				GUILayout.Label("Launch-pad launch cost: " + launchCostsLaunchPad, label_style,
					GUILayout.MinWidth(labelWidth - indentWidth));
				launchCostsLaunchPad =
					(int)GUILayout.HorizontalSlider(launchCostsLaunchPad / 100.0f, 0, 50,
						GUILayout.MinWidth(ledgerWidth)) * 100;
				GUILayout.EndHorizontal();
			}


			// Ship maintenance cost
			isActiveVesselCost =
				GUILayout.Toggle(isActiveVesselCost,
					"Enable monthly maintenance costs for vessels (per 100t)?", GUILayout.MaxWidth(modWidth));
			if (isActiveVesselCost) {
				GUILayout.BeginHorizontal(GUILayout.Width(modWidth));
				GUILayout.Space(indentWidth);
				GUILayout.Label("Maintenance cost: " + activeVesselCost, label_style,
					GUILayout.MinWidth(labelWidth - indentWidth));
				activeVesselCost =
					(int)GUILayout.HorizontalSlider(activeVesselCost / 100.0f, 0, 50,
						GUILayout.MinWidth(ledgerWidth)) * 100;
				GUILayout.EndHorizontal();
			}

			GUILayout.EndScrollView();

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Balanced")) LoadSettings(BudgetSettings.DifficultyEnum.Easy);
			if (GUILayout.Button("Normal")) LoadSettings(BudgetSettings.DifficultyEnum.Normal);
			if (GUILayout.Button("Hard")) LoadSettings(BudgetSettings.DifficultyEnum.Hard);
			GUILayout.EndHorizontal();

			GUILayout.EndVertical();
		}
	}
}