using RoR2;
using RoR2.Navigation;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(SceneInfo))]
internal class SceneInfoInspector : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        // Create a new VisualElement to be the root of our inspector UI
        VisualElement myInspector = new VisualElement();

        // Load and clone a visual tree from UXML
        VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Other/Editor/SceneInfoInspector_UXML.uxml");
        visualTree.CloneTree(myInspector);

        Button groundBakeButton = myInspector.Q<Button>(name: "btnGroundBake");
        //groundBakeButton.RegisterCallback<MouseDownEvent, MapNodeGroup>(Bake, (MapNodeGroup)serializedObject.FindProperty("groundNodeGroup").objectReferenceValue, TrickleDown.TrickleDown);
        groundBakeButton.clicked += () => Bake((MapNodeGroup)serializedObject.FindProperty("groundNodeGroup").objectReferenceValue);

        Button groundClearButton = myInspector.Q<Button>(name: "btnGroundClear");
        //groundClearButton.RegisterCallback<MouseDownEvent, MapNodeGroup>(Clear, (MapNodeGroup)serializedObject.FindProperty("groundNodeGroup").objectReferenceValue, TrickleDown.TrickleDown);
        groundClearButton.clicked += () => Clear((MapNodeGroup)serializedObject.FindProperty("groundNodeGroup").objectReferenceValue);

        Button airBakeButton = myInspector.Q<Button>(name: "btnAirBake");
        //airBakeButton.RegisterCallback<MouseDownEvent, MapNodeGroup>(Bake, (MapNodeGroup)serializedObject.FindProperty("airNodeGroup").objectReferenceValue, TrickleDown.TrickleDown);
        airBakeButton.clicked += () => Bake((MapNodeGroup)serializedObject.FindProperty("airNodeGroup").objectReferenceValue);

        Button airClearButton = myInspector.Q<Button>(name: "btnAirClear");
        //airClearButton.RegisterCallback<MouseDownEvent, MapNodeGroup>(Clear, (MapNodeGroup)serializedObject.FindProperty("airNodeGroup").objectReferenceValue, TrickleDown.TrickleDown);
        airClearButton.clicked += () => Clear((MapNodeGroup)serializedObject.FindProperty("airNodeGroup").objectReferenceValue);

        Button groundNoCeilMask = myInspector.Q<Button>(name: "btnGroundNoCeil");
        //airClearButton.RegisterCallback<MouseDownEvent, MapNodeGroup>(Clear, (MapNodeGroup)serializedObject.FindProperty("airNodeGroup").objectReferenceValue, TrickleDown.TrickleDown);
        groundNoCeilMask.clicked += () => UpdateNoCeil((MapNodeGroup)serializedObject.FindProperty("groundNodeGroup").objectReferenceValue);

        Button airNoCeilMask = myInspector.Q<Button>(name: "btnAirNoCeil");
        //airClearButton.RegisterCallback<MouseDownEvent, MapNodeGroup>(Clear, (MapNodeGroup)serializedObject.FindProperty("airNodeGroup").objectReferenceValue, TrickleDown.TrickleDown);
        airNoCeilMask.clicked += () => UpdateNoCeil((MapNodeGroup)serializedObject.FindProperty("airNodeGroup").objectReferenceValue);

        Button noTeleporter = myInspector.Q<Button>(name: "btnTeleporter");
        //airClearButton.RegisterCallback<MouseDownEvent, MapNodeGroup>(Clear, (MapNodeGroup)serializedObject.FindProperty("airNodeGroup").objectReferenceValue, TrickleDown.TrickleDown);
        noTeleporter.clicked += () => UpdateNoTeleporter((MapNodeGroup)serializedObject.FindProperty("groundNodeGroup").objectReferenceValue);

        // Return the finished inspector UI
        return myInspector;
    }

    public void Bake(MapNodeGroup mapNodeGroup)
    {
        if (!mapNodeGroup.nodeGraph)
        {
            Debug.LogError($"Cannot Bake NodeGraph in {mapNodeGroup.gameObject.name}, as it is missing a NodeGraph asset.");
            return;
        }
        Debug.LogWarning($"Baking {mapNodeGroup.gameObject.name}, it might take a while.");
        mapNodeGroup.Bake(mapNodeGroup.nodeGraph);
        return;
    }
    public void Clear(MapNodeGroup mapNodeGroup)
    {
        Debug.LogWarning($"Clearing {mapNodeGroup.gameObject.name} out of nodes!");
        mapNodeGroup.Clear();
        return;
    }

    public void UpdateNoCeil(MapNodeGroup mapNodeGroup)
    {
        mapNodeGroup.UpdateNoCeilingMasks();
    }
    public void UpdateNoTeleporter(MapNodeGroup mapNodeGroup)
    {
        mapNodeGroup.UpdateTeleporterMasks();
    }
}