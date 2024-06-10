using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This attribute allows creating an instance of this ScriptableObject from the Unity Editor menu
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
    public List<RoomNodeTypeSO> list; // List of RoomNodeTypeSO to be populated for the game

    #region Validate
#if UNITY_EDITOR
    // Unity built-in method called when the script is loaded or a value changes in the inspector (Editor only)
    private void OnValidate()
    {
        // Helper method to validate that the list is not null and its values are not null
        Helper.ValidateCheckEnumerableValues(this, nameof(list), list);
    }
#endif
    #endregion
}
