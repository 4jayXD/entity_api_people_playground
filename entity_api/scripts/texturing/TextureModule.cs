using System.Collections.Generic;
using UnityEngine;

public partial struct EntityAPI
{
    public static TextureCutoutCache DefaultTextureCutout;
    public static LimbTextureCache ErrorTextureCache;
    private static Dictionary<string, LimbTextureCache> LoadedTextures = new Dictionary<string, LimbTextureCache>();
    private static Dictionary<string, Dictionary<string, TextureCutoutCache>> LoadedPresets = new Dictionary<string, Dictionary<string, TextureCutoutCache>>();
    private static Dictionary<string, EntityTextureCache> LoadedEntityTextures = new Dictionary<string, EntityTextureCache>();

    public static void AddTextures(string key, LimbTextureCache cache)
    {
        if (LoadedTextures.ContainsKey(key))
        {
            LogError("Texturing", "Cannot create duplicate texture keys.");
            return;
        }
        
        LoadedTextures.Add(key, cache);
    }
    public static void AddCutoutPreset(string key, Dictionary<string, TextureCutoutCache> cache)
    {
        if (LoadedPresets.ContainsKey(key))
        {
            LogError("Texturing", "Cannot create duplicate preset keys.");
            return;
        }
        
        LoadedPresets.Add(key, cache);
    }
    public static void AddEntityTexture(string key, EntityTextureCache cache)
    {
        if (LoadedEntityTextures.ContainsKey(key))
        {
            LogError("Texturing", "Cannot create duplicate entity texture keys.");
            return;
        }
        LoadedEntityTextures.Add(key, cache);
    }
    public static void AddEntityTexture(string key, string textures, string cutout)
    {
        if (!LoadedTextures.ContainsKey(textures) || !LoadedPresets.ContainsKey(cutout))
            return;
        
        LoadedEntityTextures.Add(key, new EntityTextureCache() {
            textures = LoadedTextures[textures],
            limb_cutouts = LoadedPresets[cutout]
        });
    }
    
    public static Dictionary<string, TextureCutoutCache> getCutoutPreset(string key) =>
        LoadedPresets.ContainsKey(key) ? LoadedPresets[key] : LoadedPresets["Human"];
    public static LimbTextureCache getTextureCache(string key) =>
        LoadedTextures.ContainsKey(key) ? LoadedTextures[key] : ErrorTextureCache;
    
    public static LimbTextureCache CreateTextureCache(string original, List<Texture2D> skins, Texture2D flesh = null, Texture2D bone = null, Texture2D damage = null)
    {
        LimbTextureCache originalcache = LoadedTextures.ContainsKey(original) ? LoadedTextures[original] : ErrorTextureCache;

        return new LimbTextureCache()
        {
            skins = skins,
            flesh = (bool)flesh ? flesh : originalcache.flesh,
            bone = (bool)bone ? bone : originalcache.bone,
            damage = (bool)damage ? damage : originalcache.damage
        };
    }
    public static LimbTextureCache CreateTextureCache(string original, Texture2D skin, Texture2D flesh = null, Texture2D bone = null, Texture2D damage = null) =>
        CreateTextureCache(original, new List<Texture2D>() { skin }, flesh, bone, damage);
    
    public static void textureEntity(string key, GameObject entity)
    {
        if (!LoadedEntityTextures.ContainsKey(key) || !entity.HasComponent<PersonBehaviour>())
            return;
        
        LoadedEntityTextures[key].textureEntity(entity.GetComponent<PersonBehaviour>());
    }
    public static void textureLimb(string key, LimbBehaviour limb)
    {
        if (!LoadedEntityTextures.ContainsKey(key))
            return;
        
        LoadedEntityTextures[key].textureLimb(limb);
    }
    
    private static void Load_TextureModule()
    {
        DefaultTextureCutout = new TextureCutoutCache() {
            texture_cutout = new Rect(0, 0, 10, 10),
            pivot = new Vector2(.5f, .5f)
        };
        ErrorTextureCache = new LimbTextureCache() {
            skins =  new List<Texture2D>() {ModAPI.LoadTexture("entity_api/assets/skin.png")},
            flesh = ModAPI.LoadTexture("entity_api/assets/flesh.png"),
            bone = ModAPI.LoadTexture("entity_api/assets/bone.png"),
            damage = ModAPI.LoadTexture("entity_api/assets/damage.png")
        };
        AddTextures("HumanAllColored", new LimbTextureCache() {
            skins = new List<Texture2D>() {ModAPI.LoadTexture("entity_api/assets/templates/human/all/skin_colored.png")},
            flesh = ModAPI.LoadTexture("entity_api/assets/templates/human/all/flesh.png"),
            bone = ModAPI.LoadTexture("entity_api/assets/templates/human/all/bone.png"),
            damage = ModAPI.LoadTexture("entity_api/assets/templates/human/all/damage.png"),
        });
        AddTextures("HumanAll", new LimbTextureCache() {
            skins = new List<Texture2D>() {ModAPI.LoadTexture("entity_api/assets/templates/human/all/skin.png")},
            flesh = ModAPI.LoadTexture("entity_api/assets/templates/human/all/flesh.png"),
            bone = ModAPI.LoadTexture("entity_api/assets/templates/human/all/bone.png"),
            damage = ModAPI.LoadTexture("entity_api/assets/templates/human/all/damage.png"),
        });
        
        
        AddCutoutPreset("Human", new Dictionary<string, TextureCutoutCache>() {

            {
                "Head",
                new TextureCutoutCache()
                {
                    texture_cutout = new Rect(6, 68, 11, 11),
                    pivot = DefaultTextureCutout.pivot
                }
            },
            {
                "UpperBody",
                new TextureCutoutCache()
                {
                    texture_cutout = new Rect(7, 58, 9, 9),
                    pivot = DefaultTextureCutout.pivot
                }
            },
            {
                "MiddleBody",
                new TextureCutoutCache()
                {
                    texture_cutout = new Rect(7, 48, 9, 9),
                    pivot = DefaultTextureCutout.pivot
                }
            },
            {
                "LowerBody",
                new TextureCutoutCache()
                {
                    texture_cutout = new Rect(7, 35, 9, 12),
                    pivot = DefaultTextureCutout.pivot
                }
            },
            {
                "UpperArm",
                new TextureCutoutCache()
                {
                    texture_cutout = new Rect(0, 54, 5, 13),
                    pivot = DefaultTextureCutout.pivot
                }
            },
            {
                "LowerArm",
                new TextureCutoutCache()
                {
                    texture_cutout = new Rect(0, 37, 5, 16),
                    pivot = DefaultTextureCutout.pivot
                }
            },
            {
                "UpperLeg",
                new TextureCutoutCache()
                {
                    texture_cutout = new Rect(9, 18, 5, 16),
                    pivot = DefaultTextureCutout.pivot
                }
            },
            {
                "LowerLeg",
                new TextureCutoutCache()
                {
                    texture_cutout = new Rect(9, 3, 5, 14),
                    pivot = DefaultTextureCutout.pivot
                }
            },
            {
                "Foot",
                new TextureCutoutCache()
                {
                    texture_cutout = new Rect(9, 0, 9, 3),
                    pivot = DefaultTextureCutout.pivot
                }
            }
        });
        AddCutoutPreset("HumanAll", new Dictionary<string, TextureCutoutCache>() {

            {
                "Head",
                new TextureCutoutCache()
                {
                    texture_cutout = new Rect(0, 68, 11, 11),
                    pivot = DefaultTextureCutout.pivot
                }
            },
            {
                "UpperBody",
                new TextureCutoutCache()
                {
                    texture_cutout = new Rect(1, 58, 9, 9),
                    pivot = DefaultTextureCutout.pivot
                }
            },
            {
                "MiddleBody",
                new TextureCutoutCache()
                {
                    texture_cutout = new Rect(1, 48, 9, 9),
                    pivot = DefaultTextureCutout.pivot
                }
            },
            {
                "LowerBody",
                new TextureCutoutCache()
                {
                    texture_cutout = new Rect(1, 35, 9, 12),
                    pivot = DefaultTextureCutout.pivot
                }
            },
            {
                "UpperArm",
                new TextureCutoutCache()
                {
                    texture_cutout = new Rect(17, 54, 5, 13),
                    pivot = DefaultTextureCutout.pivot
                }
            },
            {
                "LowerArm",
                new TextureCutoutCache()
                {
                    texture_cutout = new Rect(17, 37, 5, 16),
                    pivot = DefaultTextureCutout.pivot
                }
            },
            {
                "UpperLeg",
                new TextureCutoutCache()
                {
                    texture_cutout = new Rect(13, 18, 5, 16),
                    pivot = DefaultTextureCutout.pivot
                }
            },
            {
                "LowerLeg",
                new TextureCutoutCache()
                {
                    texture_cutout = new Rect(13, 3, 5, 14),
                    pivot = DefaultTextureCutout.pivot
                }
            },
            {
                "Foot",
                new TextureCutoutCache()
                {
                    texture_cutout = new Rect(13, 0, 9, 3),
                    pivot = DefaultTextureCutout.pivot
                }
            },
            
            {
                "UpperArmFront",
                new TextureCutoutCache()
                {
                    texture_cutout = new Rect(11, 54, 5, 13),
                    pivot = DefaultTextureCutout.pivot
                }
            },
            {
                "LowerArmFront",
                new TextureCutoutCache()
                {
                    texture_cutout = new Rect(11, 37, 5, 16),
                    pivot = DefaultTextureCutout.pivot
                }
            },
            {
                "UpperLegFront",
                new TextureCutoutCache()
                {
                    texture_cutout = new Rect(3, 18, 5, 16),
                    pivot = DefaultTextureCutout.pivot
                }
            },
            {
                "LowerLegFront",
                new TextureCutoutCache()
                {
                    texture_cutout = new Rect(3, 3, 5, 14),
                    pivot = DefaultTextureCutout.pivot
                }
            },
            {
                "FootFront",
                new TextureCutoutCache()
                {
                    texture_cutout = new Rect(3, 0, 9, 3),
                    pivot = DefaultTextureCutout.pivot
                }
            }
        });
        AddCutoutPreset("HumanAllArms", new Dictionary<string, TextureCutoutCache>() {

            {
                "Head",
                new TextureCutoutCache()
                {
                    texture_cutout = new Rect(0, 68, 11, 11),
                    pivot = DefaultTextureCutout.pivot
                }
            },
            {
                "UpperBody",
                new TextureCutoutCache()
                {
                    texture_cutout = new Rect(1, 58, 9, 9),
                    pivot = DefaultTextureCutout.pivot
                }
            },
            {
                "MiddleBody",
                new TextureCutoutCache()
                {
                    texture_cutout = new Rect(1, 48, 9, 9),
                    pivot = DefaultTextureCutout.pivot
                }
            },
            {
                "LowerBody",
                new TextureCutoutCache()
                {
                    texture_cutout = new Rect(1, 35, 9, 12),
                    pivot = DefaultTextureCutout.pivot
                }
            },
            {
                "UpperArm",
                new TextureCutoutCache()
                {
                    texture_cutout = new Rect(17, 54, 5, 13),
                    pivot = DefaultTextureCutout.pivot
                }
            },
            {
                "LowerArm",
                new TextureCutoutCache()
                {
                    texture_cutout = new Rect(17, 37, 5, 16),
                    pivot = DefaultTextureCutout.pivot
                }
            },
            {
                "UpperLeg",
                new TextureCutoutCache()
                {
                    texture_cutout = new Rect(3, 18, 5, 16),
                    pivot = DefaultTextureCutout.pivot
                }
            },
            {
                "LowerLeg",
                new TextureCutoutCache()
                {
                    texture_cutout = new Rect(3, 3, 5, 14),
                    pivot = DefaultTextureCutout.pivot
                }
            },
            {
                "Foot",
                new TextureCutoutCache()
                {
                    texture_cutout = new Rect(3, 0, 9, 3),
                    pivot = DefaultTextureCutout.pivot
                }
            },
            
            {
                "UpperArmFront",
                new TextureCutoutCache()
                {
                    texture_cutout = new Rect(11, 54, 5, 13),
                    pivot = DefaultTextureCutout.pivot
                }
            },
            {
                "LowerArmFront",
                new TextureCutoutCache()
                {
                    texture_cutout = new Rect(11, 37, 5, 16),
                    pivot = DefaultTextureCutout.pivot
                }
            }
        });
    }
}