using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using GivlsWeapons.Content.Buffs;
using GivlsWeapons.Content.Dusts;
using System;

namespace GivlsWeapons.Content.Globals
{
    internal class BuffVisuals : GlobalNPC
    {
        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (npc.HasBuff<WhiteGlow>())
            {
                drawColor = Color.AntiqueWhite;
            }
            if (npc.HasBuff<BlackGlow>())
            {
                drawColor = Color.Black;
            }
            if (npc.HasBuff(BuffID.ChaosState))
            {
                    int numToSpawn = Main.rand.Next(3);

                    for (int i = 0; i < numToSpawn; i++)
                    {
                    Vector2 startPos = new Vector2(Main.rand.Next((int)npc.Left.X, (int)npc.Right.X), Main.rand.Next((int)npc.Top.Y, (int)npc.Bottom.Y));
                    float velocityX = Main.rand.NextFloat(-2f, 2f);
                    float velocityY = 2f - Math.Abs(velocityX);
                    if (Main.rand.NextBool())
                    {
                        velocityY *= -1f;
                    }
               
                        Dust.NewDust(startPos, 7, 7, ModContent.DustType<DiscordDust>(), velocityX, velocityY,
                            50, default, 1f);
                    }
            }
        }
    }
}
