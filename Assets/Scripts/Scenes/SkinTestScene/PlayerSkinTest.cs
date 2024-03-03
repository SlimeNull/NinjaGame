using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityTests
{
    public class PlayerSkinTest : MonoBehaviour
    {
        [field: SerializeField]
        public bool Combine { get; set; }

        [field: SerializeField]
        public GameObject CombineTarget { get; set; }

        [field: SerializeField]
        public SkinObjectActiveItem[] CombineParts { get; set; }


        public void OnSkinChanged()
        {
            // ��������˺ϲ�
            if (!Combine)
                return;

            // ��ȡ��Ҫ�ϲ������岿��
            GameObject[] activeCombineParts = CombineParts
                .Where(part => part.TargetGameObject != null)
                .Where(part => part.SelfSelected)
                .Select(part => part.TargetGameObject)
                .ToArray();

            // ��������
            foreach (var wearCombinePart in activeCombineParts)
                wearCombinePart.SetActive(false);

            // �ϲ�
            SkinnedMeshUtils.Combine(CombineTarget, activeCombineParts);
        }
    }
}
