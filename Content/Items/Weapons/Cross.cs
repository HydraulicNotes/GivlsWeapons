using Terraria.ID;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using GivlsWeapons.Common.Projectiles;

namespace GivlsWeapons.Content.Items.Weapons
{
    public class Cross : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
            ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.CrossNecklace;
            ItemID.Sets.ShimmerTransformToItem[ItemID.CrossNecklace] = Type; //Shimmer back and forth with Cross Necklace
        }
        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 12;

            Item.useStyle = ItemUseStyleID.Shoot;
            Item.autoReuse = true;
            Item.useTurn = false;

            Item.value = 150000;
            Item.rare = ItemRarityID.LightRed;

            Item.DamageType = DamageClass.Magic;
            Item.noMelee = true;
            Item.mana = 6;
            Item.damage = 45;
            Item.knockBack = 1.2f;

            Item.useTime = 10;
            Item.useAnimation = 10;

            Item.shoot = ModContent.ProjectileType<CrossAura>();
            Item.shootSpeed = 0f;
        }
        public override Vector2? HoldoutOffset()
        {
            return new Vector2(1, -6);
        }
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            position.X += player.direction * 192;
        }
    }
    public class CrossAura : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            PVPHitDictionaries.modifyHurtFix[Type] = ModifyHitPlayerFixed;
        }
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
        public static void ModifyHitPlayerFixed(Player target, Projectile source, ref Player.HurtModifiers modifiers)
        {
            modifiers.HitDirectionOverride = (int)source.ai[0];
        }
        public override bool? CanCutTiles()
        {
            return false;
        }
    }
    public class CrossEffects : ModPlayer
    {
        public override void SetControls()
        {
            if (Player.HeldItem.ModItem is Cross  && Player.ItemAnimationActive)
            {
                Player.controlDown = false;
                Player.controlJump = false;
                Player.controlLeft = false;
                Player.controlRight = false;
                Player.controlUp = false;
                Player.controlHook = false;
                Player.controlUseTile = false;
                Player.controlThrow = false;
            }
        }
    }
}