using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Terraria;
using Terraria.UI;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.UI.Elements;

using MetroidMod.Items;
using MetroidMod.Items.weapons;

namespace MetroidMod.NewUI
{
	/*
	 * The whole UI still feels a tad hacky, so it might need to get a bit of revamping here and there.
	 */
	public class PowerBeamUI : UIState
	{
		public static bool visible
		{
			get { return Main.playerInventory && Main.LocalPlayer.inventory[((MetroidMod)(MetroidMod.Instance)).selectedItem].type == ModLoader.GetMod("MetroidMod").ItemType("PowerBeam"); }
		}

		PowerBeamPanel powerBeamPanel;
		PowerBeamScrewAttackButton pbsaButton;
		ComboError comboError;

		public override void OnInitialize()
		{
			powerBeamPanel = new PowerBeamPanel();
			powerBeamPanel.Initialize();
			this.Append(powerBeamPanel);
			
			pbsaButton = new PowerBeamScrewAttackButton();
			pbsaButton.Initialize();
			this.Append(pbsaButton);
			
			comboError = new ComboError();
			comboError.Initialize();
			this.Append(comboError);
		}
	}

	public class PowerBeamPanel : UIPanel
	{
		Texture2D panelTexture;

		public PowerBeamItemBox[] beamSlots;

		public Rectangle drawRectangle
		{
			get { return new Rectangle((int)this.Left.Pixels, (int)this.Top.Pixels, (int)this.Width.Pixels, (int)this.Height.Pixels); }
		}

		public Vector2[] itemBoxPositionValues = new Vector2[MetroidMod.beamSlotAmount]
		{
			new Vector2(98, 14),
			new Vector2(174, 14),
			new Vector2(32, 14),
			new Vector2(32, 94),
			new Vector2(174, 94)
		};

		public override void OnInitialize()
		{
			panelTexture = ModContent.GetTexture("MetroidMod/Textures/UI/PowerBeam_Border");

			this.SetPadding(0);
			this.Left.Pixels = 160;
			this.Top.Pixels = 260;
			this.Width.Pixels = panelTexture.Width;
			this.Height.Pixels = panelTexture.Height;

			beamSlots = new PowerBeamItemBox[MetroidMod.beamSlotAmount];
			for (int i = 0; i < MetroidMod.beamSlotAmount; ++i)
			{
				beamSlots[i] = new PowerBeamItemBox();
				beamSlots[i].Top.Pixels = itemBoxPositionValues[i].Y;
				beamSlots[i].Left.Pixels = itemBoxPositionValues[i].X;
				beamSlots[i].addonSlotType = i;
				beamSlots[i].SetCondition();

				this.Append(beamSlots[i]);
			}
			
			this.Append(new PowerBeamFrame());
			this.Append(new PowerBeamLines());
		}

		public override void Update(GameTime gameTime)
		{
			this.Top.Pixels = 260;
			if (Main.LocalPlayer.chest != -1 || Main.npcShop != 0)
			{
				this.Top.Pixels += 170;
			}

			base.Update(gameTime);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			spriteBatch.Draw(panelTexture, drawRectangle, Color.White);            
		}
	}

	public class PowerBeamItemBox : UIPanel
	{
		Texture2D itemBoxTexture;
		
		public Condition condition;

		public int addonSlotType;

		public Rectangle drawRectangle
		{
			get { return new Rectangle((int)(Parent.Left.Pixels + Left.Pixels), (int)(Parent.Top.Pixels + Top.Pixels), (int)Width.Pixels, (int)Height.Pixels); }
		}

		public delegate bool Condition(Item item);
		public override void OnInitialize()
		{
			itemBoxTexture = ModContent.GetTexture("MetroidMod/Textures/UI/ItemBox");

			Width.Pixels = itemBoxTexture.Width; Height.Pixels = itemBoxTexture.Height;
			this.OnClick += ItemBoxClick;
		}

		public override void Update(GameTime gameTime)
		{
			// Ignore mouse input.
			if (base.IsMouseHovering)
				Main.LocalPlayer.mouseInterface = true;
		}

		public void SetCondition()
		{
			this.condition = delegate (Item addonItem)
			{
				Mod mod = ModLoader.GetMod("MetroidMod");
				if (addonItem.modItem != null && addonItem.modItem.mod == mod)
				{
					MGlobalItem mItem = addonItem.GetGlobalItem<MGlobalItem>();
					return (addonItem.type <= 0 || mItem.addonSlotType == this.addonSlotType);
				}
				return (addonItem.type <= 0 || (addonItem.modItem != null && addonItem.modItem.mod == mod));
			};
		}

		// Clicking functionality.
		private void ItemBoxClick(UIMouseEvent evt, UIElement e)
		{
			// No failsafe. Should maybe be implemented?
			PowerBeam powerBeamTarget = Main.LocalPlayer.inventory[((MetroidMod)MetroidMod.Instance).selectedItem].modItem as PowerBeam;

			if (powerBeamTarget.beamMods[addonSlotType] != null && !powerBeamTarget.beamMods[addonSlotType].IsAir)
			{
				if (Main.mouseItem.IsAir)
				{
					Main.PlaySound(SoundID.Grab);
					Main.mouseItem = powerBeamTarget.beamMods[addonSlotType].Clone();

					powerBeamTarget.beamMods[addonSlotType].TurnToAir();
				}
				else if(condition == null || (condition != null && condition(Main.mouseItem)))
				{
					Main.PlaySound(SoundID.Grab);

					Item tempBoxItem = powerBeamTarget.beamMods[addonSlotType].Clone();
					Item tempMouseItem = Main.mouseItem.Clone();

					powerBeamTarget.beamMods[addonSlotType] = tempMouseItem;
					Main.mouseItem = tempBoxItem;
				}
			}
			else if(!Main.mouseItem.IsAir)
			{
				if (condition == null || (condition != null && condition(Main.mouseItem)))
				{
					Main.PlaySound(SoundID.Grab);
					powerBeamTarget.beamMods[addonSlotType] = Main.mouseItem.Clone();
					Main.mouseItem.TurnToAir();
				}
			}
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			PowerBeam powerBeamTarget = Main.LocalPlayer.inventory[((MetroidMod)MetroidMod.Instance).selectedItem].modItem as PowerBeam;

			spriteBatch.Draw(itemBoxTexture, drawRectangle, Color.White);

			// Item drawing.
			if (powerBeamTarget.beamMods[addonSlotType].IsAir) return;

			Color itemColor = powerBeamTarget.beamMods[addonSlotType].GetAlpha(Color.White);
			Texture2D itemTexture = Main.itemTexture[powerBeamTarget.beamMods[addonSlotType].type];
			CalculatedStyle innerDimensions = base.GetDimensions();

			if (base.IsMouseHovering)
			{
				Main.hoverItemName = powerBeamTarget.beamMods[addonSlotType].Name;
				Main.HoverItem = powerBeamTarget.beamMods[addonSlotType].Clone();
			}

			var frame = Main.itemAnimations[powerBeamTarget.beamMods[addonSlotType].type] != null
						? Main.itemAnimations[powerBeamTarget.beamMods[addonSlotType].type].GetFrame(itemTexture)
						: itemTexture.Frame(1, 1, 0, 0);

			float drawScale = 1f;
			if ((float)frame.Width > innerDimensions.Width || (float)frame.Height > innerDimensions.Width)
			{
				if (frame.Width > frame.Height)
					drawScale = innerDimensions.Width / (float)frame.Width;
				else
					drawScale = innerDimensions.Width / (float)frame.Height;
			}

			var unreflectedScale = drawScale;
			var tmpcolor = Color.White;

			ItemSlot.GetItemLight(ref tmpcolor, ref drawScale, powerBeamTarget.beamMods[addonSlotType].type);

			Vector2 drawPosition = new Vector2(innerDimensions.X, innerDimensions.Y);

			drawPosition.X += (float)innerDimensions.Width * 1f / 2f - (float)frame.Width * drawScale / 2f;
			drawPosition.Y += (float)innerDimensions.Height * 1f / 2f - (float)frame.Height * drawScale / 2f;
			
			spriteBatch.Draw(itemTexture, drawPosition, new Rectangle?(frame), itemColor, 0f,
				Vector2.Zero, drawScale, SpriteEffects.None, 0f);

			if (powerBeamTarget.beamMods[addonSlotType].color != default(Color))
			{
				spriteBatch.Draw(itemTexture, drawPosition, new Rectangle?(frame), itemColor, 0f,
					Vector2.Zero, drawScale, SpriteEffects.None, 0f);
			}
		}
	}

	/*
	 * The classes in the following section do not have any functionality besides visual aesthetics.
	 */
	public class PowerBeamFrame : UIPanel
	{
		Texture2D powerBeamFrame;

		public Rectangle drawRectangle
		{
			get { return new Rectangle((int)(Parent.Left.Pixels + Left.Pixels), (int)(Parent.Top.Pixels + Top.Pixels), (int)Width.Pixels, (int)Height.Pixels); }
		}

		public override void OnInitialize()
		{
			powerBeamFrame = ModContent.GetTexture("MetroidMod/Textures/UI/PowerBeam_Frame");

			this.Width.Pixels = powerBeamFrame.Width;
			this.Height.Pixels = powerBeamFrame.Height;

			// Hardcoded position values.
			this.Top.Pixels = 80;
			this.Left.Pixels = 104;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			spriteBatch.Draw(powerBeamFrame, drawRectangle, Color.White);
		}
	}
	public class PowerBeamLines : UIPanel
	{
		Texture2D powerBeamLines;

		public Rectangle drawRectangle
		{
			get { return new Rectangle((int)(Parent.Left.Pixels + Left.Pixels), (int)(Parent.Top.Pixels + Top.Pixels), (int)Width.Pixels, (int)Height.Pixels); }
		}

		public override void OnInitialize()
		{
			powerBeamLines = ModContent.GetTexture("MetroidMod/Textures/UI/PowerBeam_Lines");

			this.Width.Pixels = powerBeamLines.Width;
			this.Height.Pixels = powerBeamLines.Height;

			// Hardcoded position values.
			this.Top.Pixels = 0;
			this.Left.Pixels = 0;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			spriteBatch.Draw(powerBeamLines, drawRectangle, Color.White);
		}
	}
	
	// Charge Somersault attack toggle button
	public class PowerBeamScrewAttackButton : UIPanel
	{
		Texture2D buttonTex, buttonTex_Hover, buttonTex_Click,
		buttonTexEnabled, buttonTexEnabled_Hover, buttonTexEnabled_Click;
		
		public Rectangle drawRectangle
		{
			get { return new Rectangle((int)(Parent.Left.Pixels + Left.Pixels), (int)(Parent.Top.Pixels + Top.Pixels), (int)Width.Pixels, (int)Height.Pixels); }
		}
		
		public override void OnInitialize()
		{
			this.Left.Pixels = 112;
			this.Top.Pixels = 274;
			
			buttonTex = ModContent.GetTexture("MetroidMod/Textures/Buttons/PsuedoScrewUIButton");
			buttonTex_Hover = ModContent.GetTexture("MetroidMod/Textures/Buttons/PsuedoScrewUIButton_Hover");
			buttonTex_Click = ModContent.GetTexture("MetroidMod/Textures/Buttons/PsuedoScrewUIButton_Click");
			
			buttonTexEnabled = ModContent.GetTexture("MetroidMod/Textures/Buttons/PsuedoScrewUIButton_Enabled");
			buttonTexEnabled_Hover = ModContent.GetTexture("MetroidMod/Textures/Buttons/PsuedoScrewUIButton_Enabled_Hover");
			buttonTexEnabled_Click = ModContent.GetTexture("MetroidMod/Textures/Buttons/PsuedoScrewUIButton_Enabled_Click");
			
			Width.Pixels = buttonTex.Width;
			Height.Pixels = buttonTex.Height;
			this.OnClick += SAButtonClick;
		}
		
		public override void Update(GameTime gameTime)
		{
			if(base.IsMouseHovering)
			{
				Main.LocalPlayer.mouseInterface = true;
			}
			
			this.Top.Pixels = 274;
			if (Main.LocalPlayer.chest != -1 || Main.npcShop != 0)
			{
				this.Top.Pixels += 170;
			}

			base.Update(gameTime);
		}
		
		bool clicked = false;
		private void SAButtonClick(UIMouseEvent evt, UIElement e)
		{
			MPlayer mp = Main.LocalPlayer.GetModPlayer<MPlayer>();
			
			mp.psuedoScrewActive = !mp.psuedoScrewActive;
			Main.PlaySound(12);
			clicked = true;
		}
		
		protected override void DrawSelf(SpriteBatch sb)
		{
			MPlayer mp = Main.LocalPlayer.GetModPlayer<MPlayer>();
			
			Texture2D tex = buttonTex, texH = buttonTex_Hover, texC = buttonTex_Click;
			if(mp.psuedoScrewActive)
			{
				tex = buttonTexEnabled;
				texH = buttonTexEnabled_Hover;
				texC = buttonTexEnabled_Click;
			}
			
			if(base.IsMouseHovering)
			{
				tex = texH;
				if(clicked)
				{
					tex = texC;
					clicked = false;
				}
				
				string psText = "Charge Somersault Attack: Disabled";
				if(mp.psuedoScrewActive)
				{
					psText = "Charge Somersault Attack: Enabled";
				}
				Main.hoverItemName = psText;
			}
			
			sb.Draw(tex, drawRectangle, Color.White);
		}
	}
	
	// Combo Error messages
	public class ComboError : UIPanel
	{
		Texture2D iconTex;
		
		public Rectangle drawRectangle
		{
			get { return new Rectangle((int)(Parent.Left.Pixels + Left.Pixels), (int)(Parent.Top.Pixels + Top.Pixels), (int)Width.Pixels, (int)Height.Pixels); }
		}
		
		public override void OnInitialize()
		{
			this.Left.Pixels = 420;
			this.Top.Pixels = 354;
			
			iconTex = ModContent.GetTexture("MetroidMod/Textures/UI/ComboErrorIcon");
			
			Width.Pixels = iconTex.Width;
			Height.Pixels = iconTex.Height;
		}
		
		public override void Update(GameTime gameTime)
		{
			if(base.IsMouseHovering)
			{
				Main.LocalPlayer.mouseInterface = true;
			}
			
			this.Top.Pixels = 354;
			if (Main.LocalPlayer.chest != -1 || Main.npcShop != 0)
			{
				this.Top.Pixels += 170;
			}

			base.Update(gameTime);
		}
		
		protected override void DrawSelf(SpriteBatch sb)
		{
			PowerBeam powerBeamTarget = Main.LocalPlayer.inventory[((MetroidMod)MetroidMod.Instance).selectedItem].modItem as PowerBeam;
			if(powerBeamTarget != null && (powerBeamTarget.comboError1 || powerBeamTarget.comboError2 || powerBeamTarget.comboError3 || powerBeamTarget.comboError4))
			{
				MPlayer mp = Main.LocalPlayer.GetModPlayer<MPlayer>();
				
				if(base.IsMouseHovering)
				{
					string text = "Error: addon version mistmatch detected.\n"+
					"The following slots have had their addon effects disabled:";
					if(powerBeamTarget.comboError1)
					{
						text = text+"\nSecondary";
					}
					if(powerBeamTarget.comboError2)
					{
						text = text+"\nUtility";
					}
					if(powerBeamTarget.comboError3)
					{
						text = text+"\nPrimary A";
					}
					if(powerBeamTarget.comboError4)
					{
						text = text+"\nPrimary B";
					}
					text = text+"\n \n"+
					"Note: Addon stat bonuses are still applied.";
					
					Main.hoverItemName = text;
				}
				
				sb.Draw(iconTex, drawRectangle, Color.White);
			}
		}
	}
}
