using Terraria.ID;
using Terraria.GameContent.Creative;
using GivlsWeapons.Content.Dusts;
using System;
using GivlsWeapons.Common.Projectiles;

namespace GivlsWeapons.Content.Items.Weapons
{
    public class BoringWand : ModItem
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
            Item.shootSpeed = 15f;
        }
    }
    public class Pentacle : ModProjectile
    {
        private ref float Timer => ref Projectile.ai[0];
        private ref float HealingEnergy => ref Projectile.ai[1];
        private ref float TurnaroundCooldown => ref Projectile.ai[2]; //Prevents the projectile from canceling out turns by hitting two enemies close together
        const int TURNDELAY = 10;
        public override void SetStaticDefaults()
        {
            PVPHitDictionaries.onHurtFix[Type] = OnHitPlayerFixed;
        }
        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;

            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 180;
            Projectile.DamageType = DamageClass.Magic;

            Projectile.aiStyle = -1;
            Projectile.penetrate = 8;

            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 10;
        }

        public override void AI()
        {
            //Setup
            Player owner = Main.player[Projectile.owner];
            Timer++;

            //Prevent from getting stuck in tight groups or large enemies
            if (TurnaroundCooldown > 0) TurnaroundCooldown--;

            //Decelerate slightly over time
            Projectile.velocity *= 0.996f;

            float maxDetectRadius = 640f; // The maximum radius at which a projectile can detect a target
            float maxAngle = MathHelper.ToRadians(30f); // The maximum angle at which the projectile will detect a target
            float projSpeed = Projectile.velocity.Length(); // The speed at which the projectile moves towards the target

            //Home towards the owner if there's healing energy and the projectile isn't moving away from them, otherwise home towards the closest enemy it can reach
            if (HealingEnergy > 0 && Math.Abs(Projectile.Center.AngleTo(owner.Center) - Projectile.velocity.ToRotation()) < MathHelper.ToRadians(90f))
            {
                float targetAngle = Projectile.Center.AngleTo(owner.Center);
                Projectile.velocity = Projectile.velocity.ToRotation().AngleTowards(targetAngle, MathHelper.ToRadians(0.6f)).ToRotationVector2() * projSpeed;
            }
            else
            {
                // Trying to find target closest to the projectile
                Vector2 target = FindTarget(maxDetectRadius, maxAngle);
                if (target != Vector2.Zero)
                {
                    float targetAngle = Projectile.Center.AngleTo(target);
                    Projectile.velocity = Projectile.velocity.ToRotation().AngleTowards(targetAngle, MathHelper.ToRadians(0.6f)).ToRotationVector2() * projSpeed;
                }
            }
            //Spin
            Projectile.rotation += 0.35f * Projectile.direction;

            //Heal the owner if they touch the projectile
            if (Projectile.Colliding(Projectile.Hitbox, owner.Hitbox) && HealingEnergy > 0)
            {
                int healAmount = (int)HealingEnergy * 2;
                owner.statLife += healAmount;
                owner.HealEffect(healAmount, true);
                HealingEnergy = 0;
            }

            //Red dust and light
            if (Main.rand.NextBool(2))
            {
                int numToSpawn = Main.rand.Next(3);
                for (int i = 0; i < numToSpawn; i++)
                {
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<BoringDust>(), Projectile.velocity.X * 0.1f, Projectile.velocity.Y * 0.1f,
                        0, default, 1f);
                }
            }
            Lighting.AddLight(Projectile.Center, 0.75f, 0.0f, 0.0f);

            //Healing energy dust
            for (int i = 0; i < (int)HealingEnergy; i++)
            {
                Vector2 dustPos = Projectile.Center + (Projectile.rotation + i * MathF.Tau * i / 12).ToRotationVector2() * Projectile.width * 0.67f;
                Dust.NewDustPerfect(dustPos, DustID.CursedTorch, Scale: Projectile.scale);
            }


        }
        public Vector2 FindTarget(float maxDetectDistance, float maxDetectAngle)
        {
            Vector2 targPos = Vector2.Zero;

            // Using squared values in distance checks will let us skip square root calculations, drastically improving this method's speed.
            float sqrMaxDetectDistance = maxDetectDistance * maxDetectDistance;

            // Loop through all NPCs
            foreach (var target in Main.ActivePlayers)
            {
                if (target.InOpposingTeam(Main.player[Projectile.owner]) && target.whoAmI != Projectile.owner)
                {
                    // The DistanceSquared function returns a squared distance between 2 points, skipping relatively expensive square root calculations
                    float sqrDistanceToTarget = Vector2.DistanceSquared(target.Center, Projectile.Center);
                    float angleToTarget = Math.Abs(Projectile.Center.AngleTo(target.Center) - Projectile.velocity.ToRotation());

                    // Check if it is within the radius and angle requirements
                    if (sqrDistanceToTarget < sqrMaxDetectDistance && angleToTarget < maxDetectAngle)
                    {
                        sqrMaxDetectDistance = sqrDistanceToTarget;
                        targPos = target.Center;
                    }
                }
            }
            if(targPos != Vector2.Zero) return targPos;
            
            foreach (var target in Main.ActiveNPCs)
            {
                // Check if NPC able to be targeted. It means that NPC is
                // 1. active (alive)
                // 2. chaseable (e.g. not a cultist archer)
                // 3. max life bigger than 5 (e.g. not a critter)
                // 4. can take damage (e.g. moonlord core after all it's parts are downed)
                // 5. hostile (!friendly)
                // 6. not immortal (e.g. not a target dummy)
                if (target.CanBeChasedBy())
                {
                    // The DistanceSquared function returns a squared distance between 2 points, skipping relatively expensive square root calculations
                    float sqrDistanceToTarget = Vector2.DistanceSquared(target.Center, Projectile.Center);
                    float angleToTarget = Math.Abs(Projectile.Center.AngleTo(target.Center) - Projectile.velocity.ToRotation());

                    // Check if it is within the radius and angle requirements
                    if (sqrDistanceToTarget < sqrMaxDetectDistance && angleToTarget < maxDetectAngle)
                    {
                        sqrMaxDetectDistance = sqrDistanceToTarget;
                        targPos = target.Center;
                    }
                }
            }

            return targPos;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (TurnaroundCooldown <= 0)
            {
                Projectile.velocity *= -0.9f;
                TurnaroundCooldown = TURNDELAY;
            }
            Projectile.timeLeft = 180;
            if (target.type != NPCID.TargetDummy)
            {
                HealingEnergy++;
            }
            if (Projectile.penetrate == 0)
            {
                Projectile.damage = 0;
            }
            Projectile.netUpdate = true;
        }
        public static void OnHitPlayerFixed(Player target, Projectile source, Player.HurtInfo info)
        {
            if (source.ai[2] <= 0)
            {
                source.velocity *= -0.9f;
                source.ai[2] = TURNDELAY;
            }
            source.timeLeft = 180;
                source.ai[1]++;
            if (source.penetrate == 0)
            {
                source.damage = 0;
            }
            source.netUpdate = true;
        }
    }
}
