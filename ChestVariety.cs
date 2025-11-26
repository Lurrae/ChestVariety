using System;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ChestVariety
{
	public class ChestVariety : Mod
	{
		private static readonly MethodInfo _tileCountsAvailable = typeof(SystemLoader).GetMethod("TileCountsAvailable", BindingFlags.Static | BindingFlags.Public);
		private static readonly MethodInfo _nearbyEffects = typeof(TileLoader).GetMethod("NearbyEffects", BindingFlags.Static | BindingFlags.Public);
		private delegate void DelegateTileCountsAvailable(ReadOnlySpan<int> tileCounts);
		
		public override void Load()
		{
			MonoModHooks.Add(_tileCountsAvailable, DisableTileCounts);
			MonoModHooks.Add(_nearbyEffects, DisableNearbyEffects);
		}

		private static void DisableTileCounts(DelegateTileCountsAvailable orig, ReadOnlySpan<int> tileCounts)
		{
			if (WorldGen.generatingWorld)
				return;

			orig(tileCounts);
		}

		public static void DisableNearbyEffects(Action<int, int, int, bool> orig, int i, int j, int type, bool closer)
		{
			if (WorldGen.generatingWorld)
				return;

			orig(i, j, type, closer);
		}
	}

	public class RecipeChanges : ModSystem
	{
		public override void PostAddRecipes()
		{
			// Don't modify recipes unless Lite Mode is disabled
			if (ModContent.GetInstance<ChestConfig>().LiteMode)
				return;

			foreach (Recipe recipe in Main.recipe)
			{
				// Can't shimmer-decraft Obsidian Chest until evil boss downed
				if (recipe.TryGetResult(ItemID.ObsidianChest, out _))
				{
					recipe.HasShimmerCondition(Condition.DownedEowOrBoc);
				}

				// Can't shimmer-decraft Bone Chest until Skeletron downed
				if (recipe.TryGetResult(ItemID.BoneChest, out _))
				{
					recipe.HasShimmerCondition(Condition.DownedSkeletron);
				}

				// Can't shimmer-decraft Spider Chest until hardmode
				if (recipe.TryGetResult(ItemID.SpiderChest, out _))
				{
					recipe.HasShimmerCondition(Condition.Hardmode);
				}
			}
		}
	}
}