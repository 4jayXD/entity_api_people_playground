using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public struct LimbTextureCache
{
    public List<Texture2D> skins;
    public Texture2D flesh, bone, damage;

    public OutputCache createSprites(TextureCutoutCache cutoutCache, Sprite original)
    {
        Sprite create(Texture2D texture)
        {
            return Sprite.Create(texture, cutoutCache.texture_cutout, cutoutCache.pivot, original.pixelsPerUnit, 0u,
                SpriteMeshType.FullRect, original.border, false);
        }
        Sprite createNull(Texture2D texture)
        {
            TextureCutoutCache cutout = new TextureCutoutCache()
            {
                texture_cutout = new Rect(0, 0, original.rect.width, original.rect.height),
                pivot = new Vector2(.5f, .5f)
            };
            
            return Sprite.Create(texture, cutout.texture_cutout, cutout.pivot, original.pixelsPerUnit, 0u,
                SpriteMeshType.FullRect, original.border, false);
        }
        
        return new OutputCache() {
            skin = skins.Count > 0 ? create(skins[new Random().Next(skins.Count)]) : createNull(EntityAPI.ErrorTextureCache.skins[0]),
            flesh = (bool)flesh ? flesh : EntityAPI.ErrorTextureCache.flesh,
            bone = (bool)bone ? bone : EntityAPI.ErrorTextureCache.bone,
            damage = (bool)damage ? damage : EntityAPI.ErrorTextureCache.damage
        };
    }
    public struct OutputCache
    {
        public Sprite skin;
        public Texture2D flesh, bone, damage;
    }
}