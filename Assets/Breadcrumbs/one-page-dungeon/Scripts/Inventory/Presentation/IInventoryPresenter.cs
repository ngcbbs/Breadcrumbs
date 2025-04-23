using UnityEngine;

namespace Breadcrumbs.Inventory.Presentation
{
    /// <summary>
    /// 인벤토리 프리젠터 인터페이스
    /// UI와 인벤토리 서비스 간의 브릿지 역할
    /// </summary>
    public interface IInventoryPresenter
    {
        /// <summary>
        /// 인벤토리 UI 초기화
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// 인벤토리 UI 새로고침
        /// </summary>
        void RefreshUI();
        
        /// <summary>
        /// 인벤토리 UI 표시
        /// </summary>
        void Show();
        
        /// <summary>
        /// 인벤토리 UI 숨기기
        /// </summary>
        void Hide();
        
        /// <summary>
        /// 슬롯 UI 클릭 핸들러
        /// </summary>
        void OnSlotClicked(int x, int y);
        
        /// <summary>
        /// 슬롯 UI 우클릭 핸들러
        /// </summary>
        void OnSlotRightClicked(int x, int y);
        
        /// <summary>
        /// 슬롯 UI 드래그 시작 핸들러
        /// </summary>
        void OnBeginDrag(int x, int y);
        
        /// <summary>
        /// 슬롯 UI 드래그 종료 핸들러
        /// </summary>
        void OnEndDrag(int fromX, int fromY, int toX, int toY);
        
        /// <summary>
        /// 아이템 툴팁 표시
        /// </summary>
        void ShowTooltip(IInventoryItem item, Vector2 position);
        
        /// <summary>
        /// 아이템 툴팁 숨기기
        /// </summary>
        void HideTooltip();
        
        /// <summary>
        /// 아이템 컨텍스트 메뉴 표시
        /// </summary>
        void ShowContextMenu(IInventoryItem item, int x, int y, Vector2 position);
        
        /// <summary>
        /// 컨텍스트 메뉴 숨기기
        /// </summary>
        void HideContextMenu();
    }
}
