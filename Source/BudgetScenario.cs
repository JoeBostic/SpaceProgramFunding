// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Project: SpaceProgramFunding -- BudgetScenario.cs
// 
// Summary: Transforms KSP funding model to play like a governmental space program rather than a commercial business.
// -------------------------------------------------------------------------------------------------------------------------

using JetBrains.Annotations;

namespace SpaceProgramFunding.Source
{
	///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary> Handles save/load of the Mod's data.</summary>
	[KSPScenario(ScenarioCreationOptions.AddToExistingCareerGames | ScenarioCreationOptions.AddToNewCareerGames,
		GameScenes.FLIGHT, GameScenes.TRACKSTATION, GameScenes.SPACECENTER)]
	[UsedImplicitly]
	internal class BudgetScenario : ScenarioModule
	{
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The current version of the mod.</summary>
		private const string _currentVersion = "5.0";


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> The mod version number as saved..</summary>
		private string _saveGameVersion = "0.0";


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Executes the save action which persists all dynamic data into the saved-game file.</summary>
		/// <param name="node"> The handle to the saved game file node.</param>
		public override void OnSave(ConfigNode node)
		{
			node.SetValue("saveGameVersion", _currentVersion, true);
			if (SpaceProgramFunding.Instance != null) SpaceProgramFunding.Instance.OnSave(node);
			if (BudgetSettings.Instance != null) BudgetSettings.Instance.OnSave(node);
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Executes the load action which retrieves all dynamic data from the saved-game file.</summary>
		/// <param name="node"> The handle to the saved game file node.</param>
		public override void OnLoad(ConfigNode node)
		{
			node.TryGetValue("saveGameVersion", ref _saveGameVersion);
			if (SpaceProgramFunding.Instance != null) {
				SpaceProgramFunding.Instance.OnLoad(node);
			}
			if (BudgetSettings.Instance != null) {
				BudgetSettings.Instance.OnLoad(node);

				if (BudgetSettings.Instance.isFirstRun) BudgetSettings.Instance.FirstRun();
			}
		}
	}
}