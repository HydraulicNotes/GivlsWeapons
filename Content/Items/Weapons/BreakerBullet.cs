using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using GivlsWeapons.Content.Projectiles.Weapons;
using Terraria.GameContent.Creative;
using Terraria.DataStructures;

namespace GivlsWeapons.Content.Items.Weapons
{
    internal class BreakerBullet : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 99;
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(5, 7));
            ItemID.Sets.AnimatesAsSoul[Item.type] = true;
        }
        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 16;

            Item.value = Item.sellPrice(copper: 2);
            Item.maxStack = Item.CommonMaxStack;
            Item.consumable = true;
            Item.ammo = AmmoID.Bullet;
            Item.rare = ItemRarityID.Cyan;

            Item.damage = 15;
            Item.DamageType = DamageClass.Ranged;
            Item.knockBack = 3f;
            Item.shootSpeed = 16f;
            Item.shoot = ModContent.ProjectileType<BreakerBulletProjectile>();
        }
        public override void AddRecipes()
        {
            CreateRecipe(200)
                .AddIngredient(ItemID.MusketBall, 200)
                .AddIngredient(ItemID.LivingDemonFireBlock, 3)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }
}
