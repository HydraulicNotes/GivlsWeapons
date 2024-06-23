/* using GivlsWeapons.Content.Buffs;
using GivlsWeapons.Content.Items.Materials;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;

namespace GivlsWeapons.Content.Items.Weapons
{
    public class BalanceBullet : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 99;
        }
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;

            Item.value = Item.sellPrice(copper: 6);
            Item.maxStack = Item.CommonMaxStack;
            Item.consumable = true;
            Item.ammo = AmmoID.Bullet;
            Item.rare = ItemRarityID.LightRed;

            Item.damage = 15;
            Item.DamageType = DamageClass.Ranged;
            Item.knockBack = 3f;
            Item.shootSpeed = 7f;
            Item.shoot = ModContent.ProjectileType<BalanceBulletProjectile>();
        }
        public override void AddRecipes()
        {
            CreateRecipe(999)
                .AddIngredient(ItemID.MusketBall, 999)
                .AddIngredient<BalanceSoul>()
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
    public class BalanceBulletProjectile : ModProjectile
    {
        public int State
        {
            get => (int)Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }
        public int BulletType
        {
            get => (int)Projectile.ai[2];
            set => Projectile.ai[2] = value;
        }
        public enum Exploded
        {
            Not,

            /// <summary>
            /// Projectile timeleft is less than 6
            /// </summary>
            Exploding
        }
        public enum BulletColor
        {
            Black,

            /// <summary>
            /// Projectile timeleft is less than 6
            /// </summary>
            White
        }
        private bool Hit;
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.aiStyle = 1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.extraUpdates = 1;
            Projectile.timeLeft = 600;
            Projectile.alpha = 255; //fade in
            Projectile.light = 0.5f;
            Projectile.penetrate = -1;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;

            Projectile.aiStyle = ProjAIStyleID.Arrow;
            AIType = ProjectileID.Bullet; // Act exactly like default Bullet
        }
        public override void OnSpawn(IEntitySource source)
        {
            if (BulletType == (int)BulletColor.Black)
            {
                Projectile.NewProjectile(source, Projectile.Center, Projectile.velocity * 2f, Type, (int)(Projectile.damage * 0.5f), Projectile.knockBack, Projectile.owner, ai2: (float)BulletColor.White);
            }
        }
        public override void AI()
        {
            if (Hit && State == (int)Exploded.Not)
            {
                State = (int)Exploded.Exploding;
                Hit = false;
                Projectile.Resize(240, 240);
                Projectile.timeLeft = 3;
                Projectile.alpha = 255;
                Projectile.velocity = Vector2.Zero;
                Projectile.tileCollide = false;
                Projectile.localNPCHitCooldown = -1;
                Projectile.netUpdate = true;
                for(int i = 0; i < 20; i++)
                {
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.FireworksRGB, 0, 0, 0, Main.rand.NextBool() ? Color.White : Color.Black, 1);
                }
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (BulletType == (int)BulletColor.Black) Hit = true;
            else Projectile.Kill();
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (BulletType == 1f)
            {
                lightColor = Color.Black;
            }
            return true;
        }
    }
} */