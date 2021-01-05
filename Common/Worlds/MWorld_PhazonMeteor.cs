﻿#region Using directives

using System;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

using Microsoft.Xna.Framework;

#endregion

namespace MetroidMod.Common.Worlds
{
	public partial class MWorld : ModWorld
	{
		public static bool spawnedPhazonMeteor = false;

		public static void AddPhazon()
		{
			int lX = 200;
			int hX = Main.maxTilesX - 200;
			int lY = (int)Main.worldSurface;
			int hY = Main.maxTilesY - 200;

			int minSpread = 5;
			int maxSpread = 8;
			int minFrequency = 5;
			int maxFrequency = 8;

			WorldGen.OreRunner(WorldGen.genRand.Next(lX, hX), WorldGen.genRand.Next(lY, hY), WorldGen.genRand.Next(minSpread, maxSpread + 1), WorldGen.genRand.Next(minFrequency, maxFrequency + 1), (ushort)ModContent.TileType<Tiles.PhazonTile>());
		}

		public static void DropPhazonMeteor()
		{
			bool generateSuccessful = false;

			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				return;
			}

			// Check to get a valid position for the generation of a Phazon Meteorite.
			// There's a certain solid tile treshold (starting at 600, stopping at 100, cancelling the generation), which needs to be met
			// Before the meteorite is allowed to generate.
			float solidTileTreshhold = 600f;
			while (!generateSuccessful)
			{
				float spawnMargin = Main.maxTilesX * 0.08f;
				int genY = (int)(Main.worldSurface * 0.3);
				int genX = Main.rand.Next(150, Main.maxTilesX - 150);

				// Do not allow a Phazon Meteorite to spawn too close to the center of the world (spawnpoint).
				while (Math.Abs(genX - Main.spawnTileX) < spawnMargin)
				{
					genX = Main.rand.Next(150, Main.maxTilesX - 150);
				}
				
				while (genY < Main.maxTilesY)
				{
					if (Main.tile[genX, genY].active() && Main.tileSolid[Main.tile[genX, genY].type] && !Main.tileSolidTop[Main.tile[genX, genY].type])
					{
						int genOffset = 15;
						int solidSpawnTiles = 0;

						for (int x = genX - genOffset; x < genX + genOffset; x++)
						{
							for (int y = genY - genOffset; y < genY + genOffset; y++)
							{
								if (WorldGen.SolidTile(x, y))
								{
									solidSpawnTiles++;
									if (Main.tile[x, y].type == TileID.Cloud || Main.tile[x, y].type == TileID.Sunplate)
									{
										solidSpawnTiles -= 100;
									}
								}
								else if (Main.tile[x, y].liquid > 0)
								{
									solidSpawnTiles--;
								}
							}
						}
						if (solidSpawnTiles < solidTileTreshhold)
						{
							solidTileTreshhold -= 0.5f;
							break;
						}

						generateSuccessful = TryGeneratePhazonMeteor(genX, genY);
						break;
					}
					genY++;
				}
				if (solidTileTreshhold < 100f)
				{
					return;
				}
			}

			spawnedPhazonMeteor = generateSuccessful;
		}

		public static bool TryGeneratePhazonMeteor(int genX, int genY)
		{
			if (genX < 50 || genX > Main.maxTilesX - 50 ||
				genY < 50 || genY > Main.maxTilesY - 50)
			{
				return false;
			}

			int spawnOffset = 35;
			Rectangle rectangle = new Rectangle((genX - spawnOffset) * 16, (genY - spawnOffset) * 16, spawnOffset * 32, spawnOffset * 32);
			
			// If there's a player within the spawn area of the Phazon Meteorite, disallow spawning.
			for (int i = 0; i < Main.maxPlayers; i++)
			{
				if (Main.player[i].active)
				{
					Rectangle playerRectangle = new Rectangle(
						(int)(Main.player[i].position.X + (Main.player[i].width / 2) - (NPC.sWidth / 2) - NPC.safeRangeX),
						(int)(Main.player[i].position.Y + (Main.player[i].height / 2) - (NPC.sHeight / 2) - NPC.safeRangeY),
						NPC.sWidth + NPC.safeRangeX * 2, NPC.sHeight + NPC.safeRangeY * 2
					);

					if (rectangle.Intersects(playerRectangle))
					{
						return false;
					}
				}
			}

			// If there's an NPC within the spawn area of the Phazon Meteorite, disallow spawning.
			for (int i = 0; i < Main.maxNPCs; i++)
			{
				if (Main.npc[i].active)
				{
					if (rectangle.Intersects(Main.npc[i].Hitbox))
					{
						return false;
					}
				}
			}

			// If there's a chest within the spawn area of the Phazon Meteorite, disallow spawning.
			for (int x = genX - spawnOffset; x < genX + spawnOffset; x++)
			{
				for (int y = genY - spawnOffset; y < genY + spawnOffset; y++)
				{
					if (Main.tile[x, y].active() && TileID.Sets.BasicChest[Main.tile[x, y].type])
					{
						return false;
					}
				}
			}

			// Initial spawn of Phazon Tiles, and removal of non-solid tiles.
			GeneratePhazonChunkAt(genX, genY, WorldGen.genRand.Next(17, 23), (x, y, margin) =>
			{
				if (y > genY + Main.rand.Next(-2, 3) - 5)
				{
					float distance = new Vector2(genX - x, genY - y).Length();
					if (distance < margin * 0.9f + Main.rand.Next(-4, 5))
					{
						if (!Main.tileSolid[Main.tile[x, y].type])
						{
							Main.tile[x, y].active(false);
						}
						Main.tile[x, y].type = (ushort)ModContent.TileType<Tiles.PhazonTile>();
					}
				}
			});

			// Removal of tiles in the center of the 'crater'.
			GeneratePhazonChunkAt(genX, genY, WorldGen.genRand.Next(8, 14), (x, y, margin) =>
			{
				if (y > genY + Main.rand.Next(-2, 3) - 4)
				{
					float distance = new Vector2(genX - x, genY - y).Length();
					if (distance < margin * 0.8f + Main.rand.Next(-3, 4))
					{
						Main.tile[x, y].active(false);
					}
				}
			});

			// Placement of Phazon Core tiles.
			GeneratePhazonChunkAt(genX, genY + 4, WorldGen.genRand.Next(4, 6), (x, y, margin) =>
			{
				if (y > genY + Main.rand.Next(-2, 3) - 9 && (Math.Abs(genX - x) + Math.Abs(genY - 4 - y)) < margin * 1.5 + Main.rand.Next(-5, 5))
				{
					if (!Main.tileSolid[Main.tile[x, y].type])
					{
						Main.tile[x, y].active(false);
					}
					WorldGen.PlaceTile(x, y, ModContent.TileType<Tiles.PhazonCore>(), true);
					WorldGen.SquareTileFrame(x, y);
				}
			});

			// First generation spread pass of Phazon Tiles.
			GeneratePhazonChunkAt(genX, genY, WorldGen.genRand.Next(25, 35), (x, y, margin) =>
			{
				float distance = new Vector2(genX - x, genY - y).Length();
				if (distance < margin * 0.7f)
				{
					if (Main.tile[x, y].type == TileID.Trees || Main.tile[x, y].type == TileID.CorruptThorns || Main.tile[x, y].type == TileID.CrimtaneThorns)
					{
						WorldGen.KillTile(x, y, false, false, false);
					}
					Main.tile[x, y].liquid = 0;
				}

				if (Main.tile[x, y].type == ModContent.TileType<Tiles.PhazonTile>())
				{
					if (!WorldGen.SolidTile(x - 1, y) && !WorldGen.SolidTile(x + 1, y) && !WorldGen.SolidTile(x, y - 1) && !WorldGen.SolidTile(x, y + 1))
					{
						Main.tile[x, y].active(false);
					}
					else if ((Main.tile[x, y].halfBrick() || Main.tile[x - 1, y].topSlope()) && !WorldGen.SolidTile(x, y + 1))
					{
						Main.tile[x, y].active(false);
					}
				}

				WorldGen.SquareTileFrame(x, y);
				WorldGen.SquareWallFrame(x, y);
			});

			// Second generation spread pass of Phazon Tiles.
			GeneratePhazonChunkAt(genX, genY, WorldGen.genRand.Next(23, 32), (x, y, margin) =>
			{
				if (y > genY + WorldGen.genRand.Next(-3, 4) - 3 && Main.tile[x, y].active() && Main.rand.Next(10) == 0)
				{
					float distance = new Vector2(genX - x, genY - y).Length();
					if (distance < margin * 0.8f)
					{
						if (Main.tile[x, y].type == TileID.Trees || Main.tile[x, y].type == TileID.CorruptThorns || Main.tile[x, y].type == TileID.CrimtaneThorns)
						{
							WorldGen.KillTile(x, y, false, false, false);
						}
						Main.tile[x, y].type = (ushort)ModContent.TileType<Tiles.PhazonTile>();
						WorldGen.SquareTileFrame(x, y);
					}
				}
			});

			// Third generation spread pass of Phazon Tiles.
			GeneratePhazonChunkAt(genX, genY, WorldGen.genRand.Next(30, 38), (x, y, margin) =>
			{
				if (y > genY + WorldGen.genRand.Next(-2, 3) && Main.tile[x, y].active() && Main.rand.Next(20) == 0)
				{
					float distance = new Vector2(genX - x, genY - y).Length();
					if (distance < margin * 0.85f)
					{
						if (Main.tile[x, y].type == TileID.Trees || Main.tile[x, y].type == TileID.CorruptThorns || Main.tile[x, y].type == TileID.CrimtaneThorns)
						{
							WorldGen.KillTile(x, y, false, false, false);
						}
						Main.tile[x, y].type = (ushort)ModContent.TileType<Tiles.PhazonTile>();
						WorldGen.SquareTileFrame(x, y);
					}
				}
			});

			if (Main.netMode == NetmodeID.SinglePlayer)
			{
				Main.NewText("A Phazon Meteor has landed!", 50, 255, 130, false);
			}
			else if (Main.netMode == NetmodeID.Server)
			{
				NetMessage.BroadcastChatMessage(NetworkText.FromLiteral("A Phazon Meteor has landed!"), new Color(50, 255, 130), -1);
			}

			// Since we are not able to get here if we're in a multiplayer client session, removed the check.
			NetMessage.SendTileSquare(-1, genX, genY, 40, TileChangeType.None);

			return (true);
		}

		/// <summary>
		/// A helper method to spawn clusters of tiles for the Phazon Meteorite.
		/// </summary>
		/// <param name="genX">The X tile coordinate of the Phazon Meteorite spawn.</param>
		/// <param name="genY">The Y tile coordinate of the Phazon Meteorite spawn.</param>
		/// <param name="margin">The margin which is used to spread out tiles to a certain point around the spawn coordinates.</param>
		/// <param name="generationAction">The action to take for each tile within the chunk/spawn area.</param>
		private static void GeneratePhazonChunkAt(int genX, int genY, int margin, Action<int, int, int> generationAction)
		{
			for (int x = genX - margin; x < genX + margin; ++x)
			{
				for (int y = genY - margin; y < genY + margin; ++y)
				{
					generationAction(x, y, margin);
				}
			}
		}
	}
}
