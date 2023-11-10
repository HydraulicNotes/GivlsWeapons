/* using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using System;

namespace GivlsWeapons.Development
{
    internal class ProjectileAIViewer : GlobalProjectile
    {
        public override void AI(Projectile projectile)
        {
            if(projectile.type == ProjectileID.Flamelash)
            {
                Main.NewText(projectile.rotation);
            }
        }
    }
} */

//Debug class for checking information about vanilla projectiles