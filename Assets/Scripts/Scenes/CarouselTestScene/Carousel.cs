using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityTests
{
    public class Carousel : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
    {
        float _radianOffset;                     // ����ƫ����
        float _radianVelocity;                   // ��ת�ٶ�

        float _cameraDistance;
        Vector3 _lastMouseWorldPosition;

        bool _dragging;

        CarouselItem _lastSelectedItem;

        /// <summary>
        /// ��С
        /// </summary>
        [field: SerializeField]
        public float Size { get; set; } = 5;

        /// <summary>
        /// ��ת����
        /// </summary>
        [field: SerializeField]
        public float RadianDrag { get; set; } = 10;

        /// <summary>
        /// ������ѡ��
        /// </summary>
        [field: SerializeField]
        public bool AllowClickSelection { get; set; } = false;

        /// <summary>
        /// ���ù���
        /// </summary>
        [field: SerializeField]
        public bool EnableInertia { get; set; } = true;

        /// <summary>
        /// �����Զ�����
        /// </summary>
        [field: SerializeField]
        public bool EnableAutoCorrection { get; set; } = true;

        /// <summary>
        /// ����ʱ��
        /// </summary>
        [field: SerializeField]
        public float AutoCorrectionDuration { get; set; } = 0.2f;

        /// <summary>
        /// ��ѡ�������
        /// </summary>
        public int SelectedIndex { get; private set; }

        /// <summary>
        /// ��ѡ���ͼ��
        /// </summary>
        public GameObject SelectedGameObject { get; private set; }

        // Start is called before the first frame update
        protected virtual void Start()
        {
            UpdateObjectStatus();
            Select(0);
        }

        /// <summary>
        /// ÿ֡����
        /// </summary>
        // Update is called once per frame
        protected virtual void Update()
        {
            // ��ק����
            if (EnableInertia && !_dragging && _radianVelocity != 0)
            {
                _radianOffset += _radianVelocity * Time.deltaTime;
                UpdateObjectStatus();

                var radianVelocitySign = Mathf.Sign(_radianVelocity);
                var radianVelocitySize = Mathf.Abs(_radianVelocity);

                radianVelocitySize -= RadianDrag * Time.deltaTime;
                if (radianVelocitySize < 0)
                    radianVelocitySize = 0;

                _radianVelocity = radianVelocitySign * radianVelocitySize;

                if (EnableAutoCorrection && _radianVelocity == 0)
                {
                    Select(SelectedIndex);
                }
            }
        }

        /// <summary>
        /// ��������״̬
        /// </summary>
        protected void UpdateObjectStatus()
        {
            var radius = Size / 2;
            var childCount = transform.childCount;
            var radianGap = Mathf.PI * 2 / childCount;

            var minSin = 1.0f;
            var selectedIndex = -1;
            GameObject selectedGameObject = null;

            _radianOffset %= Mathf.PI * 2;
            for (int i = 0; i < childCount; i++)
            {
                var radian = radianGap * i + _radianOffset - Mathf.PI / 2;
                var child = transform.GetChild(i);

                var cos = Mathf.Cos(radian);
                var sin = Mathf.Sin(radian);

                var x = cos * radius;
                var z = sin * radius;

                child.localPosition = new Vector3(x, 0, z);

                if (sin < minSin)
                {
                    selectedIndex = i;
                    selectedGameObject = child.gameObject;
                    minSin = sin;
                }
            }

            SelectedIndex = selectedIndex;
            SelectedGameObject = selectedGameObject;
        }

        /// <summary>
        /// ����Ŀ����ת��
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="target"></param>
        private void CorrectRotationTarget(float origin, ref float target)
        {
            const float doublePi = Mathf.PI * 2;

            float diff = (target - origin) % doublePi;
            if (diff > Mathf.PI)
                diff -= doublePi;
            else if (diff < -Mathf.PI)
                diff += doublePi;

            target = origin + diff;
        }

        /// <summary>
        /// ��������ȡ��ת��
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private float GetRadianFromItemIndex(int index)
        {
            var childCount = transform.childCount;
            var radianGap = Mathf.PI * 2 / childCount;

            return -radianGap * index;
        }

        public void Select(int index)
        {
            var originRadian = _radianOffset;
            var targetRadian = GetRadianFromItemIndex(index);
            CorrectRotationTarget(originRadian, ref targetRadian);

            DOTween
                .To(radian =>
                {
                    _radianOffset = radian;
                    UpdateObjectStatus();
                }, originRadian, targetRadian, AutoCorrectionDuration)
                .OnComplete(() =>
                {
                    _lastSelectedItem?.OnItemDeselected();
                    _lastSelectedItem = null;

                    if (SelectedGameObject.GetComponent<CarouselItem>() is CarouselItem carouselItem)
                    {
                        carouselItem.OnItemSelected();
                        _lastSelectedItem = carouselItem;
                    }
                });
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            var radius = Size / 2;

            var mousePosition = Input.mousePosition;
            mousePosition.z = _cameraDistance;
            var newMouseWorldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            var mouseWorldOffset = newMouseWorldPosition - _lastMouseWorldPosition;

            _lastMouseWorldPosition = newMouseWorldPosition;

            var radianChange = mouseWorldOffset.x / radius;

            _radianVelocity = radianChange / Time.deltaTime;
            _radianOffset += radianChange;

            UpdateObjectStatus();
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            var selfScreenPosition = Camera.main.WorldToScreenPoint(transform.position);
            _cameraDistance = selfScreenPosition.z;

            var mousePosition = Input.mousePosition;
            mousePosition.z = _cameraDistance;
            _lastMouseWorldPosition = Camera.main.ScreenToWorldPoint(mousePosition);

            _dragging = true;
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            _dragging = false;

            if (EnableAutoCorrection && !EnableInertia)
            {
                Select(SelectedIndex);
            }
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (!AllowClickSelection)
                return;

            var childCount = transform.childCount;

            for (int i = 0; i < childCount; i++)
            {
                var renderer = transform.GetChild(i);

                if (eventData.pointerCurrentRaycast.gameObject == renderer.gameObject)
                {
                    Select(i);
                    break;
                }
            }
        }
    }
}
