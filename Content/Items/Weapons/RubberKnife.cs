using Terraria.GameContent.Creative;
using Terraria.ID;

namespace GivlsWeapons.Content.Items.Weapons
{
    public class RubberKnife : ModItem
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

/*         public override bool? UseItem(Player player) //Works every time, as opposed to Instakill Curse which doesn't always work on other clients
        {
            if (Main.myPlayer == player.whoAmI && player.HasBuff<InstaKillCurse>())
            {
                PlayerDeathReason deathReason = PlayerDeathReason.ByCustomReason("Funny rubber knife");
                player.KillMe(deathReason, 69, player.direction, false);
            }
            return true;
        } */
    }
    public class RubberKnifeProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;

            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.ownerHitCheck = true;
            Projectile.extraUpdates = 1;
            Projectile.timeLeft = 300;
            Projectile.usesLocalNPCImmunity = true; // Uses local immunity frames
            Projectile.localNPCHitCooldown = -1;

            Projectile.aiStyle = ProjAIStyleID.ShortSword;
        }

        public override void AI()
        {
            base.AI();
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2 - MathHelper.PiOver4 * Projectile.spriteDirection;

            int halfProjWidth = Projectile.width / 2;
            int halfProjHeight = Projectile.height / 2;

            DrawOriginOffsetX = 0;
            DrawOffsetX = -(16 - halfProjWidth);
            DrawOriginOffsetY = -(16 - halfProjHeight);
        }
    }
}
