using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace ChestVariety
{
	public enum ChestType
	{
		// TileID.Containers chests
		Wood,
		Gold,
		LockedGold,
		Shadow,
		LockedShadow,
		Barrel,
		TrashCan,
		Ebonwood,
		RichMahogany,
		Pearlwood,
		Ivy,
		Frozen,
		LivingWood,
		Skyware,
		ShadeWood,
		Webbed,
		Lihzahrd,
		Water,
		JungleBiome,
		CorruptBiome,
		CrimsonBiome,
		HallowBiome,
		IceBiome,
		LockedJungleBiome,
		LockedCorruptBiome,
		LockedCrimsonBiome,
		LockedHallowBiome,
		LockedIceBiome,
		Dynasty,
		Honey,
		Steampunk,
		PalmWood,
		GlowingMushroom,
		BorealWood,
		BlueSlime,
		GreenDungeon,
		LockedGreenDungeon,
		PinkDungeon,
		LockedPinkDungeon,
		BlueDungeon,
		LockedBlueDungeon,
		Bone,
		Cactus,
		Flesh,
		Obsidian,
		Pumpkin,
		Spooky,
		Glass,
		Martian,
		Meteorite,
		Granite,
		Marble,
		Crystal,
		PirateGolden,
		// TileID.Containers2 chests
		CrystalReal,
		PirateGoldenReal,
		Spider,
		Lesion,
		DeadMans,
		Solar,
		Vortex,
		Nebula,
		Stardust,
		GolfBall,
		Sandstone,
		Bamboo,
		DesertBiome,
		LockedDesertBiome,
		Reef,
		Balloon,
		AshWood
	}

	public class ChestWorldGen : ModSystem
	{
		public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
		{
			int GenIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Final Cleanup")) + 1;
			if (GenIndex != -1)
				tasks.Insert(GenIndex++, new PassLegacy("ChestVariety: ChestVariety", ReplaceChestTypes));
		}

		private void ReplaceChestTypes(GenerationProgress progress, GameConfiguration config)
		{
			progress.Message = "Replacing Chest Types!";

			HashSet<ChestType> ChestTypesToSwap = new()
			{
				ChestType.Wood,
				ChestType.Gold,
				ChestType.LockedGold,
				ChestType.Webbed,
				ChestType.Water
			};

			bool liteMode = ModContent.GetInstance<ChestConfig>().LiteMode;

			for (int c = 0; c < Main.maxChests; c++)
			{
				Chest chest = Main.chest[c];
				if (chest == null || !WorldGen.InWorld(chest.x, chest.y, 42))
					continue;

				Tile chestTile = Main.tile[chest.x, chest.y];
				if (chest.item == null || (chestTile.TileType != TileID.Containers && chestTile.TileType != TileID.Containers2))
					continue;

				int chestTypeOffset = 0;
				if (chestTile.TileType == TileID.Containers2)
					chestTypeOffset = 54;

				int typeNum = chestTile.TileFrameX / 36 + chestTypeOffset;
				ChestType type = (ChestType)typeNum;

				// All -> Balloon
				// Doesn't happen in lite mode
				if (WorldGen.genRand.NextBool(100) && WorldGen.tenthAnniversaryWorldGen && !liteMode)
				{
					SwapChestType(chest, ChestType.Balloon, ref type);
					continue; // Skip to the next chest, since we've done our thing already
				}

				if (ChestTypesToSwap.Contains(type))
				{
					// Skip Locked Gold Chests on floating islands in FTW
					if (type == ChestType.LockedGold && chestTile.WallType == WallID.DiscWall && WorldGen.getGoodWorldGen)
						continue;

					// Only replace wooden chests in lite mode
					if (liteMode && type != ChestType.Wood)
						continue;

					Player dummyPlr = new();
					dummyPlr.position = new Vector2(chest.x, chest.y) * 16;
					dummyPlr.ForceUpdateBiomes();

					ChestType? newType = null;

					// Wood -> Ebonwood/Shadewood/Boreal Wood/Rich Mahogany or Bamboo/Bone/Pumpkin
					if (type == ChestType.Wood)
					{
						if (Main.halloween && WorldGen.genRand.NextBool(10))
							newType = ChestType.Pumpkin;
						else if (Main.wallDungeon[chestTile.WallType] && !liteMode) // Don't replace dungeon wood chests in lite mode
							newType = ChestType.Bone;
						else if (dummyPlr.ZoneCorrupt)
							newType = ChestType.Ebonwood;
						else if (dummyPlr.ZoneCrimson)
							newType = ChestType.ShadeWood;
						else if (dummyPlr.ZoneSnow)
							newType = ChestType.BorealWood;
						else if (dummyPlr.ZoneJungle)
							newType = WorldGen.genRand.NextBool() ? ChestType.RichMahogany : ChestType.Bamboo;
					}
					// Gold -> Lesion/Flesh/Obsidian/Golden/Mushroom (w/ mushroom loot)/Granite/Marble
					else if (type == ChestType.Gold)
					{
						// Mushroom Chest isn't part of the if/else chain so that it can replace its loot and then potentially be replaced by something else
						if (dummyPlr.ZoneGlowshroom)
						{
							newType = ChestType.GlowingMushroom;

							// Add glowshroom loot
							if (WorldGen.genRand.NextBool())
							{
								int i = 1;
								if (chest.item[0].type == ItemID.FlareGun)
									i = 2;

								for (int k = chest.item.Length - 1; k < i; k--)
								{
									chest.item[k] = chest.item[k - 1];
								}

								chest.item[i] = new Item();
								chest.item[i].SetDefaults(ItemID.ShroomMinecart);
								chest.item[i].Prefix(-1);
							}
							else
							{
								int i = 1;
								if (chest.item[0].type == ItemID.FlareGun)
									i = 2;

								for (int j = 0; j < 3; j++)
								{
									for (int k = chest.item.Length - 1; k < i; k--)
									{
										chest.item[k] = chest.item[k - 1];
									}

									chest.item[i] = new Item();
									chest.item[i].SetDefaults(ItemID.MushroomHat + j);
									chest.item[i].Prefix(-1);

									i++;
								}
							}
						}

						if (dummyPlr.ZoneCorrupt)
							newType = ChestType.Lesion;
						else if (dummyPlr.ZoneCrimson)
							newType = ChestType.Flesh;
						else if (chestTile.WallType == WallID.SandstoneBrick)
							newType = ChestType.PirateGoldenReal;
						else if (dummyPlr.ZoneGranite)
							newType = ChestType.Granite;
						else if (dummyPlr.ZoneMarble)
							newType = ChestType.Marble;
						
						// Also not part of the if/else chain so that it has an independent chance to replace anything (except pyramid chests)
						if (newType != ChestType.PirateGoldenReal && dummyPlr.ZoneRockLayerHeight && WorldGen.genRand.NextBool(4))
							newType = ChestType.Obsidian;
					}
					// Locked Gold -> Locked Dungeon
					else if (type == ChestType.LockedGold && Main.wallDungeon[chestTile.WallType])
					{
						switch (chestTile.WallType)
						{
							case WallID.BlueDungeonUnsafe:
							case WallID.BlueDungeonSlabUnsafe:
							case WallID.BlueDungeonTileUnsafe:
							case WallID.BlueDungeon:
							case WallID.BlueDungeonSlab:
							case WallID.BlueDungeonTile:
								newType = ChestType.LockedBlueDungeon;
								break;
							case WallID.GreenDungeonUnsafe:
							case WallID.GreenDungeonSlabUnsafe:
							case WallID.GreenDungeonTileUnsafe:
							case WallID.GreenDungeon:
							case WallID.GreenDungeonSlab:
							case WallID.GreenDungeonTile:
								newType = ChestType.LockedGreenDungeon;
								break;
							case WallID.PinkDungeonUnsafe:
							case WallID.PinkDungeonSlabUnsafe:
							case WallID.PinkDungeonTileUnsafe:
							case WallID.PinkDungeon:
							case WallID.PinkDungeonSlab:
							case WallID.PinkDungeonTile:
								newType = ChestType.LockedPinkDungeon;
								break;
						}
					}
					// Web-Covered -> Spider
					else if (type == ChestType.Webbed && WorldGen.genRand.NextBool(20))
						newType = ChestType.Spider;
					// Water -> Reef
					else if (type == ChestType.Water && WorldGen.genRand.NextBool(20))
						newType = ChestType.Reef;

					if (newType != null)
						SwapChestType(chest, (ChestType)newType, ref type);
				}
			}
		}

		private static void SwapChestType(Chest chest, ChestType newType, ref ChestType oldType)
		{
			for (int i = 0; i < 4; i++)
			{
				Vector2 off = i switch { 0 => Vector2.Zero, 1 => Vector2.UnitY, 2 => Vector2.UnitX, 3 => Vector2.One, _ => Vector2.One };
				int off2 = i > 1 ? 18 : 0;
				Tile chestTile = Main.tile[chest.x + (int)off.X, chest.y + (int)off.Y];
				chestTile.TileType = TileID.Containers;
				chestTile.TileFrameX = (short)(((int)newType * 36) + off2);

				if ((int)newType > 53) // Containers2 chests
				{
					chestTile.TileType = TileID.Containers2;
					chestTile.TileFrameX = (short)(((int)(newType - 54) * 36) + off2);
				}

				// Copy dungeon bricks' paint on Celebrationmk10 worlds
				if (WorldGen.tenthAnniversaryWorldGen && newType is ChestType.LockedBlueDungeon or ChestType.LockedGreenDungeon or ChestType.LockedPinkDungeon)
				{
					chestTile.CopyPaintAndCoating(Main.tile[chest.x, chest.y + 2]);
				}
			}

			oldType = newType;
		}
	}
}