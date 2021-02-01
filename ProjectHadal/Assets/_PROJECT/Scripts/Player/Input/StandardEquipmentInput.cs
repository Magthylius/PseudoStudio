﻿using UnityEngine;

namespace Hadal.Controls
{
    public class StandardEquipmentInput : IEquipmentInput
    {
        public bool SlotIndex(int index) => Input.GetKeyDown((index + 1).ToString());
    }
}