using GivlsWeapons.Content.Buffs;
using GivlsWeapons.Content.Dusts;
using System;

namespace GivlsWeapons.Common.NPCs
{
    public class NPCBuffEffects : GlobalNPC
    {
        public override void SetStaticDefaults()
        {
            NPCID.Sets.SpecificDebuffImmunity[NPCID.ChaosElemental][BuffID.ChaosState] = true;
        }
        public override void PostAI(NPC npc)
        {
            if(npc.HasBuff(BuffID.ChaosState) && npc.position.DistanceSQ(npc.oldPosition) > 48 * 48)
            {
                int hitDamage = 100;
                if(Main.expertMode) hitDamage = 200;
                if(Main.masterMode) hitDamage = 300;
                
                npc.SimpleStrikeNPC(hitDamage, -npc.direction);
            }
        }
        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
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
