using Terraria.ID;
using Terraria.GameContent.ItemDropRules;
using GivlsWeapons.Content.Items.Accessories;
using GivlsWeapons.Content.Items.Weapons;
using GivlsWeapons.Content.Items.Potions;
using Terraria.Enums;

namespace GivlsWeapons.Common.Items
{
    public class NPCDrops : GlobalNPC
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
            if(npc.type == NPCID.BigMimicJungle)
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<RubberKnife>(), 3));
            }
            if(npc.type == NPCID.Snail && Main.bloodMoon && Main.GetMoonPhase() == MoonPhase.Empty && NPC.downedMoonlord && !NPC.downedAncientCultist)
            {
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<RubberKnife>()));
            }
        }
    }
}
