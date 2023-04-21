using System;
using System.Collections;
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
            "Assets","Library","Packages","ProjectSettings"
        };
        const int PackagesNumber = 2;

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
            { includePackages = (bool)IncludePackagesCheckBox.IsChecked; }

            string sourceFolderPass = SelectFolderAddressTextBox.Text;
            string sourceFolderName = Path.GetFileName(sourceFolderPass);

            // コピー先になるフォルダを作成する
            DirectoryInfo sourceDirectoryInfo = new DirectoryInfo(sourceFolderPass);
            DirectoryInfo? parentDirectoryInfo = sourceDirectoryInfo.Parent;
            if (parentDirectoryInfo == null) throw new DirectoryNotFoundException($"親フォルダがNULLだよ～");

            string newFolderName = $"{sourceFolderName}_Copy";
            string newFolderPath = Path.Combine(parentDirectoryInfo.FullName, newFolderName);
            Directory.CreateDirectory(newFolderPath);

            // 子フォルダのパスを取得
            string[] subFolderPassArray = Directory.GetDirectories(sourceFolderPass);

            foreach (string subFolderPass in subFolderPassArray)
            {
                string subFolderName = Path.GetFileName(subFolderPass);
                foreach (var item in copyFolderNames)
                {
                    // フォルダ名で識別してコピーする
                    if (subFolderName != item) continue;
                    if (subFolderName == copyFolderNames[PackagesNumber])
                    {
                        if (!includePackages) continue;
                    }
                    //新しく作成したフォルダにAssets,Library...のコピー
                    string newFolderDirectory = Path.Combine(newFolderPath, subFolderName);
                    Directory.CreateDirectory(newFolderDirectory);

                    CopyDirectory(subFolderPass, newFolderDirectory);
                }
            }

            // プロジェクトフォルダ直下のファイルをコピー
            if (sourceDirectoryInfo.GetFiles().Length == 0) return;
            foreach (FileInfo file in sourceDirectoryInfo.GetFiles())
            {
                string targetFilePath = Path.Combine(newFolderPath, file.Name);
                file.CopyTo(targetFilePath);
            }

            //TODO:できたら作業進捗バー表示したいよね
            MessageBox.Show("コピー完了");
        }


        static void CopyDirectory(string sourceDir, string destinationDir)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDir);

            if (!dir.Exists) throw new DirectoryNotFoundException($"フォルダが見つからないよ～: {dir.FullName}");

            DirectoryInfo[] dirs = dir.GetDirectories();
            Directory.CreateDirectory(destinationDir);

            // ファイルやフォルダが無かったら飛ばす
            if (dirs.Length != 0)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir);
                }
            }

            if (dir.GetFiles().Length != 0)
            {
                foreach (FileInfo file in dir.GetFiles())
                {
                    string targetFilePath = Path.Combine(destinationDir, file.Name);
                    file.CopyTo(targetFilePath);
                }
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

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CopyButton.IsEnabled = SelectFolderAddressTextBox.Text != null;
        }
    }
}