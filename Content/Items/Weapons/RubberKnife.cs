﻿using Terraria;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
using Terraria.ID;
using GivlsWeapons.Content.Projectiles.Weapons;

namespace GivlsWeapons.Content.Items.Weapons
{
    internal class RubberKnife : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Rubber Knife");
            // Tooltip.SetDefault("Krav Maga!");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.value = Item.sellPrice(silver: 4, copper: 50);

            Item.useTime = 12;
            Item.useAnimation = 12;
            Item.useStyle = ItemUseStyleID.Rapier;
            Item.autoReuse = false;

            Item.UseSound = SoundID.Item1;

            Item.DamageType = DamageClass.Melee;
            Item.damage = 1;
            Item.knockBack = 1f;
            Item.crit = 4;

            Item.rare = ItemRarityID.Master;

            Item.noUseGraphic = true;
            Item.noMelee = true;

            Item.shootSpeed = 1.8f;
            Item.shoot = ModContent.ProjectileType<RubberKnifeProjectile>();
        }
    }
}
