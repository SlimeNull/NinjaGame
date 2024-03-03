using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTests
{
    public class SkinObjectActiveItem : CarouselItem
    {
        [field: SerializeField]
        public PlayerSkinTest SkinTest { get; set; }

        [field: SerializeField]
        public GameObject TargetGameObject { get; set; }

        public bool SelfSelected { get; private set; }

        public override void OnItemDeselected()
        {
            SelfSelected = false;
            base.OnItemDeselected();
            if (TargetGameObject != null)
                TargetGameObject.SetActive(false);
        }

        public override void OnItemSelected()
        {
            SelfSelected = true;
            base.OnItemSelected();

            // ����ѡ��ʱ, ����Ŀ������
            if (TargetGameObject != null)
                TargetGameObject.SetActive(true);

            // ����Ƥ�����Խű��ķ���
            if (SkinTest != null)
                SkinTest.OnSkinChanged();
        }
    }
}
