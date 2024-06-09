using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RoomNodeSO : ScriptableObject
{
    [HideInInspector] public string id;
    [HideInInspector] public List<string> parentRoomNodeIDList = new List<string>();
    [HideInInspector] public List<string> childRoomNodeIDList = new List<string>();
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
        } else
        {
            // default selected
            int selected = roomNodeTypeList.list.FindIndex(x => x == roomNodeType);

            // display selections
            int selection = EditorGUILayout.Popup("", selected, GetRoomNodeTypeToDisplay());
            roomNodeType = roomNodeTypeList.list[selection];
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
        childRoomNodeIDList.Add(child);
        return true;
    }

    public bool addParentRoomId(string parent)
    {
        parentRoomNodeIDList.Add(parent);
        return true;
    }

#endif

    #endregion
}
