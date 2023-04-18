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
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace UnityProjectCopyTool_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly string[] copyFolderNames =
        {
            "Assets","Library","ProjectSettings"
        };
        const string packagesStr = "Packages";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnTopCheckBox_Clicked(object sender, RoutedEventArgs e)
        {
            // 未確定時(null)は止める
            if (OnTopCheckBox.IsChecked == null) return;
            // チェックが入っていたら最前面表示する
            Topmost = (bool)OnTopCheckBox.IsChecked;
        }
        private void Window_DragDrop(object sender, DragEventArgs e)
        {
            // ドロップされたアイテムがフォルダか判別
            bool isFolder = IsDroppedItemFolder(e);
            // フォルダ以外は受け付けない
            e.Effects = isFolder ? DragDropEffects.Copy : DragDropEffects.None;
            if (!isFolder) return;

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            // ドロップされたフォルダのパスを保持
            SelectFolderAddressTextBox.Text = files[0];
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
            // Packagesの確認
            bool includePackages = false;
            if (IncludePackagesCheckBox.IsChecked != null)
            {
                includePackages = (bool)IncludePackagesCheckBox.IsChecked;
            }

            string folderPass = SelectFolderAddressTextBox.Text;
            if (folderPass == null) return;

            // フォルダ名の取得
            string folderName = Path.GetFileName(folderPass);
            if (folderName == null) return;
            // 子フォルダのパスを取得
            string[] subFoldersPass = Directory.GetDirectories(folderPass);

            // 選択されたフォルダの親フォルダのパスを取得する
            string parentFolderPath = Directory.GetParent(folderPass).FullName;

            // 新しいフォルダを作成する
            string newFolderName = $"{folderName}_Copy";
            string newFolderPath = Path.Combine(parentFolderPath, newFolderName);
            Directory.CreateDirectory(newFolderPath);

            foreach (string subFolderPass in subFoldersPass)
            {
                string subFolderName = Path.GetFileName(subFolderPass);
                foreach (var item in copyFolderNames)
                {
                    // フォルダ名で識別してコピーする
                    if (subFolderName != item) continue;
                    //新しく作成したフォルダにAssets,Library...のコピー
                    string newFolderDirectory = Path.Combine(newFolderPath, subFolderName);
                    Directory.CreateDirectory(newFolderDirectory);
                    DirectoryCopy(subFolderPass, newFolderDirectory);
                }

                if (!includePackages) continue;
                if (subFolderName != packagesStr) continue;
                string newPackFolderDirectory = System.IO.Path.Combine(newFolderPath, packagesStr);
                Directory.CreateDirectory(newPackFolderDirectory);
                DirectoryCopy(subFolderPass, newPackFolderDirectory);
            }
            FileCopy(folderPass, newFolderPath);
        }

        private void DirectoryCopy(string sourcePath, string destinationPath)
        {
            DirectoryInfo sourceDirectory = new DirectoryInfo(sourcePath);

            //ディレクトリのコピー（再帰を使用）
            foreach (DirectoryInfo directoryInfo in sourceDirectory.GetDirectories())
            {
                string newFolderPath = Path.Combine(destinationPath, directoryInfo.Name);
                Directory.CreateDirectory(newFolderPath);
                DirectoryCopy(directoryInfo.FullName, directoryInfo.FullName);
            }

            FileCopy(sourcePath, destinationPath);
        }

        private void FileCopy(string sourcePath, string destinationPath)
        {
            DirectoryInfo sourceDirectory = new DirectoryInfo(sourcePath);
            DirectoryInfo destinationDirectory = new DirectoryInfo(destinationPath);

            //ファイルのコピー
            foreach (FileInfo fileInfo in sourceDirectory.GetFiles())
            {
                string fileDirectory = Path.Combine(destinationDirectory.FullName, fileInfo.Name);
                fileInfo.CopyTo(fileDirectory, true);
            }
        }

        private bool IsDroppedItemFolder(DragEventArgs e)
        {
            // ドロップされたアイテムがフォルダか
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return true;
            if (((string[])e.Data.GetData(DataFormats.FileDrop)).Length == 1) return true;
            if (Directory.Exists(((string[])e.Data.GetData(DataFormats.FileDrop))[0])) return true;
            return false;
        }

    }
}