using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using GivlsWeapons.Content.Projectiles.Weapons;
using Terraria.GameContent.Creative;
using Microsoft.Xna.Framework;
using GivlsWeapons.Content.Items.Materials;

namespace GivlsWeapons.Content.Items.Weapons
{
    internal class BlackandWhiteGun : ModItem
    {
        private bool isBlackNext = false;
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 15;
            Item.rare = ItemRarityID.LightRed;
            Item.value = 100000;

            Item.useTime = 16;
            Item.useAnimation = 16;
            Item.reuseDelay = 4;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.autoReuse = false;

            Item.damage = 45;
            Item.DamageType = DamageClass.Ranged;
            Item.crit = 7;
            Item.knockBack = 3.5f;
            Item.noMelee = true;
            Item.useAmmo = AmmoID.Bullet;

            Item.shootSpeed = 50f;
            Item.shoot = ProjectileID.PurificationPowder;

            Item.UseSound = SoundID.Item41;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            type = isBlackNext ? ModContent.ProjectileType<BlackBullet>() : ModContent.ProjectileType<WhiteBullet>();
            isBlackNext = !isBlackNext;
        }

        public override void AddRecipes()
        {
            //CreateRecipe()
                //.AddIngredient(ModContent.ItemType<BalanceSoul>(), 2)
                //.AddIngredient(ItemID.TheUndertaker)
                //.AddTile(TileID.MythrilAnvil)
                //.Register();
            //CreateRecipe()
                //.AddIngredient(ModContent.ItemType<BalanceSoul>(), 2)
                //.AddIngredient(ItemID.Musket)
                //.AddTile(TileID.MythrilAnvil)
                //.Register();
            CreateRecipe()
                .AddIngredient(ItemID.DarkShard)
                .AddIngredient(ItemID.LightShard)
                .AddIngredient(ItemID.Musket)
                .AddTile(TileID.MythrilAnvil)
                .Register();
            CreateRecipe()
                .AddIngredient(ItemID.DarkShard)
                .AddIngredient(ItemID.LightShard)
                .AddIngredient(ItemID.TheUndertaker)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
