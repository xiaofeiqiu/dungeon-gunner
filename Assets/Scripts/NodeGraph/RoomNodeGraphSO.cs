using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoonNodeGraphSO", menuName = "Scriptable Object/Dungeon/Room node graph")]
public class RoomNodeGraphSO : ScriptableObject
{
    [HideInInspector] public RoomNodeTypeListSO roomNodeTypeList;
    [HideInInspector] public List<RoomNodeSO> roomNodeList = new List<RoomNodeSO>();
    [HideInInspector] public Dictionary<string, RoomNodeSO> roomNodeSODict = new Dictionary<string, RoomNodeSO>();
}
