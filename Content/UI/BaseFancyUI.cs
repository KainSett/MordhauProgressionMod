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
// rewrote to fit my usecase
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

// stole the GUIBarsDrawInner detour (with permission) from https://github.com/ZenTheMod/WizenkleBoss/blob/dev/Common/ILDetourSystems/HideResourceBarsSystem.cs
public class UIDetoursSystem : ModSystem
{
    public override void Load()
    {
        On_Main.GUIBarsDrawInner += StopBarsInFancyUI;
        On_PlayerInput.SetZoom_UI += On_PlayerInput_SetZoom_UI;
        On_UIElement.Recalculate += UnZoomPositionForMyUI;
    }

    public override void Unload()
    {
        On_Main.GUIBarsDrawInner -= StopBarsInFancyUI;
        On_PlayerInput.SetZoom_UI -= On_PlayerInput_SetZoom_UI;
        On_UIElement.Recalculate -= UnZoomPositionForMyUI;
    }


    private void UnZoomPositionForMyUI(On_UIElement.orig_Recalculate orig, UIElement self)
    {
        var cond = self.Parent is PointUIState or ArmorCoinUIState;
        if (cond)
        {
            var oldScale = Main.UIScale;
            Main.UIScale = 1f;

            orig(self);

            Main.UIScale = oldScale;

            // Only works if min height and min width are exactly half of the default height and width, accordingly
            var newHeight = Clamp(self.MinHeight.Pixels * 2 * oldScale, self.MinHeight.Pixels, self.MaxHeight.Pixels);
            var newWidth = Clamp(self.MinWidth.Pixels * 2 * oldScale, self.MinWidth.Pixels, self.MaxWidth.Pixels);
            self.Top.Set(self.Top.Pixels - (newHeight - self.Height.Pixels) / 2, self.Top.Precent);
            self.Left.Set(self.Left.Pixels - (newWidth - self.Width.Pixels) / 2, self.Left.Precent);
            self.Width.Set(newWidth, self.Width.Precent);
            self.Height.Set(newHeight, self.Height.Precent);
        }
        else orig(self);
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
            i--;
        }
        Main.spriteBatch.Begin();
    }

    public static List<GameInterfaceLayer> UILayers = [];

    public static List<string> NameList = [];

    private void StopBarsInFancyUI(On_Main.orig_GUIBarsDrawInner orig, Main self)
    {
        if (Main.InGameUI.CurrentState is not BaseFancyUI)
            orig(self);

        else
        {
            DrawSpecificLayers([..NameList]);
        }
    }

    private void On_PlayerInput_SetZoom_UI(On_PlayerInput.orig_SetZoom_UI orig)
    {
        var cond = Main.inFancyUI && WindowUISystem.IsActive();
        if (cond)
        {
            var oldScale = Main.UIScale;
            Main.UIScale = 1f;

            orig();

            Main.UIScale = oldScale;
        }
        else orig();
    }
}