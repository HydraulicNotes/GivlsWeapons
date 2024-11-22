using Terraria.GameContent.Creative;
using System.Linq;
using GivlsWeapons.Common.Projectiles;
using System;

namespace GivlsWeapons.Content.Items.Accessories
{
    public class MeteorDisk : ModItem
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

    public class MeteorDiskEquipped : ModPlayer
    {
        public bool Equipped;

        public override void ResetEffects()
        {
            Equipped = false;
        }
    }
    public class MeteorDiskAura : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 120;
            Projectile.height = 120;

            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 10;

            Projectile.DamageType = DamageClass.Magic;
            Projectile.aiStyle = -1;
            Projectile.penetrate = -1;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 20;

            Projectile.alpha = 200;
        }
        public override void SetStaticDefaults()
        {
            PVPHitDictionaries.onHurtFix[Type] = OnHitPlayerFixed;
            PVPHitDictionaries.modifyHurtFix[Type] = ModifyHitPlayerFixed;
        }
        public override bool? CanCutTiles()
        {
            return false;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            Projectile.Center = new Vector2(owner.Center.X, owner.Bottom.Y + owner.gfxOffY + 65);

            if (owner.GetModPlayer<MeteorDiskEquipped>().Equipped && !owner.dead)
            {
                Projectile.timeLeft = 3;
            }

            for(int i = 0; i < Main.rand.Next(8); i++)
            {
                Vector2 pos1 = Projectile.Center + Easings.OutQuart(Main.rand.NextFloat()) * (Projectile.width / 2) * Main.rand.NextFloat(MathF.Tau).ToRotationVector2();
                Dust.NewDustPerfect(pos1, DustID.Torch, (pos1.AngleTo(Projectile.Center) + MathHelper.ToRadians(Main.rand.NextBool() ? 90f : -90f)).ToRotationVector2() + owner.velocity + new Vector2(0, -1f));
                Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(Projectile.width / 2, Projectile.height / 2), DustID.Torch, new Vector2(0f, -1.5f) + owner.velocity, Scale: 0.6f);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player owner = Main.player[Projectile.owner];
            int manaToAdd = damageDone * 2;
            owner.ManaEffect(manaToAdd);
            owner.statMana += manaToAdd;

            target.AddBuff(BuffID.OnFire, 180);
        }
        public static void OnHitPlayerFixed(Player player, Projectile source, Player.HurtInfo info)
        {
            Player owner = Main.player[source.owner];
            int manaToAdd = info.Damage * 2;
            owner.ManaEffect(manaToAdd);
            owner.statMana += manaToAdd;

            player.AddBuff(BuffID.OnFire, 180);
        }
        public static void ModifyHitPlayerFixed(Player player, Projectile source, ref Player.HurtModifiers modifiers)
        {
            modifiers.HitDirectionOverride = Main.player[source.owner].direction;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }
    }
}