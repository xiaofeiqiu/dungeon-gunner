using UnityEngine;
using UnityEditor.Callbacks;
using UnityEditor;
using System;
using System.Collections.Generic;

public class RoomNodeGraphEditor : EditorWindow
{
    private static RoomNodeGraphSO currentRoomNodeGraph;
    private RoomNodeTypeListSO roomNodeTypeListSO;
    private RoomNodeSO currentRoomNode;

    private GUIStyle roomNodeStyle;
    private GUIStyle selectedStyle;
    private const float nodeWidth = 160f;
    private const float nodeHeight = 75f;
    private const int nodePadding = 25;
    private const int nodeBorder = 12;
    private const int connectingLineWidth = 3;

    private float arrowSize = 10f;

    [MenuItem("Window/Dungeon Editor/Room Node Graph Editor")]
    public static void ShowWindow()
    {
        RoomNodeGraphEditor window = GetWindow<RoomNodeGraphEditor>("Room Node Graph Editor");
        window.Show();
    }

    private void OnEnable()
    {
        // call InspectorSelectionChanged function when unity detects selection changed in unity
        // += registers InspectorSelectionChanged function as cahnge detection callback
        Selection.selectionChanged += inspectorSelectionChanged;

        roomNodeStyle = new GUIStyle();
        roomNodeStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
        roomNodeStyle.normal.textColor = Color.white;
        roomNodeStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        roomNodeStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);

        selectedStyle = new GUIStyle();
        selectedStyle.normal.background = EditorGUIUtility.Load("node1 on") as Texture2D;
        selectedStyle.normal.textColor = Color.white;
        selectedStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        selectedStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);


        roomNodeTypeListSO = GameResource.Instance.RoomNodeTypeListSO;
    }

    private void OnDisable()
    {
        Selection.selectionChanged -= inspectorSelectionChanged;
    }

    private void inspectorSelectionChanged()
    {
        RoomNodeGraphSO roomNodeGraph = Selection.activeObject as RoomNodeGraphSO;
        if (roomNodeGraph != null)
        {
            currentRoomNodeGraph = roomNodeGraph;
            ShowWindow();
            GUI.changed = true;            
        }
    }

    // method will be called when double click asset
    [OnOpenAsset(0)] 
    public static bool OnDoubleClickAsset(int instanceID, int line)
    {
        // if clicked asset is not RoomNodeGraphSO return false
        RoomNodeGraphSO roomNodeGraph = EditorUtility.InstanceIDToObject(instanceID) as RoomNodeGraphSO;
        if (roomNodeGraph == null)
        {
            return false;
        }

        // if clicked asset is RoomNodeGraphSO, open window, set current node graph, return true
        ShowWindow();
        currentRoomNodeGraph = roomNodeGraph;
        return true;
    }

    /*
     * listent to gui event, if mouse right click, popup menu window
     * user select create node, then node will be initialized including its field values and node size
     * when node is created, call drawRoomNodes to render created room node
     */

    private void OnGUI()
    {
        Debug.Log("OnGUI:" + GUI.changed);
        if (currentRoomNodeGraph != null)
        {
            DrawDraggedLine();

            processeEvents(Event.current);

            drawRoonNodeConnection();

            drawRoomNodes();
        }

        // render if changed
        if (GUI.changed)
        {
            Repaint();
        }
    }

    private void drawRoonNodeConnection()
    {
        foreach (RoomNodeSO room in currentRoomNodeGraph.roomNodeList)
        {
            if (room.childRoomNodeIDList.Count > 0)
            {
                foreach (string childId in room.childRoomNodeIDList)
                {
                    if (currentRoomNodeGraph.roomNodeSODict.ContainsKey(childId))
                    {
                        DrawConnectionLine(room, currentRoomNodeGraph.roomNodeSODict[childId]);
                        GUI.changed = true;
                    }
                }
            }
        }
    }

    private void DrawConnectionLine(RoomNodeSO from, RoomNodeSO to)
    {
        Vector2 start = from.rect.center;
        Vector2 end = to.rect.center;

        Handles.DrawBezier(start, end, start, end, Color.white, null, connectingLineWidth);

        drawArrow(start, end);

        GUI.changed = true;
    }

    private void drawArrow(Vector2 start, Vector2 end)
    {
        // Calculate the midpoint of the bezier curve
        Vector2 midpoint = (start + end) / 2;

        // Calculate the direction vector at the midpoint
        Vector2 direction = (end - start).normalized;

        // Calculate the perpendicular vector to create the arrow shape
        Vector2 perpendicular = new Vector2(-direction.y, direction.x);

        // Define the points of the arrow
        Vector2 arrowTip = midpoint + direction * arrowSize;
        Vector2 arrowLeft = midpoint - direction * arrowSize * 0.5f + perpendicular * arrowSize * 0.5f;
        Vector2 arrowRight = midpoint - direction * arrowSize * 0.5f - perpendicular * arrowSize * 0.5f;

        // Draw the arrow
        Handles.color = Color.white;
        Handles.DrawAAPolyLine(connectingLineWidth, new Vector3[] { arrowLeft, arrowTip, arrowRight });
    }

    private void DrawDraggedLine()
    {
        if (currentRoomNodeGraph.linePosition != Vector2.zero)
        {
            Handles.DrawBezier(currentRoomNodeGraph.fromNode.rect.center, currentRoomNodeGraph.linePosition,
                currentRoomNodeGraph.fromNode.rect.center, currentRoomNodeGraph.linePosition, Color.white, null, connectingLineWidth);
        }
    }

    private void processeEvents(Event currentEvent)
    {
        
        if (currentRoomNode == null || currentRoomNode.isDragging == false)
        {
            currentRoomNode = isMouseOverRoomNode(currentEvent);
        }

        // if mouse not over a node, or we are drawing a line
        if (currentRoomNode == null || currentRoomNodeGraph.fromNode != null)
        {
            processRoomNodeGraphEvents(currentEvent);
        }
        else
        {
            currentRoomNode.ProcessEvents(currentEvent);
        }
    }

    private RoomNodeSO isMouseOverRoomNode(Event currentEvent)
    {
        for (int i = currentRoomNodeGraph.roomNodeList.Count - 1;  i >= 0; i--)
        {
            if (currentRoomNodeGraph.roomNodeList[i].rect.Contains(currentEvent.mousePosition))
            {
                return currentRoomNodeGraph.roomNodeList[i];
            }
        }
        return null;
    }

    private void processRoomNodeGraphEvents(Event currentEvent)
    {
        switch(currentEvent.type)
        {
            case EventType.MouseDown:
                processMouseDownEvent(currentEvent);
                break;
            case EventType.MouseDrag:
                processMouseDragEvent(currentEvent);
                break;
            case EventType.MouseUp:
                processMouseUpEvent(currentEvent);
                break;
            default:
                break;
        }
    }

    private void processMouseUpEvent(Event currentEvent)
    {
        // if releasing the right mouse button, and currently dragging a line
        if (currentEvent.button == 1 && currentRoomNodeGraph.fromNode != null)
        {
            // check if over a node
            RoomNodeSO node = isMouseOverRoomNode(currentEvent);
            if (node != null)
            {
                // if so, set child and parent node
                if (currentRoomNodeGraph.fromNode.addChildRoomId(node.id))
                {
                    node.AddParentRoomId(currentRoomNodeGraph.fromNode.id);
                }             
            }
            clearLine();
        }
    }

    private void clearLine()
    {
        currentRoomNodeGraph.fromNode = null;
        currentRoomNodeGraph.linePosition = Vector2.zero;
        GUI.changed = true;
    }

    private void processMouseDragEvent(Event currentEvent)
    {
        if (currentEvent.button == 1)
        {
            processRightMouseDragEvent(currentEvent);
        }
    }

    private void processRightMouseDragEvent(Event currentEvent)
    {
        if (currentRoomNodeGraph.fromNode != null)
        {
            dragConnectingLine(currentEvent.delta);
            GUI.changed = true;
        }
    }

    private void dragConnectingLine(Vector2 delta)
    {
        currentRoomNodeGraph.linePosition += delta;
    }

    private void processMouseDownEvent(Event currentEvent)
    {
        // if mouse right click, show menu
        if (currentEvent.button == 1)
        {
            showContextMenu(currentEvent.mousePosition);
        }
        else if (currentEvent.button == 0)
        {
            clearLine();
            clearAllSelectedRoomNodes();

        }
    }

    private void clearAllSelectedRoomNodes()
    {
        foreach (RoomNodeSO node in currentRoomNodeGraph.roomNodeList)
        {
            if (node.isSelected)
            {
                node.isSelected = false;
                GUI.changed = true;
            }
        }
    }

    /* 
     * showContextMenu will show menu based on mouse position
     * when clicked, it will call createRoomNode function, and pass mousePosition to the funciton
    */
    private void showContextMenu(Vector2 mousePosition)
    {
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("Create room node"), false, createRoomNode, mousePosition);
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Select all"), false, selectAllRoomNodes);
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Delete links"), false, deleteLinks);
        menu.AddItem(new GUIContent("Delete selected nodes"), false, deleteNodes);
        menu.ShowAsContext();
    }

    private void deleteNodes()
    {
        Queue<RoomNodeSO> queue = new Queue<RoomNodeSO>();

        foreach (var node in currentRoomNodeGraph.roomNodeList)
        {
            if (node.isSelected && !node.roomNodeType.isEntrance)
            {
                queue.Enqueue(node);
                foreach (var childId in node.childRoomNodeIDList)
                {
                    RoomNodeSO childNode = currentRoomNodeGraph.GetRoomNode(childId);
                    if (childNode != null)
                    {
                        childNode.RemoveParentRoom(node.id);
                    }
                }

                foreach (var parentId in node.parentRoomNodeIDList)
                {
                    RoomNodeSO parentNode = currentRoomNodeGraph.GetRoomNode(parentId);
                    if (parentNode != null)
                    {
                        parentNode.RemoveChildRoom(node.id);
                    }
                }
            }
        }

        while (queue.Count > 0)
        {
            RoomNodeSO node = queue.Dequeue();
            currentRoomNodeGraph.roomNodeSODict.Remove(node.id);
            currentRoomNodeGraph.roomNodeList.Remove(node);
            DestroyImmediate(node, true);
            AssetDatabase.SaveAssets();
        }
    }

    private void deleteLinks()
    {
        foreach (var room in currentRoomNodeGraph.roomNodeList)
        {
            if (room.isSelected && room.childRoomNodeIDList.Count > 0)
            {
                for (int i = room.childRoomNodeIDList.Count - 1; i >= 0; i--)
                {
                    RoomNodeSO childRoom = currentRoomNodeGraph.GetRoomNode(room.childRoomNodeIDList[i]);
                    if (childRoom != null && childRoom.isSelected)
                    {
                        room.RemoveChildRoom(childRoom.id);
                        childRoom.RemoveParentRoom(room.id);
                    }
                }
            }
        }

        clearAllSelectedRoomNodes();
    }

    private void selectAllRoomNodes()
    {
        foreach (var room in currentRoomNodeGraph.roomNodeList)
        {
            room.isSelected = true;
        }
        GUI.changed = true;
    }

    // createRoomNode will create a room node, by defautl, room type is none
    private void createRoomNode(object mousePositionObj)
    {
        if (currentRoomNodeGraph.roomNodeList.Count == 0)
        {
            createRoomNode(new Vector2(200f, 200f), roomNodeTypeListSO.list.Find(x => x.isEntrance));
        }
        createRoomNode(mousePositionObj, roomNodeTypeListSO.list.Find(x => x.isNone));
        
    }

    // creates room node asset, and save the object under currentRoomNodeGraph obejct
    private void createRoomNode(object mousePositionObj, RoomNodeTypeSO roomNodeTypeSO)
    {
        Vector2 mousePos = (Vector2)mousePositionObj;

        RoomNodeSO roomNode = ScriptableObject.CreateInstance<RoomNodeSO>();
        currentRoomNodeGraph.roomNodeList.Add(roomNode);

        /*
         * init roomNode, including position & size of the node.
         */
        roomNode.Init(new Rect(mousePos, new Vector2(nodeWidth, nodeHeight)), currentRoomNodeGraph, roomNodeTypeSO);

        AssetDatabase.AddObjectToAsset(roomNode, currentRoomNodeGraph);
        AssetDatabase.SaveAssets();

        // save new created room in dict
        currentRoomNodeGraph.OnValidate();
    }

    private void drawRoomNodes()
    {
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected)
            {
                roomNode.Draw(selectedStyle);
            } else
            {
                roomNode.Draw(roomNodeStyle);
            }
            
        }
    }


}
