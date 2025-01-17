using System.Linq;
using VRGIN.Helpers;
using KK_VR.Interpreters;
using KK_VR.Settings;
using static HandCtrl;
using KK_VR.Caress;
using KK_VR.Grasp;

namespace KK_VR.Handlers
{
    class HSceneHandler : ItemHandler
    {
        private bool _injectLMB;
        private IKCaress _ikCaress;

        protected override void OnDisable()
        {
            base.OnDisable();
            TriggerRelease();
        }

        internal void StartMovingAibuItem(AibuColliderKind touch)
        {
            _hand.Shackle(touch == AibuColliderKind.kokan || touch == AibuColliderKind.anal ? 6 : 10);
            _ikCaress = GraspHelper.Instance.StartIKCaress(touch, HSceneInterp.lstFemale[0], _hand);
        }
        internal void StopMovingAibuItem()
        {
            if (_ikCaress != null)
            {
                _ikCaress.End();
                _hand.Unshackle();
                _hand.SetItemRenderer(true);
            }
        }

        protected bool AibuKindAllowed(AibuColliderKind kind, ChaControl chara)
        {
            var heroine = HSceneInterp.hFlag.lstHeroine
                .Where(h => h.chaCtrl == chara)
                .FirstOrDefault();
            return kind switch
            {
                AibuColliderKind.mouth => heroine.isGirlfriend || heroine.isKiss || heroine.denial.kiss,
                AibuColliderKind.anal => heroine.hAreaExps[3] > 0f || heroine.denial.anal,
                _ => true
            };
        }

        public bool DoUndress(bool decrease)
        {
            if (decrease && HSceneInterp.handCtrl.IsItemTouch() && IsAibuItemPresent(out var touch))
            {
                HSceneInterp.ShowAibuHand(touch, true);
                HSceneInterp.handCtrl.DetachItemByUseAreaItem(touch - AibuColliderKind.muneL);
                HSceneInterp.hFlag.click = HFlag.ClickKind.de_muneL + (int)touch - 2;
            }
            else if (Undresser.Undress(_tracker.colliderInfo.behavior.part, _tracker.colliderInfo.chara, decrease))
            {
                //HandNoises.PlaySfx(_index, 1f, HandNoises.Sfx.Undress, HandNoises.Surface.Cloth);
            }
            else
            {
                return false;
            }
            _controller.StartRumble(new RumbleImpulse(1000));
            return true;
        }
        /// <summary>
        /// Does tracker has lock on attached aibu item?
        /// </summary>
        /// <param name="touch"></param>
        /// <returns></returns>
        internal bool IsAibuItemPresent(out AibuColliderKind touch)
        {
            touch = _tracker.colliderInfo.behavior.touch;
            if (touch > AibuColliderKind.mouth && touch < AibuColliderKind.reac_head)
            {
                return HSceneInterp.handCtrl.useAreaItems[touch - AibuColliderKind.muneL] != null;
            }
            return false;
        }
        internal bool TriggerPress()
        {
            var touch = _tracker.colliderInfo.behavior.touch;
            var chara = _tracker.colliderInfo.chara;
           //VRPlugin.Logger.LogDebug($"HSceneHandler:[{touch}][{HSceneInterp.handCtrl.selectKindTouch}]");
            if (touch > AibuColliderKind.mouth
                && touch < AibuColliderKind.reac_head
                && chara == HSceneInterp.lstFemale[0])
            {
                if (IntegrationSensibleH.IsActive && !MouthGuide.Instance.IsActive && HSceneInterp.handCtrl.GetUseAreaItemActive() != -1)
                {
                    // If VRMouth isn't active but automatic caress is going. Disable it.
                    IntegrationSensibleH.OnKissEnd();
                }
                else
                {
                    HSceneInterp.SetSelectKindTouch(touch);
                    HandCtrlHooks.InjectMouseButtonDown(0);
                    _injectLMB = true;
                }
            }
            else
            {
                HSceneInterp.HitReactionPlay(_tracker.colliderInfo.behavior.react, chara, voiceWait: false);
            }
            return true;
        }
        internal void TriggerRelease()
        {
            if (_injectLMB)
            {
                HSceneInterp.SetSelectKindTouch(AibuColliderKind.none);
                HandCtrlHooks.InjectMouseButtonUp(0);
                _injectLMB = false;
            }
        }
        protected override void DoReaction(float velocity)
        {
           //VRPlugin.Logger.LogDebug($"DoReaction:{_tracker.colliderInfo.behavior.react}:{_tracker.colliderInfo.behavior.touch}:{_tracker.reactionType}:{velocity}");
            if (GameSettings.AutomaticTouching.Value > GameSettings.SceneType.TalkScene)
            {
                if (velocity > 1.5f || (_tracker.reactionType == Tracker.ReactionType.HitReaction && !IsAibuItemPresent(out _)))
                {
                    if (GameSettings.TouchReaction.Value != 0f 
                        && HSceneInterp.mode == HFlag.EMode.aibu
                        && GraspHelper.Instance != null 
                        && !GraspHelper.Instance.IsGraspActive(_tracker.colliderInfo.chara)
                        && UnityEngine.Random.value < GameSettings.TouchReaction.Value)
                    {
                        GraspHelper.Instance.TouchReaction(_tracker.colliderInfo.chara, _hand.Anchor.position, _tracker.colliderInfo.behavior.part);
                    }
                    else
                    {
                        HSceneInterp.HitReactionPlay(_tracker.colliderInfo.behavior.react, _tracker.colliderInfo.chara, voiceWait: true);
                    }
                }
                else if (_tracker.reactionType == Tracker.ReactionType.Short)
                {
                    Features.LoadGameVoice.PlayVoice(Features.LoadGameVoice.VoiceType.Short, _tracker.colliderInfo.chara, voiceWait: true);
                }
                else //if (_tracker.reactionType == ControllerTracker.ReactionType.Laugh)
                {
                    Features.LoadGameVoice.PlayVoice(Features.LoadGameVoice.VoiceType.Laugh, _tracker.colliderInfo.chara, voiceWait: true);
                }
                _controller.StartRumble(new RumbleImpulse(1000));
            }
        }
    }
}