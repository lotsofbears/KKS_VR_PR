using HarmonyLib;
using KK_VR.Grasp;

namespace KK_VR.Patches
{
    [HarmonyPatch]
    internal class GraspPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.Calc))]
        public static void MotionIKCalcPrefix(string stateName, MotionIK __instance)
        {
            GraspHelper.SetDefaultState(__instance.info, stateName);

        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MotionIK), nameof(MotionIK.Calc))]
        public static void MotionIKCalcPostfix(MotionIK __instance)
        {
            GraspHelper.SetWorkingState(__instance.info);
        }


        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(HitReaction), nameof(HitReaction.HitsEffector))]
        //public static void HitsEffectorPrefix(int _nid, ref Vector3[] forces, HitReaction __instance)
        //{
        //    var chara = __instance.ik.GetComponentInParent<ChaControl>();
        //    for (var i = 0; i < forces.Length; i++)
        //    {
        //        forces[i] = GraspController.Instance.HitReactionWorkaround(
        //            forces[i],
        //            chara,
        //            __instance.effectorHit[_nid].effectorHitPoints[i].effectorLinks[0].effector);
        //    }
        //    foreach (var force in forces)
        //    {
        //       //VRPlugin.Logger.LogDebug($"{force}");
        //    }
        //}

    }
}
