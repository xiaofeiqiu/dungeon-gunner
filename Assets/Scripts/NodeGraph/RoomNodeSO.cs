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

    // Initializes the RoomNode with position, graph reference, and type
    public void Init(Rect rect, RoomNodeGraphSO nodeGraphSO, RoomNodeTypeSO typeSO)
    {
        this.rect = rect;
        this.id = Guid.NewGuid().ToString(); // Generate a unique ID for the node
        this.name = "RoomNode"; // Set default name
        this.roomNodeGraph = nodeGraphSO; // Reference to the node graph
        this.roomNodeType = typeSO; // Set the type of room node

        roomNodeTypeList = GameResource.Instance.RoomNodeTypeListSO; // Get room node type list from resources
    }

    // Draws the node in the Unity Editor
    public void Draw(GUIStyle nodeStyle)
    {
        GUILayout.BeginArea(rect, nodeStyle);

        EditorGUI.BeginChangeCheck(); // Start checking for changes

        // If the room node has a parent or is an entrance node, display a label
        if (parentRoomNodeIDList.Count > 0 || roomNodeType.isEntrance)
        {
            EditorGUILayout.LabelField(roomNodeType.roomNodeTypeName);
        }
        else
        {
            // Default selected type
            int selected = roomNodeTypeList.list.FindIndex(x => x == roomNodeType);

            // Display dropdown for selecting room type
            int selection = EditorGUILayout.Popup("", selected, GetRoomNodeTypeToDisplay());
            roomNodeType = roomNodeTypeList.list[selection];

            // Handle changes in room type affecting child connections
            if (roomNodeTypeList.list[selected].isCorridor && !roomNodeTypeList.list[selection].isCorridor
                || !roomNodeTypeList.list[selected].isCorridor && roomNodeTypeList.list[selection].isCorridor
                || !roomNodeTypeList.list[selected].isBossRoom && roomNodeTypeList.list[selection].isBossRoom)
            {
                // If there are child room nodes
                if (childRoomNodeIDList.Count > 0)
                {
                    // Remove all child connections
                    for (int i = childRoomNodeIDList.Count - 1; i >= 0; i--)
                    {
                        RoomNodeSO childRoomNode = roomNodeGraph.GetRoomNode(childRoomNodeIDList[i]);
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

        // If any changes were made, mark the object as dirty
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(this);
        }
        GUILayout.EndArea();
    }

    // Get the display names for the room node types
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

    // Process input events in the Unity Editor
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

    // Handle mouse drag events
    private void processMouseDragEvent(Event currentEvent)
    {
        if (currentEvent.button == 0)
        {
            processMouseLeftDragEvent(currentEvent);
        }
    }

    // Handle left mouse button drag events
    private void processMouseLeftDragEvent(Event currentEvent)
    {
        isDragging = true; // Set dragging flag
        DragNode(currentEvent.delta); // Move node
        GUI.changed = true; // Indicate GUI has changed
    }

    // Move the node by a delta amount
    public void DragNode(Vector2 delta)
    {
        rect.position += delta; // Update position
        EditorUtility.SetDirty(this); // Mark object as dirty
    }

    // Handle mouse button up events
    private void processMouseUpEvent(Event currentEvent)
    {
        if (currentEvent.button == 0)
        {
            processLeftClickUpEvent(); // Handle left click up
        }
    }

    // Handle left mouse button up events
    private void processLeftClickUpEvent()
    {
        if (isDragging)
        {
            isDragging = false; // Stop dragging
        }
    }

    // Handle mouse button down events
    private void processMouseDownEvent(Event currentEvent)
    {
        if (currentEvent.button == 0)
        {
            processLeftClickDownEvent(); // Handle left click down
        }
        else if (currentEvent.button == 1)
        {
            processRightClickDownEvent(currentEvent); // Handle right click down
        }
    }

    // Handle right mouse button down events
    private void processRightClickDownEvent(Event currentEvent)
    {
        roomNodeGraph.SetLineFrom(this, currentEvent.mousePosition); // Set line start position
    }

    // Handle left mouse button down events
    private void processLeftClickDownEvent()
    {
        Selection.activeObject = this; // Select this object
        isSelected = !isSelected; // Toggle selection
    }

    // Add a child room node ID
    public bool addChildRoomId(string child)
    {
        if (isChildRoomValid(child)) // Validate child room
        {
            childRoomNodeIDList.Add(child); // Add to child list
            return true;
        }
        return false;
    }

    // Validate if a child room node ID can be added
    private bool isChildRoomValid(string child)
    {
        bool isConnectedBossNodeAlready = false;

        // Check if a boss room is already connected
        foreach (RoomNodeSO room in roomNodeGraph.roomNodeList)
        {
            if (room.roomNodeType.isBossRoom && room.parentRoomNodeIDList.Count > 0)
            {
                isConnectedBossNodeAlready = true;
            }
        }

        // Validate various conditions for adding a child room node
        if (roomNodeGraph.GetRoomNode(child).roomNodeType.isBossRoom && isConnectedBossNodeAlready)
        {
            return false;
        }

        if (roomNodeGraph.GetRoomNode(child).roomNodeType.isNone)
        {
            return false;
        }

        if (childRoomNodeIDList.Contains(child))
        {
            return false;
        }

        if (id == child)
        {
            return false;
        }

        if (parentRoomNodeIDList.Contains(child))
        {
            return false;
        }

        if (roomNodeGraph.GetRoomNode(child).parentRoomNodeIDList.Count > 0)
        {
            return false;
        }

        if (roomNodeType.isCorridor && roomNodeGraph.GetRoomNode(child).roomNodeType.isCorridor)
        {
            return false;
        }

        if (!roomNodeType.isCorridor && !roomNodeGraph.GetRoomNode(child).roomNodeType.isCorridor)
        {
            return false;
        }

        if (roomNodeGraph.GetRoomNode(child).roomNodeType.isCorridor && childRoomNodeIDList.Count >= Setting.maxChildCorridors)
        {
            return false;
        }

        if (roomNodeGraph.GetRoomNode(child).roomNodeType.isEntrance)
        {
            return false;
        }

        if (!roomNodeGraph.GetRoomNode(child).roomNodeType.isCorridor && childRoomNodeIDList.Count > 0)
        {
            return false;
        }

        return true;
    }

    // Add a parent room node ID
    public bool AddParentRoomId(string parent)
    {
        parentRoomNodeIDList.Add(parent); // Add to parent list
        return true;
    }

    // Remove a child room node ID
    public bool RemoveChildRoom(string id)
    {
        if (childRoomNodeIDList.Contains(id))
        {
            childRoomNodeIDList.Remove(id); // Remove from child list
            return true;
        }
        return false;
    }

    // Remove a parent room node ID
    public bool RemoveParentRoom(string id)
    {
        if (parentRoomNodeIDList.Contains(id))
        {
            parentRoomNodeIDList.Remove(id); // Remove from parent list
            return true;
        }
        return false;
    }

#endif

    #endregion
}
