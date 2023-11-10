using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using GivlsWeapons.Content.Projectiles.Weapons;
using Terraria.GameContent.Creative;

namespace GivlsWeapons.Content.Items.Weapons
{
    internal class BoringWand : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
            Item.staff[Type] = true;
        }
        public override void SetDefaults()
        {
            Item.width = 50;
            Item.height = 50;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.autoReuse = true;

            Item.DamageType = DamageClass.Magic;
            Item.noMelee = true;
            Item.mana = 12;
            Item.damage = 60;
            Item.knockBack = 1.5f;

            Item.value = 75000;
            Item.rare = ItemRarityID.Pink;

            Item.useTime = 37;
            Item.useAnimation = 37;

            Item.UseSound = SoundID.Item8;

            Item.shoot = ModContent.ProjectileType<Pentacle>();
            Item.shootSpeed = 1f;
        }
    }
}
