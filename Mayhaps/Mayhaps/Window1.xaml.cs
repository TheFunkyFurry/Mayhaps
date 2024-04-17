using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Mayhaps
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : System.Windows.Window
    {
        public static List<string> imgDirs = new List<string>();

        public void updateLabel()
        {
            UInt128 total = 1;
            foreach (var folder in folderListContainer.folderList)
            {
                if (folder.fullpath.Contains("6. eyes"))
                {
                    int mult = (folder.manifest.Count + (folder.manifest.Count * (folder.manifest.Count - 1)));
                    total = total * (UInt128)mult;
                }
                else if (folder.fullpath.Contains("het"))
                {
                    //accounted for in eyes section
                }
                else
                {
                    total = total * (UInt128)folder.manifest.Count;
                }
            }
            combLabel.Content = String.Format("Total combinations possible: {0}", total);
        }

        public void loadTxt()
        {
            txtLists.negList = File.ReadAllLines(Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\mayhapsRootDir", "dir", null) + "\\text\\negative_traits.txt");
            txtLists.posList = File.ReadAllLines(Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\mayhapsRootDir", "dir", null) + "\\text\\positive_traits.txt");
            txtLists.neuList = File.ReadAllLines(Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\mayhapsRootDir", "dir", null) + "\\text\\neutral_traits.txt");
            txtLists.preList = File.ReadAllLines(Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\mayhapsRootDir", "dir", null) + "\\text\\prefix_list.txt");
            txtLists.sufList = File.ReadAllLines(Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\mayhapsRootDir", "dir", null) + "\\text\\suffix_list.txt");
            txtLists.MpetList = File.ReadAllLines(Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\mayhapsRootDir", "dir", null) + "\\text\\male_kittypet.txt");
            txtLists.FpetList = File.ReadAllLines(Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\mayhapsRootDir", "dir", null) + "\\text\\female_kittypet.txt");
        }

        public List<string> mapTree(string startDir)
        {
            List<string> imgDirsIn;
            var imgDirs = new List<string>();
            imgDirsIn = findSubDirs(startDir + "\\images");
            imgDirs.AddRange(imgDirsIn);
            return imgDirs;
        }

        public List<string> findSubDirs(string dir)
        {
            List<string> subDirs = new List<string>();
            List<string> imgDirs = new List<string>();
            List<string> switchDirs = new List<string>();
            List<string> hetDirs = new List<string>();
            subDirs.AddRange(Directory.GetDirectories(dir).Where(s => !s.EndsWith("8b. human accessories") && !s.EndsWith("6a. het left") && !s.EndsWith("6b. het right")));
            switchDirs.AddRange(Directory.GetDirectories(dir).Where(s => s.EndsWith("8b. human accessories") && !s.EndsWith("6a. het left") && !s.EndsWith("6b. het right")));
            hetDirs.AddRange(Directory.GetDirectories(dir).Where(s => s.EndsWith("6a. het left") || s.EndsWith("6b. het right")));
            foreach (string subdirectory in subDirs)
            {
                imgDirs.AddRange(findSubDirs(subdirectory));
            }
            bool imgCheck = imageSearch(dir);
            if (imgCheck)
            {
                imgDirs.Add(dir);
            }
            List<Folder> classList = new List<Folder>();
            List<Folder> hetList = new List<Folder>();
            foreach (var switchDir in switchDirs)
            {
                List<string> imgPath = new List<string>();
                imgPath.AddRange(Directory.GetFiles(switchDir, "*.*", SearchOption.TopDirectoryOnly).Where(s => s.EndsWith(".png")));
                Folder imgDir = new Folder(switchDir, imgPath);
                classList.Add(imgDir);
            }
            foreach (var hetDir in hetDirs)
            {
                List<string> imgPath = new List<string>();
                imgPath.AddRange(Directory.GetFiles(hetDir, "*.*", SearchOption.TopDirectoryOnly).Where(s => s.EndsWith(".png")));
                Folder imgDir = new Folder(hetDir, imgPath);
                hetList.Add(imgDir);
            }
            folderListContainer.switchFolders.Clear();
            folderListContainer.switchFolders.AddRange(classList);
            folderListContainer.hetFolders.Clear();
            folderListContainer.hetFolders.AddRange(hetList);
            return imgDirs;
        }

        private static bool imageSearch(string dir)
        {
            List<string> images = new List<string>();
            images.AddRange(Directory.GetFiles(dir, "*.*", SearchOption.TopDirectoryOnly).Where(s => s.EndsWith(".png")));
            if (images.Count == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public Window1()
        {
            InitializeComponent();
            updateLabel();
            pathtextbox.Text = (string?)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\mayhapsRootDir", "dir", null);
            saveTextBox.Text = (string?)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\mayhapsRootDir", "saveDir", null);
            /*if (folderListContainer.folderList.Count > 0)
            {
                foreach (var folder in folderListContainer.folderList)
                {
                    System.Windows.Controls.ComboBox comboBox = new System.Windows.Controls.ComboBox();
                    comboBox.SelectedIndex = 0;
                }
            }*/
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void browsebutton_Click(object sender, RoutedEventArgs e)
        {
            string installdir = Directory.GetCurrentDirectory();
            var dlg = new FolderPicker();
            dlg.InputPath = installdir;
            if (dlg.ShowDialog() == true)
            {
                //MessageBox.Show(dlg.ResultPath);
                Registry.SetValue("HKEY_CURRENT_USER\\SOFTWARE\\mayhapsRootDir", "dir", dlg.ResultPath);
                pathtextbox.Text = (string?)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\mayhapsRootDir", "dir", null);
                Activate();
            }
        }

        private void pathtextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (pathtextbox.Text != "")
            {
                Width = double.NaN;
                Height = double.NaN;
            }
            else
            {
                Width = 100;
            }
        }

        private void Button_Click_1(object sender, object e)
        {
            loadTxt();
            imgDirs.AddRange(mapTree(pathtextbox.Text));
            List<Folder> classList = new List<Folder>();
            foreach (var dir in imgDirs)
            {
                List<string> imgPath = new List<string>();
                imgPath.AddRange(Directory.GetFiles(dir, "*.*", SearchOption.TopDirectoryOnly).Where(s => s.EndsWith(".png")));
                Folder imgDir = new Folder(dir, imgPath);
                classList.Add(imgDir);
            }
            folderListContainer.folderList.Clear();
            folderListContainer.folderList.AddRange(classList);
            foreach (var folder in classList)
            imgDirs.Clear();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                string installdir = Directory.GetCurrentDirectory();
                var dlg = new FolderPicker();
                dlg.InputPath = installdir;
                if (dlg.ShowDialog() == true)
                {
                    //MessageBox.Show(dlg.ResultPath);
                    Registry.SetValue("HKEY_CURRENT_USER\\SOFTWARE\\mayhapsRootDir", "saveDir", dlg.ResultPath);
                    saveTextBox.Text = (string?)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\mayhapsRootDir", "saveDir", null);
                    Activate();
                }
            }
            catch(Exception ex) { MessageBox.Show(ex.ToString()); }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to clear the registry?","Clear Registry",MessageBoxButton.YesNo,MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                Registry.CurrentUser.DeleteSubKey("SOFTWARE\\mayhapsRootDir");
            }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            folderListContainer.darkMode++;
            if (folderListContainer.darkMode > 1)
            {
                folderListContainer.darkMode = 0;
            }
        }
    }
}
