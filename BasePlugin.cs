using BepInEx;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Components;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.Registers;
using System.Linq;
using UnityEngine;

namespace BezzPack
{
    [BepInPlugin("detectivebaldi.pluspacks.bezz", "Bezz Pack", "1.1.0.1")]
    [BepInDependency("mtm101.rulerp.bbplus.baldidevapi")]
    public class BasePlugin : BaseUnityPlugin
    {
#pragma warning disable CS8618

        public static BasePlugin current;

        public AssetManager assetManagement;

#pragma warning restore CS8618

        public void Awake()
        {
            current = this;

            Harmony harmony = new Harmony("detectivebaldi.pluspacks.bezz");

            harmony.PatchAllConditionals();

            assetManagement = new AssetManager();

            assetManagement.Add<SoundObject>("BezzPrompt0", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, "BezzPrompt0.wav"), "BezzPrompt0", SoundType.Voice, new Color(1.0f, 0.5f, 0.0f, 1.0f)));

            assetManagement.Add<SoundObject>("BezzPrompt1", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, "BezzPrompt1.wav"), "BezzPrompt1", SoundType.Voice, new Color(1.0f, 0.5f, 0.0f, 1.0f)));

            assetManagement.Add<SoundObject>("BezzEat0", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, "BezzEat0.wav"), "BezzEat0", SoundType.Voice, new Color(1.0f, 0.5f, 0.0f, 1.0f)));

            assetManagement.Add<SoundObject>("BezzEat1", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, "BezzEat1.wav"), "BezzEat1", SoundType.Voice, new Color(1.0f, 0.5f, 0.0f, 1.0f)));

            for (int i = 0; i < 5; i++)
            {
                assetManagement.Add<SoundObject>("BezzYapping" + i, ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, "BezzYapping" + i + ".wav"), "BezzYapping" + i, SoundType.Voice, new Color(1.0f, 0.5f, 0.0f, 1.0f)));
            }

            assetManagement.Add<SoundObject>("BezzRealization0", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, "BezzRealization0.wav"), "BezzRealization0", SoundType.Voice, new Color(1.0f, 0.5f, 0.0f, 1.0f)));

            assetManagement.Add<SoundObject>("BezzFurious0", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, "BezzFurious0.wav"), "BezzFurious0", SoundType.Voice, new Color(1.0f, 0.5f, 0.0f, 1.0f)));

            assetManagement.Add<Sprite>("BezzIdle0", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "BezzCharacter/BezzIdle0.png"), 31.5f));

            for (int i = 0; i < 12; i++)
            {
                assetManagement.Add<Sprite>("BezzWalk" + i, AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "BezzCharacter/BezzWalk" + i + ".png"), 31.5f));
            }

            assetManagement.Add<Sprite>("BezzEat0", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "BezzCharacter/BezzEat0.png"), 31.5f));

            for (int i = 0; i < 2; i++)
            {
                assetManagement.Add<Sprite>("BezzYap" + i, AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "BezzCharacter/BezzYap" + i + ".png"), 31.5f));
            }

            assetManagement.Add<Sprite>("BezzFurious0", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "BezzCharacter/BezzFurious0.png"), 31.5f));

            assetManagement.Add<Sprite>("Flag0", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "Flag0.png"), 31.5f));

            assetManagement.Add<Sprite>("Flag1", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "Flag1.png"), 31.5f));

            assetManagement.Add<SoundObject>("BrownieEat0", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, "BrownieEat0.wav"), "Nothing", SoundType.Effect, Color.white, 0.0f));

            assetManagement.Add<Sprite>("Brownie0", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "Brownie0.png"), 25f));

            assetManagement.Add<Sprite>("Brownie1", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "Brownie1.png"), 50f));

            LoadingEvents.RegisterOnAssetsLoaded(Info, loadCharacters, false);

            LoadingEvents.RegisterOnAssetsLoaded(Info, loadItems, false);

            GeneratorManagement.Register(this, GenerationModType.Addend, generateCallback);
        }

        public void loadCharacters()
        {
            BezzCharacter bezzCharacter = new NPCBuilder<BezzCharacter>(Info)

            .SetName("BezzCharacter")

            .SetEnum("BezzCharacter")

            .SetPoster(AssetLoader.TextureFromMod(this, "BezzPoster.png"), "BezzPosterTitle", "BezzPosterDescription")

            .AddSpawnableRoomCategories(RoomCategory.Hall)

            .AddLooker()

            .AddTrigger()

            .Build();

            bezzCharacter.audioManagement = bezzCharacter.gameObject.AddComponent<PropagatedAudioManager>();

            bezzCharacter.bezzPrompt0 = assetManagement.Get<SoundObject>("BezzPrompt0");

            bezzCharacter.bezzPrompt1 = assetManagement.Get<SoundObject>("BezzPrompt1");

            bezzCharacter.bezzEat0 = assetManagement.Get<SoundObject>("BezzEat0");

            bezzCharacter.bezzEat1 = assetManagement.Get<SoundObject>("BezzEat1");

            bezzCharacter.bezzYapping0 = assetManagement.Get<SoundObject>("BezzYapping0");

            bezzCharacter.bezzYapping1 = assetManagement.Get<SoundObject>("BezzYapping1");

            bezzCharacter.bezzYapping2 = assetManagement.Get<SoundObject>("BezzYapping2");

            bezzCharacter.bezzYapping3 = assetManagement.Get<SoundObject>("BezzYapping3");

            bezzCharacter.bezzYapping4 = assetManagement.Get<SoundObject>("BezzYapping4");

            bezzCharacter.bezzRealization0 = assetManagement.Get<SoundObject>("BezzRealization0");

            bezzCharacter.bezzFurious0 = assetManagement.Get<SoundObject>("BezzFurious0");

            bezzCharacter.spriteRenderer[0].sprite = assetManagement.Get<Sprite>("BezzIdle0");

            bezzCharacter.animationManagement = bezzCharacter.gameObject.AddComponent<CustomSpriteAnimator>();

            bezzCharacter.animationManagement.spriteRenderer = bezzCharacter.spriteRenderer[0];

            bezzCharacter.spriteRenderer[0].gameObject.transform.localPosition += Vector3.up / 1.075f;

            assetManagement.Add<BezzCharacter>("BezzCharacter", bezzCharacter);
        }

        public void loadItems()
        {
            ItemObject brownie = new ItemBuilder(Info)

                .SetNameAndDescription("Item_Brownie", "Description_Brownie")

                .SetSprites(assetManagement.Get<Sprite>("Brownie0"), assetManagement.Get<Sprite>("Brownie1"))

                .SetEnum("Brownie")

                .SetShopPrice(375)

                .SetGeneratorCost(10)

                .SetItemComponent<BrownieItem>()

                .SetMeta(ItemFlags.Persists, new string[0])

                .Build();

            assetManagement.Add<ItemObject>("Brownie", brownie);
        }

        public void generateCallback(string lName, int lNumber, CustomLevelObject lCustomObject)
        {
            lCustomObject.potentialNPCs.Add(new WeightedNPC() { selection = assetManagement.Get<NPC>("BezzCharacter"), weight = 115 });

            lCustomObject.potentialItems = lCustomObject.potentialItems.AddItem(new WeightedItemObject() { selection = assetManagement.Get<ItemObject>("Brownie"), weight = 150 }).ToArray();

            lCustomObject.shopItems = lCustomObject.shopItems.AddItem(new WeightedItemObject() { selection = assetManagement.Get<ItemObject>("Brownie"), weight = 150 }).ToArray();

            lCustomObject.MarkAsNeverUnload();
        }
    }
}