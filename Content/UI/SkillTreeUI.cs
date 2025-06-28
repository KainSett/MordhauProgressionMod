using Microsoft.Xna.Framework;
using MordhauProgression.Common.Config;
using MordhauProgression.Content.UI;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria;
using Microsoft.Xna.Framework.Graphics;
using MordhauProgression.Common.Assets;
using Terraria.GameInput;
using Terraria.UI.Gamepad;
using System.Collections.Generic;

namespace MordhauProgression.Content.UI;
[Autoload(Side = ModSide.Client)]
public class TraitButtonUISystem : ModSystem
{
    internal TraitButtonUIState state;
    private UserInterface Interface;

    public void Show() => Interface?.SetState(state);

    public void Hide() => Interface?.SetState(null);

    public override void Load()
    {
        state = new TraitButtonUIState();
        Interface = new UserInterface();
        state.Activate();
    }

    public bool IsActive()
    {
        if (Interface.CurrentState == null)
            return false;

        return true;
    }

    public override void UpdateUI(GameTime gameTime) => Interface?.Update(gameTime);

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Fancy UI"));
        if (index == -1)
            return;

        layers.Insert(index, new LegacyGameInterfaceLayer("MordhauProgression: Trait Button", delegate
        {
            Interface.Draw(Main.spriteBatch, new GameTime());

            return true;

        }, InterfaceScaleType.UI));
    }
}

public class TraitButtonUIElement : UIElement
{
    private static readonly Vector2 ButtonOffset = new(30f);

    public override void OnInitialize() => IgnoresMouseInteraction = true;

    public override void Draw(SpriteBatch spriteBatch)
    {
        Texture2D texture = Textures.Button.Value;

        Vector2 origin = texture.Size() * 0;

        var rect = texture.Frame(4, 3, type, Tier);

        spriteBatch.Draw(texture, GetInnerDimensions().Position() / Main.UIScale, rect, Color.White, 0, origin, 2 / Main.UIScale, SpriteEffects.None, 0);
    }
    public int Tier = 0;

    public int type = 0;

    public override void LeftClick(UIMouseEvent evt)
    {
        if (evt.Target == this)
            OnLeftInteract(evt);
    }

    public override void RightClick(UIMouseEvent evt)
    {
        if (evt.Target == this)
            OnRightInteract(evt);
    }
    private void OnRightInteract(UIMouseEvent evt)
    {
        Tier = 0;
    }

    private void OnLeftInteract(UIMouseEvent evt)
    {
        Tier = (int)Clamp(Tier++, 0, 3);
    }

    public override void Update(GameTime gameTime)
    {
        if (ContainsPoint(Main.MouseScreen))
            Main.LocalPlayer.mouseInterface = true;

    }
}

public class TraitButtonUIState : UIState
{
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);
        DrawChildren(Main.spriteBatch);
    }
}