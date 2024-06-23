using System.Collections.Generic;
using GivlsWeapons.Helpers;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.GameContent.Creative;
using System.Linq;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using System;



namespace GivlsWeapons.Content.Items.Weapons
{
    public class BlackandWhiteGun : ModItem
    {
        private int Combo = 0;
        const int ComboLength = 6;
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
            //Item.reuseDelay = 4;
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
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (Combo == ComboLength - 1)
            {
                Projectile.NewProjectile(source, position, velocity * 0.7f, ModContent.ProjectileType<YinYang>(), damage, knockback, player.whoAmI);
            }
            Combo = (Combo + 1) % ComboLength;
            return true;
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
    public class YinYang : ModProjectile
    {
        private ref float Timer => ref Projectile.ai[0];
        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.timeLeft = 3000;
            Projectile.alpha = 255; //fade in
        }
        public override void AI()
        {
            Projectile.rotation += MathHelper.ToRadians(2f);
            if (Projectile.alpha > 0)
            {
                Projectile.alpha -= 6;
            }
            Projectile.velocity *= 0.9f;
            if (Projectile.velocity.LengthSquared() < 10 && Main.myPlayer == Projectile.owner)
            {
                Player owner = Main.player[Projectile.owner];
                foreach (var p in Main.projectile.Take(Main.maxProjectiles).Where(x => x.active && !owner.InOpposingTeam(Main.player[x.owner]) && x.aiStyle == ProjAIStyleID.Arrow && x.Colliding(x.Hitbox, Projectile.Hitbox)))
                {
                    /* for (int i = 0; i < 3; i++)
                    {
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Main.rand.NextVector2CircularEdge(2, 2), ModContent.ProjectileType<YinYangBullet>(), Projectile.damage, Projectile.knockBack, Projectile.owner, ai0: Projectile.identity, ai1: 0f);
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Main.rand.NextVector2CircularEdge(2, 2), ModContent.ProjectileType<YinYangBullet>(), Projectile.damage, Projectile.knockBack, Projectile.owner, ai0: Projectile.identity, ai1: 1f);
                    } */
                    int bulletCount = 0;
                    for (int i = 0; i < Main.projectile.Length; i++)
                    {
                        if (bulletCount >= 6 || bulletCount >= owner.ownedProjectileCounts[Type] - 1) break;
                        Projectile proj = Main.projectile[i];
                        if (proj.active && proj.type == Type && proj.whoAmI != Projectile.whoAmI && !owner.InOpposingTeam(Main.player[proj.owner]))
                        {
                            SpawnBullet(bulletCount % 2 == 0, proj.Center);
                            bulletCount++;
                        }
                    }
                    for (int i = 0; i < Main.npc.Length; i++)
                    {
                        if (bulletCount >= 6) break;
                        NPC target = Main.npc[i];
                        if (target.CanBeChasedBy(this))
                        {
                            SpawnBullet(bulletCount % 2 == 0, target.Center);
                            bulletCount++;
                        }
                    }
                    while (bulletCount < 6)
                    {
                        SpawnBullet(bulletCount % 2 == 0, Projectile.Center + Main.rand.NextVector2CircularEdge(1, 1));
                        bulletCount++;
                    }
                    Projectile.Kill();
                    break;
                }
            }
            else Timer++;
        }
        public void SpawnBullet(bool isBlack, Vector2 targPos)
        {
            Vector2 velocity = Vector2.One;
            if (targPos == Projectile.Center)
            {
                targPos = Projectile.Center + Main.rand.NextVector2CircularEdge(1, 1);
            }
            velocity = Vector2.Normalize(targPos - Projectile.Center) * 2;

            Projectile.NewProjectile(Projectile.GetSource_FromThis(),
            Projectile.Center,
            velocity,
            ModContent.ProjectileType<YinYangBullet>(),
            Projectile.damage,
            Projectile.knockBack,
            Projectile.owner,
            ai0: isBlack ? 0f : 1f);
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.velocity *= -0.7f; //Add actual bounce physics
            return false;
        }
        public override bool? CanDamage()
        {
            return false;
        }
    }
    public class YinYangBullet : ModProjectile, IDrawPrimitive
    {
        private bool disabled = false;
        const int LENGTH = 100;
        private List<Vector2> cache;
        private Trail trail;
        private Trail trail2;

        private float trailWidth = 0.1f;
        public int BulletType
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        public enum BulletColors
        {
            Black,
            White
        }
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 0;
            ProjectileID.Sets.TrailCacheLength[Type] = 30; //Remember to increase base velocity
        }
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.timeLeft = 6000;

            Projectile.aiStyle = ProjAIStyleID.Arrow;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.extraUpdates = 100;
            Projectile.penetrate = -1;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;

            AIType = ProjectileID.Bullet;
        }
        public override void AI()
        {
            if (Projectile.numUpdates == Projectile.extraUpdates - 1)
            {
                ManageCaches();
                ManageTrail();
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if(!disabled) Helper.DisableProjectile(Projectile);
            ManageCaches();
            disabled = true;
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if(!disabled) Helper.DisableProjectile(Projectile);
            ManageCaches();
            disabled = true;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if(!disabled) Helper.DisableProjectile(Projectile);
            ManageCaches();
            disabled = true;
            return false;
        }
        public override bool PreKill(int timeLeft)
        {
            if (timeLeft <= 0) return true;
            else
            {
                Helper.DisableProjectile(Projectile);
                return false;
            }
        }
        private void ManageCaches()
        {
            if (cache == null)
            {
                cache = new List<Vector2>();

                for (int i = 0; i < 50; i++)
                {
                    cache.Add(Projectile.Center);
                }
            }

            if (Projectile.oldPos[0] != Vector2.Zero)
                cache.Add(Projectile.oldPos[0] + new Vector2(Projectile.width / 2, Projectile.height / 2));

            while (cache.Count > 50)
            {
                cache.RemoveAt(0);
            }
        }

        private void ManageTrail()
        {
            trail ??= new Trail(Main.instance.GraphicsDevice, 50, new RoundedTip(12), factor => (10 + factor * 25) * trailWidth, factor => new Color(120, 20 + (int)(100 * factor.X), 255) * factor.X);

            trail.Positions = cache.ToArray();

            trail2 ??= new Trail(Main.instance.GraphicsDevice, 50, new RoundedTip(6), factor => (80 + 0 + factor * 0) * trailWidth, factor => new Color(100, 20 + (int)(60 * factor.X), 255) * factor.X * 0.15f);

            trail2.Positions = cache.ToArray();

            if (Projectile.velocity.Length() > 1)
            {
                trail.NextPosition = Projectile.Center + Projectile.velocity;
                trail2.NextPosition = Projectile.Center + Projectile.velocity;
            }
        }

        public void DrawPrimitives()
        {
            Effect effect = Filters.Scene["Fuck"].GetShader().Shader;

            var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.TransformationMatrix;
            var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.02f);
            effect.Parameters["repeats"].SetValue(8f);
            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("GivlsWeapons/Assets/Textures/GlowTrail").Value);
            effect.Parameters["sampleTexture2"].SetValue(ModContent.Request<Texture2D>("GivlsWeapons/Assets/Textures/DatsuzeiFlameMap2").Value);

            trail?.Render(effect);

            effect.Parameters["sampleTexture2"].SetValue(TextureAssets.MagicPixel.Value);

            trail2?.Render(effect);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }
    }
}
