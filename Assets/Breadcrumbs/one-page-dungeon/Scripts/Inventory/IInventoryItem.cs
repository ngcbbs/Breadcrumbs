using UnityEngine;

namespace Breadcrumbs.Inventory
{
    /// <summary>
    /// 인벤토리에 저장할 수 있는 아이템에 대한 인터페이스
    /// </summary>
    public interface IInventoryItem
    {
        /// <summary>
        /// 아이템의 고유 ID
        /// </summary>
        string Id { get; }
        
        /// <summary>
        /// 아이템 표시 이름
        /// </summary>
        string DisplayName { get; }
        
        /// <summary>
        /// 아이템 설명
        /// </summary>
        string Description { get; }
        
        /// <summary>
        /// 아이템 아이콘
        /// </summary>
        Sprite Icon { get; }
        
        /// <summary>
        /// 인벤토리에서 차지하는 가로 칸 수
        /// </summary>
        int Width { get; }
        
        /// <summary>
        /// 인벤토리에서 차지하는 세로 칸 수
        /// </summary>
        int Height { get; }
        
        /// <summary>
        /// 아이템을 쌓을 수 있는지 여부
        /// </summary>
        bool IsStackable { get; }
        
        /// <summary>
        /// 현재 쌓인 수량
        /// </summary>
        int StackCount { get; set; }
        
        /// <summary>
        /// 한 슬롯에 쌓을 수 있는 최대 수량
        /// </summary>
        int MaxStackCount { get; }
        
        /// <summary>
        /// 아이템 타입
        /// </summary>
        ItemType ItemType { get; }
        
        /// <summary>
        /// 아이템 희귀도
        /// </summary>
        ItemRarity Rarity { get; }
        
        /// <summary>
        /// 아이템 사용 메서드
        /// </summary>
        bool Use();
        
        /// <summary>
        /// 아이템을 버릴 수 있는지 여부
        /// </summary>
        bool CanDrop { get; }
        
        /// <summary>
        /// 수량 증가
        /// </summary>
        /// <returns>최대 스택 초과로 남은 수량</returns>
        int AddToStack(int count);
        
        /// <summary>
        /// 수량 감소
        /// </summary>
        /// <returns>남은 수량이 0이면 true 반환</returns>
        bool RemoveFromStack(int count);
    }
}
