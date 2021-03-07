using Newtonsoft.Json;
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

namespace OMSTU_COURSEWORK
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Track> tracks = new List<Track>();
        List<string> tags = new List<string>();
        public MainWindow()
        {
            InitializeComponent();
            GetTracks();
        }
        private void GetTracks ()
        {
            string path = @"C:\Users\Руслан\source\repos\ConsoleApp1\ConsoleApp1\bin\Debug\netcoreapp3.1\tracks4.json";
            using (StreamReader r = new StreamReader(path))
            {
                string json = r.ReadToEnd();
                dynamic tracksFile = JsonConvert.DeserializeObject(json);
                foreach (var track in tracksFile)
                {
                    Track track_ = new Track
                    {
                        name = track.name,
                        link = track.link,
                        artist = track.artist,
                    };
                    tracks.Add(track_);
                    foreach(var tag in track.tags)
                    {
                        AddTag((string) tag);
                    }
                }
                tracksList.ItemsSource = tracks;
                tagsList.ItemsSource = tags;
            }
        }
        private void AddTag(string tag)
        {
            bool exsistTag = tags.Exists(_tag => _tag.Equals(tag));
            if (!exsistTag) tags.Add(tag);
        }
    }
}
