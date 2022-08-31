using RoR2;
using RoR2.Navigation;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapNodeGroup))]
public class MapNodeEditor : Editor
{
    private Vector3 currentHitInfo = default;

    /// <summary>
    /// Radius of the painter.
    /// </summary>
    private int currentPainterSize = 0;

    private static Vector3 offsetUpVector = new Vector3(0, 15, 0);

    private void OnSceneGUI()
    {
        MapNodeGroup mapNodeGroup = (MapNodeGroup)target;

        Cursor.visible = true;

        // You'll need a control id to avoid messing with other tools!
        int controlID = GUIUtility.GetControlID(FocusType.Keyboard | FocusType.Passive);
        var cachedMapNodeList = mapNodeGroup.GetNodes();
        float currentMaxDistance = mapNodeGroup.graphType == MapNodeGroup.GraphType.Air ? (MapNode.maxConnectionDistance * 2) - 2 : MapNode.maxConnectionDistance;
        float zPainterOffset = currentMaxDistance / 2;

        if (Event.current.GetTypeForControl(controlID) == EventType.KeyDown)
        {
            if (Event.current.keyCode == KeyCode.B)
            {
                Cursor.visible ^= true;
            }

            if (Event.current.keyCode == KeyCode.N)
            {
                Debug.Log(currentHitInfo);
                if (currentPainterSize <= 0f)
                {
                    mapNodeGroup.AddNode(currentHitInfo);
                }
                else
                {
                    for (float x = currentHitInfo.x - currentPainterSize, zCount = 0; x <= currentHitInfo.x; x += currentMaxDistance - 4, zCount++)
                    {
                        for (float z = currentHitInfo.z - currentPainterSize; z <= currentHitInfo.z; z += currentMaxDistance - 4)
                        {
                            //Haven't found a single node that is too close, feel free to spawn.
                            //We lift the pos in case terrain is not flat...
                            //We raycast to ground
                            if ((x - currentHitInfo.x) * (x - currentHitInfo.x) + (z - currentHitInfo.z) * (z - currentHitInfo.z) <= currentPainterSize * currentPainterSize)
                            {
                                float xSym = currentHitInfo.x - (x - currentHitInfo.x);
                                float zSym = currentHitInfo.z - (z - currentHitInfo.z);

                                float offsetY = mapNodeGroup.graphType == MapNodeGroup.GraphType.Air ? 0 : 6;
                                Vector3 future1 = (int)zCount % 2 == 0 ? new Vector3(x, currentHitInfo.y + offsetY, z + zPainterOffset) : new Vector3(x, currentHitInfo.y + offsetY, z);
                                Vector3 future2 = (int)zCount % 2 == 0 ? new Vector3(x, currentHitInfo.y + offsetY, zSym + zPainterOffset) : new Vector3(x, currentHitInfo.y + offsetY, zSym);
                                Vector3 future3 = (int)zCount % 2 == 0 ? new Vector3(xSym, currentHitInfo.y + offsetY, z + zPainterOffset) : new Vector3(xSym, currentHitInfo.y + offsetY, z);
                                Vector3 future4 = (int)zCount % 2 == 0 ? new Vector3(xSym, currentHitInfo.y + offsetY, zSym + zPainterOffset) : new Vector3(xSym, currentHitInfo.y + offsetY, zSym);

                                if (!Physics.Linecast(currentHitInfo, future1, out _, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
                                {
                                    bool canPlace = true;
                                    if (mapNodeGroup.graphType == MapNodeGroup.GraphType.Ground && Physics.Raycast(future1, Vector3.down, out RaycastHit raycastHit, 9, LayerIndex.CommonMasks.bullet, QueryTriggerInteraction.Collide))
                                    {
                                        foreach (MapNode node in cachedMapNodeList)
                                        {
                                            if (Vector3.Distance(node.transform.position, raycastHit.point) <= currentMaxDistance)
                                            {
                                                canPlace = false;
                                                break;
                                            }
                                        }
                                        if (canPlace)
                                        {
                                            mapNodeGroup.AddNode(raycastHit.point);
                                        }
                                    }
                                    else
                                    {
                                        foreach (MapNode node in cachedMapNodeList)
                                        {
                                            if (Vector3.Distance(node.transform.position, future1) <= currentMaxDistance)
                                            {
                                                canPlace = false;
                                                break;
                                            }
                                        }
                                        if (canPlace)
                                        {
                                            mapNodeGroup.AddNode(future1);
                                        }
                                    }
                                }
                                if (!Physics.Linecast(currentHitInfo, future2, out _, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
                                {
                                    bool canPlace = true;
                                    if (mapNodeGroup.graphType == MapNodeGroup.GraphType.Ground && Physics.Raycast(future2, Vector3.down, out RaycastHit raycastHitto, 9, LayerIndex.CommonMasks.bullet, QueryTriggerInteraction.Collide))
                                    {
                                        foreach (MapNode node in cachedMapNodeList)
                                        {
                                            if (Vector3.Distance(node.transform.position, raycastHitto.point) <= currentMaxDistance)
                                            {
                                                canPlace = false;
                                                break;
                                            }
                                        }
                                        if (canPlace)
                                        {
                                            mapNodeGroup.AddNode(raycastHitto.point);
                                        }
                                    }
                                    else
                                    {
                                        foreach (MapNode node in cachedMapNodeList)
                                        {
                                            if (Vector3.Distance(node.transform.position, future2) <= currentMaxDistance)
                                            {
                                                canPlace = false;
                                                break;
                                            }
                                        }
                                        if (canPlace)
                                        {
                                            mapNodeGroup.AddNode(future2);
                                        }
                                    }
                                }
                                if (!Physics.Linecast(currentHitInfo, future3, out _, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
                                {
                                    bool canPlace = true;
                                    if (mapNodeGroup.graphType == MapNodeGroup.GraphType.Ground && Physics.Raycast(future3, Vector3.down, out RaycastHit raycastHittoto, 9, LayerIndex.CommonMasks.bullet, QueryTriggerInteraction.Collide))
                                    {
                                        foreach (MapNode node in cachedMapNodeList)
                                        {
                                            if (Vector3.Distance(node.transform.position, raycastHittoto.point) <= currentMaxDistance)
                                            {
                                                canPlace = false;
                                                break;
                                            }
                                        }
                                        if (canPlace)
                                        {
                                            mapNodeGroup.AddNode(raycastHittoto.point);
                                        }
                                    }
                                    else
                                    {
                                        foreach (MapNode node in cachedMapNodeList)
                                        {
                                            if (Vector3.Distance(node.transform.position, future3) <= currentMaxDistance)
                                            {
                                                canPlace = false;
                                                break;
                                            }
                                        }
                                        if (canPlace)
                                        {
                                            mapNodeGroup.AddNode(future3);
                                        }
                                    }
                                }
                                if (!Physics.Linecast(currentHitInfo, future4, out _, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
                                {
                                    bool canPlace = true;
                                    if (mapNodeGroup.graphType == MapNodeGroup.GraphType.Ground && Physics.Raycast(future4, Vector3.down, out RaycastHit raycastHittototo, 9, LayerIndex.CommonMasks.bullet, QueryTriggerInteraction.Collide))
                                    {
                                        foreach (MapNode node in cachedMapNodeList)
                                        {
                                            if (Vector3.Distance(node.transform.position, raycastHittototo.point) <= currentMaxDistance)
                                            {
                                                canPlace = false;
                                                break;
                                            }
                                        }
                                        if (canPlace)
                                        {
                                            mapNodeGroup.AddNode(raycastHittototo.point);
                                        }
                                    }
                                    else
                                    {
                                        foreach (MapNode node in cachedMapNodeList)
                                        {
                                            if (Vector3.Distance(node.transform.position, future4) <= currentMaxDistance)
                                            {
                                                canPlace = false;
                                                break;
                                            }
                                        }
                                        if (canPlace)
                                        {
                                            mapNodeGroup.AddNode(future4);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                // Causes repaint & accepts event has been handled
                Event.current.Use();
            }
            //uhhh painter
            if (Event.current.keyCode == KeyCode.KeypadPlus)
            {
                currentPainterSize += 2;
                Event.current.Use();
            }
            if (Event.current.keyCode == KeyCode.KeypadMinus)
            {
                currentPainterSize = Mathf.Max(0, currentPainterSize -= 2);
                Event.current.Use();
            }
        }

        Vector2 guiPosition = Event.current.mousePosition;
        Ray ray = HandleUtility.GUIPointToWorldRay(guiPosition);

        if (Physics.Raycast(ray, out var hitInfo, 99999999999f, LayerIndex.CommonMasks.bullet, QueryTriggerInteraction.Collide))
        {
            currentHitInfo = mapNodeGroup.graphType == MapNodeGroup.GraphType.Air ? hitInfo.point + offsetUpVector : hitInfo.point;
            if (cachedMapNodeList.Count > 0)
            {
                var inRange = false;

                foreach (MapNode mapNode in cachedMapNodeList)
                {
                    if (mapNode && Vector3.Distance(mapNode.transform.position, currentHitInfo) <= currentMaxDistance)
                    {
                        Handles.color = Color.yellow;

                        Handles.DrawLine(mapNode.transform.position, currentHitInfo);

                        inRange = true;
                    }
                }

                if (inRange)
                {
                    Handles.color = Color.yellow;
                }
                else
                {
                    Handles.color = Color.red;
                }
            }
            else
            {
                Handles.color = Color.yellow;
            }
            if (currentPainterSize <= 0)
            {
                Handles.CylinderHandleCap(controlID, currentHitInfo, Quaternion.Euler(90, 0, 0), 1, EventType.Repaint);
            }
            else
            {
                Handles.CircleHandleCap(controlID, currentHitInfo, Quaternion.Euler(90, 0, 0), currentPainterSize, EventType.Repaint);
            }
        }

        foreach (MapNode mapNode in cachedMapNodeList)
        {
            if (mapNode)
            {
                Handles.color = Color.green;
                Handles.CylinderHandleCap(controlID, mapNode.transform.position, Quaternion.Euler(90, 0, 0), 1, EventType.Repaint);
                Handles.color = Color.magenta;
                foreach (MapNode.Link link in mapNode.links)
                {
                    if (link.nodeB)
                    {
                        Handles.DrawLine(mapNode.transform.position, link.nodeB.transform.position);
                    }
                }
            }
        }

        Handles.BeginGUI();

        EditorGUILayout.BeginVertical();

        EditorGUILayout.LabelField($"Camera Position: {Camera.current.transform.position}");
        EditorGUILayout.LabelField($"Press N to add map node at cursor position (raycast)");
        EditorGUILayout.LabelField($"Current radius size: {currentPainterSize}");

        if (GUILayout.Button("Clear"))
        {
            mapNodeGroup.Clear();
        }

        if (GUILayout.Button("Add Map Node at current camera position"))
        {
            var position = Camera.current.transform.position;
            mapNodeGroup.AddNode(position);
        }

        if (GUILayout.Button("Update No Ceiling Masks"))
        {
            mapNodeGroup.UpdateNoCeilingMasks();
        }

        if (GUILayout.Button("Update Teleporter Masks"))
        {
            mapNodeGroup.UpdateTeleporterMasks();
        }

        if (GUILayout.Button("Bake Node Graph"))
        {
            EditorUtility.SetDirty(mapNodeGroup.nodeGraph);
            mapNodeGroup.Bake(mapNodeGroup.nodeGraph);
            AssetDatabase.SaveAssets();
        }

        if (GUILayout.Button("Remove Node Excess"))
        {
            EditorUtility.SetDirty(mapNodeGroup.nodeGraph);
            int c = 0;
            foreach (MapNode mapNode in cachedMapNodeList)
            {
                if (mapNode)
                {
                    if (mapNode.links.Count <= 0) //Destroy instantly as there's no links.
                    {
                        DestroyImmediate(mapNode.gameObject);
                        c++;
                        continue;
                    }
                    //List<MapNode.Link> buffer = new List<MapNode.Link>();
                    for (int i = 0; i < mapNode.links.Count; i++)
                    {
                        //Make sure the other node exists and hasn't been deleted before.
                        if (mapNode.links[i].nodeB)
                        {
                            //Too friccin close, get off
                            if ((Vector3.Distance(mapNode.links[i].nodeB.gameObject.transform.position, mapNode.gameObject.transform.position) <= currentMaxDistance / 1.5))
                            {
                                DestroyImmediate(mapNode.gameObject);
                                c++;
                                break;
                            }
                            //It's not way too close and the link has some value
                            //Make sure the buffer does not contain too many links. More than three should ensure it is connected to other two nodes, as nodes connect back to their linkee
                            //if (buffer.Count < 4 && mapNode.links[i].nodeB.links.Count > 3)
                            //{
                            //    buffer.Add(mapNode.links[i]);
                            //}
                        }
                    }
                    //Assign the new link list, whenever it is empty or not.
                    //mapNode.links = buffer;
                }
            }
            //Go again through each map node to make sure there's no empty nodes after assigning the new buffer
            //foreach (MapNode mapNode in cachedMapNodeList)
            //{
            //    if (mapNode)
            //    {
            //        //Save if its not linking back to the linkee
            //        if (mapNode.links.Count == 1 && mapNode.links[0].nodeB.links.Count == 1 && mapNode.links[0].nodeB.links[0].nodeB != mapNode)
            //        {
            //            continue;
            //        }
            //        //Destroy instantly as it either has no links or its just linking back to the linkee
            //        if (mapNode.links.Count <= 1)
            //        {
            //            //Save as the link isnt with the linker
            //            DestroyImmediate(mapNode.gameObject);
            //            c++;
            //        }
            //    }
            //}
            Debug.Log($"Removed {c} nodes that were way too close to others.");
            AssetDatabase.SaveAssets();
        }
        /*
        if (GUILayout.Button("Remove Unreachable Nodes"))
        {
            EditorUtility.SetDirty(mapNodeGroup.nodeGraph);
            Path path = new Path(mapNodeGroup.nodeGraph);
            Transform transform = mapNodeGroup.testPointA;
            int c = 0;
            if (transform)
            {
                for (int i = 0; i < cachedMapNodeList.Count - 1; i++)
                {
                    if (cachedMapNodeList[i].gameObject && cachedMapNodeList[i].gameObject != mapNodeGroup.testPointA) //Do not delete self...
                    {
                        NodeGraph.PathRequest pathRequest = new NodeGraph.PathRequest
                        {
                            startPos = transform.position,
                            endPos = cachedMapNodeList[i].gameObject.transform.position,
                            hullClassification = HullClassification.Human,
                            maxJumpHeight = 30f,
                            maxSlope = 90f,
                            maxSpeed = 100f,
                            path = path,
                        };
                        PathTask pathTask = mapNodeGroup.nodeGraph.ComputePath(pathRequest);
                        if (pathTask != null && pathTask.status == PathTask.TaskStatus.Complete && !pathTask.wasReachable || pathTask.path.waypointsCount < 1)
                        {
                            DestroyImmediate(cachedMapNodeList[i].gameObject);
                            c++;
                        }
                    }
                }
                Debug.Log($"Removed {c} unreachable Nodes");
                AssetDatabase.SaveAssets();
            }
        }*/
        EditorGUILayout.EndVertical();

        Handles.EndGUI();
    }
}