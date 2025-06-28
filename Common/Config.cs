using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace MordhauProgression.Common.Config;

public class ClientConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ClientSide;

    [DefaultValue(false)]
    public bool ButtonUIDraggable;

    [DefaultValue(typeof(Vector2), "0.78125, 0.13888")]
    public Vector2 ButtonUIPosition;
}