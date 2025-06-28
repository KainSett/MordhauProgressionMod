using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Gamepad;

namespace MordhauProgression.Content.UI;

public class WindowUIState : BaseFancyUI
{
    public override void OnActivate()
    {
        InitializeUI();

        if (PlayerInput.UsingGamepadUI)
            UILinkPointNavigator.ChangePoint(3002);
    }
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
    }
}
