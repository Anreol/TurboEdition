/* i feel like dying
namespace TurboEdition.Equipments
{
    public class CursedScythe : Equipment
    {
        public override EquipmentDef equipmentDef { get; set; } = Assets.mainAssetBundle.LoadAsset<EquipmentDef>("CursedScythe");
        public override bool isElite { get; set; } = false;

        public override bool FireAction(EquipmentSlot slot)
        {
            return false;
        }

		private class Scythe
		{
			public Vector3 impactPosition;
			public float startTime;
			public bool didTravelEffect;
			public bool valid = true;
		}

		private class ScytheSend
        {
			NodeGraphSpider nodeGraphSpider;
			Vector3 position;
			public ScytheSend(CharacterBody[] targets, Vector3 origin)
			{
				CharacterBody[] hitlist = new CharacterBody[targets.Length];
                for (int i = 0; i < hitlist.Length; i++)
                {
					targets.CopyTo(hitlist, i);
				}
				Util.ShuffleArray(targets);
				position = origin;
				nodeGraphSpider = new NodeGraphSpider(SceneInfo.instance.groundNodes, HullMask.Human);
				nodeGraphSpider.AddNodeForNextStep(SceneInfo.instance.groundNodes.FindClosestNode(position, HullClassification.Human, float.PositiveInfinity));
				int steps = 0;
				int maxSteps = 50;
				while (0 < maxSteps && this.nodeGraphSpider.PerformStep())
				{
					steps++;
				}
			}

			public ScytheSend UpdateScythe()
			{
				if (targets)
				{
					Scythe.impactPosition = characterBody.corePosition;
					Vector3 origin = meteor.impactPosition + Vector3.up * 6f;
				}
				else if (this.nodeGraphSpider.collectedSteps.Count != 0)
				{
					int index = UnityEngine.Random.Range(0, this.nodeGraphSpider.collectedSteps.Count);
					SceneInfo.instance.groundNodes.GetNodePosition(this.nodeGraphSpider.collectedSteps[index].node, out meteor.impactPosition);
				}
				else
				{
					meteor.valid = false;
				}
				meteor.startTime = Run.instance.time;
				this.currentStep++;
				return meteor;
			}
		}
	}
}
*/