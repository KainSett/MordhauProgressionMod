using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MordhauProgression.Common.Assets;
using System.Collections.Generic;
using System;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI.Chat;
using Terraria.UI;
using Terraria;
using System.Linq;
using Terraria.Localization;

namespace MordhauProgression.Content.UI;
[Autoload(Side = ModSide.Client)]
public class PointUISystem : ModSystem
{
    internal PointUIState state;
    private UserInterface Interface;
    public static Vector2 Position = new();

    public void Show()
    {
        Interface?.SetState(state);
    }

    public void Hide()
    {
        Interface?.SetState(null);
    }

    public override void Load()
    {
        UIDetoursSystem.NameList.Add(LayerName);
        ReInitialize();
    }

    public void ReInitialize()
    {
        state = new PointUIState();
        Interface = new UserInterface();
    }

    public bool IsActive()
    {
        if (Interface.CurrentState == null)
            return false;

        return true;
    }

    public override void UpdateUI(GameTime gameTime) => Interface?.Update(gameTime);

    public static string LayerName = "MordhauProgression: Point";

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
        if (index == -1)
            return;

        var l = new LegacyGameInterfaceLayer(LayerName, delegate
        {
            Interface.Draw(Main.spriteBatch, new GameTime());

            return true;

        }, InterfaceScaleType.None);

        layers.Insert(index + 1, l);

        UIDetoursSystem.UILayers = layers;
    }
}

public class PointUIElement : UIElement
{
    public override void Draw(SpriteBatch spriteBatch)
    {
        Texture2D texture = Textures.Point.Value;
        var rect = texture.Frame(1, 4, 0, frame.index, -1, -1);

        var scale = Main.UIScale;
        if (Main.InGameUI.CurrentState is BaseFancyUI)
            scale = 1.5f;


        Vector2 origin = rect.Size() * 0.5f;

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, Main.Rasterizer);

        var center = new Vector2(Left.Pixels + Width.Pixels / 2, Top.Pixels + Height.Pixels / 2);

        var effect = SpriteEffects.None;
        if (frame.second)
            effect = SpriteEffects.FlipHorizontally;

        spriteBatch.Draw(texture, center, rect, Color.White, 0, origin, 2 * scale, effect, 0);
    }

    #region Fields
    public (int index, bool second) frame = (0, false);

    private int timer = 0;
    #endregion

    public override void LeftClick(UIMouseEvent evt)
    {
        base.LeftClick(evt);
        if (evt.Target == this)
            OnLeftInteract();
    }

    private void OnLeftInteract()
    {
        if (Main.LocalPlayer.TryGetModPlayer<UIPlayer>(out var player))
        {
            if (player.WindowOpen)
                player.CloseWindow();

            else player.OpenWindow();
        }
    }

    public override bool ContainsPoint(Vector2 point)
    {
        var scale = Main.UIScale;
        if (Main.InGameUI.CurrentState is BaseFancyUI)
            scale = 1;

        return base.ContainsPoint(point * scale);
    }

    public override void Update(GameTime gameTime)
    {
        var scale = Main.UIScale;
        if (Main.InGameUI.CurrentState is BaseFancyUI)
            scale = 1;

        if (ContainsPoint(Main.MouseScreen))
        {
            Main.LocalPlayer.mouseInterface = true;

            UIPlayer.CurrentTooltipUI = () =>
            {
                var texture = Textures.Window.Value;
                var font = FontAssets.MouseText.Value;


                var sb = Main.spriteBatch;

                var open = "Open";
                var points = 0;
                var total = 0;
                if (Main.LocalPlayer.TryGetModPlayer<UIPlayer>(out var player))
                {
                    if (player.WindowOpen)
                        open = "Close";

                    points = player.Points[Main.LocalPlayer.CurrentLoadoutIndex];
                    total = player.TotalPoints;
                }

                var text = string.Format(Language.GetTextValue($"Mods.MordhauProgression.Tooltips.Point.Cur"), $"{points}") + "\n" + string.Format(Language.GetTextValue($"Mods.MordhauProgression.Tooltips.Point.Total"), $"{points}") + "\n\n"+ Language.GetTextValue($"Mods.MordhauProgression.Tooltips.Point.{open}");

                var textSize = ChatManager.GetStringSize(font, text, new Vector2(1f), 160);
                var textOffset = new Vector2(15);

                var desiredSize = textOffset * 2 + textSize;

                var color = player.WindowOpen ? Color.DarkSlateGray : Color.LightGray;
                color *= 0.4f;

                var WindowScale = desiredSize / texture.Size();

                scale = 1f;

                var pos = Main.MouseScreen - new Vector2(0, desiredSize.Y);

                sb.Draw(texture, pos, null, color, 0, Vector2.Zero, WindowScale, SpriteEffects.None, 0);

                color = Color.White;
                color *= 0.6f;

                ChatManager.DrawColorCodedString(sb, font, text, pos + textOffset, color, 0, Vector2.Zero, new Vector2(1f), 160);
            };

            timer = (timer + 1) % 5;
            if (timer % 5 != 0)
                return;

            if (frame.index == 3)
                frame.second = false;

            else if (frame.index == 0)
                frame.second = true;


            frame.index += frame.second ? 1 : -1;
        }
        else
            frame = (0, false);
    }
}

public class PointUIState : UIState
{
    public override void OnInitialize()
    {
        var pos = new Vector2(Main.instance.GraphicsDevice.Viewport.Width / 2f, Main.instance.GraphicsDevice.Viewport.Height / 1.1f);

        PointUIElement element = new();

        element.Left.Set(pos.X - 16, 0f);
        element.Top.Set(pos.Y - 16, 0f);

        element.Width.Set(32, 0);
        element.Height.Set(32, 0);
        element.MinWidth.Set(16, 0);
        element.MaxWidth.Set(64, 0);
        element.MaxHeight.Set(64, 0);
        element.MinHeight.Set(16, 0);


        Append(element);
        element.Activate();
    }
}