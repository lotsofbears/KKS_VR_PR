using KK_VR.Camera;
using UnityEngine.SceneManagement;

namespace KK_VR.Interpreters
{
    /// <summary>
    /// Scene based actions that don't correlate with the input
    /// </summary>
    abstract class SceneInterpreter
    {
        internal virtual void OnStart()
        {
#if KKS
            // KKS swaps VFX all the time gotta keep up, KK doesn't seem like.
            EffectorVFX.Refresh();
#endif
        }
        internal virtual void OnDisable()
        {

        }
        internal virtual void OnUpdate()
        {

        }
        internal virtual void OnLateUpdate()
        {

        }
        internal virtual void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {

        }

    }
}
