using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MetroidMod.Items.addons
{
    public class HyperBeamAddon : ModItem
    {
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Hyper Beam");
			Tooltip.SetDefault("Power Beam Addon\n" +
			"Slot Type: Charge\n" +
			"Increases base damage from 14 to 35, and base overheat use from 4 to 7\n" +
			"Slightly decreases firerate\n" +
			"Affected by addons regardless of version\n" + 
			"Disables freeze and other debuff effects\n" +
			"'Da babeh'");
		}
		public override void SetDefaults()
		{
			item.width = 10;
			item.height = 14;
			item.maxStack = 1;
			item.value = 2500;
			item.rare = 4;
			item.useTurn = true;
			item.autoReuse = true;
			item.useAnimation = 15;
			item.useTime = 10;
			item.useStyle = 1;
			item.consumable = true;
			item.createTile = mod.TileType("HyperBeamTile");
			MGlobalItem mItem = item.GetGlobalItem<MGlobalItem>();
			mItem.addonSlotType = 0;
		}
		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.Meowmere);
			recipe.AddIngredient(ItemID.LunarBar, 5);
			recipe.AddIngredient(ItemID.FragmentStardust, 10);
			recipe.AddIngredient(ItemID.FragmentSolar, 10);
			recipe.AddIngredient(ItemID.FragmentNebula, 10);
			recipe.AddIngredient(ItemID.FragmentVortex, 10);
			recipe.AddTile(TileID.LunarCraftingStation);
			recipe.SetResult(this);
			recipe.AddRecipe();
			
			recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.LastPrism);
			recipe.AddIngredient(ItemID.LunarBar, 5);
			recipe.AddIngredient(ItemID.FragmentStardust, 10);
			recipe.AddIngredient(ItemID.FragmentSolar, 10);
			recipe.AddIngredient(ItemID.FragmentNebula, 10);
			recipe.AddIngredient(ItemID.FragmentVortex, 10);
			recipe.AddTile(TileID.LunarCraftingStation);
			recipe.SetResult(this);
			recipe.AddRecipe();
			
			recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.RainbowCrystalStaff);
			recipe.AddIngredient(ItemID.LunarBar, 5);
			recipe.AddIngredient(ItemID.FragmentStardust, 10);
			recipe.AddIngredient(ItemID.FragmentSolar, 10);
			recipe.AddIngredient(ItemID.FragmentNebula, 10);
			recipe.AddIngredient(ItemID.FragmentVortex, 10);
			recipe.AddTile(TileID.LunarCraftingStation);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
		public override void PostDrawInInventory(SpriteBatch sb, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale )
		{
			Texture2D tex = mod.GetTexture("Items/addons/HyperBeamAddonColors");
			drawColor = new Color(Main.DiscoR, Main.DiscoG, Main.DiscoB);
			sb.Draw(tex, position, frame, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
		}
		public override void PostDrawInWorld(SpriteBatch sb, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI )
		{
			DrawColors(sb, Main.player[item.owner]);
			lightColor = new Color(Main.DiscoR, Main.DiscoG, Main.DiscoB);
			alphaColor = lightColor;
		}
		public void DrawColors(SpriteBatch sb, Player player)
		{
			Texture2D tex = mod.GetTexture("Items/addons/HyperBeamAddonColors");
			float rotation = item.velocity.X * 0.2f;
			float num3 = 1f;
			float num4 = (float)(item.height - tex.Height);
			float num5 = (float)(item.width / 2 - tex.Width / 2);
			sb.Draw(tex, new Vector2(item.position.X - Main.screenPosition.X + (float)(tex.Width / 2) + num5, item.position.Y - Main.screenPosition.Y + (float)(tex.Height / 2) + num4 + 2f), new Rectangle?(new Rectangle(0, 0, tex.Width, tex.Height)), new Color(Main.DiscoR, Main.DiscoG, Main.DiscoB), rotation, new Vector2((float)(tex.Width / 2), (float)(tex.Height / 2)), num3, SpriteEffects.None, 0f);
		}
	}
}
