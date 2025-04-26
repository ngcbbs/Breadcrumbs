# UI 시스템 예제 코드 (Part 2)

## 게임 내 HUD 및 메뉴

### GameHUD.cs

```csharp
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 게임 내 HUD UI
/// </summary>
public class GameHUD : MonoBehaviour
{
    [Header("Health Display")]
    [SerializeField] private Slider _healthSlider;
    [SerializeField] private Text _healthText;
    [SerializeField] private Image _healthFillImage;
    [SerializeField] private Gradient _healthGradient;
    
    [Header("Stamina Display")]
    [SerializeField] private Slider _staminaSlider;
    [SerializeField] private Image _staminaFillImage;
    
    [Header("Action Bar")]
    [SerializeField] private ActionSlotUI[] _actionSlots;
    [SerializeField] private KeyCode[] _actionHotkeys;
    
    [Header("Mini Map")]
    [SerializeField] private MiniMap _miniMap;
    [SerializeField] private Button _mapToggleButton;
    private bool _isMapExpanded = false;
    
    [Header("Status Effects")]
    [SerializeField] private Transform _statusEffectsContainer;
    [SerializeField] private StatusEffectIcon _statusEffectPrefab;
    
    // 캐릭터 참조
    private PlayerController _playerController;
    
    private void Start()
    {
        // 초기화
        InitializeUI();
        
        // 맵 토글 버튼 이벤트 등록
        if (_mapToggleButton != null)
            _mapToggleButton.onClick.AddListener(ToggleMiniMap);
        
        // 플레이어 찾기
        FindPlayer();
    }
    
    private void Update()
    {
        // 핫키 처리
        HandleHotkeys();
    }
    
    /// <summary>
    /// UI 초기화
    /// </summary>
    private void InitializeUI()
    {
        // 슬라이더 초기화
        if (_healthSlider != null)
            _healthSlider.value = 1.0f;
            
        if (_staminaSlider != null)
            _staminaSlider.value = 1.0f;
            
        // 텍스트 초기화
        if (_healthText != null)
            _healthText.text = "100/100";
        
        // 미니맵 초기화
        if (_miniMap != null)
            _miniMap.Initialize(false);
    }
    
    /// <summary>
    /// 플레이어 찾기
    /// </summary>
    private void FindPlayer()
    {
        // 플레이어 컨트롤러 찾기
        _playerController = FindObjectOfType<PlayerController>();
        
        // 이벤트 등록
        if (_playerController != null)
        {
            _playerController.OnHealthChanged += UpdateHealth;
            _playerController.OnStaminaChanged += UpdateStamina;
            _playerController.OnStatusEffectChanged += UpdateStatusEffects;
            
            // 초기 상태 업데이트
            UpdateHealth(_playerController.CurrentHealth, _playerController.MaxHealth);
            UpdateStamina(_playerController.CurrentStamina, _playerController.MaxStamina);
        }
    }
    
    /// <summary>
    /// 체력 업데이트
    /// </summary>
    private void UpdateHealth(float current, float max)
    {
        if (_healthSlider != null)
        {
            _healthSlider.value = Mathf.Clamp01(current / max);
        }
        
        if (_healthText != null)
        {
            _healthText.text = $"{Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
        }
        
        if (_healthFillImage != null && _healthGradient != null)
        {
            _healthFillImage.color = _healthGradient.Evaluate(_healthSlider.normalizedValue);
        }
    }
    
    /// <summary>
    /// 스태미나 업데이트
    /// </summary>
    private void UpdateStamina(float current, float max)
    {
        if (_staminaSlider != null)
        {
            _staminaSlider.value = Mathf.Clamp01(current / max);
        }
    }
    
    /// <summary>
    /// 상태 효과 업데이트
    /// </summary>
    private void UpdateStatusEffects(StatusEffect[] effects)
    {
        // 기존 상태 효과 아이콘 제거
        ClearStatusEffects();
        
        if (effects == null || _statusEffectsContainer == null || _statusEffectPrefab == null)
            return;
        
        // 새 상태 효과 아이콘 생성
        foreach (var effect in effects)
        {
            var icon = Instantiate(_statusEffectPrefab, _statusEffectsContainer);
            icon.Initialize(effect);
        }
    }
    
    /// <summary>
    /// 상태 효과 아이콘 정리
    /// </summary>
    private void ClearStatusEffects()
    {
        if (_statusEffectsContainer == null)
            return;
            
        foreach (Transform child in _statusEffectsContainer)
        {
            Destroy(child.gameObject);
        }
    }
    
    /// <summary>
    /// 미니맵 토글
    /// </summary>
    private void ToggleMiniMap()
    {
        _isMapExpanded = !_isMapExpanded;
        
        if (_miniMap != null)
            _miniMap.SetExpanded(_isMapExpanded);
    }
    
    /// <summary>
    /// 핫키 처리
    /// </summary>
    private void HandleHotkeys()
    {
        if (_actionSlots == null || _actionHotkeys == null)
            return;
            
        // 액션 핫키 처리
        for (int i = 0; i < _actionSlots.Length && i < _actionHotkeys.Length; i++)
        {
            if (Input.GetKeyDown(_actionHotkeys[i]) && _actionSlots[i] != null)
            {
                _actionSlots[i].TriggerAction();
            }
        }
    }
    
    /// <summary>
    /// 액션 슬롯 설정
    /// </summary>
    public void SetAction(int slotIndex, ActionData action)
    {
        if (_actionSlots == null || slotIndex < 0 || slotIndex >= _actionSlots.Length)
            return;
            
        _actionSlots[slotIndex].SetAction(action);
    }
    
    private void OnDestroy()
    {
        // 이벤트 제거
        if (_playerController != null)
        {
            _playerController.OnHealthChanged -= UpdateHealth;
            _playerController.OnStaminaChanged -= UpdateStamina;
            _playerController.OnStatusEffectChanged -= UpdateStatusEffects;
        }
        
        // 버튼 이벤트 제거
        if (_mapToggleButton != null)
            _mapToggleButton.onClick.RemoveListener(ToggleMiniMap);
    }
}
```

### ActionSlotUI.cs

```csharp
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 액션 슬롯 UI 컴포넌트
/// </summary>
public class ActionSlotUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [SerializeField] private Image _iconImage;
    [SerializeField] private Image _cooldownOverlay;
    [SerializeField] private Text _hotkeyText;
    [SerializeField] private Text _cooldownText;
    
    private ActionData _action;
    private float _cooldownEndTime;
    private bool _isInCooldown;
    
    // 드래그 앤 드롭 관련
    private static ActionSlotUI _draggingSlot;
    private static GameObject _dragIconInstance;
    private static GameObject _dragIconPrefab;
    
    // 이벤트
    public event Action<ActionData> OnActionTriggered;
    
    private void Awake()
    {
        // 초기 상태 설정
        UpdateVisuals();
        
        // 드래그 아이콘 프리팹 로드
        if (_dragIconPrefab == null)
            _dragIconPrefab = Resources.Load<GameObject>("UI/DragIcon");
    }
    
    private void Update()
    {
        // 쿨다운 업데이트
        if (_isInCooldown)
        {
            float remainingTime = _cooldownEndTime - Time.time;
            
            if (remainingTime <= 0)
            {
                // 쿨다운 종료
                _isInCooldown = false;
                UpdateCooldownVisual(0f);
            }
            else
            {
                // 쿨다운 진행 중
                float cooldownDuration = _action != null ? _action.CooldownDuration : 1f;
                float normalizedTime = remainingTime / cooldownDuration;
                UpdateCooldownVisual(normalizedTime, remainingTime);
            }
        }
    }
    
    /// <summary>
    /// 액션 데이터 설정
    /// </summary>
    public void SetAction(ActionData action)
    {
        _action = action;
        _isInCooldown = false;
        UpdateVisuals();
    }
    
    /// <summary>
    /// 액션 트리거
    /// </summary>
    public void TriggerAction()
    {
        if (_action == null || _isInCooldown)
            return;
            
        // 액션 실행
        OnActionTriggered?.Invoke(_action);
        
        // 쿨다운 시작
        if (_action.CooldownDuration > 0)
        {
            StartCooldown(_action.CooldownDuration);
        }
    }
    
    /// <summary>
    /// 쿨다운 시작
    /// </summary>
    private void StartCooldown(float duration)
    {
        _isInCooldown = true;
        _cooldownEndTime = Time.time + duration;
        UpdateCooldownVisual(1.0f, duration);
    }
    
    /// <summary>
    /// 쿨다운 시각적 업데이트
    /// </summary>
    private void UpdateCooldownVisual(float normalizedTime, float remainingTime = 0)
    {
        if (_cooldownOverlay != null)
        {
            _cooldownOverlay.fillAmount = normalizedTime;
            _cooldownOverlay.enabled = normalizedTime > 0;
        }
        
        if (_cooldownText != null)
        {
            if (normalizedTime > 0)
            {
                _cooldownText.text = remainingTime.ToString("0.0");
                _cooldownText.enabled = true;
            }
            else
            {
                _cooldownText.enabled = false;
            }
        }
    }
    
    /// <summary>
    /// UI 시각적 업데이트
    /// </summary>
    private void UpdateVisuals()
    {
        if (_iconImage != null)
        {
            if (_action != null && _action.Icon != null)
            {
                _iconImage.sprite = _action.Icon;
                _iconImage.enabled = true;
            }
            else
            {
                _iconImage.sprite = null;
                _iconImage.enabled = false;
            }
        }
        
        if (_cooldownOverlay != null)
        {
            _cooldownOverlay.fillAmount = 0;
            _cooldownOverlay.enabled = false;
        }
        
        if (_cooldownText != null)
        {
            _cooldownText.enabled = false;
        }
    }
    
    // 드래그 앤 드롭 인터페이스 구현
    #region DragAndDrop
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && !_isInCooldown)
        {
            TriggerAction();
        }
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_action == null || _isInCooldown)
            return;
            
        _draggingSlot = this;
        
        // 드래그 아이콘 생성
        if (_dragIconPrefab != null && _iconImage != null && _iconImage.sprite != null)
        {
            _dragIconInstance = Instantiate(_dragIconPrefab, UIManager.Instance.transform);
            Image dragImage = _dragIconInstance.GetComponent<Image>();
            
            if (dragImage != null)
            {
                dragImage.sprite = _iconImage.sprite;
                dragImage.SetNativeSize();
                dragImage.transform.position = eventData.position;
            }
        }
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (_dragIconInstance != null)
        {
            _dragIconInstance.transform.position = eventData.position;
        }
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        _draggingSlot = null;
        
        // 드래그 아이콘 제거
        if (_dragIconInstance != null)
        {
            Destroy(_dragIconInstance);
            _dragIconInstance = null;
        }
    }
    
    public void OnDrop(PointerEventData eventData)
    {
        if (_draggingSlot == null || _draggingSlot == this)
            return;
            
        // 액션 교환
        ActionData tempAction = _action;
        SetAction(_draggingSlot._action);
        _draggingSlot.SetAction(tempAction);
    }
    
    #endregion
}
```

### InventoryPanel.cs

```csharp
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 인벤토리 UI 패널
/// </summary>
public class InventoryPanel : UIPanel
{
    [SerializeField] private Transform _itemsContainer;
    [SerializeField] private ItemSlotUI _itemSlotPrefab;
    [SerializeField] private Button _closeButton;
    [SerializeField] private Text _goldText;
    [SerializeField] private Button _sortButton;
    
    [Header("Item Details")]
    [SerializeField] private GameObject _detailsPanel;
    [SerializeField] private Image _detailsIcon;
    [SerializeField] private Text _detailsName;
    [SerializeField] private Text _detailsDescription;
    [SerializeField] private Text _detailsStats;
    [SerializeField] private Button _useButton;
    [SerializeField] private Button _equipButton;
    [SerializeField] private Button _dropButton;
    
    private List<ItemSlotUI> _itemSlots = new List<ItemSlotUI>();
    private ItemData _selectedItem;
    private InventorySystem _inventorySystem;
    
    protected override void Awake()
    {
        base.Awake();
        
        // 버튼 이벤트 등록
        if (_closeButton != null)
            _closeButton.onClick.AddListener(HandleCloseClicked);
            
        if (_sortButton != null)
            _sortButton.onClick.AddListener(HandleSortClicked);
            
        if (_useButton != null)
            _useButton.onClick.AddListener(HandleUseClicked);
            
        if (_equipButton != null)
            _equipButton.onClick.AddListener(HandleEquipClicked);
            
        if (_dropButton != null)
            _dropButton.onClick.AddListener(HandleDropClicked);
            
        // 상세 패널 초기 상태
        if (_detailsPanel != null)
            _detailsPanel.SetActive(false);
    }
    
    private void Start()
    {
        // 인벤토리 시스템 참조 가져오기
        _inventorySystem = InventorySystem.Instance;
        
        if (_inventorySystem != null)
        {
            // 이벤트 구독
            _inventorySystem.OnInventoryChanged += RefreshInventory;
            _inventorySystem.OnGoldChanged += UpdateGold;
        }
    }
    
    public override void Show()
    {
        base.Show();
        
        // 인벤토리 갱신
        RefreshInventory();
        UpdateGold(_inventorySystem?.Gold ?? 0);
    }
    
    /// <summary>
    /// 인벤토리 UI 갱신
    /// </summary>
    private void RefreshInventory()
    {
        // 기존 슬롯 제거
        ClearItemSlots();
        
        if (_inventorySystem == null || _itemSlotPrefab == null || _itemsContainer == null)
            return;
        
        // 아이템 슬롯 생성
        ItemData[] items = _inventorySystem.GetAllItems();
        foreach (var item in items)
        {
            CreateItemSlot(item);
        }
        
        // 상세 패널 업데이트
        UpdateDetailsPanel();
    }
    
    /// <summary>
    /// 아이템 슬롯 생성
    /// </summary>
    private void CreateItemSlot(ItemData item)
    {
        if (_itemSlotPrefab == null || _itemsContainer == null)
            return;
            
        // 슬롯 생성
        ItemSlotUI slot = Instantiate(_itemSlotPrefab, _itemsContainer);
        
        // 슬롯 초기화
        slot.Initialize(item);
        
        // 이벤트 등록
        slot.OnSelected += HandleItemSelected;
        
        // 목록에 추가
        _itemSlots.Add(slot);
    }
    
    /// <summary>
    /// 기존 슬롯 제거
    /// </summary>
    private void ClearItemSlots()
    {
        foreach (var slot in _itemSlots)
        {
            if (slot != null)
            {
                slot.OnSelected -= HandleItemSelected;
                Destroy(slot.gameObject);
            }
        }
        
        _itemSlots.Clear();
    }
    
    /// <summary>
    /// 아이템 선택 처리
    /// </summary>
    private void HandleItemSelected(ItemData item)
    {
        _selectedItem = item;
        UpdateDetailsPanel();
    }
    
    /// <summary>
    /// 상세 패널 업데이트
    /// </summary>
    private void UpdateDetailsPanel()
    {
        if (_detailsPanel == null)
            return;
            
        if (_selectedItem == null)
        {
            // 선택된 아이템 없음
            _detailsPanel.SetActive(false);
            return;
        }
        
        // 상세 패널 활성화
        _detailsPanel.SetActive(true);
        
        // 아이콘 업데이트
        if (_detailsIcon != null)
        {
            _detailsIcon.sprite = _selectedItem.Icon;
            _detailsIcon.enabled = _selectedItem.Icon != null;
        }
        
        // 이름 업데이트
        if (_detailsName != null)
            _detailsName.text = _selectedItem.ItemName;
            
        // 설명 업데이트
        if (_detailsDescription != null)
            _detailsDescription.text = _selectedItem.Description;
            
        // 스탯 업데이트
        if (_detailsStats != null)
        {
            string statsText = "";
            
            switch (_selectedItem.ItemType)
            {
                case ItemType.Weapon:
                    statsText = $"공격력: {_selectedItem.AttackValue}";
                    break;
                case ItemType.Armor:
                    statsText = $"방어력: {_selectedItem.DefenseValue}";
                    break;
                case ItemType.Consumable:
                    statsText = $"효과: {_selectedItem.EffectDescription}";
                    break;
            }
            
            _detailsStats.text = statsText;
        }
        
        // 버튼 상태 업데이트
        if (_useButton != null)
            _useButton.gameObject.SetActive(_selectedItem.ItemType == ItemType.Consumable);
            
        if (_equipButton != null)
            _equipButton.gameObject.SetActive(_selectedItem.ItemType == ItemType.Weapon || _selectedItem.ItemType == ItemType.Armor);
            
        if (_dropButton != null)
            _dropButton.gameObject.SetActive(true);
    }
    
    /// <summary>
    /// 골드 업데이트
    /// </summary>
    private void UpdateGold(int amount)
    {
        if (_goldText != null)
            _goldText.text = amount.ToString("#,##0");
    }
    
    // 버튼 이벤트 처리
    private void HandleCloseClicked()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.GoBack();
    }
    
    private void HandleSortClicked()
    {
        if (_inventorySystem != null)
            _inventorySystem.SortItems();
    }
    
    private void HandleUseClicked()
    {
        if (_inventorySystem != null && _selectedItem != null)
            _inventorySystem.UseItem(_selectedItem.ItemId);
    }
    
    private void HandleEquipClicked()
    {
        if (_inventorySystem != null && _selectedItem != null)
            _inventorySystem.EquipItem(_selectedItem.ItemId);
    }
    
    private void HandleDropClicked()
    {
        if (_inventorySystem != null && _selectedItem != null)
            _inventorySystem.DropItem(_selectedItem.ItemId);
    }
    
    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        // 이벤트 구독 해제
        if (_inventorySystem != null)
        {
            _inventorySystem.OnInventoryChanged -= RefreshInventory;
            _inventorySystem.OnGoldChanged -= UpdateGold;
        }
        
        // 버튼 이벤트 해제
        if (_closeButton != null)
            _closeButton.onClick.RemoveListener(HandleCloseClicked);
            
        if (_sortButton != null)
            _sortButton.onClick.RemoveListener(HandleSortClicked);
            
        if (_useButton != null)
            _useButton.onClick.RemoveListener(HandleUseClicked);
            
        if (_equipButton != null)
            _equipButton.onClick.RemoveListener(HandleEquipClicked);
            
        if (_dropButton != null)
            _dropButton.onClick.RemoveListener(HandleDropClicked);
    }
}
```

[이전: UISystem_part1.md] | [다음: UISystem_part3.md]