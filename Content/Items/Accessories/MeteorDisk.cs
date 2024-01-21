using Terraria;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
using System.Linq;
using Microsoft.Xna.Framework;
using GivlsWeapons.Content.Projectiles.Accessories;
using Terraria.ID;

namespace GivlsWeapons.Content.Items.Accessories
{
    internal class MeteorDisk : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.damage = 15;
            Item.DamageType = DamageClass.Magic;
            Item.knockBack = 0;
            Item.value = 50000;
            Item.rare = ItemRarityID.Blue;

            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            int AurasActive = player.ownedProjectileCounts[ModContent.ProjectileType<MeteorDiskAura>()];

            if (AurasActive > 1)
            {
                foreach (var p in Main.projectile.Take(Main.maxProjectiles).Where(x => x.active && x.owner == player.whoAmI && x.type == ModContent.ProjectileType<MeteorDiskAura>()))
                    p.Kill();
            }
            else if (AurasActive == 0)
            {
                Projectile.NewProjectile(player.GetSource_Accessory(Item), new Vector2(player.position.X, player.Bottom.Y + 50), new Vector2(0, 0), ModContent.ProjectileType<MeteorDiskAura>(), Item.damage, 0, player.whoAmI);
            }

            player.GetModPlayer<MeteorDiskEquipped>().Equipped = true;
        }

        public override bool MagicPrefix()
        {
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.MeteoriteBar, 12)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    internal class MeteorDiskEquipped : ModPlayer
    {
        public bool Equipped;

        public override void ResetEffects()
        {
            Equipped = false;
        }
    }
}