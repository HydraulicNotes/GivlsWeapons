using Terraria.GameContent.Creative;
using Terraria.Audio;
using System;

namespace GivlsWeapons.Content.Items.Accessories
{
    public class ChlorophyteIdol : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 42;
            Item.height = 38;
            Item.value = Item.sellPrice(gold: 7, silver: 50);
            Item.accessory = true;
            Item.rare = ItemRarityID.Lime;

            Item.DamageType = DamageClass.Melee;
            Item.damage = 80;
            Item.knockBack = 8f;
        }
        public override bool MeleePrefix()
        {
            return false;
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<ChlorophyteIdolPlayer>().AccessoryEquipped = true;
            player.GetModPlayer<ChlorophyteIdolPlayer>().Accessory = Item;
        }
    }

    public class ChlorophyteIdolPlayer : ModPlayer
    {
        public bool AccessoryEquipped = false;
        public Item Accessory;
        private int CooldownTimer = DURATION;
        const int DURATION = 54; //The time in ticks it takes for a crystal to be able to be fired. I decided to experiment with putting non-round numbers in things to see how it felt
        public override void PreUpdate()
        {
            if (Main.myPlayer == Player.whoAmI && AccessoryEquipped && CooldownTimer < 2 * DURATION)
            {
                CooldownTimer++;
            }
        }
        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (hit.DamageType == DamageClass.Melee || hit.DamageType == DamageClass.MeleeNoSpeed)
            {
                TrySpawningCrystal(target);
            }
        }
        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        { //Check for true melee projectiles. If the damage type is melee, and either is their heldproj, has jts center within 8 pixels of theirs, or is of a type that is arguably true melee, it will activate
            if ((hit.DamageType == DamageClass.Melee || hit.DamageType == DamageClass.MeleeNoSpeed) &&
            (Player.heldProj == proj.whoAmI || proj.Distance(Player.Center) <= 64f || proj.aiStyle == ProjAIStyleID.Harpoon || proj.aiStyle == ProjAIStyleID.Flairon))
            { //Some edge cases might get counted or not counted incorrectly, but this should work 99% of the time, and for everything vanilla or added by this mod
                TrySpawningCrystal(target);
            }
        }
        public override void ResetEffects()
        {
            AccessoryEquipped = false;
            Accessory = null;
        }
        void TrySpawningCrystal(NPC target)
        {
            if (AccessoryEquipped && CooldownTimer >= DURATION)
            {
                Vector2 spawnVel = target.Center.AngleTo(Player.Center).ToRotationVector2() * target.Center.Distance(Player.Center) * 0.064f;
                Projectile.NewProjectile(Player.GetSource_Accessory(Accessory), target.Center, spawnVel, ModContent.ProjectileType<AltLeafCrystal>(), 100, 10f, Player.whoAmI);
                SoundEngine.PlaySound(SoundID.Item8);
                for (int i = 0; i < 15; i++)
                {
                    Dust.NewDustPerfect(target.Center, DustID.ChlorophyteWeapon, Main.rand.NextFloat(0, MathF.Tau).ToRotationVector2(), Scale: 0.7f);
                }
                if (CooldownTimer >= DURATION * 2)
                {
                    CooldownTimer = 0;
                }
                else
                {
                    CooldownTimer -= DURATION;
                }
            }
        }
    }
    public class AltLeafCrystal : ModProjectile
    {
        const int COOLDOWN = 40; //Same as regular leaf crystal's cooldown for passive targeting
        private ref float ShotTimer => ref Projectile.ai[0];
        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 42;

            Projectile.DamageType = DamageClass.Melee;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 180; //timeLeft and DURATION determine the amount of shots that can be fired over its lifetime. 180 = 4x + 20. Since it won't always have a target, it should have some extra time so it won't miss potential shots
            Projectile.noEnchantmentVisuals = true;
        }

        public override void AI() //This doesn't include the leaf crystal's beam shots in response to attacks, but maybe it should
        {
            Projectile.velocity *= 0.9f; //deceleration
            if (ShotTimer == 0)
            {
                NPC target = null;
                float finalDistance = 700f; //Maximum distance of 700 pixels or 43.75 tiles, same as the real leaf crystal's passive targeting
                for (int i = 0; i < Main.npc.Length; i++)
                {
                    NPC potentialTarget = Main.npc[i];
                    float targDistance = potentialTarget.Center.Distance(Projectile.Center);
                    if (potentialTarget.CanBeChasedBy(this) && targDistance <= finalDistance && Collision.CanHitLine(Projectile.Center, 0, 0, potentialTarget.Center, 0, 0))
                    {
                        finalDistance = targDistance;
                        target = potentialTarget;
                    }
                    if (target != null)
                    {
                        Player owner = Main.player[Projectile.owner];
                        ShotTimer = COOLDOWN;
                        Vector2 velocityVector = target.Center - Projectile.Center;
                        //Unsure what the purpose of this step is, but it seemed to be important when I decompiled the game. It's the same for both types of targeting.
                        Utils.ChaseResults chaseResults = Utils.GetChaseResults(Projectile.Center, 2160f, target.Center, target.velocity);//chase target with speed of 2160
                        if (chaseResults.InterceptionHappens && chaseResults.InterceptionTime <= 180f) //if intercepted quickly enough
                        {
                            velocityVector = chaseResults.ChaserVelocity / 180f; //set velocity to chaser velocity divided by 180, which is 12
                        }
                        Projectile.NewProjectile(Projectile.InheritSource(Projectile), new Vector2(Projectile.Center.X - 4f, Projectile.Center.Y), velocityVector, ProjectileID.CrystalLeafShot, (int)owner.GetDamage(DamageClass.Melee).ApplyTo(80f), owner.GetKnockback(DamageClass.Melee).ApplyTo(8f), Projectile.owner);
                        break;
                    }
                }
            }
            else
            {
                ShotTimer--;
            }
            //Make the projectile bob slightly up and down. Not done in velocity cause I'm lazy.
            Projectile.position += new Vector2(0, MathF.Sin(Projectile.timeLeft * 0.03f) * 0.15f);

            if (Projectile.timeLeft > COOLDOWN) //Fade out slowly, and then become fully opaque after shooting
            {
                Projectile.alpha = COOLDOWN - (int)ShotTimer;
            }
            else //Fade out near the end of the lifetime. Checks if alpha is already higher
            {
                int newAlpha = 255 - (int)(255 * (Projectile.timeLeft * 0.02f));
                if (newAlpha > Projectile.alpha) Projectile.alpha = newAlpha;
            }
        }
    }
}