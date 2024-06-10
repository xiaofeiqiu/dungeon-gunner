using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This attribute allows creating an instance of this ScriptableObject from the Unity Editor menu
[CreateAssetMenu(fileName = "RoomNodeGraphSO", menuName = "Scriptable Object/Dungeon/Room node graph")]
public class RoomNodeGraphSO : ScriptableObject
{
    // Public fields with HideInInspector attribute to make them accessible but not visible in the Inspector
    [HideInInspector] public RoomNodeTypeListSO roomNodeTypeList;
    [HideInInspector] public List<RoomNodeSO> roomNodeList = new List<RoomNodeSO>();
    [HideInInspector] public Dictionary<string, RoomNodeSO> roomNodeSODict = new Dictionary<string, RoomNodeSO>();

    // Unity built-in method called when the script instance is being loaded
    private void Awake()
    {
        // Initialize the roomNodeSODict dictionary by loading nodes from the list
        LoadRoomNodeDict();
    }

    // Loads the room nodes from the list into the dictionary for quick access by ID
    private void LoadRoomNodeDict()
    {
        roomNodeSODict.Clear(); // Clear existing entries
        foreach (RoomNodeSO item in roomNodeList) // Iterate through the roomNodeList
        {
            roomNodeSODict[item.id] = item; // Add each node to the dictionary using its ID as the key
        }
    }

    // Retrieves a RoomNodeSO object by its ID
    public RoomNodeSO GetRoomNode(string id)
    {
        if (roomNodeSODict.TryGetValue(id, out RoomNodeSO roomNode)) // Attempt to get the node from the dictionary
        {
            return roomNode; // Return the node if found
        }
        return null; // Return null if the node is not found
    }

    #region Editor Code
#if UNITY_EDITOR
    // Fields for use in the Unity Editor, hidden from the inspector
    [HideInInspector] public RoomNodeSO fromNode;
    [HideInInspector] public Vector2 linePosition;

    // Called when the script is loaded or a value changes in the inspector (Editor only)
    public void OnValidate()
    {
        LoadRoomNodeDict(); // Reload the dictionary to ensure it is up to date
    }

    // Sets the start point for a connection line in the editor
    public void SetLineFrom(RoomNodeSO node, Vector2 pos)
    {
        fromNode = node; // Set the starting node
        linePosition = pos; // Set the position for the line
    }

#endif
    #endregion
}
