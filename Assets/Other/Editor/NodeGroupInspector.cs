using RoR2.Navigation;
using UnityEditor;
using UnityEngine.UIElements;

[CustomEditor(typeof(MapNodeGroup))]
internal class NodeGroupInspector : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        // Create a new VisualElement to be the root of our inspector UI
        VisualElement myInspector = new VisualElement();

        // Add a simple label
        myInspector.Add(new Label("This is a custom inspector"));

        // Return the finished inspector UI
        return myInspector;
    }
}