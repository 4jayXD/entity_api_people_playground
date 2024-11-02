using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct EntityAPI
{
    public static void Load() 
    {
        Texturing.LoadPlaceholders();

        Texturing.BuiltIn_BodyTextureCutouts = new Dictionary<string, Texturing.BodyTextureCutout>() 
        {
            {
                "Human_Normal",
                new Texturing.BodyTextureCutout() 
                {
                    
                }
            },
            {
                "Human_AllLimbs",
                new Texturing.BodyTextureCutout()
                {

                }
            }
        };
    }

    public struct Texturing 
    {
        public static Dictionary<string, BodyTextureCutout> BuiltIn_BodyTextureCutouts;

        private static Texture2D 
            placeholder_texture_damage, 
            placeholder_texture_skin, 
            placeholder_texture_flesh, 
            placeholder_texture_bone;

        public static BodyTextures placeholderCache;

        public static void LoadPlaceholders() 
        {
            placeholder_texture_damage = ModAPI.LoadTexture("entity_api/placeholder_damage.png");
            placeholder_texture_skin = ModAPI.LoadTexture("entity_api/placeholder_skin.png");
            placeholder_texture_flesh = ModAPI.LoadTexture("entity_api/placeholder_flesh.png");
            placeholder_texture_bone = ModAPI.LoadTexture("entity_api/placeholder_bone.png");

            placeholderCache = new BodyTextures() 
            {
                skin = new List<Texture2D>() {placeholder_texture_skin},
                flesh = placeholder_texture_flesh,
                bone = placeholder_texture_bone,
                damage = placeholder_texture_damage
                
            };
        }

        public class BodyTextureCutout
        {
            public Dictionary<string, LimbTextureCutout> limbCutouts;

            public void ApplyTextures(PersonBehaviour person, BodyTextures textures, bool resetColliders = false) 
            {
                foreach(LimbBehaviour limb in person.Limbs) 
                {
                    string name = limb.name;

                    if (!limbCutouts.ContainsKey(name) && name.Contains("Front"))
                        name = name.Remove(name.Length - 5, 5);

                    if (limbCutouts.ContainsKey(name))
                        limbCutouts[name].textureLimb(limb, textures, resetColliders);
                }
            }
        }

        public class BodyTextures 
        {
            public List<Texture2D> skin;
            
            public Texture2D 
                flesh,
                bone,
                damage;


            public bool hasVariants 
            {
                get 
                {
                    return skin.Count > 0;
                }
            }

            public enum Type 
            {
                Skin,
                Flesh,
                Bone,
                Damage
            }

            public static BodyTextures Create(Texture2D skin, Texture2D flesh, Texture2D bone, Texture2D damage = null) 
            {
                Texture2D damageTex = placeholder_texture_damage;

                if (damage != null)
                    damageTex = damage;

                return new BodyTextures()
                {
                    skin = new List<Texture2D>() {skin},
                    flesh = flesh,
                    bone = bone,
                    damage = damageTex
                };
            }
        }

        public class LimbTextureCutout
        {
            public Rect cutout { get; private set; }
            public Vector2 pivot { get; private set; } = new Vector2(.5f, .5f);

            public bool resetCollider = false;

            public LimbTextureCutout(Rect cutout) 
            {
                this.cutout = cutout;
            }
            public LimbTextureCutout(int x, int y, int width, int height)
            {
                cutout = new Rect(x, y, width, height);
            }
            public LimbTextureCutout(int x, int y, int size)
            {
                cutout = new Rect(x, y, size, size);
            }
            public LimbTextureCutout(int width, int height) 
            {
                cutout = new Rect(0, 0, width, height);
            }

            public LimbTextureCutout Pivot(Vector2 pivot)
            {
                this.pivot += pivot;
                return this;
            }
            public LimbTextureCutout ResetCollider() 
            {
                resetCollider = true;
                return this;
            }

            public void textureLimb(LimbBehaviour limb, BodyTextures textures, bool resetCollider = false)
            {
                var renderer = limb.SkinMaterialHandler.renderer;
                LimbSprites sprites = LimbSprites.createSprites(textures, this, renderer.sprite);

                renderer.sprite = sprites.skin;
                
                if ((bool)textures.flesh)
                    renderer.material.SetTexture(ShaderProperties.Get("_FleshTex"), textures.flesh);
                if ((bool)textures.bone)
                    renderer.material.SetTexture(ShaderProperties.Get("_BoneTex"), textures.bone);
                if ((bool)textures.damage)
                    renderer.material.SetTexture(ShaderProperties.Get("_DamageTex"), textures.damage);

                if (resetCollider || this.resetCollider)
                    limb.GetComponent<BoxCollider2D>().size = sprites.skin.bounds.size;

                if (limb.TryGetComponent<ShatteredObjectSpriteInitialiser>(out var comp)) 
                {
                    var converted = sprites.convert();
                    comp.UpdateSprites(in converted);
                }                  
            }

            public struct LimbSprites
            {
                public Sprite skin { get; private set; }
                public Sprite flesh { get; private set; }
                public Sprite bone { get; private set; }
                public Sprite damage { get; private set; }

                public LimbSprites(Sprite damage, Sprite skin, Sprite flesh, Sprite bone)
                {
                    this.damage = damage;
                    this.skin = skin;
                    this.flesh = flesh;
                    this.bone = bone;
                }

                public static LimbSprites createSprites(BodyTextures textures, LimbTextureCutout cache, Sprite original)
                {
                    Texture2D skin = textures.skin[0];

                    if (textures.hasVariants)                     
                        skin = textures.skin[new System.Random().Next(textures.skin.Count)];
                    
                    var sprites = new LimbSprites(
                        createSprite(textures.damage),
                        createSprite(skin),
                        textures.flesh ? createSprite(textures.flesh) : null,
                        textures.bone ? createSprite(textures.bone) : null);

                    Sprite createSprite(Texture2D texture) =>
                        Sprite.Create(texture, cache.cutout, cache.pivot, original.pixelsPerUnit, 0u, SpriteMeshType.FullRect, original.border, false);

                    return sprites;
                }
                /// <summary>
                ///  Converts LimbTextureCutout.LimbsSprites to LimbSpriteCache.LimbSprites. Used for pdating the ShatteredObjectSpriteInitialiser component.
                ///  
                /// Loses the damage sprite infomation.
                /// </summary>
                /// <returns>LimbSpriteCache.LimbSprites</returns>
                public LimbSpriteCache.LimbSprites convert() 
                {
                    return new LimbSpriteCache.LimbSprites(skin, flesh, bone);
                }
                /// <summary>
                /// Converts LimbSpriteCache.LimbSprites to LimbTextureCutout.LimbsSprites. 
                /// 
                /// Does not carry over damage Sprite info.
                /// </summary>
                /// <param name="sprites"></param>
                /// <param name="damage">adds damage sprite info to cache</param>
                /// <returns>LimbTextureCutout.LimbsSprites</returns>
                public static LimbSprites Convert(LimbSpriteCache.LimbSprites sprites, Sprite damage = null) 
                {
                    return new LimbSprites(sprites.Skin, sprites.Flesh, sprites.Bone, damage);
                }
            }
        }  
    }

    public struct Builder 
    {
        public class EntityBuilder
        {
            public Texturing.BodyTextures textures = Texturing.placeholderCache;
            public List<LimbBuildInstruction> limbBuildInstructions;
            public Action<PersonBehaviour> AfterBuildAction;
            public PhysicalProperties physicalProperties = null;
            public string BloodType = Blood.ID;
            public string SpeciesIdentity = "Human";

            public float Size;
            /// <summary>
            /// Builds Entity with the provided instructions.
            /// </summary>
            /// <param name="entity"></param>
            /// <param name="textureOverride"></param>
            public virtual void Build(GameObject entity, Texturing.BodyTextures textureOverride = null) 
            {
                if (!entity.HasComponent<PersonBehaviour>()) 
                {
                    Debug.LogError("Could Not Build Enitity. Missing PersonBehaviour Component.");
                    return;
                }
                entity.name = SpeciesIdentity;
                PersonBehaviour person = entity.GetComponent<PersonBehaviour>();
                List<LimbBehaviour> existingLimbs = person.Limbs.ToList();
                List<LimbBehaviour> LIMBS = new List<LimbBehaviour>();
                
                for (int i = 0; i < limbBuildInstructions.Count; i++) 
                {
                    LimbBehaviour limb = existingLimbs[i];
                    var data = limbBuildInstructions[i];

                    if (textureOverride != null)
                        data.texture_cutout.textureLimb(limb, textureOverride, true);
                    else
                        data.texture_cutout.textureLimb(limb, textures, true);

                    if ((bool)physicalProperties)
                        limb.PhysicalBehaviour.Properties = physicalProperties;

                    limb.transform.parent = entity.transform;
                    limb.name = data.limbName;    
                    limb.transform.localPosition = Vector3.zero;
                    limb.transform.localRotation = new Quaternion(0, 0, 0, 0);
                    limb.transform.localPosition = data.position;

                    limb.HasBrain = data.hasBrain;
                    limb.IsLethalToBreak = data.dieOnBreak;
                    limb.FakeUprightForce = data.fakeUprightForce;
                    limb.CirculationBehaviour.IsPump = data.hasHeart;
                    limb.HasLungs = data.hasLungs;
                    
                    if ((bool)data.PhysicalProperties)
                        limb.PhysicalBehaviour.Properties = data.PhysicalProperties;

                    if (data.hasGrip) 
                    {
                        var grip = limb.gameObject.AddComponent<GripBehaviour>();
                        limb.GripBehaviour = grip;
                        grip.GripPosition = data.gripPosition;
                        grip.PhysicalBehaviour = limb.PhysicalBehaviour;
                        grip.NearestHoldingPos = data.gripPosition;
                        grip.Anchor = data.gripPosition;
                    }

                    if (data.afterAction != null)
                        data.afterAction.Invoke(limb);
                    LIMBS.Add(limb);
                }
                foreach (var limb in LIMBS)
                    existingLimbs.Remove(limb);
                
                void removeRubbish() 
                {
                    foreach (var limb in existingLimbs)
                    {
                        foreach (var l in LIMBS)
                        {
                            if (l.ConnectedLimbs.Contains(limb))
                            {
                                l.ConnectedLimbs.Remove(limb);

                                var connectedNodes = l.NodeBehaviour.Connections.ToList();
                                connectedNodes.Remove(limb.NodeBehaviour);
                                l.NodeBehaviour.Connections = connectedNodes.ToArray();

                                var circulation = l.CirculationBehaviour.PushesTo.ToList();
                                circulation.Remove(limb.CirculationBehaviour);
                                l.CirculationBehaviour.PushesTo = circulation.ToArray();

                                var adjacentLimbs = l.SkinMaterialHandler.adjacentLimbs.ToList();
                                adjacentLimbs.Remove(limb.SkinMaterialHandler);
                                l.SkinMaterialHandler.adjacentLimbs = adjacentLimbs.ToArray();

                                if (l.HasJoint && l.Joint.connectedBody == limb.GetComponent<Rigidbody2D>())
                                {
                                    l.HasJoint = false;
                                    GameObject.Destroy(l.Joint);
                                }

                            }
                        }

                        GameObject.Destroy(limb.gameObject);
                    }
                    GameObject.Destroy(entity.transform.Find("FrontArm").gameObject);
                    GameObject.Destroy(entity.transform.Find("Body").gameObject);
                    GameObject.Destroy(entity.transform.Find("BackArm").gameObject);
                    GameObject.Destroy(entity.transform.Find("FrontLeg").gameObject);
                    GameObject.Destroy(entity.transform.Find("BackLeg").gameObject);
                    GameObject.Destroy(entity.transform.Find("padding 0").gameObject);
                    GameObject.Destroy(entity.transform.Find("padding 1").gameObject);
                    GameObject.Destroy(entity.transform.Find("padding 2").gameObject);
                    GameObject.Destroy(entity.transform.Find("padding 3").gameObject);
                    GameObject.Destroy(entity.transform.Find("padding 4").gameObject);
                }
                removeRubbish();

                person.Limbs = LIMBS.ToArray();

                person.LinkedPoses[PoseState.Rest].ConstructDictionary();
                person.LinkedPoses[PoseState.Protective].ConstructDictionary();
                person.LinkedPoses[PoseState.Flailing].ConstructDictionary();
                person.LinkedPoses[PoseState.Stumbling].ConstructDictionary();
                person.LinkedPoses[PoseState.Swimming].ConstructDictionary();
                person.LinkedPoses[PoseState.WrithingInPain].ConstructDictionary();
                person.LinkedPoses[PoseState.Walking].ConstructDictionary();
                person.LinkedPoses[PoseState.Sitting].ConstructDictionary();
                person.LinkedPoses[PoseState.Flat].ConstructDictionary();
                person.LinkedPoses[PoseState.BrainDamage].ConstructDictionary();

                for (int i = 0; i < person.Poses.Count; i++)
                    person.Poses[i].ConstructDictionary();

                foreach (var limb in person.Limbs)
                {
                    limb.SpeciesIdentity = SpeciesIdentity;

                    Liquid oldBloodType = Liquid.GetLiquid(limb.BloodLiquidType);
                    Liquid newBloodType = Liquid.GetLiquid(BloodType);
                    Color bloodColour = newBloodType.Color;
                    var cirulation = limb.CirculationBehaviour;
                    float bloodAmount = cirulation.GetAmount(oldBloodType);
                    cirulation.RemoveLiquid(oldBloodType, bloodAmount);
                    cirulation.AddLiquid(newBloodType, bloodAmount);

                    limb.BloodLiquidType = BloodType;

                    Material material = limb.SkinMaterialHandler.renderer.material;
                    material.SetColor("_BruiseColor", bloodColour);
                    material.SetColor("_SecondBruiseColor", new Color(bloodColour.r/2, bloodColour.g/2, bloodColour.b/2));
                    material.SetColor("_ThirdBruiseColor", new Color(bloodColour.r/4, bloodColour.g/4, bloodColour.b/4));
                }

                AfterBuildAction.Invoke(person);
            }
            public void Build(Texturing.BodyTextures textureOverride = null, Direction direction = Direction.Left) 
            {
                GameObject entity = GameObject.Instantiate(ModAPI.FindSpawnable("Human").Prefab);

                if (direction == Direction.Left)
                    entity.transform.localScale = new Vector3(1, 1, 1);
                else
                    entity.transform.localScale = new Vector3(-1, 1, 1);

                var behaviour = entity.GetComponent<PersonBehaviour>();

                Build(entity, textureOverride);
            }
            public void Build(Direction direction = Direction.Left)
            {
                GameObject entity = GameObject.Instantiate(ModAPI.FindSpawnable("Human").Prefab);

                if (direction == Direction.Left)
                    entity.transform.localScale = new Vector3(1, 1, 1);
                else
                    entity.transform.localScale = new Vector3(-1, 1, 1);

                var behaviour = entity.GetComponent<PersonBehaviour>();

                Build(entity);
            }
        }
        public class LimbBuildInstruction
        {
            public string limbName { get; private set; } = "segment";
            public string sortingLayerName { get; private set; } = "Default";
            public int sortingLayer { get; private set; } = 2;
            public bool hasLungs { get; private set; } = false;
            public bool hasBrain { get; private set; } = false;
            public bool hasHeart { get; private set; } = false;
            public bool hasGrip { get; private set; }   = false;
            public bool dieOnBreak { get; private set; } = false;
            public Vector2 position, gripPosition = Vector2.zero;
            public float vitality { get; private set; } = 0;
            public List<Bounds> vitalParts { get; private set; } = null;
            public float fakeUprightForce { get; private set; } = .005f;
            public List<string> connectedLimbs { get; private set; } = null;
            public Texturing.LimbTextureCutout texture_cutout { get; private set; } = new Texturing.LimbTextureCutout(10, 10);
            public LimbBehaviour.BodyPart limbType { get; private set; }
            public JointCache joint;
            public Action<LimbBehaviour> afterAction { get; private set; } = null;
            public PhysicalProperties PhysicalProperties { get; private set; } = null;

            public bool hasConnections 
            { 
                get 
                {
                    return connectedLimbs != null && connectedLimbs.Count > 0;
                }
            }
            public bool hasJoint 
            {
                get 
                {
                    return joint.position != null;
                }
            }

            public bool hasVitalParts 
            {
                get 
                {
                    return vitalParts != null && vitalParts.Count > 0;
                }
            }

            public LimbBuildInstruction(string limbName, LimbBehaviour.BodyPart type, List<string> connectedLimbs = null) 
            {
                this.connectedLimbs = connectedLimbs;
                this.limbName = limbName;
                limbType = type;
            }

            /// <summary>
            /// Runs after limb is created.
            /// </summary>
            /// <param name="action"></param>
            /// <returns></returns>
            public LimbBuildInstruction AfterCreation(Action<LimbBehaviour> action) 
            {
                afterAction = action;
                return this;
            }

            /// <summary>
            /// Cutout of the body textures.
            /// </summary>
            /// <param name="cutout"></param>
            /// <returns></returns>
            public LimbBuildInstruction TextureCutout(Texturing.LimbTextureCutout cutout)
            {
                texture_cutout = cutout;

                return this;
            }

            /// <summary>
            /// Force applied every tick. Used to keep it upright. So thats why its called a "Upright" Force. ;)
            /// </summary>
            /// <param name="force"></param>
            /// <returns></returns>
            public LimbBuildInstruction UprightForce(float force) 
            {
                fakeUprightForce = force;
                return this;
            }

            /// <summary>
            /// Sorting layer & order of the limb
            /// </summary>
            /// <param name="sortingLayerName"></param>
            /// <param name="sortingLayer"></param>
            /// <returns></returns>
            public LimbBuildInstruction SortingLayer(string sortingLayerName, int sortingLayer) 
            {
                this.sortingLayerName = sortingLayerName;
                this.sortingLayer = sortingLayer;
                return this;
            }

            /// <summary>
            /// vital parts. Idk really...
            /// </summary>
            /// <param name="vitality"></param>
            /// <param name="vitalParts"></param>
            /// <returns></returns>
            public LimbBuildInstruction VitalParts(float vitality, List<Bounds> vitalParts) 
            {
                this.vitality = vitality;
                this.vitalParts = vitalParts;
                return this;
            }

            /// <summary>
            /// Positioning of the limb.
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <returns></returns>
            public LimbBuildInstruction Position(float x = 0, float y = 0)
            {
                return Position(new Vector2(x, y));
            }
            /// <summary>
            /// Positioning of the limb.
            /// </summary>
            public LimbBuildInstruction Position(Vector2 position)
            {
                this.position = position;
                return this;
            }

            /// <summary>
            /// Grips. Used for grabbing guns, knifes, etc.
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <returns></returns>
            public LimbBuildInstruction Grip(float x = 0, float y = 0)
            {
                hasGrip = true;
                gripPosition = new Vector2(x, y);
                return this;
            }  

            /// <summary>
            /// Gives the limb a brain. Recommended to use on one limb.
            /// </summary>
            public LimbBuildInstruction Brain() 
            {
                hasBrain = true;
                return this;
            }

            /// <summary>
            /// If the limb's bone breaks, the entity dies. Recommended to use on one limb.
            /// </summary>
            public LimbBuildInstruction DieOnBreak() 
            {
                dieOnBreak = true;
                return this;
            }

            /// <summary>
            /// Gives the limb some lungs. Recommended to use on one limb. Required for entity to live.
            /// </summary>
            public LimbBuildInstruction Lungs()
            {
                hasLungs = true;
                return this;
            }

            /// <summary>
            /// Gives the limb a heart (cute). Recommended to use on one limb. Required for entity to live.
            /// </summary>
            public LimbBuildInstruction Heart()
            {
                hasHeart = true;
                return this;
            }           

            /// <summary>
            /// Gives the limb a joint. Recommended.
            /// </summary>
            public LimbBuildInstruction Joint(Vector2 position, JointAngleLimits2D limits) 
            {
                joint = new JointCache()
                {
                    position = position,
                    limits = limits
                };
                return this;
            }

            public struct JointCache 
            {
                public Vector2 position;
                public JointAngleLimits2D limits;
            }
        }
    }

    public enum Direction 
    {
        Left,
        Right
    }
}