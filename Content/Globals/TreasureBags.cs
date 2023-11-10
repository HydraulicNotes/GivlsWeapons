using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.ItemDropRules;
using GivlsWeapons.Content.Items.Accessories;

namespace GivlsWeapons.Content.Globals
{
    internal class TreasureBags : GlobalItem
    {
        public override void ModifyItemLoot(Item item, ItemLoot itemLoot)
        {
            if(item.type == ItemID.QueenSlimeBossBag)
            {
                itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<DiscordShield>()));
            }
        }
    }
}
