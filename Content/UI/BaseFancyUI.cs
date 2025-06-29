using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.UI.Gamepad;
using Terraria.UI;
using Terraria;
using Terraria.ModLoader;
using System.Collections.Generic;
using System.Linq;

namespace MordhauProgression.Content.UI;

// stole (with permission) from https://github.com/ZenTheMod/WizenkleBoss/blob/dev/Content/UI/BaseFancyUI.cs
public abstract class BaseFancyUI : UIState
{

    public virtual bool ExitCondition => false;

    public bool InUI => Main.InGameUI.CurrentState == this;

    public virtual void InitializeUI()
    {
        // i had an overlap bug :sob:
        RemoveAllChildren();

        WindowUIElement element = new();
        element.SetPadding(0);

        element.Left.Set(0, 0f);
        element.Top.Set(0, 0f);
        element.Width.Set(0, 1f);
        element.Height.Set(0f, 1f);
        element.MinWidth.Set(0f, 1f);
        element.MaxWidth.Set(0, 1f);
        element.MaxHeight.Set(0, 1f);
        element.MinHeight.Set(0f, 1f);
        element.HAlign = 0.5f;
        Append(element);

        
    }

    public override void OnActivate()
    {
        InitializeUI();
        if (PlayerInput.UsingGamepadUI)
            UILinkPointNavigator.ChangePoint(3002);
    }

    public override void Update(GameTime gameTime)
    {
        Player player = Main.LocalPlayer;

        if (player.dead || ExitCondition || player.CCed || !player.active)
        {
            Main.menuMode = 0;
            IngameFancyUI.Close();
        }
    }
}

// stole (with permission) from https://github.com/ZenTheMod/WizenkleBoss/blob/dev/Common/ILDetourSystems/HideResourceBarsSystem.cs
public class HideResourceBarsSystem : ModSystem
{
    // ModifyInterfaceLayers does not actually work for this purpose ?
    public override void Load()
    {
        On_Main.GUIBarsDrawInner += StopBarsInFancyUI;
    }

    public override void Unload()
    {
        On_Main.GUIBarsDrawInner -= StopBarsInFancyUI;
    }

    public static void DrawSpecificLayers(string[] names)
    {
        Main.spriteBatch?.End();
        for (int i = 0; i < UILayers.Count; i++)
        {
            var l = UILayers[i];

            if (names.Contains(l.Name))
                l.Draw();

            UILayers.RemoveAt(i);
        }
        Main.spriteBatch.Begin();
    }

    public static List<GameInterfaceLayer> UILayers = new List<GameInterfaceLayer>();

    private void StopBarsInFancyUI(On_Main.orig_GUIBarsDrawInner orig, Main self)
    {
        if (Main.InGameUI.CurrentState is not BaseFancyUI)
            orig(self);

        else
        {
            DrawSpecificLayers([TraitButtonUISystem.LayerName]);
        }
    }
}