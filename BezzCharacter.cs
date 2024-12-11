using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Components;
using MTM101BaldAPI.PlusExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BezzPack
{
    public class BezzCharacter : NPC
    {
#pragma warning disable CS8618

        public AudioManager audioManagement;

        public SoundObject bezzPrompt0, bezzPrompt1;

        public SoundObject bezzEat0, bezzEat1;

        public SoundObject[] bezzYapping;

        public SoundObject bezzRealization0;

        public SoundObject bezzFurious0;

        public CustomSpriteAnimator frameController;

#pragma warning restore CS8618

        public override void Initialize()
        {
            base.Initialize();

            LoadFrames();

            frameController.ChangeSpeed(1.0f);

            frameController.Play("Idle", 1.0f);

            frameController.SetDefaultAnimation("Idle", 1.0f);

            behaviorStateMachine.ChangeState(new BezzWanderState(this, 0.0f));
        }

        public void LoadFrames()
        {
            AssetManager Assets = BasePlugin.current.assetManagement;

            List<Sprite> idleSprites = new List<Sprite>();

            idleSprites.Add(Assets.Get<Sprite>("BezzIdle0"));

            frameController.animations.Add("Idle", new CustomAnimation<Sprite>(idleSprites.ToArray(), 1.0f));

            List<Sprite> walkSprites = new List<Sprite>();

            for (int i = 0; i < 12; i++)
                walkSprites.Add(Assets.Get<Sprite>("BezzWalk" + i));

            frameController.animations.Add("Walk", new CustomAnimation<Sprite>(walkSprites.ToArray(), 1.0f));

            List<Sprite> eatSprites = new List<Sprite>();

            eatSprites.Add(Assets.Get<Sprite>("BezzEat0"));

            frameController.animations.Add("Eat", new CustomAnimation<Sprite>(eatSprites.ToArray(), 1.0f));

            List<Sprite> yapSprites = new List<Sprite>();

            for (int i = 0; i < 2; i++)
                yapSprites.Add(Assets.Get<Sprite>("BezzYap" + i));

            frameController.animations.Add("Yap", new CustomAnimation<Sprite>(yapSprites.ToArray(), 1.0f));

            List<Sprite> furiousSprites = new List<Sprite>();

            furiousSprites.Add(Assets.Get<Sprite>("BezzFurious0"));

            frameController.animations.Add("Furious", new CustomAnimation<Sprite>(furiousSprites.ToArray(), 1.0f));
        }
    }

    public class BezzBaseState : NpcState
    {
        public BezzCharacter bezzCharacter;

        public BezzBaseState(BezzCharacter bezzCharacter) : base(bezzCharacter)
        {
            this.bezzCharacter = bezzCharacter;
        }
    }

    public class BezzWanderState : BezzBaseState
    {
        public float interactTimer;

        public BezzWanderState(BezzCharacter bezzCharacter, float interactTimer) : base(bezzCharacter)
        {
            this.interactTimer = interactTimer;
        }

        public override void Enter()
        {
            base.Enter();

            bezzCharacter.Navigator.SetSpeed(22.5f);

            bezzCharacter.Navigator.maxSpeed = 22.5f;

            ChangeNavigationState(new NavigationState_WanderRandom(bezzCharacter, 0));

            bezzCharacter.frameController.ChangeSpeed(1.0f);

            bezzCharacter.frameController.Play("Walk", 1.0f);

            bezzCharacter.frameController.SetDefaultAnimation("Walk", 1.0f);
        }

        public override void Update()
        {
            base.Update();

            if (interactTimer > 0.0f)
            {
                interactTimer -= Time.deltaTime * bezzCharacter.TimeScale;
            }
        }

        public override void PlayerSighted(PlayerManager playerManagement)
        {
            base.PlayerSighted(playerManagement);

            if (!playerManagement.Tagged && interactTimer <= 0.0f)
            {
                bezzCharacter.behaviorStateMachine.ChangeState(new BezzPromptState(bezzCharacter, playerManagement));
            }
        }

        public override void Exit()
        {
            base.Exit();

            bezzCharacter.frameController.ChangeSpeed(1.0f);
        }
    }

    public class BezzPromptState : BezzBaseState
    {
        public PlayerManager playerManagement;

        public BezzPromptState(BezzCharacter bezzCharacter, PlayerManager playerManagement) : base(bezzCharacter)
        {
            this.playerManagement = playerManagement;
        }

        public override void Enter()
        {
            base.Enter();

            bezzCharacter.Navigator.SetSpeed(30.0f);

            bezzCharacter.Navigator.maxSpeed = 30.0f;

            ChangeNavigationState(new NavigationState_TargetPlayer(bezzCharacter, 1, playerManagement.transform.position, false));

            bezzCharacter.audioManagement.QueueAudio(bezzCharacter.bezzPrompt0);

            bezzCharacter.audioManagement.QueueAudio(bezzCharacter.bezzPrompt1);

            bezzCharacter.frameController.ChangeSpeed(1.325f);

            bezzCharacter.frameController.Play("Walk", 1.325f);

            bezzCharacter.frameController.SetDefaultAnimation("Walk", 1.325f);
        }

        public override void PlayerInSight(PlayerManager playerManagement)
        {
            base.PlayerInSight(playerManagement);

            if (!playerManagement.Tagged)
            {
                currentNavigationState.UpdatePosition(playerManagement.transform.position);
            }
        }

        public override void DestinationEmpty()
        {
            base.DestinationEmpty();

            bezzCharacter.behaviorStateMachine.ChangeState(new BezzWanderState(bezzCharacter, 0.0f));
        }

        public override void OnStateTriggerStay(Collider other)
        {
            base.OnStateTriggerStay(other);

            if (other.CompareTag("Player") && !other.GetComponent<PlayerManager>().Tagged)
            {
                bezzCharacter.behaviorStateMachine.ChangeState(new BezzYappingState(bezzCharacter, playerManagement));
            }
        }

        public override void Exit()
        {
            base.Exit();

            bezzCharacter.audioManagement.FlushQueue(true);

            bezzCharacter.frameController.ChangeSpeed(1.0f);
        }
    }

    public class BezzYappingState : BezzBaseState
    {
        public PlayerManager playerManagement;

        public float turnTimer;

        ValueModifier walkSpeedModifier;

        ValueModifier runSpeedModifier;

        public BezzYappingState(BezzCharacter bezzCharacter, PlayerManager playerManagement) : base(bezzCharacter)
        {
            this.playerManagement = playerManagement;

            walkSpeedModifier = new ValueModifier(0.0f, 0.0f);

            runSpeedModifier = new ValueModifier(0.0f, 0.0f);
        }

        public override void Enter()
        {
            base.Enter();

            bezzCharacter.Navigator.SetSpeed(22.5f);

            bezzCharacter.Navigator.maxSpeed = 22.5f;

            ChangeNavigationState(new NavigationState_DoNothing(bezzCharacter, 0));

            if (playerManagement.itm.Has(EnumExtensions.GetFromExtendedName<Items>("Brownie")))
            {
                bezzCharacter.audioManagement.QueueAudio(bezzCharacter.bezzEat0);

                bezzCharacter.audioManagement.QueueAudio(bezzCharacter.bezzEat1);

                ItemObject[] brownies = playerManagement.itm.items.Where(i => i.itemType == BasePlugin.current.assetManagement.Get<ItemObject>("Brownie").itemType).ToArray();

                playerManagement.itm.RemoveItem(Array.IndexOf(playerManagement.itm.items, brownies[UnityEngine.Random.Range(0, brownies.Length - 1)]));

                bezzCharacter.frameController.ChangeSpeed(10.0f);

                bezzCharacter.frameController.Play("Eat", 10.0f);

                bezzCharacter.frameController.SetDefaultAnimation("Eat", 10.0f);

                Singleton<CoreGameManager>.Instance.AddPoints(50, 0, true);
            }
            else
            {
                bezzCharacter.audioManagement.QueueAudio(bezzCharacter.bezzYapping[0]);

                bezzCharacter.audioManagement.QueueAudio(bezzCharacter.bezzYapping[1]);

                bezzCharacter.audioManagement.QueueAudio(bezzCharacter.bezzYapping[2]);

                bezzCharacter.audioManagement.QueueAudio(bezzCharacter.bezzYapping[3]);

                bezzCharacter.audioManagement.QueueAudio(bezzCharacter.bezzYapping[4]);

                bezzCharacter.frameController.ChangeSpeed(10.0f);

                bezzCharacter.frameController.Play("Yap", 10.0f);

                bezzCharacter.frameController.SetDefaultAnimation("Yap", 10.0f);

                turnTimer = 0.5f;

                playerManagement.GetMovementStatModifier().AddModifier("walkSpeed", walkSpeedModifier);

                playerManagement.GetMovementStatModifier().AddModifier("runSpeed", runSpeedModifier);
            }
        }

        public override void Update()
        {
            base.Update();

            if (bezzCharacter.audioManagement.QueuedAudioIsPlaying)
            {
                if (bezzCharacter.frameController.currentAnimationName == "Yap")
                {
                    if (Vector3.Distance(playerManagement.transform.position, bezzCharacter.transform.position) > 22.5f)
                        bezzCharacter.behaviorStateMachine.ChangeState(new BezzRealizationState(bezzCharacter, playerManagement));
                }
            }
            else
            {
                if (bezzCharacter.frameController.currentAnimationName == "Yap")
                    Singleton<CoreGameManager>.Instance.AddPoints(100, 0, true);

                bezzCharacter.behaviorStateMachine.ChangeState(new BezzWanderState(bezzCharacter, 60.0f));
            }

            if (bezzCharacter.frameController.currentAnimationName == "Yap")
            {
                if (turnTimer > 0.0f)
                {
                    playerManagement.transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(playerManagement.transform.forward.ZeroOutY(), (bezzCharacter.transform.position.ZeroOutY() - playerManagement.transform.position.ZeroOutY()).normalized, (Time.deltaTime * 2.25f) * MathF.PI, 0f), UnityEngine.Vector3.up);

                    turnTimer -= Time.deltaTime * bezzCharacter.TimeScale;
                }
                else
                {
                    playerManagement.GetMovementStatModifier().RemoveModifier(walkSpeedModifier);

                    playerManagement.GetMovementStatModifier().RemoveModifier(runSpeedModifier);
                }
            }
        }

        public override void Exit()
        {
            base.Exit();

            bezzCharacter.audioManagement.FlushQueue(true);

            bezzCharacter.frameController.ChangeSpeed(1.0f);
        }
    }

    public class BezzRealizationState : BezzBaseState
    {
        public PlayerManager playerManagement;

        public BezzRealizationState(BezzCharacter bezzCharacter, PlayerManager playerManagement) : base(bezzCharacter)
        {
            this.playerManagement = playerManagement;
        }

        public override void Enter()
        {
            base.Enter();

            bezzCharacter.audioManagement.QueueAudio(bezzCharacter.bezzRealization0);

            bezzCharacter.frameController.ChangeSpeed(1.0f);

            bezzCharacter.frameController.Play("Idle", 1.0f);

            bezzCharacter.frameController.SetDefaultAnimation("Idle", 1.0f);
        }

        public override void Update()
        {
            base.Update();

            if (!bezzCharacter.audioManagement.QueuedAudioIsPlaying)
            {
                bezzCharacter.behaviorStateMachine.ChangeState(new BezzFuriousState(bezzCharacter, playerManagement));
            }
        }

        public override void Exit()
        {
            bezzCharacter.audioManagement.FlushQueue(true);

            bezzCharacter.frameController.ChangeSpeed(1.0f);
        }
    }

    class BezzFuriousState : BezzBaseState
    {
        public PlayerManager playerManagement;

        public float coolingTimer;

        public List<Flag> flags;

        public float flagTimer;

        public BezzFuriousState(BezzCharacter bezzCharacter, PlayerManager playerManagement) : base(bezzCharacter)
        {
            this.playerManagement = playerManagement;

            coolingTimer = 75.0f;

            flagTimer = 3.0f;

            flags = new List<Flag>();
        }

        public override void Enter()
        {
            base.Enter();

            ChangeNavigationState(new NavigationState_WanderRandom(bezzCharacter, 0));

            bezzCharacter.audioManagement.QueueAudio(bezzCharacter.bezzFurious0);

            bezzCharacter.frameController.ChangeSpeed(1.0f);

            bezzCharacter.frameController.Play("Furious", 1.0f);

            bezzCharacter.frameController.SetDefaultAnimation("Furious", 1.0f);

            for (int i = 0; i < bezzCharacter.ec.Npcs.Count; i++)
            {
                NPC npc = bezzCharacter.ec.Npcs[i];

                if (npc.Character == Character.Principal)
                {
                    npc.GetComponent<Principal>().WhistleReact(bezzCharacter.transform.position);

                    break;
                }
            }

            playerManagement.RuleBreak("Bullying", 10.0f, 0.0f);
        }

        public override void Update()
        {
            base.Update();

            if (coolingTimer > 0.0f)
            {
                coolingTimer -= Time.deltaTime * bezzCharacter.TimeScale;
            }
            else
            {
                bezzCharacter.behaviorStateMachine.ChangeState(new BezzWanderState(bezzCharacter, 0.0f));
            }

            if (flagTimer > 0.0f)
            {
                flagTimer -= Time.deltaTime * bezzCharacter.TimeScale;
            }
            else
            {
                flagTimer = 3.0f;

                List<Cell> cells = bezzCharacter.ec.mainHall.AllTilesNoGarbage(false, true);

                SpriteRenderer spriteRenderer = GameObject.Instantiate<SpriteRenderer>(bezzCharacter.spriteRenderer[0], cells[UnityEngine.Random.Range(0, cells.Count)].TileTransform);

                spriteRenderer.sprite = BasePlugin.current.assetManagement.Get<Sprite>("Flag0");

                spriteRenderer.color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value, 1.0f);

                CustomSpriteAnimator frameController = spriteRenderer.gameObject.AddComponent<CustomSpriteAnimator>();

                frameController.spriteRenderer = spriteRenderer;

                List<Sprite> idleSprites = new List<Sprite>();

                idleSprites.Add(BasePlugin.current.assetManagement.Get<Sprite>("Flag0"));

                idleSprites.Add(BasePlugin.current.assetManagement.Get<Sprite>("Flag1"));

                frameController.animations.Add("Idle", new CustomAnimation<Sprite>(idleSprites.ToArray(), 1.0f));

                frameController.ChangeSpeed(1.0f);

                frameController.Play("Idle", 1.0f);

                frameController.SetDefaultAnimation("Idle", 1.0f);

                spriteRenderer.gameObject.transform.localPosition += Vector3.up * 4.65f;

                GameObject gameObject = new GameObject();

                gameObject.AddComponent<BoxCollider>();

                gameObject.GetComponent<BoxCollider>().size = new Vector3(1.975f, 10.0f, 1.975f);

                gameObject.transform.position = spriteRenderer.transform.position;

                flags.Add(new Flag(spriteRenderer, gameObject));
            }
        }

        public override void Exit()
        {
            base.Exit();

            bezzCharacter.audioManagement.FlushQueue(true);

            bezzCharacter.frameController.ChangeSpeed(1.0f);

            for (int i = 0; i < flags.Count; i++)
            {
                flags[i].spriteRenderer.enabled = false;

                flags[i].gameObject.GetComponent<BoxCollider>().enabled = false;
            }
        }
    }

    public class Flag
    {
        public SpriteRenderer spriteRenderer;

        public GameObject gameObject;

        public Flag(SpriteRenderer spriteRenderer, GameObject gameObject)
        {
            this.spriteRenderer = spriteRenderer;

            this.gameObject = gameObject;
        }
    }
}