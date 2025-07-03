using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace MordhauProgression.Common.Assets;

    // [Autoload(Side = ModSide.Client)]
public static class Textures
{
    private const string prefix = "MordhauProgression/Common/Assets/";

    public static readonly Asset<Texture2D> Window = LoadTexture2D("UI/Window");

    public static readonly Asset<Texture2D> Icons = LoadTexture2D("UI/Icons");

    public static readonly Asset<Texture2D> Point = LoadTexture2D("UI/Point");

    public static readonly Asset<Texture2D> Armor = LoadTexture2D("UI/Armor");

    public static readonly Asset<Texture2D> Roles = LoadTexture2D("UI/Roles");

    private static Asset<Texture2D> LoadTexture2D(string TexturePath)
    {
        if (Main.dedServ)
            return null;

        return ModContent.Request<Texture2D>(prefix + TexturePath);
    }

    private static Asset<Texture2D>[] LoadTexture2Ds(string TexturePath, int count)
    {
        if (Main.dedServ)
            return null;

        Asset<Texture2D>[] textures = new Asset<Texture2D>[count];

        for (int i = 0; i < count; i++)
            textures[i] = ModContent.Request<Texture2D>(prefix + TexturePath + i);

        return textures;
    }
}
