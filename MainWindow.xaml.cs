using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Drawing;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.IO;
using System.Xml;
using Newtonsoft.Json;

namespace Dev_Access
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    ///

    #region MyFiles
    public class MyFiles
    {
        public String FilePath { get; set; }
        public String FileName { get; set; }
    }
    #endregion

    #region FileToImageIconConverter
    public class FileToImageIconConverter
    {
        private string filePath;
        private System.Windows.Media.ImageSource icon;

        public string FilePath { get { return filePath; } }

        public System.Windows.Media.ImageSource Icon
        {
            get
            {
                if (icon == null && System.IO.File.Exists(FilePath))
                {
                    using (System.Drawing.Icon sysicon = System.Drawing.Icon.ExtractAssociatedIcon(FilePath))
                    {
                        icon = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                                  sysicon.Handle,
                                  System.Windows.Int32Rect.Empty,
                                  System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                    }
                }

                return icon;
            }
        }

        public FileToImageIconConverter(string filePath)
        {
            this.filePath = filePath;
        }
    }

    #endregion

    public partial class MainWindow : Window
    {
        ObservableCollection <MyFiles> myFilesList;
        string dataPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        private void addNewApp(ImageSource fileIcon, String fileName, String fullfilePath)
        {
            Button myButton = new Button
            {
                Width = 30,
                Height = 30,
                Background = new SolidColorBrush(Colors.Transparent),
                BorderThickness = new Thickness(0, 0, 0, 0),
                Margin = new Thickness(10, 10, 10, 10),
            };
            System.Windows.Controls.Image image = new System.Windows.Controls.Image();
            image.Source = fileIcon;
            RenderOptions.SetBitmapScalingMode(fileIcon, BitmapScalingMode.Fant);
            myButton.Content = image;
            myButton.Click += new RoutedEventHandler(Actionlaunch);
            myButton.ToolTip = fullfilePath;
            stack_panel.Children.Add(myButton);
        }

        public MainWindow()
        {
            InitializeComponent();
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.Fant);

            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
            reloadApps();

            //Application.Current.MainWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        void MouseEnters(object sender, RoutedEventArgs e)
        {
            this.AllowsTransparency = false;
        }

        void Quitaction(object sender, RoutedEventArgs e)
        {
            string json_serial = JsonConvert.SerializeObject(myFilesList);

            // Write the string array to a new file named "WriteLines.txt".
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(dataPath, "DevAccess.data")))
            {
                outputFile.WriteLine(json_serial);
            }

            System.Windows.Application.Current.Shutdown();
        }

        void Addaction(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = "c:\\";
            openFileDialog.Filter = "exe files (*.exe)|*.exe";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == true)
            {
                var filePath = string.Empty;
                ImageSource imgSource = null;

                //Get the path of specified file
                filePath = openFileDialog.FileName;

                FileToImageIconConverter some = new FileToImageIconConverter(filePath);
                imgSource = some.Icon;

                myFilesList.Add(new MyFiles { FilePath= filePath, FileName = openFileDialog.SafeFileName });

                addNewApp(imgSource, openFileDialog.SafeFileName, filePath);
            }

        }

        void Actionlaunch(object sender, RoutedEventArgs e)
        {
            string filename = (string)(sender as Button).ToolTip;

            if (filename.ToLower().Contains("exe"))
            {
                Process.Start(filename);
            }
            else
            {
                Process.Start("cmd", filename);
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
            this.Left = desktopWorkingArea.Right - this.Width;
            this.Top = desktopWorkingArea.Bottom - this.Height;
        }

        private void reloadApps()
        {
            try
            {
                string readstring = null;

                // Open the text file using a stream reader.
                using (var sr = new StreamReader(Path.Combine(dataPath, "DevAccess.data")))
                {
                    // Read the stream as a string, and write the string to the console.
                    readstring = sr.ReadToEnd();
                }

                myFilesList = new ObservableCollection<MyFiles>(JsonConvert.DeserializeObject<List<MyFiles>>(readstring));


                foreach (var myFile in myFilesList)
                {
                    FileToImageIconConverter some = new FileToImageIconConverter(myFile.FilePath);
                    ImageSource imgSource = some.Icon;
                    addNewApp(imgSource, myFile.FileName, myFile.FilePath);
                }
            }
            catch (Exception ex)
            {
                myFilesList = new ObservableCollection<MyFiles>();
            }
        }
    }
}
