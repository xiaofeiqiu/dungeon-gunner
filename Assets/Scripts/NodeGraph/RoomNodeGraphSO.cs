using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoonNodeGraphSO", menuName = "Scriptable Object/Dungeon/Room node graph")]
public class RoomNodeGraphSO : ScriptableObject
{
    [HideInInspector] public RoomNodeTypeListSO roomNodeTypeList;
    [HideInInspector] public List<RoomNodeSO> roomNodeList = new List<RoomNodeSO>();
    [HideInInspector] public Dictionary<string, RoomNodeSO> roomNodeSODict = new Dictionary<string, RoomNodeSO>();


    private void Awake()
    {
        LoadRoomNodeDict();
    }

    private void LoadRoomNodeDict()
    {
        roomNodeSODict.Clear();
        foreach (RoomNodeSO item in roomNodeList)
        {
            roomNodeSODict[item.id] = item;
        }
    }

    #region Editor Code
#if UNITY_EDITOR
    [HideInInspector] public RoomNodeSO fromNode;
    [HideInInspector] public Vector2 linePosition;

    public void OnValidate()
    {
        LoadRoomNodeDict();
    }

    public void SetLineFrom(RoomNodeSO node, Vector2 pos)
    {
        fromNode = node;
        linePosition = pos;
    }

#endif
    #endregion
}
