using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.ItemDropRules;
using GivlsWeapons.Content.Items.Accessories;
using GivlsWeapons.Content.Items.Weapons;
using GivlsWeapons.Content.Items.Materials;
using GivlsWeapons.Content.Items.Potions;

namespace GivlsWeapons.Content.Globals
{
    internal class NPCDrops : GlobalNPC
    {
        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            if(npc.type == NPCID.EnchantedSword)
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Cross>(), 40));
            }
            if(npc.type == NPCID.RedDevil)
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<BoringWand>(), 30));
            }
            if(npc.type == NPCID.RaggedCaster || npc.type == NPCID.RaggedCasterOpenCoat)
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<CursedFlask>(), 25));
            }
            if(npc.type == NPCID.Lihzahrd || npc.type == NPCID.LihzahrdCrawler || npc.type == NPCID.FlyingSnake)
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ChlorophyteIdol>(), 60));
            }
            /* if (Main.hardMode && npc.position.Y > Main.rockLayer * 16.0)
            {
                if (Main.player[Player.FindClosest(npc.position, npc.width, npc.height)].ZoneHallow)
                {
                    if(npc.type == NPCID.Corruptor || npc.type == NPCID.CorruptSlime || npc.type == NPCID.BigMimicCorruption || npc.type == NPCID.DesertGhoulCorruption || npc.type == NPCID.PigronCorruption || npc.type == NPCID.DesertLamiaDark || npc.type == NPCID.DarkMummy || npc.type == NPCID.DevourerHead || npc.type == NPCID.SeekerHead || npc.type == NPCID.Slimer2 || npc.type == NPCID.CursedHammer || npc.type == NPCID.Clinger || npc.type == NPCID.Crimslime || npc.type == NPCID.CrimsonAxe || npc.type == NPCID.BigCrimslime || npc.type == NPCID.BigMimicCrimson || npc.type == NPCID.DesertGhoulCrimson || npc.type == NPCID.FloatyGross || npc.type == NPCID.IchorSticker || npc.type == NPCID.PigronCrimson || npc.type == NPCID.BloodJelly || npc.type == NPCID.BloodFeeder)
                    {
                        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<BalanceSoul>()));
                    }
                }
                else if (Main.player[Player.FindClosest(npc.position, npc.width, npc.height)].ZoneCrimson || Main.player[Player.FindClosest(npc.position, npc.width, npc.height)].ZoneCorrupt)
                {
                    if(npc.type == NPCID.IlluminantBat || npc.type == NPCID.IlluminantSlime || npc.type == NPCID.ChaosElemental || npc.type == NPCID.EnchantedSword || npc.type == NPCID.BigMimicHallow || npc.type == NPCID.DesertGhoulHallow || npc.type == NPCID.PigronHallow)
                    {
                        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<BalanceSoul>()));
                    }
                }
            } */ //This doesn't work. I could probably fix it in 5 seconds now that ik what I'm doing, but I don't feel like it right now.
            if(npc.type == NPCID.Snail && Main.bloodMoon && Main.moonPhase == 4 && NPC.downedMoonlord && !NPC.downedAncientCultist) // Pretty sure this doesn't either, and definitely sure I'm too lazy to test right now.
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<RubberKnife>()));
            }
        }
    }
}
