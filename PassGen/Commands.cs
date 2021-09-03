using System.Windows.Input;

namespace PassGen
{
    public static class Commands
    {
        public static readonly RoutedCommand CmdMnuFileKeyMgr = new RoutedCommand();
        public static readonly RoutedCommand CmdMnuFileImport = new RoutedCommand();
        public static readonly RoutedCommand CmdMnuFileExport = new RoutedCommand();
        public static readonly RoutedCommand CmdMnuFileQuit = new RoutedCommand();
        public static readonly RoutedCommand CmdMnuFileAdd = new RoutedCommand();
        public static readonly RoutedCommand CmdMnuFileClose = new RoutedCommand();
        public static readonly RoutedCommand CmdMnuEditRemoveSelected = new RoutedCommand();
        public static readonly RoutedCommand CmdMnuEditSelectAll = new RoutedCommand();
        public static readonly RoutedCommand CmdMnuEditDeselectAll = new RoutedCommand();
        public static readonly RoutedCommand CmdMnuViewShowKeys = new RoutedCommand();
        public static RoutedCommand CmdMainGUIEsc = new RoutedCommand();
        public static RoutedCommand CmdKeyMgrEsc = new RoutedCommand();
    }
}