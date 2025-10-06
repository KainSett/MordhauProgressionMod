using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MordhauProgression.Content;

public class PointCoin : ModItem
{
    public override string Texture => "MordhauProgression/Common/Assets/UI/TierCoins";

    public override void SetStaticDefaults()
    {
        Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(60, 3, true));
        ItemID.Sets.AnimatesAsSoul[Item.type] = true;
    }

    public int tier = 0;

    public int counter = 0;

    public int index = 0;

    public override void SetDefaults()
    {
        Item.height = Item.width = 16;
        Item.maxStack = 4;
    }

    public override void Update(ref float gravity, ref float maxFallSpeed)
    {
        tier = Math.Clamp(Item.stack - 1, 0, 3);
    }

    public override bool OnPickup(Player player)
    {
        player.GetModPlayer<UIPlayer>().TotalPoints += tier + 1;
        player.GetModPlayer<UIPlayer>().Points[player.CurrentLoadoutIndex] += tier + 1;

        return false;
    }

    public bool PingPong = false;

    public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
    {
        tier = Math.Clamp(Item.stack - 1, 0, 3);

        var texture = TextureAssets.Item[Type];

        var frame = texture.Frame(4, 3, tier, index, -1, -1);

        Vector2 drawPosition = Item.Bottom - Main.screenPosition - new Vector2(0, (frame.Size() / 2f).Y);

        counter = (counter + 1) % 6;
        if (counter == 0)
            index = (index + (PingPong ? 1 : -1)) % 3;
        if (index == 2)
            PingPong = false;
        else if (index == 0)
            PingPong = true;

        spriteBatch.Draw(texture.Value, drawPosition - new Vector2(1, 2), frame, lightColor, rotation, frame.Size() / 2f, scale * 2, !PingPong ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);

        return false;
    }

    public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        tier = Math.Clamp(Item.stack - 1, 0, 3);

        var texture = TextureAssets.Item[Type];

        var Frame = texture.Frame(4, 3, tier, index, -1, -1);

        counter = (counter + 1) % 6;
        if (counter == 0)
            index = (index + (PingPong ? 1 : -1)) % 3;
        if (index == 2)
            PingPong = false;
        else if (index == 0)
            PingPong = true;

        spriteBatch.Draw(texture.Value, position, Frame, drawColor, 0, Frame.Size() / 2, scale * 2, PingPong ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);

        return false;
    }
}