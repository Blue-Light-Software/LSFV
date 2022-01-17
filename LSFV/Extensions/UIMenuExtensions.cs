using LSFV.NativeUI;
using RAGENativeUI;
using RAGENativeUI.Elements;

namespace LSFV.Extensions
{
    public static class UIMenuExtensions
    {
        public static bool RemoveItem(this UIMenu menu, UIMenuItem item)
        {
            int index = menu.MenuItems.IndexOf(item);
            if (index > -1)
            {
                menu.RemoveItemAt(index);
                return true;
            }

            return false;
        }

        public static bool RemoveItem<T>(this UIMenu menu, UIMenuItem<T> item)
        {
            int index = menu.MenuItems.IndexOf(item);
            if (index > -1)
            {
                menu.RemoveItemAt(index);
                return true;
            }

            return false;
        }
    }
}
