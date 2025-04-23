using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace Breadcrumbs.Inventory.Presentation
{
    /// <summary>
    /// 인벤토리 슬롯의 UI 컴포넌트
    /// </summary>
    public class InventorySlotUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        [SerializeField] private Image _background;
        [SerializeField] private Image _itemIcon;
        [SerializeField] private TextMeshProUGUI _stackCountText;
        [SerializeField] private GameObject _highlightObject;
        
        private int _x;
        private int _y;
        private IInventoryPresenter _presenter;
        private bool _isEmpty = true;
        private bool _isRootSlot = false;
        private IInventoryItem _item;
        
        // 드래그 관련 
        private Canvas _canvas;
        private CanvasGroup _canvasGroup;
        private RectTransform _rectTransform;
        
        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvas = GetComponentInParent<Canvas>();
            
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            
            // 초기 상태 설정
            if (_itemIcon != null)
            {
                _itemIcon.gameObject.SetActive(false);
            }
            
            if (_stackCountText != null)
            {
                _stackCountText.gameObject.SetActive(false);
            }
            
            if (_highlightObject != null)
            {
                _highlightObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// 슬롯 초기화
        /// </summary>
        public void Initialize(int x, int y, IInventoryPresenter presenter)
        {
            _x = x;
            _y = y;
            _presenter = presenter;
            
            name = $"Slot_{x}_{y}";
        }
        
        /// <summary>
        /// 슬롯 UI 업데이트
        /// </summary>
        public void UpdateUI(InventorySlot slot)
        {
            if (slot == null)
                return;
                
            _isEmpty = slot.IsEmpty;
            _isRootSlot = slot.IsRootSlot;
            _item = slot.Item;
            
            // 아이템 아이콘 업데이트
            if (_itemIcon != null)
            {
                if (!_isEmpty)
                {
                    _itemIcon.gameObject.SetActive(true);
                    _itemIcon.sprite = _item.Icon;
                    _itemIcon.color = Color.white;
                    
                    // 스택 개수 텍스트 업데이트
                    if (_stackCountText != null)
                    {
                        if (_item.IsStackable && _item.StackCount > 1)
                        {
                            _stackCountText.gameObject.SetActive(true);
                            _stackCountText.text = _item.StackCount.ToString();
                        }
                        else
                        {
                            _stackCountText.gameObject.SetActive(false);
                        }
                    }
                    
                    // 루트 슬롯이 아닌 경우 반투명하게 표시
                    if (!_isRootSlot)
                    {
                        _itemIcon.color = new Color(1f, 1f, 1f, 0.5f);
                    }
                }
                else
                {
                    _itemIcon.gameObject.SetActive(false);
                    
                    if (_stackCountText != null)
                    {
                        _stackCountText.gameObject.SetActive(false);
                    }
                }
            }
        }
        
        /// <summary>
        /// 슬롯 하이라이트 표시/숨김
        /// </summary>
        public void SetHighlight(bool isHighlighted)
        {
            if (_highlightObject != null)
            {
                _highlightObject.SetActive(isHighlighted);
            }
        }
        
        #region 이벤트 핸들러
        public void OnPointerClick(PointerEventData eventData)
        {
            if (_presenter == null)
                return;
                
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                _presenter.OnSlotClicked(_x, _y);
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                _presenter.OnSlotRightClicked(_x, _y);
            }
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            SetHighlight(true);
            
            if (_presenter != null && !_isEmpty)
            {
                _presenter.ShowTooltip(_item, eventData.position);
            }
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            SetHighlight(false);
            
            if (_presenter != null)
            {
                _presenter.HideTooltip();
            }
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_isEmpty || !_isRootSlot)
                return;
                
            // 드래그 중 반투명하게 표시
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0.6f;
                _canvasGroup.blocksRaycasts = false;
            }
            
            if (_presenter != null)
            {
                _presenter.OnBeginDrag(_x, _y);
                _presenter.HideTooltip();
            }
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (_isEmpty || !_isRootSlot)
                return;
                
            // 드래그 중인 아이템을 마우스 커서 위치로 이동
            if (_rectTransform != null && _canvas != null)
            {
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _canvas.GetComponent<RectTransform>(),
                    eventData.position,
                    eventData.pressEventCamera,
                    out Vector2 localPoint))
                {
                    _rectTransform.localPosition = localPoint;
                }
            }
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            if (_isEmpty || !_isRootSlot)
                return;
                
            // 드래그 종료 시 원래 상태로 복원
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
                _canvasGroup.blocksRaycasts = true;
            }
            
            if (_presenter != null)
            {
                // 드롭 타겟이 슬롯이 아닌 경우 (빈 공간 등)
                _presenter.OnEndDrag(_x, _y, _x, _y); // 자기 자신으로 돌아옴
            }
        }
        
        public void OnDrop(PointerEventData eventData)
        {
            GameObject draggedObject = eventData.pointerDrag;
            if (draggedObject == null)
                return;
                
            InventorySlotUI sourceSlot = draggedObject.GetComponent<InventorySlotUI>();
            if (sourceSlot == null || sourceSlot == this)
                return;
                
            if (_presenter != null)
            {
                _presenter.OnEndDrag(sourceSlot._x, sourceSlot._y, _x, _y);
            }
        }
        #endregion
    }
}
