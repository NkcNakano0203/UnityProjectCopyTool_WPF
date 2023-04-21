using System.IO;
using System.Windows;
using System.Windows.Controls;
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

        private void OnTopCheckBoxClicked(object sender, RoutedEventArgs e)
        {
            // 未確定時(null)は止める
            if (OnTopCheckBox.IsChecked == null) return;
            // チェックが入っていたら最前面表示する
            Topmost = (bool)OnTopCheckBox.IsChecked;
        }

        private void FolderDragDrop(object sender, DragEventArgs e)
        {
            // ドロップされたアイテムがフォルダか判別
            bool isFolder = IsDroppedItemFolder(e.Data);

            // フォルダ以外は受け付けない
            e.Effects = isFolder ? DragDropEffects.Copy : DragDropEffects.None;
            if (!isFolder) return;

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            // ドロップされたフォルダのパスを保持
            SelectFolderAddressTextBox.Text = files[0];
        }

        private bool IsDroppedItemFolder(IDataObject data)
        {
            // ドロップされたアイテムがフォルダか
            if (!data.GetDataPresent(DataFormats.FileDrop)) return true;
            if (((string[])data.GetData(DataFormats.FileDrop)).Length == 1) return true;
            if (Directory.Exists(((string[])data.GetData(DataFormats.FileDrop))[0])) return true;
            return false;
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

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CopyButton.IsEnabled = SelectFolderAddressTextBox.Text != null;
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

        private void CopyDirectory(string sourcePath, string destinationPath)
        {
            DirectoryInfo sourceDir = new DirectoryInfo(sourcePath);

            if (!sourceDir.Exists) throw new DirectoryNotFoundException($"フォルダが見つからないよ～: {sourceDir.FullName}");

            DirectoryInfo[] dirs = sourceDir.GetDirectories();
            Directory.CreateDirectory(destinationPath);

            // ファイルやフォルダが無かったら飛ばす
            if (dirs.Length != 0)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationPath, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir);
                }
            }
            if (sourceDir.GetFiles().Length == 0) return;
            foreach (FileInfo file in sourceDir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationPath, file.Name);
                file.CopyTo(targetFilePath);
            }
        }
    }
}