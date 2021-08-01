using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Hadal
{
    public class SettingsSliderHandler : MonoBehaviour
    {
        public Slider slider;

        public void SetValueWithoutNotify(float value) => slider.SetValueWithoutNotify(value);
        public void SetValue(float value) => slider.value = value;
        public float Value => slider.value;
    }
}
