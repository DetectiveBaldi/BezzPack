using MTM101BaldAPI.Components;
using MTM101BaldAPI.PlusExtensions;

namespace BezzPack
{
    public class BrownieItem : Item
    {
        public override bool Use(PlayerManager playerManagement)
        {
            ValueModifier staminaRiseModifier = new ValueModifier(0.85f, 0.0f);

            ValueModifier walkSpeedModifier = new ValueModifier(0.85f, 0.0f);

            ValueModifier runSpeedModifier = new ValueModifier(0.85f, 0.0f);

            playerManagement.GetMovementStatModifier().AddModifier("staminaRise", staminaRiseModifier);

            playerManagement.GetMovementStatModifier().AddModifier("walkSpeed", walkSpeedModifier);

            playerManagement.GetMovementStatModifier().AddModifier("runSpeed", runSpeedModifier);

            playerManagement.plm.stamina = playerManagement.plm.staminaMax * 3.0f;

            Singleton<CoreGameManager>.Instance.audMan.PlaySingle(BasePlugin.current.assetManagement.Get<SoundObject>("BrownieEat0"));

            return true;
        }
    }
}