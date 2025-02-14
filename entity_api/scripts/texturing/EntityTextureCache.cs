using System.Collections.Generic;
using UnityEngine;

public struct EntityTextureCache
{
    public LimbTextureCache textures;
    public Dictionary<string, TextureCutoutCache> limb_cutouts;

    public void textureEntity(PersonBehaviour behaviour)
    {
        foreach (var limb in behaviour.Limbs)
            textureLimb(limb);
    }
    public void textureLimb(LimbBehaviour limb)
    {
        Dictionary<string, TextureCutoutCache> limb_cutouts = this.limb_cutouts;
        
        SpriteRenderer renderer = limb.SkinMaterialHandler.renderer;
        Sprite original = renderer.sprite;
        var sprites = textures.createSprites(getCutout(), original);
        renderer.sprite = sprites.skin;
        renderer.material.SetTexture(ShaderProperties.Get("_FleshTex"), sprites.flesh);
        renderer.material.SetTexture(ShaderProperties.Get("_BoneTex"), sprites.bone);
        renderer.material.SetTexture(ShaderProperties.Get("_DamageTex"), sprites.damage);
        TextureCutoutCache getCutout()
        {
            string name = limb.name;

            if (!limb_cutouts.ContainsKey(name) && name.Contains("Front"))
                name = name.Remove(limb.name.Length - 5);
            
            if (limb_cutouts.ContainsKey(name))
            {
                return limb_cutouts[name];
            }
            
            return EntityAPI.DefaultTextureCutout;
        }
    }
}