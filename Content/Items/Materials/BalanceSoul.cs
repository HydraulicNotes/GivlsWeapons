using Terraria.DataStructures;
using Terraria.GameContent.Creative;

namespace GivlsWeapons.Content.Items.Materials
{
    public class BalanceSoul : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.Deprecated[Type] = true;
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 5;
            ItemID.Sets.AnimatesAsSoul[Type] = true;
            Main.RegisterItemAnimation(Type, new DrawAnimationVertical(5, 17));
        }
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.maxStack = Item.CommonMaxStack;
        }
    }
}