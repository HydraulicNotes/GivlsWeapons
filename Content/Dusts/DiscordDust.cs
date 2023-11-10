using Terraria;
using Terraria.ModLoader;

namespace GivlsWeapons.Content.Dusts
{
    internal class DiscordDust : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = true;
            dust.rotation = Main.rand.NextFloat();
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.alpha += 15;

            Lighting.AddLight(dust.position, 0.9f, 0.5f, 1f);

            if(dust.alpha >= 140)
            {
                dust.active = false;
            }

            return false;
        }
    }
}
