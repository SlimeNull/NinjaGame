using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NinjaGame
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class SpiderWeb : MaskableGraphic
    {
        /// <summary>
        /// ������С
        /// </summary>
        [field: SerializeField]
        public float Size { get; set; } = 100;

        /// <summary>
        /// ��������
        /// </summary>
        [field: SerializeField]
        public int VertexCount { get; set; } = 6;

        /// <summary>
        /// ��������
        /// </summary>
        [field: SerializeField]
        public int LineCount { get; set; } = 5;

        /// <summary>
        /// �������
        /// </summary>
        [field: SerializeField]
        public float LineThickness { get; set; } = 1;

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            var radius = Size / 2;                            // �뾶
            var vertexCount = VertexCount;                    // ��������
            var lineCount = LineCount;                        // ��������
            var lineThickness = LineThickness;                // �������
            var halfLineThickness = lineThickness / 2;        // ������ȵ�һ��
            var radianGap = Mathf.PI * 2 / vertexCount;       // ���ȼ��

            vh.Clear();

            // ѭ��ÿһ����
            for (int i = 0; i < lineCount; i++)
            {
                var outerRadius = radius - (radius * i / lineCount);                     // �������ⲿ�뾶
                var innerRadius = radius - (radius * i / lineCount) - lineThickness;     // �������ڲ��뾶
                var lineVertexIndexOffset = vh.currentVertCount;                         // ��ǰ�߶�������ƫ����

                // ѭ��ÿһ������
                for (int j = 0; j < vertexCount; j++)
                {
                    var radian = radianGap * j;            // ��ǰ����Ƕ�
                    var cos = Mathf.Cos(radian);           // COS ֵ
                    var sin = Mathf.Sin(radian);           // SIN ֵ

                    var outerX = cos * outerRadius;        // �ⲿ����� X ����
                    var outerY = sin * outerRadius;        // �ⲿ����� Y ����

                    var innerX = cos * innerRadius;        // �ڲ������ X ����
                    var innerY = sin * innerRadius;        // �ڲ������ Y ����

                    var currentOuterVertexIndex = lineVertexIndexOffset + j * 2;                          // ��ǰ���ⲿ���������
                    var currentInnerVertexIndex = lineVertexIndexOffset + j * 2 + 1;                      // ��ǰ���ڲ����������
                    var nextOuterVertexIndex = lineVertexIndexOffset + (j + 1) % vertexCount * 2;         // ��һ�����ⲿ���������
                    var nextInnerVertexIndex = lineVertexIndexOffset + (j + 1) % vertexCount * 2 + 1;     // ��һ�����ڲ����������

                    // �����������
                    vh.AddVert(new Vector3(outerX, outerY), color, new Vector4());
                    vh.AddVert(new Vector3(innerX, innerY), color, new Vector4());

                    // �������������
                    vh.AddTriangle(currentInnerVertexIndex, nextInnerVertexIndex, nextOuterVertexIndex);
                    vh.AddTriangle(currentInnerVertexIndex, nextOuterVertexIndex, currentOuterVertexIndex);
                }
            }


            for (int i = 0; i < vertexCount; i++)
            {
                var outerRadius = radius - lineThickness / 2;           // �ⲿ�뾶
                var vertexIndexOffset = vh.currentVertCount;

                var radian = radianGap * i;                             // ��ǰ����Ƕ�
                var cos = Mathf.Cos(radian);                            // ��ǰ�Ƕ� COS ֵ
                var sin = Mathf.Sin(radian);                            // ��ǰ�Ƕ� SIN ֵ
                var tanCos = Mathf.Cos(radian - Mathf.PI / 2);          // ��ǰ�������ߵ� COS ֵ
                var tanSin = Mathf.Sin(radian - Mathf.PI / 2);          // ��ǰ�������ߵ� SIN ֵ
                var sideX = cos * outerRadius;
                var sideY = sin * outerRadius;

                var centerPoint0 = new Vector3(tanCos * halfLineThickness, tanSin * halfLineThickness);
                var centerPoint1 = -centerPoint0;

                var sidePoint0 = new Vector3(sideX + centerPoint0.x, sideY + centerPoint0.y);
                var sidePoint1 = new Vector3(sideX + centerPoint1.x, sideY + centerPoint1.y);

                vh.AddVert(centerPoint0, color, new Vector4());
                vh.AddVert(centerPoint1, color, new Vector4());
                vh.AddVert(sidePoint0, color, new Vector4());
                vh.AddVert(sidePoint1, color, new Vector4());

                vh.AddTriangle(vertexIndexOffset, vertexIndexOffset + 1, vertexIndexOffset + 3);
                vh.AddTriangle(vertexIndexOffset, vertexIndexOffset + 3, vertexIndexOffset + 2);
            }
        }
    }
}
