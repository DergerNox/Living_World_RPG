using UnityEngine;

namespace LivingWorldRPG.Characters
{
    /// <summary>
    /// MonoBehaviour wrapper that lets in-game NPC GameObjects
    /// hold a reference to the data-only <see cref="NPC"/> class.
    /// </summary>
    public class NPCView : MonoBehaviour
    {
        public NPC Data { get; private set; }

        public void Initialize(NPC npcData, Vector3 worldPosition)
        {
            if (npcData == null)
            {
                Debug.LogError("NPCView.Initialize received a null NPC data reference.");
                return;
            }

            Data = npcData;
            transform.position = worldPosition;
            gameObject.name = npcData.Name ?? gameObject.name;

            NPCManager.Instance?.RegisterNPC(npcData);
        }
    }
}
