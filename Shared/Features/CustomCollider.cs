using System;
using System.IO;
using System.Reflection;
using BepInEx;
using UnityEngine;

namespace KK_VR.Features
{
    internal static class CustomCollider
    {
        internal static bool IsActive => _rascal != null;
        private const string _assemblyName = "RASCAL.dll";
        private static Assembly _rascal;

        private static void Init()
        {
            var path = Assembly.GetAssembly(typeof(VRPlugin)).Location;
            path = path.Remove(path.LastIndexOf('\\'));
            _rascal = Assembly.LoadFile(Path.Combine(path, _assemblyName));
            VRPlugin.Logger.LogDebug($"MeshCollider:Init:_rascal = {_rascal.FullName}");
        }
        /// <summary>
        /// Adds custom MeshCollider to ChaControl.gameObject
        /// </summary>
        internal static MonoBehaviour Add(ChaControl chara, bool cleanUp = true)
        {
            if (!IsActive)
            {
                Init();
            }
            if (IsActive && chara != null)
            {
                var type = _rascal.GetType("RASCALSkinnedMeshCollider");
                if (type != null && type.IsSubclassOf(typeof(MonoBehaviour)))
                {
                    VRPlugin.Logger.LogDebug($"MeshCollider:Add:type = {type.Name}");
                    if (cleanUp)
                    {
                        CleanUpColliders(chara);
                    }

                    return (MonoBehaviour)chara.gameObject.AddComponent(type);
                }
                else
                {

                    VRPlugin.Logger.LogDebug($"MeshCollider:NotAdd:type = {type.Name}");
                }
            }
            return null;
        }

        /// <summary>
        /// Cleans up normal colliders
        /// </summary>
        internal static void CleanUpColliders(ChaControl chara)
        {
            if (chara == null) return;

            foreach (var collider in chara.GetComponentsInChildren<Collider>(includeInactive: true))
            {
                if (!Physics.GetIgnoreLayerCollision(10, collider.gameObject.layer))
                {
                    GameObject.Destroy(collider);
#if DEBUG
                    VRPlugin.Logger.LogDebug($"MeshCollider:CleanUpColliders:Destroy:{collider.name} at layer {collider.gameObject.layer}, was enabled = {collider.gameObject.active && collider.gameObject.activeSelf && collider.enabled}");
#endif
                }
            }
        }
        internal static void GetMeshRends(ChaControl chara)
        {
            if (chara == null) return;

            foreach (var rend in chara.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true))
            {
                VRPlugin.Logger.LogDebug($"MeshCollider:GetMeshRends: name = {rend.name}");
            }
        }
        internal static void GetMeshCols(ChaControl chara)
        {
            if (chara == null) return;

            foreach (var rend in chara.GetComponentsInChildren<UnityEngine.MeshCollider>(includeInactive: true))
            {
                VRPlugin.Logger.LogDebug($"MeshCollider:GetMeshRends: name = {rend.name}");
            }
        }
    }
}

