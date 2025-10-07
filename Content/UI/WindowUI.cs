using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria;
using Terraria.GameInput;
using Terraria.UI.Gamepad;
using Microsoft.Xna.Framework.Graphics;
using MordhauProgression.Common.Assets;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent;

namespace MordhauProgression.Content.UI;

[Autoload(Side = ModSide.Client)]
public class WindowUISystem : ModSystem
{
    internal WindowUIState state;

    public void OpenUI()
    {
        //if (player.mount.Active)
        //    player.mount.Dismount(player);

        Main.ClosePlayerChat();

        // Fixes some bugs with menus opened via right click.
        Main.mouseRightRelease = false;

        Main.ingameOptionsWindow = false;
        Main.chatText = string.Empty;

        IngameFancyUI.OpenUIState(state);
    }

    public override void Load()
    {
        ReInitialize();
    }

    public void ReInitialize()
    {
        state = new WindowUIState();
        ModContent.GetInstance<ArmorUISystem>()?.ReInitialize();
        ModContent.GetInstance<TraitButtonUISystem>()?.ReInitialize();
        ModContent.GetInstance<RoleUISystem>()?.ReInitialize();
    }

    public static bool IsActive()
    {
        if (!Main.inFancyUI)
            return false;

        return Main.InGameUI.CurrentState is BaseFancyUI;
    }

    public override void UpdateUI(GameTime gameTime)
    {
        if (state.InUI)
            Main.LocalPlayer.mouseInterface = true;
    }
}

public class WindowUIState : BaseFancyUI
{
    public override void OnActivate()
    {
        InitializeUI();
        ModContent.GetInstance<ArmorUISystem>()?.state.Activate();
        ModContent.GetInstance<TraitButtonUISystem>()?.state.Activate();
        ModContent.GetInstance<RoleUISystem>()?.state.Activate();

        if (PlayerInput.UsingGamepadUI)
            UILinkPointNavigator.ChangePoint(3002);
    }
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
    }

    public override bool ExitCondition => !Main.LocalPlayer.GetModPlayer<UIPlayer>().WindowOpen;
}

public class WindowUIElement : UIElement
{
    public override void OnInitialize() => IgnoresMouseInteraction = true;

    public override void Draw(SpriteBatch spriteBatch)
    {
        Texture2D texture = Textures.Window.Value;

        Vector2 origin = texture.Size() * 0;

        var color = Color.White * 150f;

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, Main.Rasterizer);

        spriteBatch.Draw(texture, GetInnerDimensions().Position(), null, color with { A = 240 }, 0, origin, ScreenSize / texture.Size(), SpriteEffects.None, 0);

    }
}