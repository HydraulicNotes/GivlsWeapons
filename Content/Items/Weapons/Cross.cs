using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using GivlsWeapons.Content.Projectiles.Weapons;

namespace GivlsWeapons.Content.Items.Weapons
{
    internal class Cross : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 12;

            Item.useStyle = ItemUseStyleID.Shoot;
            Item.autoReuse = true;
            Item.useTurn = false;

            Item.value = 150000;
            Item.rare = ItemRarityID.LightRed;

            Item.DamageType = DamageClass.Magic;
            Item.noMelee = true;
            Item.mana = 6;
            Item.damage = 45;
            Item.knockBack = 1.2f;

            Item.useTime = 10;
            Item.useAnimation = 10;

            Item.shoot = ModContent.ProjectileType<CrossAura>();
            Item.shootSpeed = 0f;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(1, -6);
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            position.X += player.direction * 192;
        }
    }
}
