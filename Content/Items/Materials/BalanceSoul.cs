using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.GameContent.Creative;

namespace GivlsWeapons.Content.Items.Materials
{
    internal class BalanceSoul : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 10;
        }
        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 16;
            Item.value = Item.sellPrice(1000);
            Item.maxStack = 9999;
            Item.rare = ItemRarityID.Orange;
        }

    }
}
