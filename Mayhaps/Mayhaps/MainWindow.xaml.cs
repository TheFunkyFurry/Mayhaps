using Microsoft.VisualBasic.Devices;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using Windows.Gaming.Input.ForceFeedback;
using Windows.UI.WebUI;
using Brushes = System.Windows.Media.Brushes;
using Image = System.Windows.Controls.Image;

namespace Mayhaps
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public clanList clanList = new clanList();
        public Random random = new Random();
        public static List<string> imgDirs = new List<string>();

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

        public static PngBitmapEncoder savePng(int width, int height, Canvas imageCanvas)
        {
            var rect = new Rect(imageCanvas.RenderSize);
            var visual = new DrawingVisual();
            using (var dc = visual.RenderOpen())
            {
                dc.DrawRectangle(new VisualBrush(imageCanvas), null, rect);
            }
            var bitmap = new RenderTargetBitmap((int)rect.Width, (int)rect.Height, 96, 96, PixelFormats.Default);
            bitmap.Render(visual);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            return encoder;
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

        public Folder findFolder(string item)
        {
            foreach (Folder folder in folderListContainer.folderList)
            {
                List<string> img = new List<string>();
                img.AddRange(Directory.GetFiles(folder.fullpath, "*", SearchOption.TopDirectoryOnly).Where(s => s == folder.fullpath + "\\" + item));
                if (img.Count != 0)
                {
                    return folder;
                }
            }
            foreach (Folder folder in folderListContainer.switchFolders)
            {
                List<string> img = new List<string>();
                img.AddRange(Directory.GetFiles(folder.fullpath, "*", SearchOption.TopDirectoryOnly).Where(s => s == folder.fullpath + "\\" + item));
                if (img.Count != 0)
                {
                    return folder;
                }
            }
            return null;
        }

        public void staticGenerate()
        {
            imageCanvas.Children.Clear();
            if (folderListContainer.hetFolders[0].currentIndex == null)
            {
                foreach (Folder folder in folderListContainer.folderList)
                {
                    List<string> manifestHolder = folder.manifest;
                    if (folder.currentIndex != null && folder.setIndex == null)
                    {
                        Image image = new Image();
                        image.Source = new BitmapImage(new Uri(manifestHolder[(int)folder.currentIndex]));
                        image.Width = 500;
                        image.Height = 250;
                        image.Stretch = Stretch.UniformToFill;
                        image.HorizontalAlignment = HorizontalAlignment.Left;
                        imageCanvas.Children.Add(image);
                    }
                    else if (folder.setIndex != null)
                    {
                        Image image = new Image();
                        image.Source = new BitmapImage(new Uri(manifestHolder[(int)folder.setIndex]));
                        image.Width = 500;
                        image.Height = 250;
                        image.Stretch = Stretch.UniformToFill;
                        image.HorizontalAlignment = HorizontalAlignment.Left;
                        imageCanvas.Children.Add(image);
                    }
                    else
                    {
                        //MessageBox.Show("fuck");
                    }
                }
                foreach (Folder folder in folderListContainer.switchFolders)
                {
                    List<string> manifestHolder = folder.manifest;
                    if (folder.currentIndex != null && folder.setIndex == null)
                    {
                        Image image = new Image();
                        image.Source = new BitmapImage(new Uri(manifestHolder[(int)folder.currentIndex]));
                        image.Width = 500;
                        image.Height = 250;
                        image.Stretch = Stretch.UniformToFill;
                        image.HorizontalAlignment = HorizontalAlignment.Left;
                        imageCanvas.Children.Add(image);
                    }
                    else if (folder.setIndex != null)
                    {
                        Image image = new Image();
                        image.Source = new BitmapImage(new Uri(manifestHolder[(int)folder.setIndex]));
                        image.Width = 500;
                        image.Height = 250;
                        image.Stretch = Stretch.UniformToFill;
                        image.HorizontalAlignment = HorizontalAlignment.Left;
                        imageCanvas.Children.Add(image);
                    }
                    else
                    {
                        //MessageBox.Show("fuck");
                    }
                }
            }
            else
            {
                int count = 0;
                foreach (Folder folder in folderListContainer.folderList)
                {
                    if (count != 5)
                    {
                        List<string> manifestHolder = folder.manifest;
                        if (folder.currentIndex != null && folder.setIndex == null)
                        {
                            Image image = new Image();
                            image.Source = new BitmapImage(new Uri(manifestHolder[(int)folder.currentIndex]));
                            image.Width = 500;
                            image.Height = 250;
                            image.Stretch = Stretch.UniformToFill;
                            image.HorizontalAlignment = HorizontalAlignment.Left;
                            imageCanvas.Children.Add(image);
                        }
                        else if (folder.setIndex != null)
                        {
                            Image image = new Image();
                            image.Source = new BitmapImage(new Uri(manifestHolder[(int)folder.setIndex]));
                            image.Width = 500;
                            image.Height = 250;
                            image.Stretch = Stretch.UniformToFill;
                            image.HorizontalAlignment = HorizontalAlignment.Left;
                            imageCanvas.Children.Add(image);
                        }
                        else
                        {
                            //MessageBox.Show("fuck");
                        }
                        count++;
                    }
                    else
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            List<string> manifestHolder = folderListContainer.hetFolders[i].manifest;
                            if (folderListContainer.hetFolders[i].currentIndex != null && folderListContainer.hetFolders[i].setIndex == null)
                            {
                                Image image = new Image();
                                image.Source = new BitmapImage(new Uri(manifestHolder[(int)folderListContainer.hetFolders[i].currentIndex]));
                                image.Width = 500;
                                image.Height = 250;
                                image.Stretch = Stretch.UniformToFill;
                                image.HorizontalAlignment = HorizontalAlignment.Left;
                                imageCanvas.Children.Add(image);
                            }
                            else if (folderListContainer.hetFolders[i].setIndex != null)
                            {
                                Image image = new Image();
                                image.Source = new BitmapImage(new Uri(manifestHolder[(int)folderListContainer.hetFolders[i].setIndex]));
                                image.Width = 500;
                                image.Height = 250;
                                image.Stretch = Stretch.UniformToFill;
                                image.HorizontalAlignment = HorizontalAlignment.Left;
                                imageCanvas.Children.Add(image);
                            }
                            else
                            {
                                //MessageBox.Show("fuck");
                            }
                            count++;
                        }
                    }
                }
                foreach (Folder folder in folderListContainer.switchFolders)
                {
                    List<string> manifestHolder = folder.manifest;
                    if (folder.currentIndex != null && folder.setIndex == null)
                    {
                        Image image = new Image();
                        image.Source = new BitmapImage(new Uri(manifestHolder[(int)folder.currentIndex]));
                        image.Width = 500;
                        image.Height = 250;
                        image.Stretch = Stretch.UniformToFill;
                        image.HorizontalAlignment = HorizontalAlignment.Left;
                        imageCanvas.Children.Add(image);
                    }
                    else if (folder.setIndex != null)
                    {
                        Image image = new Image();
                        image.Source = new BitmapImage(new Uri(manifestHolder[(int)folder.setIndex]));
                        image.Width = 500;
                        image.Height = 250;
                        image.Stretch = Stretch.UniformToFill;
                        image.HorizontalAlignment = HorizontalAlignment.Left;
                        imageCanvas.Children.Add(image);
                    }
                    else
                    {
                        //MessageBox.Show("fuck");
                    }
                }
            }
        }

        private void indexChanged(object sender, EventArgs e)
        {
            foreach (ComboBox box in gridDropdown.Children)
            {
                Folder folder;
                string boxSelect;
                try
                {
                    boxSelect = ((ComboBoxItem)box.Items[box.SelectedIndex]).Content.ToString();
                }
                catch
                {
                    boxSelect = "";
                }
                if (boxSelect == "0.png" || box.SelectedIndex == 0)
                {
                    string newBox = ((ComboBoxItem)box.Items[box.SelectedIndex + 2]).Content.ToString();
                    folder = findFolder(newBox);
                }
                else
                {
                    //boxSelect = ((ComboBoxItem)box.Items[box.SelectedIndex]).Content.ToString();
                    folder = findFolder(boxSelect);
                }
                if (boxSelect != "")
                {
                    folder.setIndex = box.SelectedIndex - 1;
                }
                else if (box.SelectedIndex == 0)
                {
                    folder.setIndex = null;
                }
            }
            staticGenerate();
        }

        public void updateGrid()
        {
            gridDropdown.Children.Clear();
            foreach (Folder folder in folderListContainer.folderList)
            {
                ComboBox comboBox = new ComboBox();
                comboBox.Height = 50;
                ComboBoxItem itemBlank = new ComboBoxItem();
                itemBlank.Content = "";
                comboBox.Items.Add(itemBlank);
                foreach (var img in folder.manifest)
                {
                    ComboBoxItem item = new ComboBoxItem();
                    string remString = folder.fullpath + "\\";
                    int index = img.IndexOf(remString);
                    string cleanPath = (index < 0) ? img : img.Remove(index, remString.Length);
                    item.Content = cleanPath;
                    comboBox.Items.Add(item);
                }
                comboBox.SelectedIndex = 0;
                gridDropdown.Children.Add(comboBox);
                comboBox.SelectionChanged += new SelectionChangedEventHandler(indexChanged);
            }
            foreach (Folder folder in folderListContainer.switchFolders)
            {
                ComboBox comboBox = new ComboBox();
                comboBox.Height = 50;
                ComboBoxItem itemBlank = new ComboBoxItem();
                itemBlank.Content = "";
                comboBox.Items.Add(itemBlank);
                foreach (var img in folder.manifest)
                {
                    ComboBoxItem item = new ComboBoxItem();
                    string remString = folder.fullpath + "\\";
                    int index = img.IndexOf(remString);
                    string cleanPath = (index < 0) ? img : img.Remove(index, remString.Length);
                    item.Content = cleanPath;
                    comboBox.Items.Add(item);
                }

                comboBox.SelectedIndex = 0;
                gridDropdown.Children.Add(comboBox);
                comboBox.SelectionChanged += new SelectionChangedEventHandler(indexChanged);
            }
        }


        public MainWindow()
        {
            try
            {
                InitializeComponent();
                string installDir;
                if (Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\mayhapsRootDir", "dir", null) == null)
                {
                    installDir = Directory.GetCurrentDirectory();
                    string[] assetDir = Directory.GetDirectories(installDir, "assets", SearchOption.TopDirectoryOnly);
                    if (assetDir.Length != 1)
                    {
                        MessageBox.Show("Please select the assets folder.");
                        var dlg = new FolderPicker();
                        dlg.InputPath = installDir;
                        if (dlg.ShowDialog() == true)
                        {
                            string imgRootDir = dlg.ResultPath;
                            //MessageBox.Show(dlg.ResultPath);
                            Registry.SetValue("HKEY_CURRENT_USER\\SOFTWARE\\mayhapsRootDir", "dir", imgRootDir);
                            imgDirs.AddRange(mapTree((string)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\mayhapsRootDir", "dir", Directory.GetCurrentDirectory)));
                        }
                    }
                    else
                    {
                        Registry.SetValue("HKEY_CURRENT_USER\\SOFTWARE\\mayhapsRootDir", "dir", assetDir[0]);
                        imgDirs.AddRange(mapTree((string)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\mayhapsRootDir", "dir", Directory.GetCurrentDirectory)));
                    }

                }
                else
                {
                    if (Directory.Exists((string)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\mayhapsRootDir", "dir", null)))
                        MessageBox.Show("Please select the assets folder.");
                    var dlg = new FolderPicker();
                    installDir = Directory.GetCurrentDirectory();
                    dlg.InputPath = installDir;
                    if (dlg.ShowDialog() == true)
                    {
                        string imgRootDir = dlg.ResultPath;
                        //MessageBox.Show(dlg.ResultPath);
                        Registry.SetValue("HKEY_CURRENT_USER\\SOFTWARE\\mayhapsRootDir", "dir", imgRootDir);
                        imgDirs.AddRange(mapTree((string)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\mayhapsRootDir", "dir", Directory.GetCurrentDirectory)));
                    }
                }
            
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
                updateGrid();
                Activate();
                loadTxt();
                clanList = clanList.loadClans();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Window1 window1 = new Window1();
            window1.ShowDialog();
            updateGrid();
            List<Control> controls = new List<Control>();
            List<ComboBox> boxes = new List<ComboBox>();
            switch (folderListContainer.darkMode)
            {
                case 0:
                    Mayhaps.Background = Brushes.White;
                    try
                    {
                        foreach (Control control in mainGrid.Children)
                        {
                            try
                            {
                                controls.Add(control);
                            }
                            catch { }
                        }
                    }
                    catch { }
                    finally
                    {
                        foreach (ComboBox comboBox in gridDropdown.Children)
                        {
                            boxes.Add(comboBox);
                            controls.Add(comboBox);
                        }
                    }
                    foreach (Control control in controls)
                    {
                        if (control.Name == "bioLabel")
                        {
                            control.Background = Brushes.White;
                            control.Foreground = Brushes.Black;
                        }
                        else
                        {
                            control.Background = Brushes.LightGray;
                            control.Foreground = Brushes.Black;
                        }
                    }
                    /*foreach (ComboBox box in boxes)
                    {
                        foreach (ComboBoxItem item in box.Items)
                        {
                            item.Background = Brushes.LightGray;
                        }
                    }*/
                    break;

                case 1:
                    SolidColorBrush brush = new SolidColorBrush();
                    brush.Color = System.Windows.Media.Color.FromRgb(31, 32, 38);
                    Mayhaps.Background = brush;
                    try
                    {
                        foreach (Control control in mainGrid.Children)
                        {
                            try
                            {
                                controls.Add(control);
                            }
                            catch { }
                        }
                    }
                    catch { }
                    finally
                    {
                        foreach (ComboBox comboBox in gridDropdown.Children)
                        {
                            boxes.Add(comboBox);
                        }
                    }
                    foreach (Control control in controls)
                    {
                        control.Background = brush;
                        control.Foreground = Brushes.White;
                        control.BorderBrush = Brushes.Black;
                    }
                    /*foreach (ComboBox box in boxes)
                    {
                        box.Background = brush;
                        box.BorderBrush = brush;
                        foreach (ComboBoxItem item in box.Items)
                        {
                            item.Background = brush;
                            item.Foreground = Brushes.White;
                            item.BorderBrush = brush;
                        }
                    }*/
                    break;

            }


        }
        private void generatebutton_Click(object sender, RoutedEventArgs e)
        {
            int count = 0;
            int switchCount = 0;
            foreach (ComboBox comboBox in gridDropdown.Children)
            {
                if (comboBox.SelectedIndex != 0 && count < 8)
                {
                    Folder setFolder = folderListContainer.folderList[count];
                    setFolder.setIndex = comboBox.SelectedIndex - 1;
                    if (count == 0)
                    {
                        folderListContainer.coatType = comboBox.Text;
                    }
                    count++;
                }
                else if (count < 8)
                {
                    Folder setFolder = folderListContainer.folderList[count];
                    setFolder.setIndex = null;
                    count++;
                }
                else if (comboBox.SelectedIndex != 0 && count == 8)
                {
                    Folder setFolder = folderListContainer.switchFolders[switchCount];
                    setFolder.setIndex = comboBox.SelectedIndex - 1;
                    switchCount++;
                }
                else
                {
                    Folder setFolder = folderListContainer.switchFolders[switchCount];
                    setFolder.setIndex = null;
                    switchCount++;
                }
            }
            imageCanvas.Children.Clear();
            bio bio = new bio();
            bio.generateBio(clanList);
            bioLabel.Content = string.Format("Name: {0}\nGender: {1}\nAge: {2} moons\nClan: {3}\nRole: {4}\n{5}", bio.name, bio.gender, bio.age, bio.clan, bio.role, bio.pers);
            folderListContainer.bioName = bio.name;
            List<string> manifestHolder;
            bool outEligible = bio.clan is "SkyClan" || bio.clan is "Outsider";
            bool hetProc = false;
            bool hetBeenProc = false;
            try
            {
                foreach (var folder in folderListContainer.folderList)
                {
                    //shortening strings so i don't have to type as much
                    string remString = (string)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\mayhapsRootDir", "dir", null) + "\\images\\";
                    int index = folder.fullpath.IndexOf(remString);
                    string cleanPath = (index < 0) ? folder.fullpath : folder.fullpath.Remove(index, remString.Length);
                    manifestHolder = folder.manifest;
                    int listLen = manifestHolder.Count;
                    ///generation code vvv
                    //does the folder have a specific image selected?
                    if (folder.setIndex == null)
                    {
                        //generation logic for each folder that needs something different
                        switch (cleanPath)
                        {
                            case "8a. nature accessories":
                                if (bio.clan == "SkyClan" && random.Next(0, 100) <= 10 && outEligible && folderListContainer.switchFolders[0].setIndex == null)
                                {
                                    manifestHolder = folderListContainer.switchFolders[0].manifest;
                                    listLen = manifestHolder.Count;
                                    break;
                                }
                                else if (bio.clan == "Outsider" && random.Next(0, 100) <= 30 && outEligible && folderListContainer.switchFolders[0].setIndex == null)
                                {
                                    manifestHolder = folderListContainer.switchFolders[0].manifest;
                                    listLen = manifestHolder.Count;
                                    break;
                                }
                                break;
                            case "4. scars":
                                if (random.Next(0, 100) <= 70)
                                {
                                    listLen = 0;
                                    break;
                                }
                                break;
                            case "2. white spotting":
                                //looks bad / genetically impossible with these bases
                                if (random.Next(0, 100) <= 50 || folderListContainer.coatType.Contains("wide band point.png") || folderListContainer.coatType.Contains("silver tipped.png") || folderListContainer.coatType.Contains("silver shaded.png"))
                                {
                                    listLen = 0;
                                    break;
                                }
                                break;
                            case "3. somatic mutations":
                                if (random.Next(0, 100) <= 98)
                                {
                                    listLen = 0;
                                    break;
                                }
                                break;
                            case "7. eyelashes":
                                if (random.Next(0, 100) <= 50)
                                {
                                    listLen = 0;
                                    break;
                                }
                                break;
                            case "6. eyes":
                                if (random.Next(0, 100) <= 2)
                                {
                                    listLen = 6;
                                    break;
                                }
                                else if (random.Next(0, 100) <= 2)
                                {
                                    hetProc = true;
                                }
                                break;
                        }
                        //adding image layers
                        if (!hetProc) //no heterochromia
                        {
                            Image image = new Image();
                            int randIndex = random.Next(0, listLen);
                            folder.currentIndex = randIndex;
                            if (!hetBeenProc)
                            {
                                for (int i = 0; i < 2; i++)
                                {
                                    folderListContainer.hetFolders[i].currentIndex = null;
                                }
                            }
                            image.Source = new BitmapImage(new Uri(manifestHolder[randIndex]));
                            if (cleanPath == "1. coat colours")
                            {
                                folderListContainer.coatType = manifestHolder[randIndex];
                            }
                            image.Width = 500;
                            image.Height = 250;
                            image.Stretch = Stretch.UniformToFill;
                            image.HorizontalAlignment = HorizontalAlignment.Left;
                            imageCanvas.Children.Add(image);
                        }
                        else //with heterochromia
                        {
                            hetBeenProc = true;
                            int? prevRand = null;
                            for (int i = 0; i < 2; i++)
                            {
                                manifestHolder = folderListContainer.hetFolders[i].manifest;
                                listLen = manifestHolder.Count;
                                Image image = new Image();
                                int randIndex = random.Next(0, listLen);
                                while (randIndex == prevRand)
                                {
                                    randIndex = random.Next(0, listLen);
                                }
                                prevRand = randIndex;
                                folderListContainer.hetFolders[i].currentIndex = prevRand;
                                folder.currentIndex = null;
                                image.Source = new BitmapImage(new Uri(manifestHolder[randIndex]));
                                image.Width = 500;
                                image.Height = 250;
                                image.Stretch = Stretch.UniformToFill;
                                image.HorizontalAlignment = HorizontalAlignment.Left;
                                imageCanvas.Children.Add(image);
                            }
                            hetProc = false;
                        }
                    }
                    else
                    {
                        folder.currentIndex = folder.setIndex;
                        Image image = new Image();
                        image.Source = new BitmapImage(new Uri(manifestHolder[(int)folder.setIndex]));
                        image.Width = 500;
                        image.Height = 250;
                        image.Stretch = Stretch.UniformToFill;
                        image.HorizontalAlignment = HorizontalAlignment.Left;
                        imageCanvas.Children.Add(image);
                    }
                }
                foreach (Folder switchFolder in folderListContainer.switchFolders) 
                {
                    if (switchFolder.setIndex != null) 
                    {
                        Image image = new Image();
                        image.Source = new BitmapImage(new Uri(switchFolder.manifest[(int)switchFolder.setIndex]));
                        image.Width = 500;
                        image.Height = 250;
                        image.Stretch = Stretch.UniformToFill;
                        image.HorizontalAlignment = HorizontalAlignment.Left;
                        imageCanvas.Children.Add(image);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Please send this exception to the developer.");
            }
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string installDir;
                //string? dir = (string?)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\mayhapsRootDir", "saveDir", null);
                if (Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\mayhapsRootDir", "saveDir", null) == null)
                {
                    installDir = Directory.GetCurrentDirectory();
                    string[] saveDir = Directory.GetDirectories(installDir, "saves", SearchOption.TopDirectoryOnly);
                    if (saveDir.Length != 1)
                    {
                        MessageBox.Show("Please select the saves folder.");
                        var dlg = new FolderPicker();
                        dlg.InputPath = installDir;
                        if (dlg.ShowDialog() == true)
                        {
                            string saveRootDir = dlg.ResultPath;
                            //MessageBox.Show(dlg.ResultPath);
                            Registry.SetValue("HKEY_CURRENT_USER\\SOFTWARE\\mayhapsRootDir", "saveDir", saveRootDir);
                        }
                    }
                    else
                    {
                        Registry.SetValue("HKEY_CURRENT_USER\\SOFTWARE\\mayhapsRootDir", "saveDir", saveDir[0]);
                    }
                }
                else
                {
                    if (!Directory.Exists((string)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\mayhapsRootDir", "saveDir", null)))
                        MessageBox.Show("Please select the saves folder.");
                        var dlg = new FolderPicker();
                        installDir = Directory.GetCurrentDirectory();
                        dlg.InputPath = installDir;
                        if (dlg.ShowDialog() == true)
                        {
                            string imgRootDir = dlg.ResultPath;
                            //MessageBox.Show(dlg.ResultPath);
                            Registry.SetValue("HKEY_CURRENT_USER\\SOFTWARE\\mayhapsRootDir", "saveDir", imgRootDir);
                        }
                }
                var image = savePng((int)imageCanvas.Width, (int)imageCanvas.Height, imageCanvas);
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                saveFileDialog1.Filter = "PNG|*.png";
                saveFileDialog1.Title = "Save an Image File";
                saveFileDialog1.InitialDirectory = (string)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\mayhapsRootDir", "saveDir", null);
                saveFileDialog1.FileName = folderListContainer.bioName;
                saveFileDialog1.ShowDialog();
                if (saveFileDialog1.FileName != "")
                {
                    FileStream fs = (FileStream)saveFileDialog1.OpenFile();
                    image.Save(fs);
                    fs.Close();
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }

        private void hetCheck_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
