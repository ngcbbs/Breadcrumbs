using UnityEngine;

namespace Breadcrumbs.Inventory
{
    /// <summary>
    /// Interface for item dropping functionality
    /// </summary>
    public interface IItemDropService
    {
        /// <summary>
        /// Drops an item in the world
        /// </summary>
        /// <param name="item">The item to drop</param>
        /// <param name="quantity">The quantity to drop</param>
        /// <param name="position">The position to drop at (null for default)</param>
        /// <returns>True if the item was dropped successfully</returns>
        bool DropItem(IInventoryItem item, int quantity = 1, Vector3? position = null);
        
        /// <summary>
        /// Sets the default drop position transform
        /// </summary>
        /// <param name="dropPosition">The transform to use as drop position</param>
        void SetDropPosition(Transform dropPosition);
    }
}
