using System;
using System.Linq;

namespace NWaveform.ViewModels
{
    public static class MenuExtensions
    {
        public static bool IsEnabled(this IMenuItemViewModel item, object context = null)
        {
            if (item?.Command == null) return false;
            try { return item.Command.CanExecute(context); }
            catch (Exception) { return false; }
        }

        public static bool IsEmpty(this IMenuViewModel menu)
        {
            if (menu?.Items == null) return true;
            return menu.Items.All(item => !item.IsEnabled());
        }

        public static bool IsEmpty<TContext>(this IMenuViewModel menu, TContext context)
        {
            if (menu?.Items == null) return true;
            return menu.Items.All(item => !item.IsEnabled(context));
        }
    }
}