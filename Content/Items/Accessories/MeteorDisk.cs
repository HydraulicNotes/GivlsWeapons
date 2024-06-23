using Terraria.GameContent.Creative;
using System.Linq;
using Terraria.ID;

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
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player owner = Main.player[Projectile.owner];
            int manaToAdd = 1 + damageDone * 2;
            owner.ManaEffect(manaToAdd);
            owner.statMana += manaToAdd;

            target.AddBuff(BuffID.OnFire, 180);
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            Player owner = Main.player[Projectile.owner];
            int manaToAdd = 1 + (int)(info.Damage * 0.2f);
            owner.ManaEffect(manaToAdd);
            owner.statMana += manaToAdd;

            target.AddBuff(BuffID.OnFire, 60);
        }
//Due to a bug in TML, this can't be fixed right now. ModifyHitPlayer runs only on the owner's client, even though the target client is the one that actually has authority over the hit
/*         public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
        { //Prevent from dealing knockback to players
            modifiers.Knockback *= 0;
        } */
    }
}