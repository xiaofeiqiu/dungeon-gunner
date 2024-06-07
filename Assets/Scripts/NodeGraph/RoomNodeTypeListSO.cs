using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomNodeTypeListSO", menuName = "Scriptable Object/Dungeon/Room node type list")]
public class RoomNodeTypeListSO : ScriptableObject
{
    #region Header ROOM NODE TYPE LIST
    [Space(1)]
    [Header("ROOM NODE TYPE LIST")]
    #endregion Header

    #region Tooltip
    [Tooltip("This list should be populated with all the RoomNodeTypeSO for the game - it is used instead of an enum")]
    #endregion Tooltip
    public List<RoomNodeTypeSO> list;

    #region Validate
#if UNITY_EDITOR
    private void OnValidate()
    {
        Helper.ValidateCheckEnumerableValues(this, nameof(list), list);
    }
#endif
    #endregion
}
