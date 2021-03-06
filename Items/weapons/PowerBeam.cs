using System;
using System.IO;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Terraria;
using Terraria.ID;
using Terraria.UI;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

using MetroidMod.Items.damageclass;
using MetroidMod.Projectiles;
using MetroidMod.Projectiles.chargelead;

namespace MetroidMod.Items.weapons
{
	/*
	 * TODO: 
	 * IMPLEMENT ERROR MESSAGE WITH MOD MISMATCH: ('comboErrorType')
	 * 
	 */
	public class PowerBeam : HunterDamageItem//ModItem
	{
		// Failsaves.
		private Item[] _beamMods;
		public Item[] beamMods
		{
			get
			{
				if(_beamMods == null)
				{
					_beamMods = new Item[MetroidMod.beamSlotAmount];
					for(int i = 0; i < _beamMods.Length; ++i)
					{
						_beamMods[i] = new Item();
						_beamMods[i].TurnToAir();
					}
				}

				return _beamMods;
			}
			set { _beamMods = value; }
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Power Beam");
			Tooltip.SetDefault("Select this item in your hotbar and open your inventory to open the Beam Addon UI");

			beamMods = new Item[5];
		}
		//public override void SetDefaults()
		public override void SafeSetDefaults()
		{
			item.damage = 14;
			//item.ranged = true;
			item.width = 24;
			item.height = 12;
			item.scale = 0.8f;
			item.useTime = 14;
			item.useAnimation = 14;
			item.useStyle = 5;
			item.noMelee = true;
			item.knockBack = 4;
			item.value = 20000;
			item.rare = 2;
			//item.UseSound = mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/PowerBeamSound");
			item.autoReuse = true;
			item.shoot = mod.ProjectileType("PowerBeamShot");
			item.shootSpeed = 8f;
			item.crit = 3;
		}
		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "ChoziteBar", 8);
			recipe.AddIngredient(null, "EnergyShard", 3);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
		
		public override void UseStyle(Player P)
		{
			P.itemLocation.X = P.MountedCenter.X - (float)item.width * 0.5f;
			P.itemLocation.Y = P.MountedCenter.Y - (float)item.height * 0.5f;
		}
		
		public override bool CanUseItem(Player player)
		{
			MPlayer mp = player.GetModPlayer<MPlayer>();
			if(player.whoAmI == Main.myPlayer && item.type == Main.mouseItem.type)
			{
				return false;
			}
			if(beamMods[0].type == mod.ItemType("PhazonBeamAddon") && !mp.canUsePhazonBeam)
			{
				return false;
			}
			return (mp.statOverheat < mp.maxOverheat);
		}
		
		float iceDmg = 0f;
		float waveDmg = 0f;
		float spazDmg = 0f;
		float plasDmg = 0f;

		float iceHeat = 0f;
		float waveHeat = 0f;
		float spazHeat = 0f;
		float plasHeat = 0f;
		
		float iceSpeed = 0f;
		float waveSpeed = 0f;
		float spazSpeed = 0f;
		float plasSpeed = 0f;
		
		int finalDmg = 14;
		
		float chargeDmgMult = 3f;
		float chargeCost = 2f;
		
		int overheat = 4;
		int useTime = 14;
		
		string shot = "PowerBeamShot";
		string chargeShot = "PowerBeamChargeShot";
		string shotSound = "PowerBeamSound";
		string chargeShotSound = "PowerBeamChargeSound";
		string chargeUpSound = "ChargeStartup_Power";
		string chargeTex = "ChargeLead";
		int dustType = 64;
		Color dustColor = default(Color);
		Color lightColor = MetroidMod.powColor;
		int shotAmt = 1;
		int chargeShotAmt = 1;
		
		public Terraria.Audio.LegacySoundStyle ShotSound;
		public Terraria.Audio.LegacySoundStyle ChargeShotSound;
		
		int waveDir = -1;

		bool isCharge = false;
		
		bool isHyper = false;
		bool isPhazon = false;
		
		public bool comboError1, comboError2, comboError3, comboError4;
		
		string altTexture => this.Texture + "_Alt";
		string texture = "";

		public override void UpdateInventory(Player P)
		{
			MPlayer mp = P.GetModPlayer<MPlayer>();

			int ch = mod.ItemType("ChargeBeamAddon");
			int ic = mod.ItemType("IceBeamAddon");
			int wa = mod.ItemType("WaveBeamAddon");
			int sp = mod.ItemType("SpazerAddon");
			int plR = mod.ItemType("PlasmaBeamRedAddon");
			int plG = mod.ItemType("PlasmaBeamGreenAddon");
			
			int ch2 = mod.ItemType("ChargeBeamV2Addon");
			int ic2 = mod.ItemType("IceBeamV2Addon");
			int wa2 = mod.ItemType("WaveBeamV2Addon");
			int wi = mod.ItemType("WideBeamAddon");
			int nv = mod.ItemType("NovaBeamAddon");
			
			int ch3 = mod.ItemType("LuminiteBeamAddon");
			int sd = mod.ItemType("StardustBeamAddon");
			int nb = mod.ItemType("NebulaBeamAddon");
			int vt = mod.ItemType("VortexBeamAddon");
			int sl = mod.ItemType("SolarBeamAddon");

			int hy = mod.ItemType("HyperBeamAddon");
			int ph = mod.ItemType("PhazonBeamAddon");
			
			Item slot1 = beamMods[0];
			Item slot2 = beamMods[1];
			Item slot3 = beamMods[2];
			Item slot4 = beamMods[3];
			Item slot5 = beamMods[4];
			
			int damage = 14;
			overheat = 4;
			useTime = 14;
			shot = "PowerBeamShot";
			chargeShot = "PowerBeamChargeShot";
			shotAmt = 1;
			chargeShotAmt = 1;
			shotSound = "PowerBeamSound";
			chargeShotSound = "PowerBeamChargeSound";
			chargeUpSound = "ChargeStartup_Power";
			chargeTex = "ChargeLead";
			dustType = 64;
			dustColor = default(Color);
			lightColor = MetroidMod.powColor;
			
			texture = "";
			
			ShotSound = null;
			ChargeShotSound = null;
			
			isCharge = (slot1.type == ch || slot1.type == ch2 || slot1.type == ch3);
			isHyper = (slot1.type == hy);
			isPhazon = (slot1.type == ph);
			
			comboError1 = false;
			comboError2 = false;
			comboError3 = false;
			comboError4 = false;
			
			bool chargeV1 = (slot1.type == ch), 
				chargeV2 = (slot1.type == ch2), 
				chargeV3 = (slot1.type == ch3);
			
			bool addonsV1 = (slot2.type == ic || slot3.type == wa || slot4.type == sp || ((slot5.type == plG || slot5.type == plR) && !chargeV2 && !chargeV3));
			bool addonsV2 = (slot2.type == ic2 || slot3.type == wa2 || slot4.type == wi || slot5.type == nv);
				addonsV2 |= ((slot5.type == plG || slot5.type == plR) && (chargeV2 || chargeV3) && !addonsV1);
			bool addonsV3 = (slot2.type == sd || slot3.type == nb || slot4.type == vt || slot5.type == sl);
			
			int versionType = 1;
			if(addonsV3 || (chargeV3 && !addonsV1 && !addonsV2))
			{
				versionType = 3;
			}
			else if(addonsV2 || (chargeV2 && !addonsV1))
			{
				versionType = 2;
			}
			

			// Default Combos
			if(!isHyper && !isPhazon)
			{
				//if(slot1.type != ch2 && slot1.type != ch3 && (slot1.type == ch || slot2.type == ic || slot3.type == wa || slot4.type == sp || slot5.type == plG || slot5.type == plR))
				if(versionType == 1)
				{
					// Ice
					if(slot2.type == ic)
					{
						shot = "IceBeamShot";
						chargeShot = "IceBeamChargeShot";
						shotSound = "IceBeamSound";
						chargeShotSound = "IceBeamChargeSound";
						chargeUpSound = "ChargeStartup_Ice";
						chargeTex = "ChargeLead_Ice";
						dustType = 59;
						lightColor = MetroidMod.iceColor;
						texture = "IceBeam_Item";

						// Ice Wave
						if(slot3.type == wa)
						{
							shot = "IceWaveBeamShot";
							chargeShot = "IceWaveBeamChargeShot";
							chargeShotAmt = 2;

							// Ice Wave Spazer
							if(slot4.type == sp)
							{
								shot = "IceWaveSpazerShot";
								chargeShot = "IceWaveSpazerChargeShot";
								shotSound = "IceComboSound";
								shotAmt = 3;
								chargeShotAmt = 3;
								
								// Ice Wave Spazer Plasma (Green)
								if(slot5.type == plG)
								{
									shot = "IceWaveSpazerPlasmaBeamGreenShot";
									chargeShot = "IceWaveSpazerPlasmaBeamGreenChargeShot";
									shotSound = "IceComboSound";
								}
								// Ice Wave Spazer Plasma (Red)
								if(slot5.type == plR)
								{
									shot = "IceWaveSpazerPlasmaBeamRedShot";
									chargeShot = "IceWaveSpazerPlasmaBeamRedChargeShot";
									shotSound = "IceComboSound";
									dustType = 135;
								}
							}
							else
							{
								// Ice Wave Plasma (Green)
								if(slot5.type == plG)
								{
									shot = "IceWavePlasmaBeamGreenShot";
									chargeShot = "IceWavePlasmaBeamGreenChargeShot";
									shotSound = "IceComboSound";
									shotAmt = 2;
								}
								// Ice Wave Plasma (Red)
								if(slot5.type == plR)
								{
									shot = "IceWavePlasmaBeamRedShot";
									chargeShot = "IceWavePlasmaBeamRedChargeShot";
									shotSound = "IceComboSound";
									shotAmt = 2;
									dustType = 135;
								}
							}
						}
						else
						{
							// Ice Spazer
							if(slot4.type == sp)
							{
								shot = "IceSpazerShot";
								chargeShot = "IceSpazerChargeShot";
								shotSound = "IceComboSound";
								shotAmt = 3;
								chargeShotAmt = 3;

								// Ice Spazer Plasma (Green)
								if(slot5.type == plG)
								{
									shot = "IceSpazerPlasmaBeamGreenShot";
									chargeShot = "IceSpazerPlasmaBeamGreenChargeShot";
									shotSound = "IceComboSound";
								}
								// Ice Spazer Plasma (Red)
								if(slot5.type == plR)
								{
									shot = "IceSpazerPlasmaBeamRedShot";
									chargeShot = "IceSpazerPlasmaBeamRedChargeShot";
									shotSound = "IceComboSound";
									dustType = 135;
								}
							}
							else
							{
								// Ice Plasma (Green)
								if(slot5.type == plG)
								{
									shot = "IcePlasmaBeamGreenShot";
									chargeShot = "IcePlasmaBeamGreenChargeShot";
									shotSound = "IceComboSound";
								}
								// Ice Plasma (Red)
								if(slot5.type == plR)
								{
									shot = "IcePlasmaBeamRedShot";
									chargeShot = "IcePlasmaBeamRedChargeShot";
									shotSound = "IceComboSound";
									dustType = 135;
								}
							}
						}
					}
					else
					{
						// Wave
						if(slot3.type == wa)
						{
							shot = "WaveBeamShot";
							chargeShot = "WaveBeamChargeShot";
							shotSound = "WaveBeamSound";
							chargeShotSound = "WaveBeamChargeSound";
							chargeUpSound = "ChargeStartup_Wave";
							chargeTex = "ChargeLead_Wave";
							dustType = 62;
							lightColor = MetroidMod.waveColor;
							chargeShotAmt = 2;
							texture = "WaveBeam_Item";

							// Wave Spazer
							if(slot4.type == sp)
							{
								shot = "WaveSpazerShot";
								chargeShot = "WaveSpazerChargeShot";
								shotSound = "SpazerSound";
								shotAmt = 3;
								chargeShotAmt = 3;

								// Wave Spazer Plasma (Green)
								if(slot5.type == plG)
								{
									shot = "WaveSpazerPlasmaBeamGreenShot";
									chargeShot = "WaveSpazerPlasmaBeamGreenChargeShot";
									shotSound = "PlasmaBeamGreenSound";
									chargeShotSound = "PlasmaBeamGreenChargeSound";
									chargeUpSound = "ChargeStartup_Power";
									chargeTex = "ChargeLead_PlasmaGreen";
									dustType = 61;
									lightColor = MetroidMod.plaGreenColor;
									texture = "PlasmaBeamG_Item";
								}
								// Wave Spazer Plasma (Red)
								if(slot5.type == plR)
								{
									shot = "WaveSpazerPlasmaBeamRedShot";
									chargeShot = "WaveSpazerPlasmaBeamRedChargeShot";
									shotSound = "PlasmaBeamRedSound";
									chargeShotSound = "PlasmaBeamRedChargeSound";
									chargeUpSound = "ChargeStartup_PlasmaRed";
									chargeTex = "ChargeLead_PlasmaRed";
									dustType = 6;
									lightColor = MetroidMod.plaRedColor;
									texture = "PlasmaBeamR_Item";
								}
							}
							else
							{
								// Wave Plasma (Green)
								if(slot5.type == plG)
								{
									shot = "WavePlasmaBeamGreenShot";
									chargeShot = "WavePlasmaBeamGreenChargeShot";
									shotSound = "PlasmaBeamGreenSound";
									chargeShotSound = "PlasmaBeamGreenChargeSound";
									chargeUpSound = "ChargeStartup_Power";
									chargeTex = "ChargeLead_PlasmaGreen";
									dustType = 61;
									lightColor = MetroidMod.plaGreenColor;
									shotAmt = 2;
									texture = "PlasmaBeamG_Item";
								}
								// Wave Plasma (Red)
								if(slot5.type == plR)
								{
									shot = "WavePlasmaBeamRedShot";
									chargeShot = "WavePlasmaBeamRedChargeShot";
									shotSound = "PlasmaBeamRedSound";
									chargeShotSound = "PlasmaBeamRedChargeSound";
									chargeUpSound = "ChargeStartup_PlasmaRed";
									chargeTex = "ChargeLead_PlasmaRed";
									dustType = 6;
									lightColor = MetroidMod.plaRedColor;
									shotAmt = 2;
									texture = "PlasmaBeamR_Item";
								}
							}
						}
						else
						{
							// Spazer
							if(slot4.type == sp)
							{
								shot = "SpazerShot";
								chargeShot = "SpazerChargeShot";
								shotSound = "SpazerSound";
								chargeShotSound = "SpazerChargeSound";
								chargeTex = "ChargeLead_Spazer";
								shotAmt = 3;
								chargeShotAmt = 3;
								texture = "Spazer_Item";

								// Spazer Plasma (Green)
								if(slot5.type == plG)
								{
									shot = "SpazerPlasmaBeamGreenShot";
									chargeShot = "SpazerPlasmaBeamGreenChargeShot";
									shotSound = "PlasmaBeamGreenSound";
									chargeShotSound = "PlasmaBeamGreenChargeSound";
									chargeTex = "ChargeLead_PlasmaGreen";
									dustType = 61;
									lightColor = MetroidMod.plaGreenColor;
									texture = "PlasmaBeamG_Item";
								}
								// Spazer Plasma (Red)
								if(slot5.type == plR)
								{
									shot = "SpazerPlasmaBeamRedShot";
									chargeShot = "SpazerPlasmaBeamRedChargeShot";
									shotSound = "PlasmaBeamRedSound";
									chargeShotSound = "PlasmaBeamRedChargeSound";
									chargeUpSound = "ChargeStartup_PlasmaRed";
									chargeTex = "ChargeLead_PlasmaRed";
									dustType = 6;
									lightColor = MetroidMod.plaRedColor;
									texture = "PlasmaBeamR_Item";
								}
							}
							else
							{
								// Plasma (Green)
								if(slot5.type == plG)
								{
									shot = "PlasmaBeamGreenShot";
									chargeShot = "PlasmaBeamGreenChargeShot";
									shotSound = "PlasmaBeamGreenSound";
									chargeShotSound = "PlasmaBeamGreenChargeSound";
									chargeTex = "ChargeLead_PlasmaGreen";
									dustType = 61;
									lightColor = MetroidMod.plaGreenColor;
									texture = "PlasmaBeamG_Item";
								}
								// Plasma (Red)
								if(slot5.type == plR)
								{
									shot = "PlasmaBeamRedShot";
									chargeShot = "PlasmaBeamRedChargeShot";
									shotSound = "PlasmaBeamRedSound";
									chargeShotSound = "PlasmaBeamRedChargeSound";
									chargeUpSound = "ChargeStartup_PlasmaRed";
									chargeTex = "ChargeLead_PlasmaRed";
									dustType = 6;
									lightColor = MetroidMod.plaRedColor;
									texture = "PlasmaBeamR_Item";
								}
							}
						}
					}
				}
				// Charge V2
				//else if(slot1.type != ch3 && (slot1.type == ch2 || slot2.type == ic2 || slot3.type == wa2 || slot4.type == wi || slot5.type == nv || slot5.type == plG || slot5.type == plR))
				else if(versionType == 2)
				{
					shot = "PowerBeamV2Shot";
					chargeShot = "PowerBeamV2ChargeShot";
					shotSound = "PowerBeamV2Sound";

					// Ice V2
					if(slot2.type == ic2)
					{
						shot = "IceBeamV2Shot";
						chargeShot = "IceBeamV2ChargeShot";
						shotSound = "IceBeamV2Sound";
						chargeShotSound = "IceBeamChargeSound";
						chargeUpSound = "ChargeStartup_Ice";
						chargeTex = "ChargeLead_Ice";
						dustType = 59;
						lightColor = MetroidMod.iceColor;
						texture = "IceBeam_Item";

						// Ice Wave V2
						if(slot3.type == wa2)
						{
							shot = "IceWaveBeamV2Shot";
							chargeShot = "IceWaveBeamV2ChargeShot";
							//shotAmt = 2;
							chargeShotAmt = 2;

							// Ice Wave Wide
							if(slot4.type == wi)
							{
								shot = "IceWaveWideBeamShot";
								chargeShot = "IceWaveWideBeamChargeShot";
								shotAmt = 3;
								chargeShotAmt = 3;

								// Ice Wave Wide Nova
								if(slot5.type == nv)
								{
									shot = "IceWaveWideNovaBeamShot";
									chargeShot = "IceWaveWideNovaBeamChargeShot";
									shotSound = "IceWaveNovaBeamV2Sound";
								}
								// Ice Wave Wide Plasma (Green)
								if(slot5.type == plG)
								{
									shot = "IceWaveWidePlasmaBeamGreenV2Shot";
									chargeShot = "IceWaveWidePlasmaBeamGreenV2ChargeShot";
									shotSound = "FinalBeamSound";
									chargeShotSound = "FinalBeamChargeSound";
									chargeUpSound = "ChargeStartup_Final";
								}
								// Ice Wave Wide Plasma (Red)
								if(slot5.type == plR)
								{
									shot = "IceWaveWidePlasmaBeamRedV2Shot";
									chargeShot = "IceWaveWidePlasmaBeamRedV2ChargeShot";
									shotSound = "FinalBeamSound";
									chargeShotSound = "FinalBeamChargeSound";
									chargeUpSound = "ChargeStartup_Final";
									dustType = 135;
								}
							}
							else
							{
								// Ice Wave Nova
								if(slot5.type == nv)
								{
									shot = "IceWaveNovaBeamShot";
									chargeShot = "IceWaveNovaBeamChargeShot";
									shotAmt = 2;
								}
								// Ice Wave Plasma (Green)
								if(slot5.type == plG)
								{
									shot = "IceWavePlasmaBeamGreenV2Shot";
									chargeShot = "IceWavePlasmaBeamGreenV2ChargeShot";
									shotAmt = 2;
								}
								// Ice Wave Plasma (Red)
								if(slot5.type == plR)
								{
									shot = "IceWavePlasmaBeamRedV2Shot";
									chargeShot = "IceWavePlasmaBeamRedV2ChargeShot";
									shotAmt = 2;
									dustType = 135;
								}
							}
						}
						else
						{
							// Ice Wide
							if(slot4.type == wi)
							{
								shot = "IceWideBeamShot";
								chargeShot = "IceWideBeamChargeShot";
								shotAmt = 3;
								chargeShotAmt = 3;

								// Ice Wide Nova
								if(slot5.type == nv)
								{
									shot = "IceWideNovaBeamShot";
									chargeShot = "IceWideNovaBeamChargeShot";
								}
								// Ice Wide Plasma (Green)
								if(slot5.type == plG)
								{
									shot = "IceWidePlasmaBeamGreenV2Shot";
									chargeShot = "IceWidePlasmaBeamGreenV2ChargeShot";
								}
								// Ice Wide Plasma (Red)
								if(slot5.type == plR)
								{
									shot = "IceWidePlasmaBeamRedV2Shot";
									chargeShot = "IceWidePlasmaBeamRedV2ChargeShot";
									dustType = 135;
								}
							}
							else
							{
								// Ice Nova
								if(slot5.type == nv)
								{
									shot = "IceNovaBeamShot";
									chargeShot = "IceNovaBeamChargeShot";
								}
								// Ice Plasma (Green)
								if(slot5.type == plG)
								{
									shot = "IcePlasmaBeamGreenV2Shot";
									chargeShot = "IcePlasmaBeamGreenV2ChargeShot";
								}
								// Ice Plasma (Red)
								if(slot5.type == plR)
								{
									shot = "IcePlasmaBeamRedV2Shot";
									chargeShot = "IcePlasmaBeamRedV2ChargeShot";
									dustType = 135;
								}
							}
						}
					}
					else
					{
						// Wave V2
						if(slot3.type == wa2)
						{
							shot = "WaveBeamV2Shot";
							chargeShot = "WaveBeamV2ChargeShot";
							shotSound = "WaveBeamV2Sound";
							chargeShotSound = "WaveBeamChargeSound";
							chargeUpSound = "ChargeStartup_Wave";
							chargeTex = "ChargeLead_WaveV2";
							dustType = 62;
							lightColor = MetroidMod.waveColor2;
							//shotAmt = 2;
							chargeShotAmt = 2;
							texture = "WaveBeam_Item";

							// Wave Wide
							if(slot4.type == wi)
							{
								shot = "WaveWideBeamShot";
								chargeShot = "WaveWideBeamChargeShot";
								shotAmt = 3;
								chargeShotAmt = 3;

								// Wave Wide Nova
								if(slot5.type == nv)
								{
									shot = "WaveWideNovaBeamShot";
									chargeShot = "WaveWideNovaBeamChargeShot";
									shotSound = "NovaBeamSound";
									chargeShotSound = "NovaBeamChargeSound";
									chargeUpSound = "ChargeStartup_Nova";
									chargeTex = "ChargeLead_Nova";
									dustType = 75;
									lightColor = MetroidMod.novColor;
									texture = "NovaBeam_Item";
								}
								// Wave Wide Plasma (Green)
								if(slot5.type == plG)
								{
									shot = "WaveWidePlasmaBeamGreenV2Shot";
									chargeShot = "WaveWidePlasmaBeamGreenV2ChargeShot";
									shotSound = "WavePlasmaBeamGreenSound";
									chargeShotSound = "PlasmaBeamGreenChargeSound";
									chargeUpSound = "ChargeStartup_Power";
									chargeTex = "ChargeLead_PlasmaGreenV2";
									dustType = 15;
									lightColor = MetroidMod.plaGreenColor;
									texture = "PlasmaBeamG_Item";
								}
								// Wave Wide Plasma (Red)
								if(slot5.type == plR)
								{
									shot = "WaveWidePlasmaBeamRedV2Shot";
									chargeShot = "WaveWidePlasmaBeamRedV2ChargeShot";
									shotSound = "PlasmaBeamRedV2Sound";
									chargeShotSound = "PlasmaBeamRedChargeSound";
									chargeUpSound = "ChargeStartup_PlasmaRed";
									chargeTex = "ChargeLead_PlasmaRed";
									dustType = 6;
									lightColor = MetroidMod.plaRedColor;
									texture = "PlasmaBeamR_Item";
								}
							}
							else
							{
								// Wave Nova
								if(slot5.type == nv)
								{
									shot = "WaveNovaBeamShot";
									chargeShot = "WaveNovaBeamChargeShot";
									shotSound = "NovaBeamSound";
									chargeShotSound = "NovaBeamChargeSound";
									chargeUpSound = "ChargeStartup_Nova";
									chargeTex = "ChargeLead_Nova";
									dustType = 75;
									lightColor = MetroidMod.novColor;
									shotAmt = 2;
									texture = "NovaBeam_Item";
								}
								// Wave Plasma (Green)
								if(slot5.type == plG)
								{
									shot = "WavePlasmaBeamGreenV2Shot";
									chargeShot = "WavePlasmaBeamGreenV2ChargeShot";
									shotSound = "WavePlasmaBeamGreenSound";
									chargeShotSound = "PlasmaBeamGreenChargeSound";
									chargeUpSound = "ChargeStartup_Power";
									chargeTex = "ChargeLead_PlasmaGreenV2";
									dustType = 15;
									lightColor = MetroidMod.plaGreenColor;
									shotAmt = 2;
									texture = "PlasmaBeamG_Item";
								}
								// Wave Plasma (Red)
								if(slot5.type == plR)
								{
									shot = "WavePlasmaBeamRedV2Shot";
									chargeShot = "WavePlasmaBeamRedV2ChargeShot";
									shotSound = "PlasmaBeamRedV2Sound";
									chargeShotSound = "PlasmaBeamRedChargeSound";
									chargeUpSound = "ChargeStartup_PlasmaRed";
									chargeTex = "ChargeLead_PlasmaRed";
									dustType = 6;
									lightColor = MetroidMod.plaRedColor;
									shotAmt = 2;
									texture = "PlasmaBeamR_Item";
								}
							}
						}
						else
						{
							// Wide
							if(slot4.type == wi)
							{
								shot = "WideBeamShot";
								chargeShot = "WideBeamChargeShot";
								shotSound = "WideBeamSound";
								chargeShotSound = "SpazerChargeSound";
								chargeTex = "ChargeLead_Wide";
								dustType = 63;
								lightColor = MetroidMod.wideColor;
								dustColor = MetroidMod.wideColor;
								shotAmt = 3;
								chargeShotAmt = 3;
								texture = "WideBeam_Item";

								// Wide Nova
								if(slot5.type == nv)
								{
									shot = "WideNovaBeamShot";
									chargeShot = "WideNovaBeamChargeShot";
									shotSound = "NovaBeamSound";
									chargeShotSound = "NovaBeamChargeSound";
									chargeUpSound = "ChargeStartup_Nova";
									chargeTex = "ChargeLead_Nova";
									dustType = 75;
									lightColor = MetroidMod.novColor;
									dustColor = default(Color);
									texture = "NovaBeam_Item";
								}
								// Wide Plasma (Green)
								if(slot5.type == plG)
								{
									shot = "WidePlasmaBeamGreenV2Shot";
									chargeShot = "WidePlasmaBeamGreenV2ChargeShot";
									shotSound = "WidePlasmaBeamGreenSound";
									chargeShotSound = "PlasmaBeamGreenChargeSound";
									chargeTex = "ChargeLead_PlasmaGreen";
									dustType = 61;
									lightColor = MetroidMod.plaGreenColor;
									dustColor = default(Color);
									texture = "PlasmaBeamG_Item";
								}
								// Wide Plasma (Red)
								if(slot5.type == plR)
								{
									shot = "WidePlasmaBeamRedV2Shot";
									chargeShot = "WidePlasmaBeamRedV2ChargeShot";
									shotSound = "PlasmaBeamRedSound";
									chargeShotSound = "PlasmaBeamRedChargeSound";
									chargeUpSound = "ChargeStartup_PlasmaRed";
									chargeTex = "ChargeLead_PlasmaRed";
									dustType = 6;
									lightColor = MetroidMod.plaRedColor;
									dustColor = default(Color);
									texture = "PlasmaBeamR_Item";
								}
							}
							else
							{
								// Nova
								if(slot5.type == nv)
								{
									shot = "NovaBeamShot";
									chargeShot = "NovaBeamChargeShot";
									shotSound = "NovaBeamSound";
									chargeShotSound = "NovaBeamChargeSound";
									chargeUpSound = "ChargeStartup_Nova";
									chargeTex = "ChargeLead_Nova";
									dustType = 75;
									lightColor = MetroidMod.novColor;
									texture = "NovaBeam_Item";
								}
								// Plasma (Green)
								if(slot5.type == plG)
								{
									shot = "PlasmaBeamGreenV2Shot";
									chargeShot = "PlasmaBeamGreenV2ChargeShot";
									shotSound = "PlasmaBeamGreenSound";
									chargeShotSound = "PlasmaBeamGreenChargeSound";
									chargeTex = "ChargeLead_PlasmaGreen";
									dustType = 61;
									lightColor = MetroidMod.plaGreenColor;
									texture = "PlasmaBeamG_Item";
								}
								// Plasma (Red)
								if(slot5.type == plR)
								{
									shot = "PlasmaBeamRedV2Shot";
									chargeShot = "PlasmaBeamRedV2ChargeShot";
									shotSound = "PlasmaBeamRedSound";
									chargeShotSound = "PlasmaBeamRedChargeSound";
									chargeUpSound = "ChargeStartup_PlasmaRed";
									chargeTex = "ChargeLead_PlasmaRed";
									dustType = 6;
									lightColor = MetroidMod.plaRedColor;
									texture = "PlasmaBeamR_Item";
								}
							}
						}
					}
					
					if(slot2.type == ic)
					{
						comboError1 = true;
					}
					if(slot3.type == wa)
					{
						comboError2 = true;
					}
					if(slot4.type == sp)
					{
						comboError3 = true;
					}
				}
				// Charge V3
				//else if(slot1.type == ch3 || slot2.type == sd || slot3.type == nb || slot4.type == vt || slot5.type == sl)
				else if(versionType == 3)
				{
					shot = "LuminiteBeamShot";
					chargeShot = "LuminiteBeamChargeShot";
					shotSound = "PowerBeamV2Sound";
					//ShotSound = SoundID.Item91;
					//chargeShotSound = "IceBeamChargeSound";
					//chargeUpSound = "ChargeStartup_Ice";
					chargeTex = "ChargeLead_Luminite";
					dustType = 229;
					lightColor = MetroidMod.lumColor;
					
					// Stardust
					if(slot2.type == sd)
					{
						shot = "StardustBeamShot";
						chargeShot = "StardustBeamChargeShot";
						shotSound = "IceBeamV2Sound";
						chargeShotSound = "IceBeamChargeSound";
						chargeUpSound = "ChargeStartup_Ice";
						chargeTex = "ChargeLead_Stardust";
						dustType = 87;
						lightColor = MetroidMod.iceColor;
						texture = "StardustBeam_Item";
						
						// Stardust Nebula
						if(slot3.type == nb)
						{
							shot = "StardustNebulaBeamShot";
							chargeShot = "StardustNebulaBeamChargeShot";
							shotAmt = 2;
							chargeShotAmt = 2;
							
							// Stardust Nebula Vortex
							if(slot4.type == vt)
							{
								shot = "StardustNebulaVortexBeamShot";
								chargeShot = "StardustNebulaVortexBeamChargeShot";
								shotAmt = 5;
								chargeShotAmt = 5;
								
								// Stardust Nebula Vortex Solar
								if(slot5.type == sl)
								{
									shot = "StardustNebulaVortexSolarBeamShot";
									chargeShot = "StardustNebulaVortexSolarBeamChargeShot";
									shotSound = "PlasmaBeamRedV2Sound";
									chargeShotSound = "PlasmaBeamRedChargeSound";
									chargeUpSound = "ChargeStartup_PlasmaRed";
									chargeTex = "ChargeLead_Solar";
									lightColor = MetroidMod.plaRedColor;
									texture = "SolarBeam_Item";
								}
							}
							else
							{
								// Stardust Nebula Solar
								if(slot5.type == sl)
								{
									shot = "StardustNebulaSolarBeamShot";
									chargeShot = "StardustNebulaSolarBeamChargeShot";
									shotSound = "PlasmaBeamRedV2Sound";
									chargeShotSound = "PlasmaBeamRedChargeSound";
									chargeUpSound = "ChargeStartup_PlasmaRed";
									chargeTex = "ChargeLead_Solar";
									lightColor = MetroidMod.plaRedColor;
									texture = "SolarBeam_Item";
								}
							}
						}
						else
						{
							// Stardust Vortex
							if(slot4.type == vt)
							{
								shot = "StardustVortexBeamShot";
								chargeShot = "StardustVortexBeamChargeShot";
								shotAmt = 5;
								chargeShotAmt = 5;
								
								// Stardust Vortex Solar
								if(slot5.type == sl)
								{
									shot = "StardustVortexSolarBeamShot";
									chargeShot = "StardustVortexSolarBeamChargeShot";
									shotSound = "PlasmaBeamRedV2Sound";
									chargeShotSound = "PlasmaBeamRedChargeSound";
									chargeUpSound = "ChargeStartup_PlasmaRed";
									chargeTex = "ChargeLead_Solar";
									lightColor = MetroidMod.plaRedColor;
									texture = "SolarBeam_Item";
								}
							}
							else
							{
								// Stardust Solar
								if(slot5.type == sl)
								{
									shot = "StardustSolarBeamShot";
									chargeShot = "StardustSolarBeamChargeShot";
									shotSound = "PlasmaBeamRedV2Sound";
									chargeShotSound = "PlasmaBeamRedChargeSound";
									chargeUpSound = "ChargeStartup_PlasmaRed";
									chargeTex = "ChargeLead_Solar";
									lightColor = MetroidMod.plaRedColor;
									texture = "SolarBeam_Item";
								}
							}
						}
					}
					else
					{
						// Nebula
						if(slot3.type == nb)
						{
							shot = "NebulaBeamShot";
							chargeShot = "NebulaBeamChargeShot";
							shotSound = "WaveBeamV2Sound";
							chargeShotSound = "WaveBeamChargeSound";
							chargeUpSound = "ChargeStartup_Wave";
							chargeTex = "ChargeLead_Nebula";
							dustType = 255;
							lightColor = MetroidMod.waveColor;
							shotAmt = 2;
							chargeShotAmt = 2;
							texture = "NebulaBeam_Item";
							
							// Nebula Vortex
							if(slot4.type == vt)
							{
								shot = "NebulaVortexBeamShot";
								chargeShot = "NebulaVortexBeamChargeShot";
								shotSound = "WideBeamSound";
								shotAmt = 5;
								chargeShotAmt = 5;
								
								// Nebula Vortex Solar
								if(slot5.type == sl)
								{
									shot = "NebulaVortexSolarBeamShot";
									chargeShot = "NebulaVortexSolarBeamChargeShot";
									shotSound = "PlasmaBeamRedV2Sound";
									chargeShotSound = "PlasmaBeamRedChargeSound";
									chargeUpSound = "ChargeStartup_PlasmaRed";
									chargeTex = "ChargeLead_Solar";
									dustType = 6;
									lightColor = MetroidMod.plaRedColor;
									texture = "SolarBeam_Item";
								}
							}
							else
							{
								// Nebula Solar
								if(slot5.type == sl)
								{
									shot = "NebulaSolarBeamShot";
									chargeShot = "NebulaSolarBeamChargeShot";
									shotSound = "PlasmaBeamRedV2Sound";
									chargeShotSound = "PlasmaBeamRedChargeSound";
									chargeUpSound = "ChargeStartup_PlasmaRed";
									chargeTex = "ChargeLead_Solar";
									dustType = 6;
									lightColor = MetroidMod.plaRedColor;
									texture = "SolarBeam_Item";
								}
							}
						}
						else
						{
							// Vortex
							if(slot4.type == vt)
							{
								shot = "VortexBeamShot";
								chargeShot = "VortexBeamChargeShot";
								shotSound = "WideBeamSound";
								chargeShotSound = "SpazerChargeSound";
								chargeTex = "ChargeLead_Vortex";
								shotAmt = 5;
								chargeShotAmt = 5;
								texture = "VortexBeam_Item";
								
								// Vortex Solar
								if(slot5.type == sl)
								{
									shot = "VortexSolarBeamShot";
									chargeShot = "VortexSolarBeamChargeShot";
									shotSound = "PlasmaBeamRedV2Sound";
									chargeShotSound = "PlasmaBeamRedChargeSound";
									chargeUpSound = "ChargeStartup_PlasmaRed";
									chargeTex = "ChargeLead_Solar";
									dustType = 6;
									lightColor = MetroidMod.plaRedColor;
									texture = "SolarBeam_Item";
								}
							}
							else
							{
								// Solar
								if(slot5.type == sl)
								{
									shot = "SolarBeamShot";
									chargeShot = "SolarBeamChargeShot";
									shotSound = "PlasmaBeamRedV2Sound";
									chargeShotSound = "PlasmaBeamRedChargeSound";
									chargeUpSound = "ChargeStartup_PlasmaRed";
									chargeTex = "ChargeLead_Solar";
									dustType = 6;
									lightColor = MetroidMod.plaRedColor;
									texture = "SolarBeam_Item";
								}
							}
						}
					}
					
					if(slot2.type == ic || slot2.type == ic2)
					{
						comboError1 = true;
					}
					if(slot3.type == wa || slot3.type == wa2)
					{
						comboError2 = true;
					}
					if(slot4.type == sp || slot4.type == wi)
					{
						comboError3 = true;
					}
					if(slot5.type == plR || slot5.type == plG || slot5.type == nv)
					{
						comboError4 = true;
					}
				}
			}
			// Hyper
			else if(isHyper)
			{
				shot = "HyperBeamShot";
				shotSound = "HyperBeamSound";
				useTime = 16;
				
				damage = 35;
				overheat = 7;
				
				texture = "HyperBeam_Item";
				
				// Wave / Nebula
				if(slot3.type == wa || slot3.type == wa2 || slot3.type == nb)
				{
					string wave = "Wave";
					if(slot3.type == nb)
					{
						wave = "Nebula";
					}
					shot = wave+"HyperBeamShot";
					
					// Wave Spazer
					if(slot4.type == sp || slot4.type == wi || slot4.type == vt)
					{
						shot = wave+"SpazerHyperBeamShot";
						shotAmt = 3;
						if(slot4.type == vt)
						{
							shotAmt = 5;
						}
						
						// Wave Spazer Plasma
						if(slot5.type == plG || slot5.type == nv || slot5.type == sl)
						{
							shot = wave+"SpazerPlasmaHyperBeamShot";
						}
					}
					// Wave Plasma
					else if(slot5.type == plG || slot5.type == nv || slot5.type == sl)
					{
						shot = wave+"PlasmaHyperBeamShot";
					}
				}
				// Spazer
				else if(slot4.type == sp || slot4.type == wi || slot4.type == vt)
				{
					shot = "SpazerHyperBeamShot";
					shotAmt = 3;
					if(slot4.type == vt)
					{
						shotAmt = 5;
					}
					
					// Spazer Plasma
					if(slot5.type == plG || slot5.type == nv || slot5.type == sl)
					{
						shot = "SpazerPlasmaHyperBeamShot";
					}
				}
				// Plasma
				else if(slot5.type == plG || slot5.type == nv || slot5.type == sl)
				{
					shot = "PlasmaHyperBeamShot";
				}
			}
			// Phazon
			else if(isPhazon)
			{
				shot = "PhazonBeamShot";
				shotSound = "PhazonBeamSound";
				useTime = 6;
				
				damage = 6;
				overheat = 1;
				
				texture = "PhazonBeam_Item";
				
				// Wave / Nebula
				if(slot3.type == wa || slot3.type == wa2 || slot3.type == nb)
				{
					string wave = "Wave";
					if(slot3.type == nb)
					{
						wave = "Nebula";
					}
					shot = wave+"PhazonBeamShot";
					
					// Wave Spazer
					if(slot4.type == sp || slot4.type == wi || slot4.type == vt)
					{
						shot = wave+"SpazerPhazonBeamShot";
						shotAmt = 3;
						if(slot4.type == vt)
						{
							shotAmt = 5;
						}
						
						// Wave Spazer Plasma
						if(slot5.type == plG || slot5.type == nv || slot5.type == sl)
						{
							shot = wave+"SpazerPlasmaPhazonBeamShot";
						}
					}
					// Wave Plasma
					else if(slot5.type == plG || slot5.type == nv || slot5.type == sl)
					{
						shot = wave+"PlasmaPhazonBeamShot";
					}
				}
				// Spazer
				else if(slot4.type == sp || slot4.type == wi || slot4.type == vt)
				{
					shot = "SpazerPhazonBeamShot";
					shotAmt = 3;
					if(slot4.type == vt)
					{
						shotAmt = 5;
					}
					
					// Spazer Plasma
					if(slot5.type == plG || slot5.type == nv || slot5.type == sl)
					{
						shot = "SpazerPlasmaPhazonBeamShot";
					}
				}
				// Plasma
				else if(slot5.type == plG || slot5.type == nv || slot5.type == sl)
				{
					shot = "PlasmaPhazonBeamShot";
				}
			}
			
			iceDmg = 0f;
			waveDmg = 0f;
			spazDmg = 0f;
			plasDmg = 0f;

			iceHeat = 0f;
			waveHeat = 0f;
			spazHeat = 0f;
			plasHeat = 0f;
			
			iceSpeed = 0f;
			waveSpeed = 0f;
			spazSpeed = 0f;
			plasSpeed = 0f;
			
			if(!slot1.IsAir)
			{
				MGlobalItem mItem = slot1.GetGlobalItem<MGlobalItem>();
				chargeDmgMult = mItem.addonChargeDmg;
				chargeCost = mItem.addonChargeHeat;
			}

			if(!slot2.IsAir)
			{
				MGlobalItem mItem = slot2.GetGlobalItem<MGlobalItem>();
				iceDmg = mItem.addonDmg;
				iceHeat = mItem.addonHeat;
				iceSpeed = mItem.addonSpeed;
			}
			if(!slot3.IsAir)
			{
				MGlobalItem mItem = slot3.GetGlobalItem<MGlobalItem>();
				waveDmg = mItem.addonDmg;
				waveHeat = mItem.addonHeat;
				waveSpeed = mItem.addonSpeed;
			}
			if(!slot4.IsAir)
			{
				MGlobalItem mItem = slot4.GetGlobalItem<MGlobalItem>();
				spazDmg = mItem.addonDmg;
				spazHeat = mItem.addonHeat;
				spazSpeed = mItem.addonSpeed;
			}
			if(!slot5.IsAir)
			{
				MGlobalItem mItem = slot5.GetGlobalItem<MGlobalItem>();
				plasDmg = mItem.addonDmg;
				plasHeat = mItem.addonHeat;
				plasSpeed = mItem.addonSpeed;
			}
			
			finalDmg = (int)Math.Round((double)((float)damage * (1f + iceDmg + waveDmg + spazDmg + plasDmg)));
			overheat = (int)Math.Max(Math.Round((double)((float)overheat * (1 + iceHeat + waveHeat + spazHeat + plasHeat))), 1);
			
			float shotsPerSecond = (60 / useTime) * (1f + iceSpeed + waveSpeed + spazSpeed + plasSpeed);
			
			useTime = (int)Math.Max(Math.Round(60.0 / (double)shotsPerSecond), 2);
			
			item.damage = finalDmg;
			item.useTime = useTime;
			item.useAnimation = useTime;
			item.shoot = mod.ProjectileType(shot);
			if(ShotSound == null)
				ShotSound = mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/"+shotSound);
			if(ChargeShotSound == null)
				ChargeShotSound = mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/"+chargeShotSound);
			//item.UseSound = ShotSound;
			
			//item.autoReuse = (!slot1.IsAir);//(isCharge);

			item.shootSpeed = 8f;
			item.reuseDelay = 0;
			item.mana = 0;
			item.knockBack = 4f;
			item.scale = 0.8f;
			item.crit = 3;
			item.value = 20000;
			
			item.rare = 2;
			
			item.Prefix(item.prefix);
			
			if(isPhazon)
			{
				item.useAnimation = 9;
				item.useTime = 3;
				item.UseSound = mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/PhazonBeamSound");
			}
			else
			{
				item.UseSound = null;
			}
			
			
			/*MGlobalItem mi = item.GetGlobalItem<MGlobalItem>();
			if(texture != "")
			{
				string alt = "";
				if(MetroidMod.UseAltWeaponTextures)
				{
					alt = "_alt";
				}
				mi.itemTexture = mod.GetTexture("Items/weapons/textures"+alt+"/"+texture);
			}
			else
			{
				if(MetroidMod.UseAltWeaponTextures)
				{
					mi.itemTexture = ModContent.GetTexture(altTexture);
				}
				else
				{
					mi.itemTexture = Main.itemTexture[item.type];
				}
			}
			
			if(mi.itemTexture != null)
			{
				item.width = mi.itemTexture.Width;
				item.height = mi.itemTexture.Height;
			}*/
		}
		public override bool PreDrawInWorld(SpriteBatch sb, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
		{
			MGlobalItem mi = item.GetGlobalItem<MGlobalItem>();
			Texture2D tex = Main.itemTexture[item.type];
			this.setTexture(mi);
			if(mi.itemTexture != null)
			{
				tex = mi.itemTexture;
			}
			float num5 = (float)(item.height - tex.Height);
			float num6 = (float)(item.width / 2 - tex.Width / 2);
			sb.Draw(tex, new Vector2(item.position.X - Main.screenPosition.X + (float)(tex.Width / 2) + num6, item.position.Y - Main.screenPosition.Y + (float)(tex.Height / 2) + num5 + 2f),
			new Rectangle?(new Rectangle(0, 0, tex.Width, tex.Height)), alphaColor, rotation, new Vector2((float)(tex.Width / 2), (float)(tex.Height / 2)), scale, SpriteEffects.None, 0f);
			return false;
		}
		public override bool PreDrawInInventory(SpriteBatch sb, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
			MGlobalItem mi = item.GetGlobalItem<MGlobalItem>();
			Texture2D tex = Main.itemTexture[item.type];
			this.setTexture(mi);
			if(mi.itemTexture != null)
			{
				tex = mi.itemTexture;
			}
			sb.Draw(tex, position, new Rectangle?(new Rectangle(0, 0, tex.Width, tex.Height)), drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
			return false;
		}
		void setTexture(MGlobalItem mi)
		{
			if(texture != "")
			{
				string alt = "";
				if(MetroidMod.UseAltWeaponTextures)
				{
					alt = "_alt";
				}
				mi.itemTexture = mod.GetTexture("Items/weapons/textures"+alt+"/"+texture);
			}
			else
			{
				if(MetroidMod.UseAltWeaponTextures)
				{
					mi.itemTexture = ModContent.GetTexture(altTexture);
				}
				else
				{
					mi.itemTexture = Main.itemTexture[item.type];
				}
			}
			if(mi.itemTexture != null)
			{
				item.width = mi.itemTexture.Width;
				item.height = mi.itemTexture.Height;
			}
		}
		
		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			base.ModifyTooltips(tooltips);
			
			Player player = Main.player[Main.myPlayer];
			MPlayer mp = player.GetModPlayer<MPlayer>();

			if(item == Main.HoverItem)
				item.modItem.UpdateInventory(player);

			int dmg = player.GetWeaponDamage(item);
			int chDmg = (int)((float)dmg*chargeDmgMult);
			TooltipLine chDmgLine = new TooltipLine(mod, "ChargeDamage", chDmg+" Charge Shot damage");

			int oh = (int)((float)overheat*mp.overheatCost);
			TooltipLine ohLine = new TooltipLine(mod, "Overheat", "Overheats by "+oh+" points per use");
			int chOh = (int)((float)oh*chargeCost);
			TooltipLine chOhLine = new TooltipLine(mod, "ChargeOverheat", "Overheats by "+chOh+" points on Charge Shot");

			for (int k = 0; k < tooltips.Count; k++)
			{
				if(tooltips[k].Name == "Damage" && isCharge)
				{
					tooltips.Insert(k + 1, chDmgLine);
				}
				if(tooltips[k].Name == "Knockback")
				{
					tooltips.Insert(k + 1, ohLine);
					if(isCharge)
					{
						tooltips.Insert(k + 2, chOhLine);
					}
				}
				if(tooltips[k].Name == "PrefixDamage")
				{
					double num19 = (double)((float)item.damage - (float)finalDmg);
					num19 = num19 / (double)((float)finalDmg) * 100.0;
					num19 = Math.Round(num19);
					if (num19 > 0.0)
					{
						tooltips[k].text = "+" + num19 + Lang.tip[39].Value;
					}
					else
					{
						tooltips[k].text = num19 + Lang.tip[39].Value;
					}
				}
				if(tooltips[k].Name == "PrefixSpeed")
				{
					double num20 = (double)((float)item.useAnimation - (float)useTime);
					num20 = num20 / (double)((float)useTime) * 100.0;
					num20 = Math.Round(num20);
					num20 *= -1.0;
					if (num20 > 0.0)
					{
						tooltips[k].text = "+" + num20 + Lang.tip[40].Value;
					}
					else
					{
						tooltips[k].text = num20 + Lang.tip[40].Value;
					}
				}
			}
		}
		
		/*public override void GetWeaponDamage(Player P, ref int dmg)
		{
			dmg = (int)((float)dmg*baseDmg * (1f + iceDmg + waveDmg + spazDmg + plasDmg));
		}*/
		
		public override ModItem Clone(Item item)
		{
			ModItem clone = this.NewInstance(item);
			PowerBeam beamClone = (PowerBeam)clone;
			beamClone.beamMods = new Item[MetroidMod.beamSlotAmount];
			for (int i = 0; i < MetroidMod.beamSlotAmount; ++i)
			{
				beamClone.beamMods[i] = this.beamMods[i];
			}

			return clone;
		}
		
		int chargeLead = -1;
		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
		{
			MPlayer mp = player.GetModPlayer<MPlayer>();

			if(isCharge)
			{
				int ch = Projectile.NewProjectile(position.X,position.Y,speedX,speedY,mod.ProjectileType("ChargeLead"),damage,knockBack,player.whoAmI);
				ChargeLead cl = (ChargeLead)Main.projectile[ch].modProjectile;
				cl.ChargeUpSound = chargeUpSound;
				cl.ChargeTex = chargeTex;
				cl.ChargeShotAmt = chargeShotAmt;
				cl.DustType = dustType;
				cl.DustColor = dustColor;
				cl.LightColor = lightColor;
				cl.canPsuedoScrew = mp.psuedoScrewActive;
				cl.ShotSound = shotSound;
				cl.ChargeShotSound = chargeShotSound;
				cl.projectile.netUpdate = true;

				chargeLead = ch;
			}
			
			if(isHyper)
			{
				int hyperProj = Projectile.NewProjectile(position.X,position.Y,speedX,speedY,item.shoot,damage,knockBack,player.whoAmI);
				
				if(shotAmt > 1)
				{
					for(int i = 0; i < shotAmt; i++)
					{
						if(i != 2)
						{
							int extraProj = Projectile.NewProjectile(position.X,position.Y,speedX,speedY,mod.ProjectileType("Extra"+shot),damage,knockBack,player.whoAmI, 0, i);
							MProjectile mProj = (MProjectile)Main.projectile[extraProj].modProjectile;
							mProj.waveDir = waveDir;
							Main.projectile[extraProj].netUpdate = true;
						}
					}
				}
				
				mp.hyperColors = 23;
			}
			else
			{
				for(int i = 0; i < shotAmt; i++)
				{
					int shotProj = Projectile.NewProjectile(position.X,position.Y,speedX,speedY,item.shoot,damage,knockBack,player.whoAmI, 0, i);
					MProjectile mProj = (MProjectile)Main.projectile[shotProj].modProjectile;
					mProj.waveDir = waveDir;
					Main.projectile[shotProj].netUpdate = true;
				}
			}
			waveDir *= -1;
			
			mp.statOverheat += (int)((float)overheat*mp.overheatCost);
			mp.overheatDelay = Math.Max(useTime-10,2);

			/* Sound & Sound Networking */
			if (Main.netMode != 0 && mp.player.whoAmI == Main.myPlayer)
			{
				// Send a packet to have the sound play on all clients.
				ModPacket packet = mod.GetPacket();
				packet.Write((byte)MetroidMessageType.PlaySyncedSound);
				packet.Write((byte)player.whoAmI);
				packet.Write(shotSound);
				packet.Send();
			}

			// Play the shot sound for the local player.
			if(!isPhazon)
			{
				Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/" + shotSound), player.position);
			}

			return false;
		}
		
		public override void HoldItem(Player player)
		{
			if(isCharge && player.whoAmI == Main.myPlayer)
			{
				MPlayer mp = player.GetModPlayer<MPlayer>();

				if(!mp.ballstate && !mp.shineActive && !player.dead && !player.noItems)
				{
					if(player.controlUseItem && chargeLead != -1 && Main.projectile[chargeLead].active && Main.projectile[chargeLead].owner == player.whoAmI && Main.projectile[chargeLead].type == mod.ProjectileType("ChargeLead"))
					{
						if(mp.statCharge < MPlayer.maxCharge && mp.statOverheat < mp.maxOverheat)
							mp.statCharge = Math.Min(mp.statCharge + 1, MPlayer.maxCharge);
					}
					else
					{
						Vector2 oPos = player.RotatedRelativePoint(player.MountedCenter, true);

						float MY = Main.mouseY + Main.screenPosition.Y;
						float MX = Main.mouseX + Main.screenPosition.X;
						if (player.gravDir == -1f)
							MY = Main.screenPosition.Y + (float)Main.screenHeight - (float)Main.mouseY;

						float targetrotation = (float)Math.Atan2((MY-oPos.Y),(MX-oPos.X));
						
						Vector2 velocity = targetrotation.ToRotationVector2()*item.shootSpeed;

						float dmgMult = 1f + ((chargeDmgMult - 1f)/MPlayer.maxCharge) * mp.statCharge;
						int damage = player.GetWeaponDamage(item);
						
						int oHeat = (int)((float)overheat*mp.overheatCost);
						
						double sideangle = Math.Atan2(velocity.Y, velocity.X) + (Math.PI/2);

						if(mp.statCharge >= (MPlayer.maxCharge*0.5))
						{
							for(int i = 0; i < chargeShotAmt; i++)
							{
								int chargeProj = Projectile.NewProjectile(oPos.X, oPos.Y, velocity.X, velocity.Y, mod.ProjectileType(chargeShot), (int)((float)damage * dmgMult), item.knockBack, player.whoAmI, 0, i);
								MProjectile mProj = (MProjectile)Main.projectile[chargeProj].modProjectile;
								mProj.waveDir = waveDir;
								mProj.canDiffuse = (mp.statCharge >= (MPlayer.maxCharge*0.9));
								mProj.projectile.netUpdate2 = true;
							}
							
							//Main.PlaySound(SoundLoader.customSoundType, (int)oPos.X, (int)oPos.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/"+chargeShotSound));
							
							mp.statOverheat += (int)((float)oHeat*chargeCost);
							mp.overheatDelay = useTime-10;
						}
						else if(mp.statCharge > 0)
						{
							if(mp.statCharge >= 30)
							{
								for(int i = 0; i < shotAmt; i++)
								{
									int shotProj = Projectile.NewProjectile(oPos.X,oPos.Y,velocity.X,velocity.Y,mod.ProjectileType(shot),damage,item.knockBack,player.whoAmI, 0, i);
									MProjectile mProj = (MProjectile)Main.projectile[shotProj].modProjectile;
									mProj.waveDir = waveDir;
									mProj.projectile.netUpdate = true;
								}

								//Main.PlaySound(SoundLoader.customSoundType, (int)oPos.X, (int)oPos.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/"+shotSound));
								
								mp.statOverheat += oHeat;
								mp.overheatDelay = useTime-10;
							}
						}
						if(chargeLead == -1 || !Main.projectile[chargeLead].active || Main.projectile[chargeLead].owner != player.whoAmI || Main.projectile[chargeLead].type != mod.ProjectileType("ChargeLead"))
							mp.statCharge = 0;
					}
				}
				else if(!mp.ballstate)
					mp.statCharge = 0;
			}
		}

		public override TagCompound Save()
		{
			TagCompound tag = new TagCompound();
			for(int i = 0; i < beamMods.Length; ++i)
			{
				// Failsave check.
				if (beamMods[i] == null) beamMods[i] = new Item();
				tag.Add("beamItem" + i, ItemIO.Save(beamMods[i]));
			}
			return tag;
		}
		public override void Load(TagCompound tag)
		{
			try
			{
				beamMods = new Item[MetroidMod.beamSlotAmount];
				for(int i = 0; i < beamMods.Length ; i++)
				{
					Item item = tag.Get<Item>("beamItem"+i);
					beamMods[i] = item;
				}
			}
			catch{}
		}

		public override void NetSend(BinaryWriter writer)
		{
			for(int i = 0; i < beamMods.Length; ++i)
				writer.WriteItem(beamMods[i]);
			writer.Write(chargeLead);
		}
		public override void NetRecieve(BinaryReader reader)
		{
			for(int i = 0; i < beamMods.Length; ++i)
				beamMods[i] = reader.ReadItem();
			chargeLead = reader.ReadInt32();
		}
	}
}
