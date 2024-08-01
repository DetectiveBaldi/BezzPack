using MTM101BaldAPI;
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

        public SoundObject bezzPrompt0;

        public SoundObject bezzPrompt1;

        public SoundObject bezzEat0;

        public SoundObject bezzEat1;

        public SoundObject bezzYapping0;

        public SoundObject bezzYapping1;

        public SoundObject bezzYapping2;

        public SoundObject bezzYapping3;

        public SoundObject bezzYapping4;

        public SoundObject bezzRealization0;

        public SoundObject bezzFurious0;

        public CustomSpriteAnimator animationManagement;

        public List<Sprite> idleSprites;

        public List<Sprite> walkSprites;

        public List<Sprite> eatSprites;

        public List<Sprite> yapSprites;

        public List<Sprite> furiousSprites;

#pragma warning restore CS8618

        public override void Initialize()
        {
            base.Initialize();

            behaviorStateMachine.ChangeState(new BezzWanderState(this, 0.0f));

            idleSprites = new List<Sprite>();

            idleSprites.Add(BasePlugin.current.assetManagement.Get<Sprite>("BezzIdle0"));

            animationManagement.animations.Add("Idle", new CustomAnimation<Sprite>(idleSprites.ToArray(), 1.0f));

            walkSprites = new List<Sprite>();

            for (int i = 0; i < 12; i++)
            {
                walkSprites.Add(BasePlugin.current.assetManagement.Get<Sprite>("BezzWalk" + i));
            }

            animationManagement.animations.Add("Walk", new CustomAnimation<Sprite>(walkSprites.ToArray(), 1.0f));

            eatSprites = new List<Sprite>();

            eatSprites.Add(BasePlugin.current.assetManagement.Get<Sprite>("BezzEat0"));

            animationManagement.animations.Add("Eat", new CustomAnimation<Sprite>(eatSprites.ToArray(), 1.0f));

            yapSprites = new List<Sprite>();

            for (int i = 0; i < 2; i++)
            {
                yapSprites.Add(BasePlugin.current.assetManagement.Get<Sprite>("BezzYap" + i));
            }

            animationManagement.animations.Add("Yap", new CustomAnimation<Sprite>(yapSprites.ToArray(), 1.0f));

            furiousSprites = new List<Sprite>();

            furiousSprites.Add(BasePlugin.current.assetManagement.Get<Sprite>("BezzFurious0"));

            animationManagement.animations.Add("Furious", new CustomAnimation<Sprite>(furiousSprites.ToArray(), 1.0f));

            animationManagement.ChangeSpeed(1.0f);

            animationManagement.Play("Idle", 0.0f);

            animationManagement.SetDefaultAnimation("Idle", 0.0f);
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

            bezzCharacter.animationManagement.ChangeSpeed(1.0f);

            bezzCharacter.animationManagement.Play("Walk", 0.0f);

            bezzCharacter.animationManagement.SetDefaultAnimation("Walk", 0.0f);
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

            bezzCharacter.animationManagement.ChangeSpeed(1.0f);
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

            bezzCharacter.animationManagement.ChangeSpeed(1.325f);

            bezzCharacter.animationManagement.Play("Walk", 0.0f);

            bezzCharacter.animationManagement.SetDefaultAnimation("Walk", 0.0f);
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

            bezzCharacter.animationManagement.ChangeSpeed(1.0f);
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

            turnTimer = 0.5f;

            walkSpeedModifier = new ValueModifier(0.0f, 0.0f);

            runSpeedModifier = new ValueModifier(0.0f, 0.0f);

            playerManagement.GetMovementStatModifier().AddModifier("walkSpeed", walkSpeedModifier);

            playerManagement.GetMovementStatModifier().AddModifier("runSpeed", runSpeedModifier);
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

                bezzCharacter.animationManagement.ChangeSpeed(10.0f);

                bezzCharacter.animationManagement.Play("Eat", 0.0f);

                bezzCharacter.animationManagement.SetDefaultAnimation("Eat", 0.0f);
            }
            else
            {
                bezzCharacter.audioManagement.QueueAudio(bezzCharacter.bezzYapping0);

                bezzCharacter.audioManagement.QueueAudio(bezzCharacter.bezzYapping1);

                bezzCharacter.audioManagement.QueueAudio(bezzCharacter.bezzYapping2);

                bezzCharacter.audioManagement.QueueAudio(bezzCharacter.bezzYapping3);

                bezzCharacter.audioManagement.QueueAudio(bezzCharacter.bezzYapping4);

                bezzCharacter.animationManagement.ChangeSpeed(10.0f);

                bezzCharacter.animationManagement.Play("Yap", 0.0f);

                bezzCharacter.animationManagement.SetDefaultAnimation("Yap", 0.0f);
            }
        }

        public override void Update()
        {
            base.Update();

            if (bezzCharacter.audioManagement.QueuedAudioIsPlaying)
            {
                if (Vector3.Distance(playerManagement.transform.position, bezzCharacter.transform.position) > 22.5f)
                {
                    bezzCharacter.behaviorStateMachine.ChangeState(new BezzRealizationState(bezzCharacter, playerManagement));
                }
            }
            else
            {
                Singleton<CoreGameManager>.Instance.AddPoints(100, 0, true);

                bezzCharacter.behaviorStateMachine.ChangeState(new BezzWanderState(bezzCharacter, 60.0f));
            }

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

        public override void Exit()
        {
            base.Exit();

            bezzCharacter.audioManagement.FlushQueue(true);

            bezzCharacter.animationManagement.ChangeSpeed(1.0f);
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

            bezzCharacter.animationManagement.ChangeSpeed(1.0f);

            bezzCharacter.animationManagement.Play("Idle", 0.0f);

            bezzCharacter.animationManagement.SetDefaultAnimation("Idle", 0.0f);
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

            bezzCharacter.animationManagement.ChangeSpeed(1.0f);
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

            bezzCharacter.animationManagement.ChangeSpeed(1.0f);

            bezzCharacter.animationManagement.Play("Furious", 0.0f);

            bezzCharacter.animationManagement.SetDefaultAnimation("Furious", 0.0f);

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

                CustomSpriteAnimator animationManagement = spriteRenderer.gameObject.AddComponent<CustomSpriteAnimator>();

                animationManagement.spriteRenderer = spriteRenderer;

                List<Sprite> idleSprites = new List<Sprite>();

                idleSprites.Add(BasePlugin.current.assetManagement.Get<Sprite>("Flag0"));

                idleSprites.Add(BasePlugin.current.assetManagement.Get<Sprite>("Flag1"));

                animationManagement.animations.Add("Idle", new CustomAnimation<Sprite>(idleSprites.ToArray(), 1.0f));

                animationManagement.ChangeSpeed(1.0f);

                animationManagement.Play("Idle", 0.0f);

                animationManagement.SetDefaultAnimation("Idle", 0.0f);

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

            bezzCharacter.animationManagement.ChangeSpeed(1.0f);

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