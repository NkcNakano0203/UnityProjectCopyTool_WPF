using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace UnityProjectCopyTool_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void AlwaysOnTopCheckBox_Clicked(object sender, RoutedEventArgs e)
        {
            // 未確定時(null)はやめる
            if (AlwaysOnTopCheckBox.IsChecked == null) return;
            Topmost = (bool)AlwaysOnTopCheckBox.IsChecked;
        }

        private void OpenFolderDialogButton_Click(object sender, RoutedEventArgs e)
        {
            // 参考記事
            // https://threeshark3.com/commonopenfiledialog/
            using (var cofd = new CommonOpenFileDialog()
            { Title = "フォルダを選択してください", IsFolderPicker = true })
            {
                if (cofd.ShowDialog() != CommonFileDialogResult.Ok) return;
                // 選択されたフォルダ名を保持
                SelectFolderAddressTextBox.Text = cofd.FileName;
            }
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
