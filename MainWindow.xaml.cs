using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
        Matrix matrixTags;
        List<TagIntersection> tagsIntersection = new List<TagIntersection>();
        private const int MAX_RECURSIVE_CALLS = 2000;
        static int ctr = 0;
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
                matrixTags = new Matrix(tags.Count, tags.Count);
                matrixTags.AddFirstRow(tags.ToArray());
                matrixTags.AddFirstCol(tags.ToArray());
            }
        }
        private void AddTag(string tag)
        {
            bool exsistTag = tags.Exists(_tag => _tag.Equals(tag));
            if (!exsistTag) tags.Add(tag);
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string path = @"H:\coding\table-tags\src\tags-full.json";
            using (StreamReader r = new StreamReader(path))
            {
                string json = r.ReadToEnd();
                dynamic tracksFile = JsonConvert.DeserializeObject(json);
                foreach (var track in tracksFile)
                {
                    List<Tag> intersectionTags = new List<Tag>();
                    foreach(var interSectionTag in track.Value) {
                        intersectionTags.Add(new Tag
                        {
                            name = interSectionTag.Name,
                            count = interSectionTag.Value
                        });
                    }

                    tagsIntersection.Add(new TagIntersection
                    {
                        tag = track.Name,
                        intersectionTags = intersectionTags,
                    });
                }
            }

            string[,] intersectionsTags = new string[(tagsIntersection.Count + 1), (tagsIntersection.Count + 1)];
            intersectionsTags[0, 0] = "";
            //Заполняем матрицу (колонки и верхнюю строчку) тегами
            for (int i = 0; i < tagsIntersection.Count; i++)
            {
                for(int j = 0; j < tagsIntersection.Count; j++)
                {
                    if(i == 0)
                    {
                        intersectionsTags[i, j + 1] = tagsIntersection[j].tag;
                    }
                    if (j == 0)
                    {
                        intersectionsTags[i + 1, j] = tagsIntersection[i].tag;
                    }
                }
            }
            //Заполняем матрицу пересечениями
            for (int i = 0; i <= tagsIntersection.Count; i++)
            {
                for (int j = 0; j <= tagsIntersection.Count; j++)
                {
                    if(i != 0 && j != 0)
                    {
                        string firstTag = intersectionsTags[i, 0];
                        string secondTag = intersectionsTags[0, j];
                        intersectionsTags[i, j] = GetIntersection(tagsIntersection, firstTag, secondTag).ToString();
                    }
                }
            }
            List<List<string>> allTags = new List<List<string>>();
            uint x = 2;
            uint y = 0;
            List<MatrixTag> arrayFinds = new List<MatrixTag>();
            List<string> _tags = new List<string>();
            while (true)
            {
                ResponseMatrixTag first = new ResponseMatrixTag();
                first = MatrixFind(tagsIntersection, intersectionsTags, x, y, arrayFinds, _tags, new List<string>());
                if (first.allTags.Count == 0) break;
                    allTags.Add(new List<string>(first.allTags));
                x = first.currentI;
                y = first.currentJ;
                arrayFinds = first.arrayFinds;
                _tags = first.tags;
            }
            uint index = 1;
            List<string> _findTags = new List<string>();
            foreach (List<string> s in allTags)
            {
                string _tag = "";
                foreach (string _s in s)
                {
                    _tag += _s + " / ";
                }
                _findTags.Add(_tag);
                index++;
            }
            findTags.ItemsSource = _findTags;
        }

        private uint GetIntersection(List<TagIntersection> tagsIntersection, string firstTag, string secondTag)
        {
            TagIntersection tag = tagsIntersection.Find(_tag => _tag.tag == firstTag);
            uint intersection = tag.intersectionTags.Find(_tag => _tag.name == secondTag).count;
            return intersection;
        }
        private ResponseMatrixTag MatrixFind(List<TagIntersection> arrayTags, string[,] array, uint currentI, uint currentJ, List<MatrixTag> arrayFinds, List<string> tags, List<string> allTags)
        {
            if (tags.Count == 7)
            {
                allTags = new List<string>(tags);
                allTags = new List<string>(tags);
                tags.RemoveAt(tags.Count - 1);
                uint x = 0, y = 0;
                MatrixTag lastTag = arrayFinds[arrayFinds.Count - 1];
                arrayFinds.RemoveAt(arrayFinds.Count - 1);
                x = lastTag.x;
                y = lastTag.y;
                return new ResponseMatrixTag { arrayTags = arrayTags, array = array, currentI = x, currentJ = y + 1, arrayFinds = arrayFinds, tags = tags, allTags = allTags };
            }
            for (uint i = currentI; i <= arrayTags.Count; i++)
            {
                for(uint j = currentJ; j <= arrayTags.Count; j++)
                {
                    if(i != 0 && j != 0)
                    {
                        if(array[i, j] == "0")
                        {
                            if(tags.Count == 0)
                            {
                                tags.Add(array[i, 0]);
                                arrayFinds.Add(new MatrixTag { x = i, y = 0 });
                            }
                            if(isIncompatible(tags, array[0, j])) {
                                arrayFinds.Add(new MatrixTag { x = i, y = j });
                                tags.Add(array[0, j]);
                                if (j == arrayTags.Count)
                                    return MatrixFind(arrayTags, array, j, j, arrayFinds, tags, allTags);
                                else
                                    return MatrixFind(arrayTags, array, j, j + 1, arrayFinds, tags, allTags);

                            }
                            else
                            {
                                return MatrixFind(arrayTags, array, j, j + 1, arrayFinds, tags, allTags);
                            }
                        }
                        else
                        {
                            if(j == arrayTags.Count)
                            {
                                allTags = new List<string>(tags);
                                tags.RemoveAt(tags.Count - 1);
                                uint x = 0, y = 0;

                                if(tags.Count >= 2)
                                {
                                    MatrixTag lastTag = arrayFinds[arrayFinds.Count - 1];
                                    arrayFinds.RemoveAt(arrayFinds.Count - 1);
                                    x = lastTag.x;
                                    y = lastTag.y;
                                }

                                if (arrayFinds[arrayFinds.Count - 1].y == arrayTags.Count && tags.Count == 1) return new ResponseMatrixTag { arrayTags = arrayTags, array = array, currentI = x, currentJ = y + 1, arrayFinds = arrayFinds, tags = tags, allTags = new List<string>() }; ;

                                if(tags.Count < 2)
                                {
                                    MatrixTag lastTag = arrayFinds[arrayFinds.Count - 1];
                                    arrayFinds.RemoveAt(arrayFinds.Count - 1);
                                    x = lastTag.x;
                                    y = lastTag.y;
                                }

                                if(y == arrayTags.Count)
                                {
                                    MatrixTag lastTag = arrayFinds[arrayFinds.Count - 1];
                                    arrayFinds.RemoveAt(arrayFinds.Count - 1);
                                    x = lastTag.x;
                                    y = lastTag.y;

                                    if(tags.Count == 5) allTags = new List<string>(tags);
                                    tags.RemoveAt(tags.Count - 1);
                                    return new ResponseMatrixTag {arrayTags = arrayTags,array = array, currentI = x,  currentJ =y + 1, arrayFinds = arrayFinds, tags = tags, allTags = allTags };
                                }
                                return new ResponseMatrixTag { arrayTags = arrayTags, array = array, currentI = x, currentJ = y + 1, arrayFinds = arrayFinds, tags = tags, allTags = allTags };
                            }
                        }
                    }
                }
            }
            return new ResponseMatrixTag { arrayTags = arrayTags, array = array, currentI = 0, currentJ = 0, arrayFinds = arrayFinds, tags = tags, allTags = allTags };
        }
        private bool isIncompatible(List<string> incompatibleTags, string tag)
        {
            bool isIncompatible = true;
            foreach(string incompatibleTag in incompatibleTags)
            {
                if(GetIntersection(tagsIntersection, incompatibleTag, tag) != 0 && isIncompatible)
                {
                    isIncompatible = false;
                }
            }
            return isIncompatible;
        }
    }
}
