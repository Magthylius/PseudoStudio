using UnityEngine;

namespace Hadal.Legacy
{
    public class Spawnpoint : MonoBehaviour
    {
        [SerializeField] GameObject graphics;

        private void Awake()
        {
            graphics.SetActive(false);
        }
    }
}