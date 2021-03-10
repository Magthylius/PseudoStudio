using UnityEngine;

//! E: Jon
namespace Hadal.Networking
{
    public class MenuManager : MonoBehaviour
    {
        public static MenuManager Instance;

        [Header("Menu references")]
        [SerializeField] Menu[] menus;

        private void Awake()
        {
            Instance = this;
        }

        public void OpenMenu(string menuName)
        {
            for (int i = 0; i < menus.Length; i++)
            {
                if (menus[i].menuName == menuName)
                {
                    menus[i].Open();
                }
                else if (menus[i].open)
                {
                    CloseMenu(menus[i]);
                }
            }
        }
        public void OpenMenu(Menu menu)
        {
            /*for (int i = 0; i < menus.Length; i++)
            {
                if (menus[i].open)
                {
                    CloseMenu(menus[i]);
                }
            }*/
            menu.Open();
        }

        public void CloseAllMenus()
        {
            for (int i = 0; i < menus.Length; i++)
            {
                if (menus[i].open)
                {
                    CloseMenu(menus[i]);
                }
            }
        }

        public void CloseMenu(Menu menu)
        {
            menu.Close();
        }

        public void QuitGame()
        {
            Application.Quit();
        }

    }
}