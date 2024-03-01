using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace UnityTests
{
    [RequireComponent(typeof(RectTransform))]
    public class UICarousel : UIBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
    {
        RectTransform _rectTransform;

        // ��תƫ���� (������)
        float _radianOffset;
        float _radianVelocity;
        float _radianVelocityDrag = 10;

        bool _dragging;


        [SerializeField]
        Sprite[] _images;

        [SerializeField]
        private bool _scaleImages = true;

        List<UnityEngine.UI.Image> _imageComponents = new();

        protected RectTransform rectTransform => _rectTransform ??= GetComponent<RectTransform>();

        // ͼƬ����
        public Sprite[] Images { get => _images; set => SetImages(value); }

        // ͼƬ�ߴ� (�������� Image �������ô�С)
        [field: SerializeField]
        public Vector3 ImageSize { get; set; } = new Vector3(100, 100);

        /// <summary>
        ///�Ƿ����ͼ���ǰ���ϵ����ͼ���С  <br/>
        /// ����� Overlap, �����ڽ���ԶС, ����Ҫ�������, ��������� WorldSpace �� Canvas ��������������ʹ���ڳ�����, ����Ҫ�������
        /// </summary>
        public bool ScaleImages
        {
            get => _scaleImages; 
            set
            {
                _scaleImages = value;
                UpdateImagesStatus();
            }
        }
        [field: SerializeField]
        public bool AllowClickSelection { get; set; } = false;

        /// <summary>
        /// ��С���ű��� (��󷽵�ͼ�������ϵ���������ֵ)
        /// </summary>
        [field: SerializeField]
        public float MinScale { get; set; } = 0.3f;

        /// <summary>
        /// ��ѡ�������
        /// </summary>
        public int SelectedIndex { get; private set; }

        /// <summary>
        /// ��ѡ���ͼ��
        /// </summary>
        public Sprite SelectedImage { get; private set; }


        protected override void Awake()
        {
            Initialize();
        }

        protected override void Start()
        {
            UpdateImagesStatus();
        }

        protected virtual void Update()
        {
            // ��ק����
            if (!_dragging && _radianVelocity != 0)
            {
                _radianOffset += _radianVelocity * Time.deltaTime;
                UpdateImagesStatus();

                var radianVelocitySign = Mathf.Sign(_radianVelocity);
                var radianVelocitySize = Mathf.Abs(_radianVelocity);

                radianVelocitySize -= _radianVelocityDrag * Time.deltaTime;
                if (radianVelocitySize < 0)
                    radianVelocitySize = 0;

                _radianVelocity = radianVelocitySign * radianVelocitySize;

                if (_radianVelocity == 0)
                {
                    Select(SelectedIndex);
                }
            }
        }

        /// <summary>
        /// ��ʼ��
        /// </summary>
        private void Initialize()
        {
            if (_images is null)
                _images = Array.Empty<Sprite>();

            UpdateRenderers();
        }

        /// <summary>
        /// ������ͼ��ʱ, ͬʱ������Ⱦ�� (������ɾ�� Image ����)
        /// </summary>
        /// <param name="images"></param>
        private void SetImages(Sprite[] images)
        {
            UpdateRenderers();
        }

        /// <summary>
        /// ����ͼ�񴴽� Image ����, ���Զ���������
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        private UnityEngine.UI.Image CreateRendererFor(Sprite image)
        {
            GameObject gameObject = new("Image");
            gameObject.transform.SetParent(transform);

            var rectTransform = gameObject.AddComponent<RectTransform>();
            var renderer = gameObject.AddComponent<UnityEngine.UI.Image>();

            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, ImageSize.x);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ImageSize.y);
            renderer.sprite = image;

            return renderer;
        }

        /// <summary>
        /// ���� Sprite ����Ⱦ��
        /// </summary>
        private void UpdateRenderers()
        {
            if (_imageComponents is null)
            {
                _imageComponents = _images
                    .Select(image => CreateRendererFor(image))
                    .ToList();
            }
            else
            {
                if (_imageComponents.Count < _images.Length)
                {
                    for (int i = 0; i < _imageComponents.Count; i++)
                        _imageComponents[i].sprite = _images[i];
                    while (_imageComponents.Count < _images.Length)
                        _imageComponents.Add(CreateRendererFor(_images[_imageComponents.Count]));
                }
                else
                {
                    for (int i = 0; i < _images.Length; i++)
                        _imageComponents[i].sprite = _images[i];
                    while (_imageComponents.Count > _images.Length)
                    {
                        int lastIndex = _imageComponents.Count - 1;

                        // destroy the last
                        Destroy(_imageComponents[lastIndex].gameObject);

                        // remove the last
                        _imageComponents.RemoveAt(lastIndex);
                    }
                }
            }
        }

        /// <summary>
        /// ��ת���߼�
        /// ���� "��תƫ����" ��������ͼ���λ��, ��С, �Լ�ǰ���ϵ
        /// </summary>
        private void UpdateImagesStatus()
        {
            UpdateRenderers();

            var scaleGap = 1 - MinScale;
            var radianGap = Mathf.PI * 2 / _images.Length;
            var selfSizeDelta = rectTransform.sizeDelta;
            var radius = selfSizeDelta.x / 2;
            var imageCount = _images.Length;
            var halfPi = Mathf.PI / 2;

            var minSin = 1f;
            var selectedImageIndex = -1;

            _radianOffset %= (Mathf.PI * 2);
            for (int i = 0; i < imageCount; i++)
            {
                var scaleShrink = scaleGap * i / (imageCount - 1);
                var renderer = _imageComponents[i];

                float cos = Mathf.Cos(radianGap * i + _radianOffset - halfPi);
                float sin = Mathf.Sin(radianGap * i + _radianOffset - halfPi);
                var x = cos * radius;
                var z = sin * radius;

                if (sin <= minSin)
                {
                    selectedImageIndex = i;
                    minSin = sin;
                }

                renderer.transform.localPosition = new Vector3(x, 0, z);

                if (ScaleImages)
                {
                    var scale = Mathf.Lerp(MinScale, 1, ((-sin) + 1) / 2);
                    renderer.transform.localScale = new Vector3(scale, scale, scale);
                }
                else
                {
                    renderer.transform.localScale = new Vector3(1, 1, 1);
                }
            }

            // ���ݴ�С, ����˳��, ��ΪС���ں���, ����ֱ�Ӹ��ݴ�С, ���õ���˳�򷽷�
            foreach (var com in _imageComponents.OrderBy(com => com.rectTransform.localScale.x))
                com.transform.SetAsLastSibling();

            // ��¼��ǰѡ�������Լ�ͼ��
            SelectedIndex = selectedImageIndex;
            SelectedImage = _images[selectedImageIndex];
        }

        /// <summary>
        /// ����Ŀ�����ת��
        /// ����Բ����תʱ, ����Ҫ�� 350 ��ת�� -10 ʱ, �������� 350 ���ɵ� -10, ���Ǵ� 350 ���ɵ� 370
        /// </summary>
        /// <param name="current"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        private float CorrectRadianTarget(float current, float target)
        {
            if (target > current)
            {
                if (target - current > Mathf.PI)
                    target -= Mathf.PI * 2;
            }
            else
            {
                if (current - target > Math.PI)
                    target += Mathf.PI * 2;
            }

            return target;
        }

        /// <summary>
        /// ���ݶ�������, ��ȡ��Ӧ��תƫ����
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private float GetRadianOffsetFromIndex(int index)
        {
            var radianGap = Mathf.PI * 2 / _images.Length;

            return radianGap * -index;
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();

            UpdateImagesStatus();
        }

        /// <summary>
        /// ��תĿ�������Ķ���
        /// </summary>
        /// <param name="index"></param>
        public void Select(int index)
        {
            var startValue = _radianOffset;
            var endValue = GetRadianOffsetFromIndex(index);

            endValue = CorrectRadianTarget(startValue, endValue);

            DOTween.To(() => startValue, (newValue) =>
                {
                    _radianOffset = newValue;
                    UpdateImagesStatus();
                }, endValue, .1f)
                .SetEase(Ease.OutCirc);
        }

        /// <summary>
        /// ��קʱ, ����ʵ�ʽǶ�ƫ����, ������ͼ��״̬
        /// </summary>
        /// <param name="eventData"></param>
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            var selfSizeDelta = _rectTransform.sizeDelta;
            float normalizedOffset = eventData.delta.x / (selfSizeDelta.x / 2);
            float radianChange = Mathf.Asin(normalizedOffset % 1);

            _radianOffset += radianChange;
            _radianVelocity = radianChange / Time.deltaTime;
            UpdateImagesStatus();
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            _dragging = true;
        }

        /// <summary>
        /// ��ת����ʱ, Ҳѡ����ǰ���Ķ���
        /// </summary>
        /// <param name="eventData"></param>
        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            //Select(SelectedIndex);

            _dragging = false;
        }

        /// <summary>
        /// �����ʱ, ��������ĳ��ͼ��, ������ת����ǰ��
        /// </summary>
        /// <param name="eventData"></param>
        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (!AllowClickSelection)
                return;

            var imageCount = _images.Length;

            for (int i = 0; i < imageCount; i++)
            {
                var renderer = _imageComponents[i];

                if (eventData.pointerCurrentRaycast.gameObject == renderer.gameObject)
                {
                    Select(i);
                    break;
                }
            }
        }
    }
}
