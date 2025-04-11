using System;

namespace Breadcrumbs.one_page_dungeon {
    [Flags]
    public enum Wall : byte {
        None = 0x0,
        North = 0x1,
        East = 0x2,
        South = 0x4,
        West = 0x8
    }
}