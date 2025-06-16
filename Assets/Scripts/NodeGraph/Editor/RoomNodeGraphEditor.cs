// RoomNodeGraphEditor.cs - Custom Unity Editor window for creating and managing room node graphs
// This editor allows users to create, connect, and manage room nodes for dungeon generation

using UnityEngine;
using UnityEditor.Callbacks;
using UnityEditor;
using System;
using System.Collections.Generic;

public class RoomNodeGraphEditor : EditorWindow
{
    // Current room node graph being edited
    private static RoomNodeGraphSO currentRoomNodeGraph;
    // List of available room node types
    private RoomNodeTypeListSO roomNodeTypeListSO;
    // Currently selected room node
    private RoomNodeSO currentRoomNode;

    // Graph view offset and drag values for panning
    private Vector2 graphOffset;
    private Vector2 graphDrag;
    // Grid sizes for visual reference
    private const float gridLarge = 100f;
    private const float gridsmall = 25f;

    // Styles for room nodes
    private GUIStyle roomNodeStyle;
    private GUIStyle selectedStyle;
    // Node dimensions and styling constants
    private const float nodeWidth = 160f;
    private const float nodeHeight = 75f;
    private const int nodePadding = 25;
    private const int nodeBorder = 12;
    private const int connectingLineWidth = 3;

    // Size of connection arrows
    private float arrowSize = 10f;

    // Menu item to open the editor window
    [MenuItem("Window/Dungeon Editor/Room Node Graph Editor")]
    public static void ShowWindow()
    {
        RoomNodeGraphEditor window = GetWindow<RoomNodeGraphEditor>("Room Node Graph Editor");
        window.Show();
    }

    private void OnEnable()
    {
        // Subscribe to selection change events
        Selection.selectionChanged += inspectorSelectionChanged;

        // Initialize node styles
        roomNodeStyle = new GUIStyle();
        roomNodeStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
        roomNodeStyle.normal.textColor = Color.white;
        roomNodeStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        roomNodeStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);

        // Initialize selected node style
        selectedStyle = new GUIStyle();
        selectedStyle.normal.background = EditorGUIUtility.Load("node1 on") as Texture2D;
        selectedStyle.normal.textColor = Color.white;
        selectedStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        selectedStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);

        // Get room node type list from game resources
        roomNodeTypeListSO = GameResource.Instance.RoomNodeTypeListSO;
    }

    private void OnDisable()
    {
        // Unsubscribe from selection change events
        Selection.selectionChanged -= inspectorSelectionChanged;
    }

    // Called when selection changes in the Unity Inspector
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

    // Called when double-clicking an asset in the Project window
    [OnOpenAsset(0)]
    public static bool OnDoubleClickAsset(int instanceID, int line)
    {
        RoomNodeGraphSO roomNodeGraph = EditorUtility.InstanceIDToObject(instanceID) as RoomNodeGraphSO;
        if (roomNodeGraph == null)
        {
            return false;
        }

        ShowWindow();
        currentRoomNodeGraph = roomNodeGraph;
        return true;
    }

    // Main GUI update method
    private void OnGUI()
    {
        if (currentRoomNodeGraph != null)
        {
            // Draw background grid
            DrawBackgroundGrid(gridsmall, 0.2f, Color.gray);
            DrawBackgroundGrid(gridLarge, 0.3f, Color.gray);

            // Draw connecting lines
            DrawDraggedLine();

            // Process user input events
            processeEvents(Event.current);

            // Draw node connections
            drawRoonNodeConnection();

            // Draw room nodes
            drawRoomNodes();
        }

        // Repaint if changes were made
        if (GUI.changed)
        {
            Repaint();
        }
    }

    // Draws the background grid for visual reference
    private void DrawBackgroundGrid(float gridSize, float gridOpacity, Color gridColor)
    {
        // Calculate number of grid lines needed
        int verticalLineCount = Mathf.CeilToInt((position.width + gridSize) / gridSize);
        int horizontalLineCount = Mathf.CeilToInt((position.height + gridSize) / gridSize);

        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

        // Update grid offset based on drag
        graphOffset += graphDrag * 0.5f;

        Vector3 gridOffset = new Vector3(graphOffset.x % gridSize, graphOffset.y % gridSize, 0f);

        // Draw vertical grid lines
        for (int i = 0; i < verticalLineCount; i++)
        {
            Handles.DrawLine(new Vector3(gridSize * i, -gridSize, 0) + gridOffset, 
                           new Vector3(gridSize * i, position.height + gridSize, 0f) + gridOffset);
        }

        // Draw horizontal grid lines
        for (int j = 0; j < horizontalLineCount; j++)
        {
            Handles.DrawLine(new Vector3(-gridSize, gridSize * j, 0) + gridOffset, 
                           new Vector3(position.width + gridSize, gridSize * j, 0f) + gridOffset);
        }

        Handles.color = Color.white;
    }

    // Draws connections between room nodes
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

    // Draws a connection line between two room nodes
    private void DrawConnectionLine(RoomNodeSO from, RoomNodeSO to)
    {
        Vector2 start = from.rect.center;
        Vector2 end = to.rect.center;

        // Draw bezier curve between nodes
        Handles.DrawBezier(start, end, start, end, Color.white, null, connectingLineWidth);

        // Draw arrow indicating direction
        drawArrow(start, end);

        GUI.changed = true;
    }

    // Draws an arrow at the midpoint of a connection
    private void drawArrow(Vector2 start, Vector2 end)
    {
        // Calculate arrow position and direction
        Vector2 midpoint = (start + end) / 2;
        Vector2 direction = (end - start).normalized;
        Vector2 perpendicular = new Vector2(-direction.y, direction.x);

        // Calculate arrow points
        Vector2 arrowTip = midpoint + direction * arrowSize;
        Vector2 arrowLeft = midpoint - direction * arrowSize * 0.5f + perpendicular * arrowSize * 0.5f;
        Vector2 arrowRight = midpoint - direction * arrowSize * 0.5f - perpendicular * arrowSize * 0.5f;

        // Draw arrow shape
        Handles.color = Color.white;
        Handles.DrawAAPolyLine(connectingLineWidth, new Vector3[] { arrowLeft, arrowTip, arrowRight });
    }

    // Draws the line being dragged for new connections
    private void DrawDraggedLine()
    {
        if (currentRoomNodeGraph.linePosition != Vector2.zero)
        {
            Handles.DrawBezier(currentRoomNodeGraph.fromNode.rect.center, currentRoomNodeGraph.linePosition,
                currentRoomNodeGraph.fromNode.rect.center, currentRoomNodeGraph.linePosition, Color.white, null, connectingLineWidth);
        }
    }

    // Process user input events
    private void processeEvents(Event currentEvent)
    {
        graphDrag = Vector2.zero;

        // Check if mouse is over a room node
        if (currentRoomNode == null || currentRoomNode.isDragging == false)
        {
            currentRoomNode = isMouseOverRoomNode(currentEvent);
        }

        // Process events based on context
        if (currentRoomNode == null || currentRoomNodeGraph.fromNode != null)
        {
            processRoomNodeGraphEvents(currentEvent);
        }
        else
        {
            currentRoomNode.ProcessEvents(currentEvent);
        }
    }

    // Check if mouse is over any room node
    private RoomNodeSO isMouseOverRoomNode(Event currentEvent)
    {
        for (int i = currentRoomNodeGraph.roomNodeList.Count - 1; i >= 0; i--)
        {
            if (currentRoomNodeGraph.roomNodeList[i].rect.Contains(currentEvent.mousePosition))
            {
                return currentRoomNodeGraph.roomNodeList[i];
            }
        }
        return null;
    }

    // Process graph-level events
    private void processRoomNodeGraphEvents(Event currentEvent)
    {
        switch (currentEvent.type)
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

    // Handle mouse up events
    private void processMouseUpEvent(Event currentEvent)
    {
        // Handle connection creation
        if (currentEvent.button == 1 && currentRoomNodeGraph.fromNode != null)
        {
            RoomNodeSO node = isMouseOverRoomNode(currentEvent);
            if (node != null)
            {
                // Create connection between nodes
                if (currentRoomNodeGraph.fromNode.addChildRoomId(node.id))
                {
                    node.AddParentRoomId(currentRoomNodeGraph.fromNode.id);
                }
            }
            clearLine();
        }
    }

    // Clear the current connection line
    private void clearLine()
    {
        currentRoomNodeGraph.fromNode = null;
        currentRoomNodeGraph.linePosition = Vector2.zero;
        GUI.changed = true;
    }

    // Handle mouse drag events
    private void processMouseDragEvent(Event currentEvent)
    {
        if (currentEvent.button == 1)
        {
            processRightMouseDragEvent(currentEvent);
        }
        else if (currentEvent.button == 0)
        {
            processLeftMouseDragEvent(currentEvent.delta);
        }
    }

    // Handle left mouse drag (pan graph)
    private void processLeftMouseDragEvent(Vector2 delta)
    {
        graphDrag = delta;

        // Move all nodes
        for (int i = 0; i < currentRoomNodeGraph.roomNodeList.Count; i++)
        {
            currentRoomNodeGraph.roomNodeList[i].DragNode(delta);
        }
        GUI.changed = true;
    }

    // Handle right mouse drag (draw connection line)
    private void processRightMouseDragEvent(Event currentEvent)
    {
        if (currentRoomNodeGraph.fromNode != null)
        {
            dragConnectingLine(currentEvent.delta);
            GUI.changed = true;
        }
    }

    // Update connection line position during drag
    private void dragConnectingLine(Vector2 delta)
    {
        currentRoomNodeGraph.linePosition += delta;
    }

    // Handle mouse down events
    private void processMouseDownEvent(Event currentEvent)
    {
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

    // Clear selection from all room nodes
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

    // Show context menu on right click
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

    // Delete selected nodes
    private void deleteNodes()
    {
        Queue<RoomNodeSO> queue = new Queue<RoomNodeSO>();

        // Collect nodes to delete
        foreach (var node in currentRoomNodeGraph.roomNodeList)
        {
            if (node.isSelected && !node.roomNodeType.isEntrance)
            {
                queue.Enqueue(node);
                // Remove connections
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

        // Delete nodes and save changes
        while (queue.Count > 0)
        {
            RoomNodeSO node = queue.Dequeue();
            currentRoomNodeGraph.roomNodeSODict.Remove(node.id);
            currentRoomNodeGraph.roomNodeList.Remove(node);
            DestroyImmediate(node, true);
            AssetDatabase.SaveAssets();
        }
    }

    // Delete connections between selected nodes
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

    // Select all room nodes
    private void selectAllRoomNodes()
    {
        foreach (var room in currentRoomNodeGraph.roomNodeList)
        {
            room.isSelected = true;
        }
        GUI.changed = true;
    }

    // Create a new room node at mouse position
    private void createRoomNode(object mousePositionObj)
    {
        // Create entrance node if this is the first node
        if (currentRoomNodeGraph.roomNodeList.Count == 0)
        {
            createRoomNode(new Vector2(200f, 200f), roomNodeTypeListSO.list.Find(x => x.isEntrance));
        }
        // Create normal room node
        createRoomNode(mousePositionObj, roomNodeTypeListSO.list.Find(x => x.isNone));
    }

    // Create and initialize a new room node
    private void createRoomNode(object mousePositionObj, RoomNodeTypeSO roomNodeTypeSO)
    {
        Vector2 mousePos = (Vector2)mousePositionObj;

        // Create new room node
        RoomNodeSO roomNode = ScriptableObject.CreateInstance<RoomNodeSO>();
        currentRoomNodeGraph.roomNodeList.Add(roomNode);

        // Initialize node properties
        roomNode.Init(new Rect(mousePos, new Vector2(nodeWidth, nodeHeight)), currentRoomNodeGraph, roomNodeTypeSO);

        // Save node as asset
        AssetDatabase.AddObjectToAsset(roomNode, currentRoomNodeGraph);
        AssetDatabase.SaveAssets();

        // Update node dictionary
        currentRoomNodeGraph.OnValidate();
    }

    // Draw all room nodes
    private void drawRoomNodes()
    {
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected)
            {
                roomNode.Draw(selectedStyle);
            }
            else
            {
                roomNode.Draw(roomNodeStyle);
            }
        }
    }
}