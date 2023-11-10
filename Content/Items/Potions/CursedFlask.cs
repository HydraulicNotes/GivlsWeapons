using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
using GivlsWeapons.Content.Buffs;
using Microsoft.Xna.Framework;

namespace GivlsWeapons.Content.Items.Potions
{
    internal class CursedFlask : ModItem
    {
        const int CURSEDURATION = 1200; //Base duration for the potion curse 
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;

            // Dust that will appear in these colors when the item with ItemUseStyleID.DrinkLiquid is used
            ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
                new Color(87, 9, 26),
                new Color(137, 13, 39),
                new Color(220, 210, 92)
            };
        }
        public override void SetDefaults()
        {
            Item.width = 42;
            Item.height = 54;

            Item.rare = ItemRarityID.Yellow; //Hardmode dungeon tier
            Item.value = Item.sellPrice(gold: 10);

            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.useTime = 17;
            Item.useAnimation = 17;
            Item.useTurn = true;
            Item.UseSound = SoundID.Item3;

            //I don't think there's actually all that much to set. Most health-potion related stats are intentionally not being used.
            //I don't want it to be used by quick-heal, stack, have a set healing amount, or display an automatic tooltip showing the healing amount
        }

        public override bool CanUseItem(Player player)
        {
            return !player.HasBuff(ModContent.BuffType<InstaKillCurse>()) && player.statLife < player.statLifeMax2;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                player.Heal(player.statLifeMax2 - player.statLife);
                player.AddBuff(ModContent.BuffType<InstaKillCurse>(), player.pStone ? (int)(CURSEDURATION * 0.75f) : CURSEDURATION); //15 seconds, multiply by 0.75 if the player has a philosopher's stone effect
            }
            return true;
        }
    }
}