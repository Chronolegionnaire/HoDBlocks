using Vintagestory.API.Common;

namespace HoDBlocks.Winch
{
    public class ItemSlotWinch : ItemSlot
    {
        public ItemSlotWinch(InventoryBase inventory) : base(inventory)
        {
        }
        public override int MaxSlotStackSize
        {
            get => 1;
            set {}
        }
    }
}