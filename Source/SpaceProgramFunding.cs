// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Project: SpaceProgramFunding -- Transforms KSP funding model to play like a governmental space program.
// Source:  https://github.com/JoeBostic/SpaceProgramFunding
// License: https://github.com/JoeBostic/SpaceProgramFunding/wiki/MIT-License
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;

namespace SpaceProgramFunding.Source
{
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary> A space program funding.</summary>
	[KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
	public class SpaceProgramFunding : MonoBehaviour
	{
		private const float _budgetWidth = 350;
		private const float _budgetHeight = 300;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Reference to the singleton of this object.</summary>
		///
		/// <value> The instance.</value>
		public static SpaceProgramFunding Instance { get; private set; }


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Has one-time initialization taken place? It uses this to enforce a singleton
		/// 		  character for this class.</summary>
		private static bool _initialized;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Builds an array of all the entries in the facilities enumeration so that iterating
		/// 		  through the facilities is possible.</summary>
		private SpaceCenterFacility[] _facilities;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The big project that the monthly budget manages.</summary>
		public BigProject bigProject = new BigProject();


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The research lab where funds are converted into science points.</summary>
		public ResearchLab researchLab = new ResearchLab();

		private Rect _budgetDialogPosition;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Length of the day expressed in hours. This might be different than stock Kerbin (6
		/// 		  hours).</summary>
		public double dayLength;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The home world which may be Kerbin, or not, according to what planet-pack has been
		/// 		  installed. Knowing the home world allows accurate calculation of days since the
		/// 		  number of hours per day might be different.</summary>
		public CelestialBody homeWorld;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The public relations department. This department is in charge of spending funds to
		/// 		  raise the reputation of the Space Agency.</summary>
		public PublicRelations publicRelations = new PublicRelations();


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The last time the budget process was run. This is the time of the start of the fiscal
		/// 		  budget period that the budget was last processed.</summary>
		public double lastUpdate;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Records the total of all launch costs that have accumulated during this budget period.</summary>
		public int launchCostsAccumulator;


		private Rect _settingsDialogPosition;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> True to show, false to hide the budget dialog.</summary>
		public bool showBudgetDialog;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> True to show, false to hide the settings dialog.</summary>
		public bool showSettingsDialog;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> True if the GUI should be visible. This might be turned false if the game signals it
		/// 		  wants to hide all GUI such as for a screen shot.</summary>
		private bool _visibleGui = true;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> A record of the maintenance costs for the Space Center. This has to be stored rather
		/// 		  than calculated on the fly since the Space Center is often unloaded and
		/// 		  calculation of costs cannot be performed.</summary>
		private int _buildingCostsArchive;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Length of the year for the home system. This might be different than stock Kerbin as
		/// 		  a result of any planet-pack installed.</summary>
		public double yearLength;


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Awakes this object. </summary>
		[UsedImplicitly]
		private void Awake()
		{
			if (Instance != null && Instance != this) {
				Destroy(this);
				//DestroyImmediate(this);
				return;
			}

			DontDestroyOnLoad(this);
			Instance = this;
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Starts this object. </summary>
		[UsedImplicitly]
		private void Start()
		{
			if (_initialized) return;

			_budgetDialogPosition.width = _budgetWidth;
			_budgetDialogPosition.height = _budgetHeight;
			_budgetDialogPosition.x = (Screen.width - _budgetDialogPosition.width) / 2;
			_budgetDialogPosition.y = (Screen.height - _budgetDialogPosition.height) / 2;

			_settingsDialogPosition.height = BudgetSettings._settingsHeight;
			_settingsDialogPosition.height = BudgetSettings._settingsWidth;
			_settingsDialogPosition = _budgetDialogPosition;
			_settingsDialogPosition.x = _budgetDialogPosition.x + _budgetDialogPosition.width;

			KACWrapper.InitKACWrapper();
			PopulateHomeWorldData();

			// Fetch Space Center structure enums into an array. This eases traversing through all Space Center structures.
			_facilities = (SpaceCenterFacility[]) Enum.GetValues(typeof(SpaceCenterFacility));
			GameEvents.OnVesselRollout.Add(OnVesselRollout);
			//GameEvents.onGameSceneSwitchRequested.Add(OnSceneSwitch);
			GameEvents.onHideUI.Add(OnHideUI);
			GameEvents.onShowUI.Add(OnShowUI);
			GameEvents.onGameSceneLoadRequested.Add(OnGameSceneLoad);

			_initialized = true;
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Updates this object. </summary>
		[UsedImplicitly]
		private void Update()
		{
			if (HighLogic.CurrentGame == null) return;
			if (HighLogic.CurrentGame.Mode != Game.Modes.CAREER) return;
			if (BudgetSettings.Instance == null) return;

			// Don't process budget while settings dialog is open.
			if (showSettingsDialog) return;

			var time = Planetarium.GetUniversalTime();

			// Handle time travel paradox. This should never happen.
			while (lastUpdate > time) lastUpdate = lastUpdate - BudgetInterval();


			// Perform the budget process if it is time to do so.
			var time_since_last_update = time - lastUpdate;
			if (time_since_last_update >= BudgetInterval()) Budget();

			// Always try to keep KAC populated with the budget alarm.
			if (!KACWrapper.AssemblyExists || !BudgetSettings.Instance.isAlarmClockPerBudget) return;
			if (!KACWrapper.APIReady) return;
			var alarms = KACWrapper.KAC.Alarms;
			if (alarms.Count >= 0)
				foreach (var alarm in alarms)
					if (alarm.Name == "Next Budget")
						return;

			KACWrapper.KAC.CreateAlarm(KACWrapper.KACAPI.AlarmTypeEnum.Raw, "Next Budget", lastUpdate + BudgetInterval());
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Executes the destroy action. </summary>
		[UsedImplicitly]
		private void OnDestroy()
		{
			if (Instance != this) return;

			GameEvents.OnVesselRollout.Remove(OnVesselRollout);
			//GameEvents.onGameSceneSwitchRequested.Remove(OnSceneSwitch);
			GameEvents.onHideUI.Remove(OnHideUI);
			GameEvents.onShowUI.Remove(OnShowUI);
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Executes the save action.</summary>
		///
		/// <param name="node"> The node.</param>
		public void OnSave(ConfigNode node)
		{
			node.SetValue("LastBudgetUpdate", lastUpdate, true);
			node.SetValue("LaunchCosts", launchCostsAccumulator, true);


			publicRelations.OnSave(node);
			researchLab.OnSave(node);
			bigProject.OnSave(node);
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Executes the load action.</summary>
		///
		/// <param name="node"> The node.</param>
		public void OnLoad(ConfigNode node)
		{
			node.TryGetValue("LastBudgetUpdate", ref lastUpdate);
			node.TryGetValue("LaunchCosts", ref launchCostsAccumulator);


			publicRelations.OnLoad(node);
			researchLab.OnLoad(node);
			bigProject.OnLoad(node);
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Called when the game scene is loaded. We want the mod's pop-up windows to close when
		/// 		  a the scene changes.</summary>
		///
		/// <param name="scene"> The scene that is loaded.</param>
		private void OnGameSceneLoad(GameScenes scene)
		{
			if (scene == GameScenes.FLIGHT || scene == GameScenes.TRACKSTATION || scene == GameScenes.EDITOR ||
			    scene == GameScenes.SPACECENTER) return;
			showBudgetDialog = false;
			showSettingsDialog = false;
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Called when the game wants the UI to be hidden -- temporarily.</summary>
		private void OnHideUI()
		{
			_visibleGui = false;
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Called when the game wants to re-display the UI. </summary>
		private void OnShowUI()
		{
			_visibleGui = true;
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Calculates the gross budget. This is the budget just considering reputation and not
		/// 		  counting any costs.</summary>
		///
		/// <returns> The gross budget.</returns>
		public float GrossBudget()
		{
			if (BudgetSettings.Instance == null) return 0;
			return Math.Max(Reputation.CurrentRep, BudgetSettings.Instance.minimumRep) * BudgetSettings.Instance.budgetRepMultiplier;
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Budget interval expressed in time units.</summary>
		///
		/// <returns> A double that is the time units of one budget period.</returns>
		public double BudgetInterval()
		{
			if (BudgetSettings.Instance == null) return 0;
			return BudgetSettings.Instance.budgetIntervalDays * dayLength;
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Performs the main budget action. This is where funds are collected from the Kerbal
		/// 		  government and distributed to all the necessary recipients. The core function of
		/// 		  this mod is handled by the logic in this method.</summary>
		private void Budget()
		{
			if (BudgetSettings.Instance == null) return;

			try {
				VABHack();

				var current_funds = Funding.Instance.Funds;

				var gross_budget = GrossBudget();

				/*
				 * Calculate the hard costs such as crew salaries and launch costs. 
				 */
				float costs = CostCalculate();
				launchCostsAccumulator = 0;

				/*
				 * Calculate the adjusted net budget. This the gross budget less hard costs.
				 */
				var net_budget = gross_budget - costs;

				/*
				 * If the net budget is less than zero, then costs exceed budget. Don't ever remove funds from player if cost
				 * covering is enabled.
				 */
				if (BudgetSettings.Instance.isCostsCovered)
					if (net_budget < 0)
						net_budget = 0;


				/*
				 * If the net budget is negative (due to hard costs exceeding gross budget), then forgive the debt if
				 * that setting is true. Think of it as the Kerbal central government covering those costs out of the
				 * general fund. If not forgiven, then the player has to pony-up the debt.
				 */
				if (net_budget < 0 && BudgetSettings.Instance.isCostsCovered) net_budget = 0;

				/*
				 * netBudget now becomes the amount of funds to add to the player's bank account.
				 */
				if (current_funds < net_budget)
					net_budget = (float) (net_budget - current_funds);
				else
					net_budget = 0;


				/*
				 * Actually update the player's current fund total by raising the player's current funds to match
				 * the budget or charging the player if the net budget is negative.
				 */
				Funding.Instance.AddFunds(net_budget, TransactionReasons.None);
				var net_funds = Funding.Instance.Funds;


				/*
				 * Divert some funds to Public Relations in order to keep reputation points up.
				 */
				net_funds = publicRelations.SiphonFunds(net_funds);


				/*
				 * Do R&D before funding big project reserve. It typically costs 10,000 funds for 1 science point!
				 */
				net_funds = researchLab.SiphonFunds(net_funds);


				/*
				 * Divert some portion of available funds of the current net budget toward the emergency ("big project") reserve.
				 */
				net_funds = bigProject.SiphonFunds(net_funds);


				/*
				 * Update current funds to reflect the funds siphoned off.
				 */
				if (net_funds <= Funding.Instance.Funds) Funding.Instance.AddFunds(-(Funding.Instance.Funds - net_funds), TransactionReasons.None);

				/*
				 * Record the time of the start of the next fiscal period.
				 */
				lastUpdate += BudgetInterval();


				/*
				 * Decay reputation if the game settings indicate. Never reduce to below minimum reputation allowed.
				 */
				if (BudgetSettings.Instance.isRepDecayEnabled) {
					var max_decay = Reputation.CurrentRep - BudgetSettings.Instance.minimumRep;
					var amount_to_decay = Math.Min(BudgetSettings.Instance.repDecayRate, max_decay);
					if (amount_to_decay > 0) Reputation.Instance.AddReputation(-amount_to_decay, TransactionReasons.None);
				}

				/*
				 * Add Alarm Clock reminder.
				 */
				if (!KACWrapper.AssemblyExists && BudgetSettings.Instance.isAlarmClockPerBudget) TimeWarp.SetRate(0, true);
			} catch {
				if (HighLogic.LoadedScene != GameScenes.MAINMENU)
					Debug.Log("[MonthlyBudgets]: Problem calculating the budget");
			}
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Creates a date string that represents the time that the next budget period will occur.</summary>
		///
		/// <returns> A string for the date of the next budget period.</returns>
		private string NextBudgetDateString()
		{
			if (BudgetSettings.Instance == null) return "<error>";

			if (homeWorld == null) PopulateHomeWorldData();
			var next_update_raw = lastUpdate + BudgetSettings.Instance.budgetIntervalDays * dayLength;
			var next_update_refine = next_update_raw / dayLength;
			var year = 1;
			var day = 1;
			while (next_update_refine > yearLength / dayLength) {
				year += 1;
				next_update_refine = next_update_refine - yearLength / dayLength;
			}

			day += (int) next_update_refine;
			return "Year " + year + ", Day " + day;
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Handles the layout of the main interface window for the mod.</summary>
		///
		/// <param name="windowID"> Identifier for the window.</param>
		protected void WindowGUI(int windowID)
		{
			const int ledgerWidth = 120;
			const int labelWidth = 230;

			Assert.IsTrue(BudgetSettings.Instance != null);
			if (BudgetSettings.Instance == null) return;

			var ledger_style = new GUIStyle(GUI.skin.label) {alignment = TextAnchor.UpperRight};
			ledger_style.normal.textColor = ledger_style.normal.textColor = Color.white;

			var label_style = new GUIStyle(GUI.skin.label);
			label_style.normal.textColor = label_style.normal.textColor = Color.white;

			GUILayout.BeginVertical(GUILayout.Width(_budgetWidth));


			GUILayout.BeginHorizontal(GUILayout.MaxWidth(_budgetWidth));
			GUILayout.Label("Next Funding Period:", label_style, GUILayout.MaxWidth(labelWidth));
			GUILayout.Label(NextBudgetDateString(), ledger_style, GUILayout.MaxWidth(ledgerWidth));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal(GUILayout.MaxWidth(_budgetWidth));
			GUILayout.Label("Current Reputation:", label_style, GUILayout.MaxWidth(labelWidth));
			GUILayout.Label(Math.Max(Reputation.CurrentRep, BudgetSettings.Instance.minimumRep).ToString("n0"),
				ledger_style, GUILayout.MaxWidth(ledgerWidth));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal(GUILayout.MaxWidth(_budgetWidth));
			GUILayout.Label("Estimated Gross Funding:", label_style, GUILayout.MaxWidth(labelWidth));
			GUILayout.Label(GrossBudget().ToString("n0"), ledger_style, GUILayout.MaxWidth(ledgerWidth));
			GUILayout.EndHorizontal();


			if (BudgetSettings.Instance.isBuildingCostsEnabled) {
				GUILayout.BeginHorizontal(GUILayout.MaxWidth(_budgetWidth));
				GUILayout.Label("Space Center Costs:", label_style, GUILayout.MaxWidth(labelWidth));
				GUILayout.Label(CostBuildings() == 0 ? "???" : CostBuildings().ToString("n0"), ledger_style,
					GUILayout.MaxWidth(ledgerWidth));
				GUILayout.EndHorizontal();
			}

			if (BudgetSettings.Instance.isKerbalWages) {
				GUILayout.BeginHorizontal(GUILayout.MaxWidth(_budgetWidth));
				GUILayout.Label("Assigned Kerbal Wages:", label_style, GUILayout.MaxWidth(labelWidth));
				GUILayout.Label(ActiveCostWages().ToString("n0"), ledger_style, GUILayout.MaxWidth(ledgerWidth));
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal(GUILayout.MaxWidth(_budgetWidth));
				GUILayout.Label("Unassigned Kerbal Wages:", label_style, GUILayout.MaxWidth(labelWidth));
				GUILayout.Label(InactiveCostWages().ToString("n0"), ledger_style, GUILayout.MaxWidth(ledgerWidth));
				GUILayout.EndHorizontal();
			}


			if (BudgetSettings.Instance.isActiveVesselCost) {
				GUILayout.BeginHorizontal(GUILayout.MaxWidth(_budgetWidth));
				GUILayout.Label("Vessel Maintenance:", label_style, GUILayout.MaxWidth(labelWidth));
				GUILayout.Label(CostVessels().ToString("n0"), ledger_style, GUILayout.MaxWidth(ledgerWidth));
				GUILayout.EndHorizontal();
			}


			if (BudgetSettings.Instance.isLaunchCostsEnabled) {
				GUILayout.BeginHorizontal(GUILayout.MaxWidth(_budgetWidth));
				GUILayout.Label("Launch Costs:", label_style, GUILayout.MaxWidth(labelWidth));
				GUILayout.Label(CostLaunches().ToString("n0"), ledger_style, GUILayout.MaxWidth(ledgerWidth));
				GUILayout.EndHorizontal();
			}

			GUILayout.BeginHorizontal(GUILayout.MaxWidth(_budgetWidth));
			GUILayout.Label("Estimated Net Funding:", label_style, GUILayout.MaxWidth(labelWidth));
			GUILayout.Label((GrossBudget() - CostCalculate()).ToString("n0"), ledger_style,
				GUILayout.MaxWidth(ledgerWidth));
			GUILayout.EndHorizontal();


			publicRelations.isPREnabled = GUILayout.Toggle(publicRelations.isPREnabled, "Divert funding to Public Relations?");
			if (publicRelations.isPREnabled) {
				GUILayout.BeginHorizontal(GUILayout.MaxWidth(_budgetWidth));
				GUILayout.Label("Funds diverted : " + publicRelations.reputationDivertPercentage + "%", label_style,
					GUILayout.MaxWidth(labelWidth));
				publicRelations.reputationDivertPercentage = (int) GUILayout.HorizontalSlider((int) publicRelations.reputationDivertPercentage, 1, 50,
					GUILayout.MaxWidth(ledgerWidth));
				GUILayout.EndHorizontal();
			} else {
				GUILayout.Label("No funds diverted to Public Relations.");
			}


			researchLab.isRNDEnabled = GUILayout.Toggle(researchLab.isRNDEnabled, "Divert funding to science research?");
			if (researchLab.isRNDEnabled) {
				GUILayout.BeginHorizontal(GUILayout.MaxWidth(_budgetWidth));
				GUILayout.Label("Funds diverted : " + researchLab.scienceDivertPercentage + "%", label_style,
					GUILayout.MaxWidth(labelWidth));
				researchLab.scienceDivertPercentage = (int) GUILayout.HorizontalSlider((int) researchLab.scienceDivertPercentage, 1, 50,
					GUILayout.MaxWidth(ledgerWidth));
				GUILayout.EndHorizontal();
			} else {
				GUILayout.Label("No funds diverted to create science points.");
			}

			bigProject.isEnabled = GUILayout.Toggle(bigProject.isEnabled, "Divert funding to Big-Project reserve?");
			if (bigProject.isEnabled) {
				GUILayout.BeginHorizontal(GUILayout.MaxWidth(_budgetWidth));
				GUILayout.Label("Funds diverted : " + bigProject.divertPercentage + "%", label_style,
					GUILayout.MaxWidth(labelWidth - 50));
				bigProject.divertPercentage = (int) GUILayout.HorizontalSlider((int) bigProject.divertPercentage, 1, 50,
					GUILayout.MaxWidth(ledgerWidth + 50));
				GUILayout.EndHorizontal();
			} else {
				GUILayout.Label("No funds being diverted to Big-Project.");
			}

			if (bigProject.fundsAccumulator > 0) {
				GUILayout.BeginHorizontal(GUILayout.MaxWidth(_budgetWidth));
				GUILayout.Label(
					"Big-Project: " + bigProject.fundsAccumulator.ToString("n0") + " / " + bigProject.MaximumBigBudget().ToString("n0"),
					label_style, GUILayout.MaxWidth(labelWidth - 50));
				if (GUILayout.Button("Extract all Funds")) bigProject.WithdrawFunds();

				GUILayout.EndHorizontal();
			} else {
				GUILayout.Label("No funds available in Big-Project reserve.");
			}


			GUILayout.BeginHorizontal(GUILayout.MaxWidth(_budgetWidth));
			if (GUILayout.Button("Settings")) showSettingsDialog = !showSettingsDialog;

			if (GUILayout.Button("Close")) {
				showBudgetDialog = false;
				showSettingsDialog = false;
			}

			GUILayout.EndHorizontal();


			GUILayout.EndVertical();

			GUI.DragWindow();
		}



		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Executes the graphical user interface action.</summary>
		[UsedImplicitly]
		private void OnGUI()
		{
			if (_visibleGui && showBudgetDialog) {
				GUI.skin = HighLogic.Skin;
				//GUIPosition.height = 30;	// tighten up height each time
				_budgetDialogPosition = GUILayout.Window(0, _budgetDialogPosition, WindowGUI, "Space Program Funding",
					GUILayout.Width(_budgetWidth));
			}

			if (_visibleGui && showSettingsDialog && BudgetSettings.Instance != null) {
				_settingsDialogPosition.x = _budgetDialogPosition.x + _budgetDialogPosition.width;
				_settingsDialogPosition.y = _budgetDialogPosition.y;
				GUI.skin = HighLogic.Skin;
				_settingsDialogPosition = GUILayout.Window(1, _settingsDialogPosition, BudgetSettings.Instance.SettingsGUI,
					"Space Program Funding Settings", GUILayout.MaxHeight(BudgetSettings._settingsHeight));
			}
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Fetch time data for the home-world. This is usually Kerbin, but can change with some
		/// 		  planet-pack mods. By fetching the data in this way the timing values for days
		/// 		  remains true no matter if the home-world has changed.</summary>
		public void PopulateHomeWorldData()
		{
			homeWorld = FlightGlobals.GetHomeBody();
			dayLength = homeWorld.solarDayLength;
			yearLength = homeWorld.orbit.period;
		}


#if false
		public void DoPopUp(string message)
		{
			PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f),
				new Vector2(0.5f, 0.5f),
				new MultiOptionDialog("TEST",
					message,
					"Debug Window",
					HighLogic.UISkin,
					new Rect(0.5f, 0.5f, 350f, 260f),
					new DialogGUIFlexibleSpace(),
					new DialogGUIVerticalLayout(
						new DialogGUIFlexibleSpace(),
						new DialogGUIButton("Close", () => { }, 140.0f, 30.0f, true)
					)),
				false,
				HighLogic.UISkin);
		}
#endif


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> There is a quirk with the game such that modules get saved/loaded around the SPH or
		/// 		  VAB but other game settings do not. This means anything that adjusts the game
		/// 		  funds will persist after leaving the ship editor, but the mod modules will have
		/// 		  their state restored. The result is that unless this is handled in a special way,
		/// 		  extracting funds from the big-project budget will magically be restored when
		/// 		  returning to the Space Center -- a HUG exploit. This handles that quirk.</summary>
		public void VABHack()
		{
			if (!bigProject.isHack) return;
			bigProject.fundsAccumulator = 0;
			bigProject.isHack = false;
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> When the vessel/plane rolls out of the VAB or SPH, the launch costs are calculated.
		/// 		  These will get rolled back naturally if the player reverts the editor. These
		/// 		  launch costs represent extraordinary wear and tear on the launch facilities and
		/// 		  are a one-time cost. Typically, the runway requires minimal cost as compared to
		/// 		  the launch-pad.</summary>
		///
		/// <param name="ship"> The ship that is being launched.</param>
		private void OnVesselRollout(ShipConstruct ship)
		{
			if (BudgetSettings.Instance == null) return;
			if (!BudgetSettings.Instance.isLaunchCostsEnabled) return;

			/*
			 * Launch costs are based on the total-mass of the vehicle and the launch facility upgrade level. Runways
			 * take less wear-n-tear than rocket launch pad.
			 */
			ship.GetShipMass(out var dry_mass, out var fuel_mass);
			var total_mass = dry_mass + fuel_mass;

			/*
			 * Determine the percentage to charge.
			 */
			float launch_cost;
			int facility_level;
			if (ship.shipFacility == EditorFacility.VAB) {
				launch_cost = BudgetSettings.Instance.launchCostsLaunchPad;
				facility_level = FacilityLevel(SpaceCenterFacility.LaunchPad);
			} else {
				launch_cost = BudgetSettings.Instance.launchCostsRunway;
				facility_level = FacilityLevel(SpaceCenterFacility.Runway);
			}

			/*
			 * Only launch facilities that are have been upgraded at least once and only for vehicles that
			 * are 100 tons or heavier will cause launch costs to be applied.
			 */
			if (facility_level > 1 && total_mass >= 100.0f) launchCostsAccumulator += (int) (total_mass / 100.0f * launch_cost * LevelCoefficient(facility_level));
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Kerbal astronauts that are inactive (sitting in the Astronaut complex) are paid at a
		/// 		  different rate than Kerbals that are on missions. Usually less.</summary>
		///
		/// <returns> The total wages for all astronauts that are unassigned.</returns>
		public int InactiveCostWages()
		{
			if (BudgetSettings.Instance == null) return 0;

			if (!BudgetSettings.Instance.isKerbalWages) return 0;

			var crew = HighLogic.CurrentGame.CrewRoster.Crew;
			var total_wages = 0;
			foreach (var p in crew) {
				if (p.type == ProtoCrewMember.KerbalType.Tourist) continue;
				float kerbal_level = p.experienceLevel + 1;

				float kerbal_wage = 0;
				if (p.rosterStatus == ProtoCrewMember.RosterStatus.Available)
					kerbal_wage = kerbal_level * BudgetSettings.Instance.baseKerbalWage;

				total_wages += (int) kerbal_wage;
			}

			return total_wages;
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Kerbal astronauts that are on missions (not in Astronaut complex) are paid at a
		/// 		  different rate. Usually higher to account for "hazard pay".</summary>
		///
		/// <returns> The total wages for all astronauts that are on a mission. (aka, "active")</returns>
		public int ActiveCostWages()
		{
			if (BudgetSettings.Instance == null) return 0;

			if (!BudgetSettings.Instance.isKerbalWages) return 0;

			var total_wages = 0;
			var crew = HighLogic.CurrentGame.CrewRoster.Crew;
			foreach (var p in crew) {
				if (p.type == ProtoCrewMember.KerbalType.Tourist) continue;
				float kerbal_level = p.experienceLevel + 1;

				float kerbal_wage = 0;
				if (p.rosterStatus == ProtoCrewMember.RosterStatus.Assigned)
					kerbal_wage = kerbal_level * BudgetSettings.Instance.assignedKerbalWage;

				total_wages += (int) kerbal_wage;
			}

			return total_wages;
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Figures out the monthly maintenance cost for all vessels. This is based on the mass
		/// 		  of the vessel. The presumption is that larger vessels need more Space Center
		/// 		  support on an ongoing basis.</summary>
		///
		/// <returns> The sum cost of maintenance cost for all vessels.</returns>
		public int CostVessels()
		{
			if (BudgetSettings.Instance == null) return 0;

			if (!BudgetSettings.Instance.isActiveVesselCost) return 0;

			var vessels = FlightGlobals.Vessels.Where(v =>
				v.vesselType != VesselType.Debris && v.vesselType != VesselType.Flag &&
				v.vesselType != VesselType.SpaceObject && v.vesselType != VesselType.Unknown &&
				v.vesselType != VesselType.EVA);

			return vessels.Sum(v => (int) (v.GetTotalMass() / 100.0 * BudgetSettings.Instance.activeVesselCost));
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Determines the total costs -- accumulated so far -- for launches during this budget
		/// 		  period. These costs cover janitorial maintenance and handy-man repair work
		/// 		  necessary when the launch facility is used for heavy vehicles. The heavier the
		/// 		  launch vehicle, the more expensive it is to clean up afterward.</summary>
		///
		/// <returns> The total, so far, of launch costs.</returns>
		public int CostLaunches()
		{
			if (BudgetSettings.Instance == null) return 0;

			var costs = 0;
			if (BudgetSettings.Instance.isLaunchCostsEnabled) costs = launchCostsAccumulator;

			return costs;
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Calculates all the non-discretionary spending for the current budget period.</summary>
		///
		/// <returns> The non-discretionary bill for this budget period.</returns>
		public int CostCalculate()
		{
			var costs = ActiveCostWages();
			costs += InactiveCostWages();
			costs += CostVessels();
			costs += CostBuildings();
			costs += CostLaunches();
			return costs;
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Calculates the level (1..3) of the facility specified.</summary>
		///
		/// <param name="facility"> The facility to fetch the level for.</param>
		///
		/// <returns> The level of the facility. This will be 1..3 where 1 is the initial level on a new
		/// 		  career game and 3 is fully upgraded.</returns>
		private int FacilityLevel(SpaceCenterFacility facility)
		{
			var level = ScenarioUpgradeableFacilities.GetFacilityLevel(facility); // 0 .. 1
			var count = ScenarioUpgradeableFacilities
				.GetFacilityLevelCount(facility); // max upgrades allowed (usually 2)
			return (int) (level * count) + 1;
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Figures out the ongoing maintenance cost for all structures in the Space Center.</summary>
		///
		/// <returns> Returns with the funds cost for Space Center structure maintenance.</returns>
		public int CostBuildings()
		{
			if (BudgetSettings.Instance == null) return 0;

			if (!BudgetSettings.Instance.isBuildingCostsEnabled) return 0;

			return _buildingCostsArchive;
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Calculates the base maintenance cost for the facility specified.</summary>
		///
		/// <param name="facility"> The facility to fetch the maintenance cost for.</param>
		///
		/// <returns> The building costs.</returns>
		private int BaseStructureCost(SpaceCenterFacility facility)
		{
			if (BudgetSettings.Instance == null) return 0;

			switch (facility) {
				case SpaceCenterFacility.Administration:
					return BudgetSettings.Instance.structureCostAdministration;
				case SpaceCenterFacility.AstronautComplex:
					return BudgetSettings.Instance.structureCostAstronautComplex;
				case SpaceCenterFacility.MissionControl:
					return BudgetSettings.Instance.structureCostMissionControl;
				case SpaceCenterFacility.ResearchAndDevelopment:
					return BudgetSettings.Instance.structureCostRnD;
				case SpaceCenterFacility.SpaceplaneHangar:
					return BudgetSettings.Instance.structureCostSph;
				case SpaceCenterFacility.TrackingStation:
					return BudgetSettings.Instance.structureCostTrackingStation;
				case SpaceCenterFacility.VehicleAssemblyBuilding:
					return BudgetSettings.Instance.structureCostVab;
				case SpaceCenterFacility.LaunchPad:
				case SpaceCenterFacility.Runway:
					return 0;
				default:
					return BudgetSettings.Instance.structureCostOtherFacility;
			}
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Calculates the building costs for the entire Kerbal Space Center. This takes into
		/// 		  account structure upgrade state.</summary>
		public void CalculateBuildingCosts()
		{
			var costs = 0;
			for (var i = 0; i < _facilities.Length; i++) {
				var facility = _facilities.ElementAt(i);

				// Launch-pad and runway have no ongoing facility costs.
				if (facility == SpaceCenterFacility.LaunchPad || facility == SpaceCenterFacility.Runway) continue;

				costs += LevelCoefficient(FacilityLevel(facility)) * BaseStructureCost(facility);
			}

			_buildingCostsArchive = costs;
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Calculates the maintenance cost coefficient that is multiplied by the structure's
		/// 		  base cost. The effect is that a level 3 structure is 4 times the cost of the
		/// 		  level 1 structure.</summary>
		///
		/// <param name="level"> The level to calculate the coefficient for.</param>
		///
		/// <returns> The coefficient to multiply with the structure's base cost to arrive at the current
		/// 		  maintenance cost.</returns>
		private int LevelCoefficient(int level)
		{
			switch (level) {
				case 1:
					return 0;

				case 2:
					return 2;

				case 3:
					return 4;

				default:
					return 1;
			}
		}
	}
}