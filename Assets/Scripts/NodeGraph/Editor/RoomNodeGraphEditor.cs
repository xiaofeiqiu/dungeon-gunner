using UnityEngine;
using UnityEditor.Callbacks;
using UnityEditor;
using System;

public class RoomNodeGraphEditor : EditorWindow
{
    private static RoomNodeGraphSO currentRoomNodeGraph;
    private RoomNodeTypeListSO roomNodeTypeListSO;

    private GUIStyle roomNodeStyle;
    private const float nodeWidth = 160f;
    private const float nodeHeight = 75f;
    private const int nodePadding = 25;
    private const int nodeBorder = 12;

    [MenuItem("Window/Dungeon Editor/Room Node Graph Editor")]
    public static void ShowWindow()
    {
        RoomNodeGraphEditor window = GetWindow<RoomNodeGraphEditor>("Room Node Graph Editor");
        window.Show();
    }

    private void OnEnable()
    {
        roomNodeStyle = new GUIStyle();
        roomNodeStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
        roomNodeStyle.normal.textColor = Color.white;
        roomNodeStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        roomNodeStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);

        roomNodeTypeListSO = GameResource.Instance.RoomNodeTypeListSO;
    }


    [OnOpenAsset(0)] // method will be called when double click asset
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
        if (currentRoomNodeGraph != null)
        {
            processeEvents(Event.current);

            drawRoomNodes();
        }

        // render if changed
        if (GUI.changed)
        {
            Repaint();
        }
    }

    private void processeEvents(Event currentEvent)
    {
        processRoomNodeGraphEvents(currentEvent);
    }

    private void processRoomNodeGraphEvents(Event currentEvent)
    {
        switch(currentEvent.type)
        {
            case EventType.MouseDown:
                processMouseDownEvent(currentEvent);
                break;
            default:
                break;
        }
    }

    private void processMouseDownEvent(Event currentEvent)
    {
        // if mouse right click, show menu
        if (currentEvent.button == 1)
        {
            showContextMenu(currentEvent.mousePosition);
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
        menu.ShowAsContext();
    }

    // createRoomNode will create a room node, by defautl, room type is none
    private void createRoomNode(object mousePositionObj)
    {
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

    }

    private void drawRoomNodes()
    {
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            roomNode.Draw(roomNodeStyle);
        }
    }


}
