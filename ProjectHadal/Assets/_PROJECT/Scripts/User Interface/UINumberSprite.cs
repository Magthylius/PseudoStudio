using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Hadal.UI
{
    public class UINumberSprite : MonoBehaviour
    {
        public List<Sprite> NumberSprites;
        public Image AssignedImage;
        
        public void SwapNumbers(int newNum)
        {
            AssignedImage.sprite = NumberSprites[newNum];
        }
    }
}
