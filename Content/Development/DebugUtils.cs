using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.ID;
using Terraria.DataStructures;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria.GameContent;
using System.Net;

namespace GivlsWeapons.Development
{
    public static class DebugUtils
    {
        public static void SolidTilesView(Vector2 position, int width, int height) //Thanks to photonic for providing these tools to visualize collision
        {
            Texture2D blankTexture = Terraria.GameContent.TextureAssets.Extra[195].Value;
            Vector2 texScale = new Vector2(width * 16, height * 16) * 0.00390625f;//1/256, texture is 256x256
            position /= 16;
            position.X = MathF.Floor(position.X);
            position.Y = MathF.Floor(position.Y);
            position.Y *= 16;
            Main.EntitySpriteDraw(blankTexture, position - Main.screenPosition, null, Color.Red, 0, Vector2.Zero, texScale, SpriteEffects.None);
        }

        public static void AABBLineVisualizer(Vector2 lineStart, Vector2 lineEnd, float lineWidth)
        {
            Texture2D blankTexture = Terraria.GameContent.TextureAssets.Extra[195].Value;
            Vector2 texScale = new Vector2((lineStart - lineEnd).Length(), lineWidth) * 0.00390625f;//1/256, texture is 256x256
            Main.EntitySpriteDraw(blankTexture, (lineStart) - Main.screenPosition, null, Color.Red, (lineEnd - lineStart).ToRotation(), new Vector2(0, 128), texScale, SpriteEffects.None);
        }
    }
}