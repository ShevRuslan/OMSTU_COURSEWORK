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
        Matrix matrixTags;
        List<TagIntersection> tagsIntersection = new List<TagIntersection>();
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
            string path = @"H:\coding\table-tags\src\tags.json";
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
            List<List<string>> first = new List<List<string>>();
            first = MatrixFind(tagsIntersection, intersectionsTags, 1, 0, new List<MatrixTag>(), new List<string>(), new List<List<string>>());
            using (TextWriter tw = new StreamWriter("SavedList.txt"))
            {
                foreach (List<string> s in first)
                {
                    foreach (string _s in s)
                    {
                        tw.Write(_s + " / ");
                    }
                    tw.WriteLine("\n");
                }
            }
        }

        private uint GetIntersection(List<TagIntersection> tagsIntersection, string firstTag, string secondTag)
        {
            TagIntersection tag = tagsIntersection.Find(_tag => _tag.tag == firstTag);
            uint intersection = tag.intersectionTags.Find(_tag => _tag.name == secondTag).count;
            return intersection;
        }
        private List<List<string>> MatrixFind(List<TagIntersection> arrayTags, string[,] array, uint currentI, uint currentJ, List<MatrixTag> arrayFinds, List<string> tags, List<List<string>> allTags)
        {
            for(uint i = currentI; i <= arrayTags.Count; i++)
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
                                if (tags.Count == 1) return allTags;

                                allTags.Add(new List<string>(tags));
                                tags.RemoveAt(tags.Count - 1);
                                uint x = 0, y = 0;

                                if(tags.Count >= 2)
                                {
                                    MatrixTag lastTag = arrayFinds[arrayFinds.Count - 1];
                                    arrayFinds.RemoveAt(arrayFinds.Count - 1);
                                    x = lastTag.x;
                                    y = lastTag.y;
                                }

                                if (arrayFinds[arrayFinds.Count - 1].y == arrayTags.Count && tags.Count == 1) return allTags;

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

                                    if(tags.Count == 5) allTags.Add(new List<string>(tags));
                                    tags.RemoveAt(tags.Count - 1);
                                    return MatrixFind(arrayTags, array, x, y + 1, arrayFinds, tags, allTags);
                                }
                                return MatrixFind(arrayTags, array, x, y + 1, arrayFinds, tags, allTags);
                            }
                        }
                    }
                }
            }
            return allTags;
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
