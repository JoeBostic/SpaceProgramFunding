// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Project: SpaceProgramFunding -- Transforms KSP funding model to play like a governmental space program.
// Source:  https://github.com/JoeBostic/SpaceProgramFunding
// License: https://github.com/JoeBostic/SpaceProgramFunding/wiki/MIT-License
// --------------------------------------------------------------------------------------------------------------------

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
		/// <summary> Executes the save action which persists all dynamic data into the saved-game file.</summary>
		///
		/// <param name="node"> The handle to the saved game file node.</param>
		public override void OnSave(ConfigNode node)
		{
			if (SpaceProgramFunding.Instance != null) SpaceProgramFunding.Instance.OnSave(node);
		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary> Executes the load action which retrieves all dynamic data from the saved-game file.</summary>
		///
		/// <param name="node"> The handle to the saved game file node.</param>
		public override void OnLoad(ConfigNode node)
		{
			if (SpaceProgramFunding.Instance != null) SpaceProgramFunding.Instance.OnLoad(node);

			//if (BudgetSettings.Instance == null) return;
			//BudgetSettings.Instance.OnLoad(node);
			//if (BudgetSettings.Instance.isFirstRun) BudgetSettings.Instance.FirstRun();
		}
	}
}