using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This attribute allows creating an instance of this ScriptableObject from the Unity Editor menu
[CreateAssetMenu(fileName = "RoomNodeType_", menuName = "Scriptable Object/Dungeon/Room node type")]
public class RoomNodeTypeSO : ScriptableObject
{
    // Name of the room node type
    public string roomNodeTypeName;

    #region Header
    [Header("Only flag the RoomNodeTypes that should be visible in the editor")]
    #endregion Header
    public bool displayInNodeGraphEditor = true; // Flag to determine if the type should be displayed in the node graph editor

    #region Header
    [Header("One Type Should Be A Corridor")]
    #endregion Header
    public bool isCorridor; // Indicates if the type is a corridor

    #region Header
    [Header("One Type Should Be A CorridorNS ")]
    #endregion Header
    public bool isCorridorNS; // Indicates if the type is a North-South corridor

    #region Header
    [Header("One Type Should Be A CorridorEW")]
    #endregion Header
    public bool isCorridorEW; // Indicates if the type is an East-West corridor

    #region Header
    [Header("One Type Should Be An Entrance")]
    #endregion Header
    public bool isEntrance; // Indicates if the type is an entrance

    #region Header
    [Header("One Type Should Be A Boss Room")]
    #endregion Header
    public bool isBossRoom; // Indicates if the type is a boss room

    #region Header
    [Header("One Type Should Be None (Unassigned)")]
    #endregion Header
    public bool isNone; // Indicates if the type is unassigned

    #region Validate
#if UNITY_EDITOR
    // Unity built-in method called when the script is loaded or a value changes in the inspector (Editor only)
    private void OnValidate()
    {
        // Helper method to validate that the roomNodeTypeName is not an empty string
        Helper.ValidateCheckEmptyString(this, nameof(roomNodeTypeName), roomNodeTypeName);
    }
#endif
    #endregion
}
