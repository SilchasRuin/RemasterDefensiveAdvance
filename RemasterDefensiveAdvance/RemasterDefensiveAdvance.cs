using Dawnsbury.Core.CharacterBuilder.Feats;
using Dawnsbury.Core.Mechanics.Enumerations;
using Dawnsbury.Core.Mechanics;
using Dawnsbury.Modding;
using Dawnsbury.Core.Possibilities;
using Dawnsbury.Core.CombatActions;
using Dawnsbury.Core;
using Dawnsbury.Core.Mechanics.Targeting;
using Dawnsbury.Audio;
using Dawnsbury.Core.CharacterBuilder.FeatsDb.Common;
using Dawnsbury.Display.Illustrations;

namespace RemasterDefensiveAdvance
{
    public class ChampionDefensiveAdvance
    {
        [DawnsburyDaysModMainMethod]
        public static void LoadMod()
        {
            ModManager.AddFeat(
                new TrueFeat(ModManager.RegisterFeatName("Defensive Advance"), 1,
                    "With the protection of your shield, you dive into battle!",
                    "You Raise your Shield and Stride. If you end your movement within melee reach of at least one enemy, you can make a melee Strike against that enemy.",
                    [Trait.Champion, Trait.Flourish])
                .WithActionCost(2)
                .WithPermanentQEffect(null, qf => 
                    qf.ProvideMainAction = qfSelf =>
                    {
                        return new ActionPossibility(new CombatAction(qfSelf.Owner, new ModdedIllustration("RDAssets/Advance.png"), "Defensive Advance",
                        [Trait.Flourish], 
                            "You Raise your Shield and Stride. If you end your movement within melee reach of at least one enemy, you can make a melee Strike against that enemy.", Target.Self().WithAdditionalRestriction(cr =>
                            {
                                if (!qfSelf.Owner.HeldItems.Any(item => item.HasTrait(Trait.Shield)) && (!qfSelf.Owner.CarriedItems.Any(item => item.HasTrait(Trait.Shield) && item.IsWorn) ||
                                        (qfSelf.Owner.HeldItems.All(item => item.HasTrait(Trait.Weapon)) &&
                                         !qfSelf.Owner.HasFreeHand)))
                                    return "You cannot raise a shield.";
                                return qfSelf.Owner.HasEffect(QEffectId.RaisingAShield) ? "You are already raising a shield." : null;
                            }))
                            .WithShortDescription("Raise a Shield and Stride. Make a melee Strike at the end of this movement.")
                            .WithActionCost(2).WithSoundEffect(SfxName.Footsteps).WithEffectOnSelf(async (action, self) =>
                            {
                                {
                                    List<ICombatAction> actions = Possibilities.Create(qfSelf.Owner)
                                        .Filter(ap =>
                                        {
                                            if (ap.CombatAction.ActionId != ActionId.RaiseShield ||
                                                ap.CombatAction.Name == "Raise shield (Devoted Guardian)")
                                                return false;
                                            ap.CombatAction.ActionCost = 0;
                                            ap.RecalculateUsability();
                                            return true;
                                        }).CreateActions(true);
                                    if (actions.FirstOrDefault() is CombatAction raiseAShield)
                                        await self.Battle.GameLoop.FullCast(raiseAShield);
                                    if (!await self.StrideAsync(
                                            "Choose where to Stride with Defensive Advance. You should end your movement within melee reach of an enemy.",
                                            allowCancel: true))
                                    {
                                        action.RevertRequested = true;
                                        self.RemoveAllQEffects(effect => effect.Id == QEffectId.RaisingAShield);
                                    }
                                    else
                                    {
                                        await CommonCombatActions.StrikeAdjacentCreature(self, null);
                                    }
                                }
                            })).WithPossibilityGroup("Abilities");
                    }));



        }
    }
}
