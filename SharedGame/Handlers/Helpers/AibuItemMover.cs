using KK_VR.Interpreters;
using static HandCtrl;
using UnityEngine;

namespace KK_VR.Handlers
{
    /// <summary>
    /// Feeds ~controller position to the hFlag.xy field 
    /// </summary>
    internal class AibuItemMover
    {
        internal AibuItemMover(AibuColliderKind touch, Transform anchor)
        {
            _itemId = (int)touch - 2;
            _anchor = anchor;
            _lastPos = anchor.position;
            _item = HSceneInterp.handCtrl.useAreaItems[_itemId].obj.transform;
        }
        private readonly int _itemId;
        private readonly Transform _item;
        private readonly Transform _anchor;
        private Vector3 _lastPos;


        internal void Move()
        {
            var vec = (Vector2)_item.InverseTransformVector(_lastPos - _anchor.position);
            vec.y = 0f - vec.y;
            HSceneInterp.hFlag.xy[_itemId] += vec * 10f;
            _lastPos = _anchor.position;
        }

    }
}
