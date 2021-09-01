using ThunderKit.Core.Pipelines;
using UnityEditor;

namespace Assets.Editor
{
    [PipelineSupport(typeof(Pipeline))]
    public class SaveAssets : PipelineJob
    {
        public override void Execute(Pipeline pipeline)
        {
            AssetDatabase.SaveAssets();
        }
    }
}