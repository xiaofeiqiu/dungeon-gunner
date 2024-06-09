using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RoomNodeSO : ScriptableObject
{
    [HideInInspector] public string id;
    public List<string> parentRoomNodeIDList = new List<string>();
    public List<string> childRoomNodeIDList = new List<string>();
    [HideInInspector] public RoomNodeGraphSO roomNodeGraph;
    public RoomNodeTypeSO roomNodeType;
    [HideInInspector] public RoomNodeTypeListSO roomNodeTypeList;

    #region Editor Code
#if UNITY_EDITOR
    [HideInInspector] public Rect rect;
    [HideInInspector] public bool isDragging = false;
    [HideInInspector] public bool isSelected = false;
    public void Init(Rect rect, RoomNodeGraphSO nodeGraphSO, RoomNodeTypeSO typeSO)
    {
        this.rect = rect;
        this.id = Guid.NewGuid().ToString();
        this.name = "RoomNode";
        this.roomNodeGraph = nodeGraphSO;
        this.roomNodeType = typeSO;

        roomNodeTypeList = GameResource.Instance.RoomNodeTypeListSO;
    }

    public void Draw(GUIStyle nodeStyle)
    {
        GUILayout.BeginArea(rect, nodeStyle);

        EditorGUI.BeginChangeCheck();

        // if room node has a parent, or is a entrance node, display a label else dispay a dropdown
        if (parentRoomNodeIDList.Count > 0 || roomNodeType.isEntrance)
        {
            EditorGUILayout.LabelField(roomNodeType.roomNodeTypeName);
        }
        else
        {
            // default selected
            int selected = roomNodeTypeList.list.FindIndex(x => x == roomNodeType);

            // display selections
            int selection = EditorGUILayout.Popup("", selected, GetRoomNodeTypeToDisplay());
            roomNodeType = roomNodeTypeList.list[selection];

            // If the room type selection has changed, making child connections potentially invalid
            if (roomNodeTypeList.list[selected].isCorridor && !roomNodeTypeList.list[selection].isCorridor
    || !roomNodeTypeList.list[selected].isCorridor && roomNodeTypeList.list[selection].isCorridor
    || !roomNodeTypeList.list[selected].isBossRoom && roomNodeTypeList.list[selection].isBossRoom)
            {
                // Check if there are any child room nodes to process
                if (childRoomNodeIDList.Count > 0)
                {
                    // Iterate through the child room node ID list in reverse order
                    for (int i = childRoomNodeIDList.Count - 1; i >= 0; i--)
                    {
                        // Get the child room node from the graph using its ID
                        RoomNodeSO childRoomNode = roomNodeGraph.GetRoomNode(childRoomNodeIDList[i]);

                        // If the child room node is not null
                        if (childRoomNode != null)
                        {
                            // Remove child ID from parent room node
                            RemoveChildRoom(childRoomNode.id);

                            // Remove parent ID from child room node
                            childRoomNode.RemoveParentRoom(id);
                        }
                    }
                }
            }

        }

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(this);
        }
        GUILayout.EndArea();
    }

    private string[] GetRoomNodeTypeToDisplay()
    {
        string[] roomArray = new string[roomNodeTypeList.list.Count];
        for (int i = 0; i < roomNodeTypeList.list.Count; i++)
        {
            if (roomNodeTypeList.list[i].displayInNodeGraphEditor)
            {
                roomArray[i] = roomNodeTypeList.list[i].roomNodeTypeName;
            }
        }
        return roomArray;
    }

    public void ProcessEvents(Event currentEvent)
    {
        switch (currentEvent.type)
        {
            case EventType.MouseDown:
                processMouseDownEvent(currentEvent);
                break;
            case EventType.MouseUp:
                processMouseUpEvent(currentEvent);
                break;
            case EventType.MouseDrag:
                processMouseDragEvent(currentEvent);
                break;
            default:
                break;
        }
    }

    private void processMouseDragEvent(Event currentEvent)
    {
        if (currentEvent.button == 0)
        {
            processMouseLeftDragEvent(currentEvent);
        }
    }

    private void processMouseLeftDragEvent(Event currentEvent)
    {
        isDragging = true;
        DragMode(currentEvent.delta);
        GUI.changed = true;
    }

    private void DragMode(Vector2 delta)
    {
        rect.position += delta;
        EditorUtility.SetDirty(this);
    }

    private void processMouseUpEvent(Event currentEvent)
    {
        if (currentEvent.button == 0)
        {
            processLeftClickUpEvent();
        }
    }

    private void processLeftClickUpEvent()
    {
        if (isDragging)
        {
            isDragging = false;
        }
    }

    private void processMouseDownEvent(Event currentEvent)
    {
        if (currentEvent.button == 0)
        {
            processLeftClickDownEvent();
        }
        else if (currentEvent.button == 1)
        {
            processRightClickDownEvent(currentEvent);
        }
    }

    private void processRightClickDownEvent(Event currentEvent)
    {
        roomNodeGraph.SetLineFrom(this, currentEvent.mousePosition);
    }

    private void processLeftClickDownEvent()
    {
        // set selected object in project pannel to this
        Selection.activeObject = this;

        // update isSelected
        isSelected = !isSelected;
    }

    public bool addChildRoomId(string child)
    {
        if (isChildRoomValid(child))
        {
            childRoomNodeIDList.Add(child);
            return true;
        }
        return false;
    }

    private bool isChildRoomValid(string child)
    {
        bool isConnectedBossNodeAlready = false;

        // is boss room connected as child
        foreach (RoomNodeSO room in roomNodeGraph.roomNodeList)
        {
            if (room.roomNodeType.isBossRoom && room.parentRoomNodeIDList.Count > 0)
            {
                isConnectedBossNodeAlready = true;
            }
        }

        // is child room a boss room and boss already exist
        if (roomNodeGraph.GetRoomNode(child).roomNodeType.isBossRoom && isConnectedBossNodeAlready)
        {
            return false;
        }

        // return false if child room is none type
        if (roomNodeGraph.GetRoomNode(child).roomNodeType.isNone)
        {
            return false;
        }

        // avoid duplicated child
        if (childRoomNodeIDList.Contains(child))
        {
            return false;
        }

        // void connecting to itself
        if (id == child)
        {
            return false;
        }

        // return false if this child is one of the parent already
        if (parentRoomNodeIDList.Contains(child))
        {
            return false;
        }

        // only allow a room have single parent
        if (roomNodeGraph.GetRoomNode(child).parentRoomNodeIDList.Count > 0)
        {
            return false;
        }

        // not allow connecting corridor to corridor
        if (roomNodeType.isCorridor && roomNodeGraph.GetRoomNode(child).roomNodeType.isCorridor)
        {
            return false;
        }

        // one of the node must be corridor
        if (!roomNodeType.isCorridor && !roomNodeGraph.GetRoomNode(child).roomNodeType.isCorridor)
        {
            return false;
        }

        // if child is corridor and reaches max corridor allowed
        if (roomNodeGraph.GetRoomNode(child).roomNodeType.isCorridor && childRoomNodeIDList.Count >= Setting.maxChildCorridors)
        {
            return false;
        }

        // child must not be an entrance
        if (roomNodeGraph.GetRoomNode(child).roomNodeType.isEntrance)
        {
            return false;
        }

        // if child not is not corridor (meaning this node is corridor)
        // a corridor can only attach to one room, so return false if child > 0
        if (!roomNodeGraph.GetRoomNode(child).roomNodeType.isCorridor && childRoomNodeIDList.Count > 0)
        {
            return false;
        }



        return true;
    }

    public bool AddParentRoomId(string parent)
    {
        parentRoomNodeIDList.Add(parent);
        return true;
    }

    public bool RemoveChildRoom(string id)
    {
        if (childRoomNodeIDList.Contains(id))
        {
            childRoomNodeIDList.Remove(id);
            return true;
        }
        return false;
    }

    public bool RemoveParentRoom(string id)
    {
        if (parentRoomNodeIDList.Contains(id))
        {
            parentRoomNodeIDList.Remove(id);
            return true;
        }
        return false;
    }

#endif

    #endregion
}
