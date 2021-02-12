using UnityEngine;

//Created by Jet
namespace Hadal.Utility
{
    public class DebugScript : MonoBehaviour
    {
        #region Test Bool Bit Array

        [ContextMenu(nameof(TestBoolBitArray))]
        private void TestBoolBitArray()
        {
            BoolBitArray bools = new BoolBitArray();
            bools[0] = true;
            bools[1] = false;
            bools[2] = true;
            bools[3] = true;
            bools[3] = false;

            for(int i = 0; i < 5; i++)
            {
                print($"Index: {i}, {bools[i]}.\n");
            }
        }

        #endregion
    }
}