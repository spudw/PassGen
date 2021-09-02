using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace PassGen
{
    /// <summary>
    /// Interaction logic for KeyManager.xaml
    /// </summary>
    public partial class KeyManager : Window
    {
        const string KEYMASKEDKEYVALUE = "••••••••••••••••";
        int iActiveItemIndex = -1;
        ToolTip toolTip = new ToolTip();
        CollectionView view;
        bool ChangesMade = false;
        ListSortDirection KeyManagerListViewSortDirection = ListSortDirection.Descending;
        public KeyManager()
        {
            InitializeComponent();
        }

        #region GUI Event Logic
        private void KeyManager_Activated(object sender, EventArgs e)
        {
            KeyManager_PopulateListView();
        }

        private void KeyManager_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            //KeyManager_Hide();
            BtnCancel_Click(sender, null);
        }

        private void KeyManager_Deactivated(object sender, EventArgs e)
        {
            MnuViewShowKeys.IsChecked = false;
            KeyManager_MaskListView(false);
        }

        private void KeyManager_Hide()
        {
            KeyValueEditCtrl.Visibility = Visibility.Hidden;
            toolTip.IsOpen = false;
            this.Visibility = Visibility.Hidden;
            KeyManagerListView.Items.Clear();
        }
        #endregion

        #region Control Fundamental Logic
        private void BtnSave_Enable(bool bFlag = true)
        {
            BtnSave.IsEnabled = bFlag;
        }
        #endregion

        #region Control Event Logic
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (ChangesMade == true)
            {
                MessageBoxResult result = MessageBox.Show(
                    "Are you you sure you want to cancel?\n" +
                    "You will lose any unsaved changes.", 
                    "Unsaved Changes Detected", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Warning,
                    MessageBoxResult.No);
                if (result == MessageBoxResult.No) { return; }
            }
            ChangesMade = false;
            KeyManager_Hide();
        }

        private void MnuEdit_Click(object sender, RoutedEventArgs e)
        {
            bool bItemsExist = KeyManagerListView.HasItems;
            MnuEditSelectAll.IsEnabled = bItemsExist;
            MnuEditDeselectAll.IsEnabled = bItemsExist;

            if (bItemsExist == true)
            {
                int iItemsSelectedCount = KeyManagerListView.SelectedItems.Count;
                bool bEnabled = (iItemsSelectedCount > 0) ? true : false;
                MnuEditRemoveSelected.IsEnabled = bEnabled;
            }
        }

        private void MnuEditDeselectAll_Click(object sender, RoutedEventArgs e)
        {
            KeyManagerListView_SelectAll(false);
        }

        private void MnuEditSelectAll_Click(object sender, RoutedEventArgs e)
        {
            KeyManagerListView_SelectAll();
        }

        private void MnuEditRemoveSelected_Click(object sender, RoutedEventArgs e)
        {
            KeyManagerListView_RemoveSelected();
        }

        private bool KeyManagerListView_FilterDeletedItems(object item)
        {
            return ((item as KeyManagerListViewItemData).Deleted != true);
        }

        private void MnuFileAdd_Click(object sender, RoutedEventArgs e)
        {
            KeyManager_AddNewKey();            
        }

        private void MnuFile_Click(object sender, RoutedEventArgs e)
        {
            //MnuFileExport.IsEnabled = KeyManagerListView.HasItems;
        }

        private void MnuFileClose_Click(object sender, RoutedEventArgs e)
        {
            KeyManager_Hide();
        }

        private void MnuView_Click(object sender, RoutedEventArgs e)
        {
            MnuViewShowKeys.IsEnabled = KeyManagerListView.HasItems;
        }
        #endregion

        #region Additional UI Logic
        private void KeyManagerListView_ChangesMade(bool bFlag = true)
        {
            ChangesMade = bFlag;
            BtnSave_Enable(bFlag);
        }
        
        private void KeyManagerListView_RemoveSelected()
        {
            var oSelectedItems = new List<KeyManagerListViewItemData>();
            foreach (KeyManagerListViewItemData oItem in KeyManagerListView.SelectedItems)
            {
                oSelectedItems.Add(oItem);
            }
            foreach (KeyManagerListViewItemData oItem in oSelectedItems)
            {
                oItem.Deleted = true;
            }
            view = (CollectionView)CollectionViewSource.GetDefaultView(KeyManagerListView.Items.SourceCollection);
            view.Filter = KeyManagerListView_FilterDeletedItems;
            KeyManagerListView_EnsureActiveItemInList();
            KeyManagerListView_ChangesMade();
        }

        private void KeyManagerListView_SelectAll(bool bFlag = true)
        {
            if (bFlag == true)
            {
                KeyManagerListView.SelectAll();
                KeyManagerListView.Focus();
            }
            else
            {
                KeyManagerListView.SelectedItems.Clear();
            }
        }

        private void KeyManagerListView_Sort(object sender, RoutedEventArgs e)
        {
            if (KeyManagerListViewSortDirection == ListSortDirection.Descending)
            {
                KeyManagerListViewSortDirection = ListSortDirection.Ascending;
            }
            else
            {
                KeyManagerListViewSortDirection = ListSortDirection.Descending;
            }
            KeyManagerListView.Items.SortDescriptions.Clear();
            KeyManagerListView.Items.SortDescriptions.Add(new SortDescription("KeyDate", KeyManagerListViewSortDirection));
            KeyManagerListView.Items.Refresh();
        }

        private void KeyManagerListViewItem_CheckedEvent(object sender, RoutedEventArgs e)
        {
            var oItemCheckBox = sender as CheckBox;
            string sActiveGUID = oItemCheckBox.Tag.ToString();
            KeyManagerListView.Items.Refresh();
            foreach (KeyManagerListViewItemData oItem in KeyManagerListView.Items)
            {
                if (oItem.KeyGUID == sActiveGUID)
                {
                    oItem.Active = true;
                }
                else if (oItem.KeyGUID != sActiveGUID)
                {
                    oItem.Active = false;
                }
            }
            KeyManagerListView.Items.Refresh();
            KeyManagerListView_ChangesMade();
        }
        #endregion

        private void KeyManagerListView_EnsureActiveItemInList()
        {
            if (KeyManagerListView.Items.Count < 1) { return; }

            bool bActiveItemFound = false;
            foreach (KeyManagerListViewItemData oItem in KeyManagerListView.Items)
            {
                if (oItem.Active == true)
                {
                    bActiveItemFound = true;
                }
            }
            if (bActiveItemFound != true)
            {
                var oItem = KeyManagerListView.Items.GetItemAt(0) as KeyManagerListViewItemData;
                oItem.Active = true;
                KeyManagerListView.Items.Refresh();
            }
        }

        private void KeyManager_AddNewKey()
        {
            KeyManager_AddKeyToListView((KeyManagerListView.Items.Count > 0) ? false : true,
                DateTime.Now.ToString("yyyy/MM/dd"),
                "New_K3y!",
                "{" + Guid.NewGuid().ToString().ToUpper() + "}",
                Brushes.Blue,
                Brushes.Blue,
                true);
        }

        private void KeyManager_AddKeyToListView(bool bActive, string sKeyDate, string sKeyValue, string sKeyGUID, SolidColorBrush cDateColor, SolidColorBrush cKeyColor, bool bModified = false, bool bDeleted = false)
        {
            int iItem = KeyManagerListView.Items.Add(new KeyManagerListViewItemData
            {
                Active = bActive,
                KeyDate = sKeyDate,
                KeyValue = sKeyValue,
                KeyGUID = sKeyGUID,
                KeyDateColorData = cDateColor,
                KeyValueColorData = cKeyColor,
                Modified = bModified,
                Deleted = bDeleted
            });
            KeyManagerListView_ChangesMade();
        }

        private void KeyManager_PopulateListView()
        {
            string sCurrentKey = PassGenFuncs.KeyGetActive();
            if (KeyManagerListView.Items.Count > 0) { return; }
            RegistryKey oKey = PassGenFuncs.RegistryOpen();
            foreach (string Key in oKey.GetValueNames())
            {
                if (PassGenFuncs.StringIsValidGUID(Key))
                {
                    KeyManager_AddKeyToListView((sCurrentKey==Key) ? true : false, 
                        PassGenFuncs.KeyParseDate(PassGenFuncs.KeyRead(Key)),
                        KEYMASKEDKEYVALUE, 
                        Key,
                        Brushes.Black,
                        Brushes.Black);
                }
            }
            oKey.Close();
            KeyManagerListView.Items.SortDescriptions.Add(new SortDescription("KeyDate", ListSortDirection.Descending));
            KeyManagerListView_ChangesMade(false);
        }

        private void KeyManager_MaskListView(bool bFlag = true)
        {
            foreach (KeyManagerListViewItemData oItem in KeyManagerListView.Items)
            {
                if (bFlag == true)
                {
                    oItem.KeyValue = PassGenFuncs.KeyParseValue(PassGenFuncs.KeyRead(oItem.KeyGUID));
                }
                else if (bFlag == false)
                {
                    if (oItem.Modified == false) { oItem.KeyValue = KEYMASKEDKEYVALUE; }
                }
            }
            KeyManagerListView.Items.Refresh();
        }

        private void MnuViewShowKeys_Click(object sender, RoutedEventArgs e)
        {
            KeyManager_MaskListView(MnuViewShowKeys.IsChecked);
        }

        private void ListViewItem_KeyDateGotFocus(object sender, RoutedEventArgs e)
        {
            if (MouseButtonState.Pressed == Mouse.RightButton) { return; }
            KeyDateEdit(sender);
        }

        private void ListViewItem_KeyValueGotFocus(object sender, RoutedEventArgs e)
        {
            if (MouseButtonState.Pressed == Mouse.RightButton) { return; }
            KeyValueEdit(sender);
        }

        public static T FindVisualParent<T>(UIElement element) where T : UIElement
        {
            UIElement parent = element; while (parent != null)
            {
                T correctlyTyped = parent as T; if (correctlyTyped != null)
                {
                    return correctlyTyped;
                }
                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }
            return null;
        }

        private void KeyDateEdit(object sender)
        {
            TextBox oTextBox = sender as TextBox;
            Point relativePoint = oTextBox.TransformToAncestor(Application.Current.MainWindow).Transform(new Point(0, 0));
            ListViewItem oItem = FindVisualParent<ListViewItem>(sender as TextBox);
            int iIndex = KeyManagerListView.ItemContainerGenerator.IndexFromContainer(oItem);
            iActiveItemIndex = iIndex;
            KeyDateEditCtrl.SetValue(LeftProperty, relativePoint.X - 10);
            KeyDateEditCtrl.SetValue(TopProperty, relativePoint.Y - 20);
            KeyDateEditCtrl.SelectedDate = Convert.ToDateTime(oTextBox.Text);
            KeyDateEditCtrl.IsDropDownOpen = true;
        }

        private void KeyValueEdit(object sender)
        {
            TextBox oTextBox = sender as TextBox;
            Point relativePoint = oTextBox.TransformToAncestor(Application.Current.MainWindow).Transform(new Point(0, 0));
            ListViewItem oItem = FindVisualParent<ListViewItem>(sender as TextBox);
            int iIndex = KeyManagerListView.ItemContainerGenerator.IndexFromContainer(oItem);
            iActiveItemIndex = iIndex;
            if (oTextBox.Text == KEYMASKEDKEYVALUE)
            {
                var oListViewItem = KeyManagerListView.Items.GetItemAt(iActiveItemIndex) as KeyManagerListViewItemData;
                string KeyGUID = oListViewItem.KeyGUID;
                string KeyValue = PassGenFuncs.KeyParseValue(PassGenFuncs.KeyRead(KeyGUID));
                KeyValueEditCtrl.Text = KeyValue;
            }
            else
            {
                KeyValueEditCtrl.Text = oTextBox.Text;
            }
            KeyValueEditCtrl.SetValue(LeftProperty, relativePoint.X - 1);
            KeyValueEditCtrl.SetValue(TopProperty, relativePoint.Y - 1);
            KeyValueEditCtrl.Visibility = Visibility.Visible;
            KeyValueEditCtrl.Focus();
        }

        private void KeyDateEditCtrl_CalendarClosed(object sender, RoutedEventArgs e)
        {
            if (iActiveItemIndex == -1) { return; }
            var oItem = KeyManagerListView.Items.GetItemAt(iActiveItemIndex) as KeyManagerListViewItemData;
            string sCurrentDate = oItem.KeyDate;
            string sNewDate = DateTime.Parse((string)KeyDateEditCtrl.SelectedDate.ToString()).ToString("yyyy/MM/dd");
            if (sCurrentDate != sNewDate)
            {
                oItem.KeyDate = sNewDate;
                oItem.KeyDateColorData = Brushes.Blue;
                oItem.Modified = true;
                KeyManagerListView_ChangesMade();
                KeyManagerListView.Items.Refresh();
            }
            iActiveItemIndex = -1;
            KeyManagerListView.Items.SortDescriptions.Add(new SortDescription("KeyDate", KeyManagerListViewSortDirection));
        }

        private void KeyValueEditCtrl_KeyDown(object sender, KeyEventArgs e)
        {
            Key key = e.Key;
            if (key == Key.Escape)
            {
                KeyValueEdit_Cancel();
            }
            else if (key == Key.Return)
            {
                string sNewKeyValue = KeyValueEditCtrl.Text;
                TextBox oTextBox = sender as TextBox;
                var oItem = KeyManagerListView.Items.GetItemAt(iActiveItemIndex) as KeyManagerListViewItemData;
                string sCurrentKeyValue = oItem.KeyValue;
                if (KeyValueEdit_IsKeyComplex(sNewKeyValue, oTextBox) != true)
                {
                    return;
                }
                if (sCurrentKeyValue != sNewKeyValue)
                {
                    oItem.KeyValue = sNewKeyValue;
                    oItem.KeyValueColorData = Brushes.Blue;
                    oItem.Modified = true;
                    KeyManagerListView_ChangesMade();
                    KeyManagerListView.Items.Refresh();
                    KeyValueEditCtrl.Visibility = Visibility.Hidden;
                }
                else
                {
                    KeyValueEdit_Cancel();
                    return;
                }
            }
        }

        private void KeyValueEdit_Cancel()
        {
            toolTip.IsOpen = false;
            iActiveItemIndex = -1;
            KeyValueEditCtrl.Visibility = Visibility.Hidden;
        }

        private bool KeyValueEdit_IsKeyComplex(string sKeyValue, TextBox oTextBox)
        {
            if (PassGenFuncs.StringIsComplex(sKeyValue) != true)
            {
                Point absPoint = oTextBox.PointToScreen(new Point(0d, 0d));
                string sErrorMsg = "Key does not meet complexity requirements.\n\n" +
                                    "Key must contain at least 8 characters,\n" +
                                    "and must contain at least one uppercase leter,\n" +
                                    "lowercase letter, number, and special character.";
                toolTip.Content = sErrorMsg;
                toolTip.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
                toolTip.Placement = System.Windows.Controls.Primitives.PlacementMode.AbsolutePoint;
                toolTip.PlacementRectangle = new Rect(absPoint.X - 50, absPoint.Y + 20, 0, 0);
                toolTip.IsOpen = true;
                return false;
            }
            return true;
        }

        private void KeyValueEditCtrl_LostFocus(object sender, RoutedEventArgs e)
        {
            toolTip.IsOpen = false;
            iActiveItemIndex = -1;
            KeyValueEditCtrl.Visibility = Visibility.Hidden;
        }

        private void KeyValueEditCtrl_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox oTextBox = sender as TextBox;
            if (oTextBox.Visibility != Visibility.Visible) { return; }
            string sKeyValue = oTextBox.Text;
            if (KeyValueEdit_IsKeyComplex(sKeyValue, oTextBox) == true)
            { 
                toolTip.IsOpen = false;
            }
        }

        private void ListView_KeyDateMouseDown(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine(e.LeftButton);
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            
            KeyManagerListView.Items.Filter = null;
            foreach (KeyManagerListViewItemData oItem in KeyManagerListView.Items){
                if (oItem.Deleted == true)
                {
                    PassGenFuncs.KeyDelete(oItem.KeyGUID);
                }
                else if (oItem.Modified == true)
                {
                    if (oItem.KeyValue == KEYMASKEDKEYVALUE)
                    {
                        oItem.KeyValue = PassGenFuncs.KeyParseValue(PassGenFuncs.KeyRead(oItem.KeyGUID));
                    }
                    PassGenFuncs.KeySave(oItem.KeyValue, oItem.KeyDate, oItem.KeyGUID);
                }
                if (oItem.Active == true)
                {
                    PassGenFuncs.KeySetActive(oItem.KeyGUID);
                }
                Console.WriteLine(oItem.KeyGUID + "|Modified:" + oItem.Modified + "|Deleted:" + oItem.Deleted);
            }
            KeyManagerListView_ChangesMade(false);
            KeyManager_Hide();
        }
    }

    public static class KeyManagerCommands
    {
        public static readonly RoutedUICommand AddKey = new RoutedUICommand(
            "AddKey",
            "AddKey",
            typeof(KeyManagerCommands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.N, ModifierKeys.Control)
            }
        );
    }

    public class KeyManagerListViewItemData
    {
        public bool Active { get; set; }
        public string KeyDate { get; set; }
        public string KeyValue { get; set; }
        public string KeyGUID { get; set; }
        public SolidColorBrush KeyDateColorData { get; set; }
        public SolidColorBrush KeyValueColorData { get; set; }
        public bool Modified { get; set; }
        public bool Deleted { get; set; }
    }
}
