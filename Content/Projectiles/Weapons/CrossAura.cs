using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace GivlsWeapons.Content.Projectiles.Weapons
{
    internal class CrossAura : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 364; //The size is massive, because this one projectile is the entire area of the Cross' attack
            Projectile.height = 192;

            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 10;

            Projectile.DamageType = DamageClass.Magic;

            Projectile.aiStyle = -1;

            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true; // Uses local immunity frames
            Projectile.localNPCHitCooldown = -1;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.ai[0] = Main.player[Projectile.owner].direction;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.HitDirectionOverride = (int)Projectile.ai[0];
        }

        public override bool? CanCutTiles()
        {
            return false;
        }
    }
}
