// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Project: SpaceProgramFunding -- Transforms KSP funding model to play like a governmental space program.
// Source:  https://github.com/JoeBostic/SpaceProgramFunding
// License: https://github.com/JoeBostic/SpaceProgramFunding/wiki/MIT-License
// --------------------------------------------------------------------------------------------------------------------


#if false

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

		public const float SETTINGS_WIDTH = 500;
		public const float SETTINGS_HEIGHT = 600;
		private Vector2 _settingsScrollViewPosition = new Vector2(0, 0);


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The current version of the mod.</summary>
		public const string currentVersion = "1.1";


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The mod version number as saved.</summary>
		public string saveGameVersion = "0.0";


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The active vessel cost is determined from the mass of the vessel expressed as cost
		/// 		  per 100 tons. This represents the Mission Control staff and equipment expenses
		/// 		  that all ongoing missions require. Small vessels (such as tiny relay satellites)
		/// 		  imply low-maintenance missions so have less maintenance cost, as they should.</summary>
		//public float activeVesselCost = 500.0f;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The assigned kerbal wage is paid to each Kerbal that is on a mission (not in the
		/// 		  Astronaut Complex). The wage is multiplied by the XP level of the Kerbal.</summary>
		//public int assignedKerbalWage = 2000;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The base kerbal wage is paid to each Kerbal that is sitting around in the Astronaut
		/// 		  Complex. The wage is multiplied by the XP level of the Kerbal.</summary>
		//public int baseKerbalWage = 1000;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The funding is run every month (typically). This specifies the number of days in a
		/// 		  funding period.</summary>
		//public float fundingIntervalDays = 30;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The funds to grant is based on a multiple of the current reputation value. To
		/// 		  get more funding, get a higher reputation.</summary>
		//public int fundingRepMultiplier = 2200;


		//public bool isBigProjectAllowed = true;
		//public bool isReputationAllowed = true;
		//public bool isScienceAllowed = true;

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The big-project is capped at a multiple of the gross funding. This prevents the
		/// 		  exploit of letting the big-project accumulate indefinitely. The value is the
		/// 		  number of reputation points per multiple. For example, reputation of 150 would be
		/// 		  3x multiple for a value of 50.</summary>
		//public int bigProjectMultiple = 50;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Fee cost to moving money into the big-project fund. This needs to be large
		/// 		  enough to discourage keeping maximum funds flowing into big-project. The
		/// 		  big-project account should just be used for big-projects.</summary>
		//public int bigProjectFee = 20;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Mission reward funds are converted to reputation at the follow rate./summary></summary>
		//public int fundsPerRep = 10000;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> If all active vessels require a maintenance cost, then this will be true.</summary>
		//public bool isActiveVesselCost = true;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Should maintenance costs be applied for the Kerbal Space Center? This makes upgrading
		/// 		  the space center have a tradeoff due to higher maintenance costs. Maintenance
		/// 		  costs are a non-discretionary expenditure that is taken out of the funding first.</summary>
		//public bool isBuildingCostsEnabled = true;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Should contracts reward reputation instead of funds? Typically, this is what you want
		/// 		  to do to fit with the philosophy of this mod.</summary>
		//public bool isContractInterceptor = true;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Should non-discretionary costs that happen to exceed the current funding be forgiven
		/// 		  rather than charged to the player's current bank account? A responsible Kerbal
		/// 		  government would take care of these costs and this flag would be true. A more
		/// 		  mercenary government would set this flag to false and make the player pay these
		/// 		  costs regardless.</summary>
		//public bool isCostsCovered;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Records if this mod has not yet been run. Some initial setups are necessary in such a
		/// 		  case. This value should start as 'true'.</summary>
		public bool isFirstRun = true;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Should a hit to reputation occur if a Kerbal is killed? As with all reputation hits,
		/// 		  it hurts the most when the reputation level is highest since gaining reputation
		/// 		  at high levels is extremely difficult.</summary>
		//public bool isKerbalDeathPenalty = true;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Should Kerbals be paid wages? Wages are a non-discretionary expenditure that is taken
		/// 		  out of the funding first.</summary>
		//public bool isKerbalWages = true;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Should costs for each launch (to cover wear-n-tear on launch facility) be charged
		/// 		  whenever a vessel is launched? Heavy vessels, and particularly with the launch-
		/// 		  pad, cause the launch costs to increase. This cost is a one-time charge.</summary>
		//public bool isLaunchCostsEnabled = true;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Is reputation decay per funding period enabled? Reputation decay means the player must
		/// 		  always pat attention to reputation and perform missions as necessary to keep the
		/// 		  reputation level sustained.</summary>
		//public bool isRepDecayEnabled;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The number of reputation points (per Kerbal XP level) to reduce when a Kerbal is
		/// 		  killed.</summary>
		//public int kerbalDeathPenalty = 15;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The launch costs per 100t on the launch-pad. Essentially this is the rocket launch
		/// 		  cost. This is the cost per level of the launch-pad where the initial level equals
		/// 		  zero. This represents the wear-n-tear of the launch-pad where heavier rockets
		/// 		  cause more damage.</summary>
		//public int launchCostsLaunchPad = 1000;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The launch costs per 100t on the runway. Essentially this is the space-plane launch
		/// 		  cost which should be pretty low. This is the cost per level of the runway where
		/// 		  the initial runway level equals zero. This number should be small (or even zero)
		/// 		  to encourage space-plane use.</summary>
		//public int launchCostsRunway;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The minimum reputation to use when calculating gross funding. There is always a loyal
		/// 		  cadre within the Kerbal government that ensures a minimum funding.</summary>
//		public int minimumRep = 20;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The number of reputation points deducted per funding period if reputation decay has
		/// 		  been enabled.</summary>
//		public int repDecayRate = 5;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> When diverting funds to create science points, this is the number of credits it
		/// 		  takes to create one science point.</summary>
//		public int sciencePointCost = 10000;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Administration Structure. </summary>
//		public int structureCostAdministration = 4000;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Astronaut Complex. </summary>
//		public int structureCostAstronautComplex = 2000;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Mission Control Structure. </summary>
//		public int structureCostMissionControl = 6000;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Other structures (added by mods) </summary>
//		public int structureCostOtherFacility = 5000;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Research & Development Structure. </summary>
//		public int structureCostRnD = 8000;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Space-Plane Hangar. </summary>
//		public int structureCostSph = 8000;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Tracking-Station. </summary>
//		public int structureCostTrackingStation = 4000;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Vehicle Assembly Building. </summary>
//		public int structureCostVab = 8000;


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

			if (Instance != null) return;

			Instance = this;
			DontDestroyOnLoad(this);
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Executes the save action which saves all the current settings to the save-game file. </summary>
		/// <param name="node"> The saved node. </param>
		public void OnSave(ConfigNode node)
		{
			node.SetValue("saveGameVersion", currentVersion, true);

			// New as of v1.1
			//node.SetValue("IsBigProjectAllowed", isBigProjectAllowed, true);
			//node.SetValue("IsReputationAllowed", isReputationAllowed, true);
			//node.SetValue("IsScienceAllowed", isScienceAllowed, true);

			//node.SetValue("EmergencyFundMultiple", bigProjectMultiple, true);
			//node.SetValue("EmergencyFundFee", bigProjectFee, true);
			//node.SetValue("sciencePointCost", sciencePointCost, true);
			//node.SetValue("ContractInterceptor", isContractInterceptor, true);
			//node.SetValue("FundsPerRep", fundsPerRep, true);
			//node.SetValue("CoverCosts", isCostsCovered, true);
			//node.SetValue("KerbalDeathPenaltyActive", isKerbalDeathPenalty, true);
			//node.SetValue("DecayEnabled", isRepDecayEnabled, true);
			//node.SetValue("MinimumRep", minimumRep, true);
			//node.SetValue("RepDecay", repDecayRate, true);
			//node.SetValue("Multiplier", fundingRepMultiplier, true);
			//node.SetValue("FriendlyInterval", fundingIntervalDays, true);
			//node.SetValue("KerbalWageActive", isKerbalWages, true);
			//node.SetValue("AvailableWages", baseKerbalWage, true);
			//node.SetValue("AssignedWages", assignedKerbalWage, true);
			//node.SetValue("VesselCostEnabled", isActiveVesselCost, true);
			//node.SetValue("VesselCost", activeVesselCost, true);
			node.SetValue("FirstRun", isFirstRun, true);
			//node.SetValue("BuildingCostsEnabled", isBuildingCostsEnabled, true);
			//node.SetValue("sphCost", structureCostSph, true);
			//node.SetValue("missionControlCost", structureCostMissionControl, true);
			//node.SetValue("astronautComplexCost", structureCostAstronautComplex, true);
			//node.SetValue("administrationCost", structureCostAdministration, true);
			//node.SetValue("vabCost", structureCostVab, true);
			//node.SetValue("trackingStationCost", structureCostTrackingStation, true);
			//node.SetValue("rndCost", structureCostRnD, true);
			//node.SetValue("otherFacilityCost", structureCostOtherFacility, true);
			//node.SetValue("LaunchCostsEnabled", isLaunchCostsEnabled, true);
			//node.SetValue("LaunchCostsVAB", launchCostsLaunchPad, true);
			//node.SetValue("LaunchCostsSPH", launchCostsRunway, true);
			//node.SetValue("kerbalDeathPenalty", kerbalDeathPenalty, true);
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Executes the load action which loads the settings from the saved-game file. </summary>
		/// <param name="node"> The node. </param>
		public void OnLoad(ConfigNode node)
		{
			//masterSwitch = true;

			node.TryGetValue("saveGameVersion", ref saveGameVersion);

			/*
			 * Old games don't store these options, so set them to default values when such an old
			 * game is loaded. Afterwards, these values get saved and loaded normally.
			 */
			if (saveGameVersion == "1.0" || saveGameVersion == "0.0") {
				//isBigProjectAllowed = true;
				//isReputationAllowed = true;
				//isScienceAllowed = true;
			} else {
				// New as of v1.1
				//node.TryGetValue("IsBigProjectAllowed", ref isBigProjectAllowed);
				//node.TryGetValue("IsReputationAllowed", ref isReputationAllowed);
				//node.TryGetValue("IsScienceAllowed", ref isScienceAllowed);
			}


			//node.TryGetValue("EmergencyFundMultiple", ref bigProjectMultiple);
			//node.TryGetValue("EmergencyFundFee", ref bigProjectFee);
			//node.TryGetValue("sciencePointCost", ref sciencePointCost);
			//node.TryGetValue("ContractInterceptor", ref isContractInterceptor);
			//node.TryGetValue("FundsPerRep", ref fundsPerRep);
			//node.TryGetValue("CoverCosts", ref isCostsCovered);
			//node.TryGetValue("KerbalDeathPenaltyActive", ref isKerbalDeathPenalty);
			//node.TryGetValue("DecayEnabled", ref isRepDecayEnabled);
			//node.TryGetValue("RepDecay", ref repDecayRate);
			//node.TryGetValue("MinimumRep", ref minimumRep);
			//node.TryGetValue("Multiplier", ref fundingRepMultiplier);
			//node.TryGetValue("FriendlyInterval", ref fundingIntervalDays);
			//node.TryGetValue("KerbalWageActive", ref isKerbalWages);
			//node.TryGetValue("AvailableWages", ref baseKerbalWage);
			//node.TryGetValue("AssignedWages", ref assignedKerbalWage);
			//node.TryGetValue("VesselCostEnabled", ref isActiveVesselCost);
			//node.TryGetValue("VesselCost", ref activeVesselCost);
			node.TryGetValue("FirstRun", ref isFirstRun);
			//node.TryGetValue("BuildingCostsEnabled", ref isBuildingCostsEnabled);
			//node.TryGetValue("sphCost", ref structureCostSph);
			//node.TryGetValue("missionControlCost", ref structureCostMissionControl);
			//node.TryGetValue("astronautComplexCost", ref structureCostAstronautComplex);
			//node.TryGetValue("administrationCost", ref structureCostAdministration);
			//node.TryGetValue("vabCost", ref structureCostVab);
			//node.TryGetValue("trackingStationCost", ref structureCostTrackingStation);
			//node.TryGetValue("rndCost", ref structureCostRnD);
			//node.TryGetValue("otherFacilityCost", ref structureCostOtherFacility);
			//node.TryGetValue("LaunchCostsEnabled", ref isLaunchCostsEnabled);
			//node.TryGetValue("LaunchCostsVAB", ref launchCostsLaunchPad);
			//node.TryGetValue("LaunchCostsSPH", ref launchCostsRunway);
			//node.TryGetValue("kerbalDeathPenalty", ref kerbalDeathPenalty);
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

			return KSPUtil.ApplicationRootPath + "/GameData/SpaceProgramFunding/Config/" + filename;
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

			// New as of v1.1
			//bool.TryParse(settings.GetValue("isBigProjectAllowed"), out isBigProjectAllowed);
			//bool.TryParse(settings.GetValue("isReputationAllowed"), out isReputationAllowed);
			//bool.TryParse(settings.GetValue("isScienceAllowed"), out isScienceAllowed);


			//bool.TryParse(settings.GetValue("contractInterceptor"), out isContractInterceptor);
			//int.TryParse(settings.GetValue("FundsPerRep"), out fundsPerRep);
			//bool.TryParse(settings.GetValue("coverCosts"), out isCostsCovered);
			//bool.TryParse(settings.GetValue("decayEnabled"), out isRepDecayEnabled);
			//float.TryParse(settings.GetValue("friendlyInterval"), out fundingIntervalDays);
			//int.TryParse(settings.GetValue("repDecay"), out repDecayRate);
			//int.TryParse(settings.GetValue("minimumRep"), out minimumRep);
			//int.TryParse(settings.GetValue("multiplier"), out fundingRepMultiplier);
			//int.TryParse(settings.GetValue("availableWages"), out baseKerbalWage);
			//int.TryParse(settings.GetValue("assignedWages"), out assignedKerbalWage);
			//float.TryParse(settings.GetValue("activeVesselCost"), out activeVesselCost);
			//bool.TryParse(settings.GetValue("VesselCostsEnabled"), out isActiveVesselCost);
			//bool.TryParse(settings.GetValue("buildingCostsEnabled"), out isBuildingCostsEnabled);
			//bool.TryParse(settings.GetValue("launchCostsEnabled"), out isLaunchCostsEnabled);
			//int.TryParse(settings.GetValue("launchCostsVAB"), out launchCostsLaunchPad);
			//int.TryParse(settings.GetValue("launchCostsSPH"), out launchCostsRunway);
			//int.TryParse(settings.GetValue("sphCost"), out structureCostSph);
			//int.TryParse(settings.GetValue("missionControlCost"), out structureCostMissionControl);
			//int.TryParse(settings.GetValue("astronautComplexCost"), out structureCostAstronautComplex);
			//int.TryParse(settings.GetValue("administrationCost"), out structureCostAdministration);
			//int.TryParse(settings.GetValue("vabCost"), out structureCostVab);
			//int.TryParse(settings.GetValue("trackingStationCost"), out structureCostTrackingStation);
			//int.TryParse(settings.GetValue("rndCost"), out structureCostRnD);
			//int.TryParse(settings.GetValue("otherFacilityCost"), out structureCostOtherFacility);
			//bool.TryParse(settings.GetValue("kerbalDeathPenaltyActive"), out isKerbalDeathPenalty);
			//int.TryParse(settings.GetValue("kerbalDeathPenalty"), out kerbalDeathPenalty);
			//int.TryParse(settings.GetValue("sciencePointCost"), out sciencePointCost);
			//int.TryParse(settings.GetValue("emergencyBudgetMultiple"), out bigProjectMultiple);
			//int.TryParse(settings.GetValue("emergencyBudgetFee"), out bigProjectFee);
		}

#if false
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Handles the layout of the settings window.</summary>
		///
		/// <param name="window_id"> Identifier for the window.</param>
		public void SettingsGUI(int window_id)
		{
			const int ledger_width = 185;
			const int label_width = 270;
			const int indent_width = 35;
			const int mod_width = ledger_width + label_width;

			Assert.IsTrue(Instance != null);
			if (Instance == null) return;

			var label_style = new GUIStyle(GUI.skin.label);
			label_style.normal.textColor = label_style.normal.textColor = Color.white;

			GUILayout.BeginVertical(GUILayout.Width(SETTINGS_WIDTH));

			// Preset configuration buttons
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Balanced")) LoadSettings(DifficultyEnum.Easy);
			if (GUILayout.Button("Normal")) LoadSettings(DifficultyEnum.Normal);
			if (GUILayout.Button("Hard")) LoadSettings(DifficultyEnum.Hard);
			GUILayout.EndHorizontal();


			_settingsScrollViewPosition = GUILayout.BeginScrollView(_settingsScrollViewPosition,
				GUILayout.Width(SETTINGS_WIDTH), GUILayout.Height(SETTINGS_HEIGHT));

			GUILayout.BeginHorizontal(GUILayout.Width(mod_width));
			GUILayout.Label("Funding Interval: ", label_style);
			var day_string = GUILayout.TextField(fundingIntervalDays.ToString(CultureInfo.CurrentCulture), GUILayout.Width(50));
			if (int.TryParse(day_string, out var day_number)) {
				day_number = Math.Max(day_number, 1);
				fundingIntervalDays = day_number;
			}
			GUILayout.Label(" days", label_style);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal(GUILayout.Width(mod_width));
			GUILayout.Label(
				"Funding Multiplier times Rep: " + fundingRepMultiplier.ToString("n0"),
				label_style, GUILayout.MinWidth(label_width));
			fundingRepMultiplier =
				(int)GUILayout.HorizontalSlider(fundingRepMultiplier / 100.0f, 0, 50,
					GUILayout.MinWidth(ledger_width)) * 100;
			GUILayout.EndHorizontal();


			GUILayout.BeginHorizontal(GUILayout.Width(mod_width));
			GUILayout.Label("Minimum Reputation: " + minimumRep, label_style,
				GUILayout.MinWidth(label_width));
			minimumRep = (int)GUILayout.HorizontalSlider(minimumRep * 10, 0, 100, GUILayout.MinWidth(ledger_width)) / 10;
			GUILayout.EndHorizontal();


			isContractInterceptor = GUILayout.Toggle(
				isContractInterceptor, "Contracts pay rep instead of funds?",
				GUILayout.MaxWidth(mod_width));
			if (isContractInterceptor) {
				GUILayout.BeginHorizontal(GUILayout.Width(mod_width));
				GUILayout.Space(indent_width);
				GUILayout.Label("Funds per Reputation Point: " + fundsPerRep.ToString("n0"),
					label_style, GUILayout.MinWidth(label_width - indent_width));
				fundsPerRep =
					(int)GUILayout.HorizontalSlider(fundsPerRep / 1000.0f, 0, 50,
						GUILayout.MinWidth(ledger_width)) * 1000;
				GUILayout.EndHorizontal();
			}

			isCostsCovered = GUILayout.Toggle(isCostsCovered,
				"Fixed costs above funding level are forgiven?", GUILayout.MaxWidth(mod_width));

			isRepDecayEnabled = GUILayout.Toggle(isRepDecayEnabled,
				"Decay Reputation each funding period?", GUILayout.MaxWidth(mod_width));
			if (isRepDecayEnabled) {
				GUILayout.BeginHorizontal(GUILayout.Width(mod_width));
				GUILayout.Space(indent_width);
				GUILayout.Label("Reputation Decay per period: " + repDecayRate, label_style,
					GUILayout.MinWidth(label_width - indent_width));
				repDecayRate =
					(int)GUILayout.HorizontalSlider(repDecayRate, 0, 50,
						GUILayout.MinWidth(ledger_width));
				GUILayout.EndHorizontal();
			}


			isKerbalWages = GUILayout.Toggle(isKerbalWages,
				"Enable Kerbal wages (per Kerbal per XP level)?", GUILayout.MaxWidth(mod_width));
			if (isKerbalWages) {
				GUILayout.BeginHorizontal(GUILayout.Width(mod_width));
				GUILayout.Space(indent_width);
				GUILayout.Label("Assigned Kerbal Wage: " + assignedKerbalWage.ToString("n0"),
					label_style, GUILayout.MinWidth(label_width - indent_width));
				assignedKerbalWage =
					(int)GUILayout.HorizontalSlider(assignedKerbalWage / 100.0f, 0, 100,
						GUILayout.MinWidth(ledger_width)) * 100;
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal(GUILayout.Width(mod_width));
				GUILayout.Space(indent_width);
				GUILayout.Label("Unassigned Kerbal Wage: " + baseKerbalWage.ToString("n0"),
					label_style, GUILayout.MinWidth(label_width - indent_width));
				baseKerbalWage =
					(int)GUILayout.HorizontalSlider(baseKerbalWage / 100.0f, 0, 100,
						GUILayout.MinWidth(ledger_width)) * 100;
				GUILayout.EndHorizontal();
			}

			isKerbalDeathPenalty =
				GUILayout.Toggle(isKerbalDeathPenalty,
					"Enable Kerbal death penalty (Rep per XP level)?", GUILayout.MaxWidth(mod_width));
			if (isKerbalDeathPenalty) {
				GUILayout.BeginHorizontal(GUILayout.Width(mod_width));
				GUILayout.Space(indent_width);
				GUILayout.Label("Reputation penalty: " + kerbalDeathPenalty, label_style,
					GUILayout.MinWidth(label_width - indent_width));
				kerbalDeathPenalty =
					(int)GUILayout.HorizontalSlider(kerbalDeathPenalty, 0, 100,
						GUILayout.MinWidth(ledger_width));
				GUILayout.EndHorizontal();
			}

			isBigProjectAllowed = GUILayout.Toggle(isBigProjectAllowed, "Is Big-Project savings account allowed?", GUILayout.MaxWidth(mod_width));
			if (isBigProjectAllowed) {

				// Big-Project multiple
				GUILayout.BeginHorizontal(GUILayout.Width(mod_width));
				GUILayout.Space(indent_width);
				GUILayout.Label("Big-Project multiple: " + bigProjectMultiple, label_style, GUILayout.MinWidth(label_width - indent_width));
				bigProjectMultiple = (int) GUILayout.HorizontalSlider(bigProjectMultiple / 10.0f, 0, 25, GUILayout.MinWidth(ledger_width)) * 10;
				GUILayout.EndHorizontal();

				// Big-Project penalty
				GUILayout.BeginHorizontal(GUILayout.Width(mod_width));
				GUILayout.Space(indent_width);
				GUILayout.Label("Big-Project Fund Transfer Fee: " + bigProjectFee + "%", label_style, GUILayout.MinWidth(label_width - indent_width));
				bigProjectFee = (int) GUILayout.HorizontalSlider(bigProjectFee, 0, 50, GUILayout.MinWidth(ledger_width));
				GUILayout.EndHorizontal();
			}

			isScienceAllowed = GUILayout.Toggle(isScienceAllowed, "Is diverting funds to create science points allowed?", GUILayout.MaxWidth(mod_width));

			isReputationAllowed = GUILayout.Toggle(isReputationAllowed, "Is diverting funds to increase reputation allowed?", GUILayout.MaxWidth(mod_width));

			// Cost per science point
			GUILayout.BeginHorizontal(GUILayout.Width(mod_width));
			GUILayout.Label("R&D cost per science point : " + sciencePointCost.ToString("n0"),
				label_style, GUILayout.MinWidth(label_width));
			sciencePointCost =
				(int)GUILayout.HorizontalSlider(sciencePointCost / 1000.0f, 0, 50,
					GUILayout.MinWidth(ledger_width)) * 1000;
			GUILayout.EndHorizontal();




			isBuildingCostsEnabled =
				GUILayout.Toggle(isBuildingCostsEnabled,
					"Structure maintenance costs (per Structure per level)", GUILayout.MaxWidth(mod_width));
			if (isBuildingCostsEnabled) {
				GUILayout.BeginHorizontal(GUILayout.Width(mod_width));
				GUILayout.Space(indent_width);
				GUILayout.Label("Space-Plane Hangar: " + structureCostSph.ToString("n0"), label_style,
					GUILayout.MinWidth(label_width - indent_width));
				structureCostSph =
					(int)GUILayout.HorizontalSlider(structureCostSph / 1000.0f, 0, 10,
						GUILayout.MinWidth(ledger_width)) * 1000;
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal(GUILayout.Width(mod_width));
				GUILayout.Space(indent_width);
				GUILayout.Label("Mission Control: " + structureCostMissionControl.ToString("n0"),
					label_style, GUILayout.MinWidth(label_width - indent_width));
				structureCostMissionControl =
					(int)GUILayout.HorizontalSlider(structureCostMissionControl / 1000.0f, 0, 10,
						GUILayout.MinWidth(ledger_width)) * 1000;
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal(GUILayout.Width(mod_width));
				GUILayout.Space(indent_width);
				GUILayout.Label("Astronaut Complex: " + structureCostAstronautComplex.ToString("n0"),
					label_style, GUILayout.MinWidth(label_width - indent_width));
				structureCostAstronautComplex =
					(int)GUILayout.HorizontalSlider(structureCostAstronautComplex / 1000.0f, 0, 10,
						GUILayout.MinWidth(ledger_width)) * 1000;
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal(GUILayout.Width(mod_width));
				GUILayout.Space(indent_width);
				GUILayout.Label("Administration: " + structureCostAdministration.ToString("n0"),
					label_style, GUILayout.MinWidth(label_width - indent_width));
				structureCostAdministration =
					(int)GUILayout.HorizontalSlider(structureCostAdministration / 1000.0f, 0, 10,
						GUILayout.MinWidth(ledger_width)) * 1000;
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal(GUILayout.Width(mod_width));
				GUILayout.Space(indent_width);
				GUILayout.Label("Vehicle Assembly Building: " + structureCostVab.ToString("n0"),
					label_style, GUILayout.MinWidth(label_width - indent_width));
				structureCostVab =
					(int)GUILayout.HorizontalSlider(structureCostVab / 1000.0f, 0, 10,
						GUILayout.MinWidth(ledger_width)) * 1000;
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal(GUILayout.Width(mod_width));
				GUILayout.Space(indent_width);
				GUILayout.Label("Tracking Station: " + structureCostTrackingStation.ToString("n0"),
					label_style, GUILayout.MinWidth(label_width - indent_width));
				structureCostTrackingStation =
					(int)GUILayout.HorizontalSlider(structureCostTrackingStation / 1000.0f, 0, 10,
						GUILayout.MinWidth(ledger_width)) * 1000;
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal(GUILayout.Width(mod_width));
				GUILayout.Space(indent_width);
				GUILayout.Label("R&D Facility: " + structureCostRnD.ToString("n0"), label_style,
					GUILayout.MinWidth(label_width - indent_width));
				structureCostRnD =
					(int)GUILayout.HorizontalSlider(structureCostRnD / 1000.0f, 0, 10,
						GUILayout.MinWidth(ledger_width)) * 1000;
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal(GUILayout.Width(mod_width));
				GUILayout.Space(indent_width);
				GUILayout.Label(
					"Other Facilities (non-stock): " + structureCostOtherFacility.ToString("n0"),
					label_style, GUILayout.MinWidth(label_width - indent_width));
				structureCostOtherFacility =
					(int)GUILayout.HorizontalSlider(structureCostOtherFacility / 1000.0f, 0, 10,
						GUILayout.MinWidth(ledger_width)) * 1000;
				GUILayout.EndHorizontal();
			}

			isLaunchCostsEnabled =
				GUILayout.Toggle(isLaunchCostsEnabled,
					"Launch costs (per launch per level per 100t)?", GUILayout.MaxWidth(mod_width));
			if (isLaunchCostsEnabled) {
				GUILayout.BeginHorizontal(GUILayout.Width(mod_width));
				GUILayout.Space(indent_width);
				GUILayout.Label("Runway launch cost: " + launchCostsRunway, label_style,
					GUILayout.MinWidth(label_width - indent_width));
				launchCostsRunway =
					(int)GUILayout.HorizontalSlider(launchCostsRunway / 10.0f, 0, 100,
						GUILayout.MinWidth(ledger_width)) * 10;
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal(GUILayout.Width(mod_width));
				GUILayout.Space(indent_width);
				GUILayout.Label("Launch-pad launch cost: " + launchCostsLaunchPad, label_style,
					GUILayout.MinWidth(label_width - indent_width));
				launchCostsLaunchPad =
					(int)GUILayout.HorizontalSlider(launchCostsLaunchPad / 100.0f, 0, 50,
						GUILayout.MinWidth(ledger_width)) * 100;
				GUILayout.EndHorizontal();
			}


			// Ship maintenance cost
			isActiveVesselCost =
				GUILayout.Toggle(isActiveVesselCost,
					"Enable monthly maintenance costs for vessels (per 100t)?", GUILayout.MaxWidth(mod_width));
			if (isActiveVesselCost) {
				GUILayout.BeginHorizontal(GUILayout.Width(mod_width));
				GUILayout.Space(indent_width);
				GUILayout.Label("Maintenance cost: " + activeVesselCost, label_style,
					GUILayout.MinWidth(label_width - indent_width));
				activeVesselCost =
					(int)GUILayout.HorizontalSlider(activeVesselCost / 100.0f, 0, 50,
						GUILayout.MinWidth(ledger_width)) * 100;
				GUILayout.EndHorizontal();
			}

			GUILayout.EndScrollView();


			GUILayout.EndVertical();
		}
#endif

	}
}

#endif
