using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using GivlsWeapons.Content.Projectiles.Weapons;
using Terraria.GameContent.Creative;

namespace GivlsWeapons.Content.Items.Weapons
{
    internal class RisingFlameArrow : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 99;
        }
        public override void SetDefaults()
        {
           /*  Item.ammo = AmmoID.Arrow;
            Item.value = Item.sellPrice(copper: 20);
            Item.consumable = true;
            Item.maxStack = 9999;
            Item.rare = ItemRarityID.Orange;

            
            Item.DamageType = DamageClass.Ranged;
            Item.damage = 13;
            Item.knockBack = 2;
            Item.shootSpeed = 2f; */
            Item.CloneDefaults(ItemID.HolyArrow);
            Item.shoot = ModContent.ProjectileType<RisingFlameArrowProjectile>();
        }

        public override void AddRecipes()
        {
            CreateRecipe(100)
                .AddIngredient(ItemID.WoodenArrow, 120)
                .AddIngredient(ItemID.RedTorch, 2)
                .AddIngredient(ItemID.LivingFireBlock, 5)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}