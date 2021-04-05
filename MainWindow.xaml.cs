using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace OMSTU_COURSEWORK
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Track> tracks = new List<Track>();
        List<Tag> tags = new List<Tag>();
        List<TagIntersection> tagsIntersection = new List<TagIntersection>();
        Dictionary<string, Dictionary<string, int>> crossingTag = new Dictionary<string, Dictionary<string, int>>();
        Dictionary<string, int> allTags = new Dictionary<string, int>();
        private string path = @"C:\Users\Руслан\source\repos\ConsoleApp1\ConsoleApp1\bin\Debug\netcoreapp3.1\tracks5.json";
        public MainWindow()
        {
            InitializeComponent();
            GetTracks();
            GetCrossingTags();
        }
        private void GetTracks ()
        {
            using (StreamReader r = new StreamReader(path))
            {
                string json = r.ReadToEnd();
                dynamic tracksFile = JsonConvert.DeserializeObject(json);
                foreach (var trackInFile in tracksFile)
                {

                    tracks.Add(new Track
                    {
                        name = trackInFile.name,
                        link = trackInFile.link,
                        artist = trackInFile.artist,
                    });
                    foreach(string tag in trackInFile.tags)
                    {
                        AddTag(tag);
                        allTags[tag] += 1;
                    }
                }
                List<Tag> orderTags = DictionaryToList(allTags);
                tags = orderTags;
                tracksList.ItemsSource = tracks;
                tagsList.ItemsSource = orderTags;
                countTrack.Text = string.Format("Количество треков: {0}",tracks.Count.ToString());
                countTags.Text = string.Format("Количество тегов: {0}", orderTags.Count.ToString());
            }
        }
        private List<Tag> DictionaryToList (Dictionary<string, int> allTags)
        {
            List<Tag> tagsList = new List<Tag>();
            foreach (var tag in allTags)
            {
                tagsList.Add(new Tag { name = tag.Key, count = (uint)tag.Value });
            }
            return tagsList.OrderByDescending(o => o.count).ToList();
        }
        private void GetCrossingTags ()
        {
            List<Tag> filterList = tags.Where(tag => Convert.ToDouble(tag.count) / Convert.ToDouble(tracks.Count) * 100 >= 4).ToList();
            using (StreamReader r = new StreamReader(path))
            {
                string json = r.ReadToEnd();
                dynamic tracksFile = JsonConvert.DeserializeObject(json);
                foreach(Tag tag in tags)
                {
                    if(filterList.Any(_tag => _tag.name == tag.name))
                    {
                        foreach(var track in tracksFile)
                        {
                            List<string> tracksTagss = GetTrackTags(track.tags);
                            if(tracksTagss.Any(_trackTag => _trackTag == tag.name))
                            {
                                List<string> trackTag = new List<string>();
                                foreach (var tracksTags in track.tags)
                                {
                                    bool exsistTag = filterList.Any(_tag => _tag.name == (string)tracksTags);
                                    if (exsistTag) trackTag.Add((string)tracksTags);
                                }
                                foreach (string _tag in trackTag)
                                {
                                    bool exsistTag = filterList.Any(tagFind => tagFind.name == _tag);
                                    if (exsistTag)
                                    {
                                        if (!crossingTag.ContainsKey(tag.name)) crossingTag.Add(tag.name, new Dictionary<string, int>());
                                        if (!crossingTag[tag.name].ContainsKey(_tag)) crossingTag[tag.name].Add(_tag, 1);
                                        else crossingTag[tag.name][_tag] += 1;
                                    }
                                }
                            }
                        }
                    }
                }
                /*
                foreach (Tag tag in tags)
                {
                    foreach(var track in tracksFile)
                    {
                        List<string> trackTag = new List<string>();
                        foreach(var tracksTags in track.tags)
                        {
                            bool exsistTag = filterList.Any(_tag => _tag.name == (string)tracksTags);
                            if (exsistTag) trackTag.Add((string)tracksTags);
                        }
                        bool exsist = trackTag.Exists(_trackTag => _trackTag == tag.name);
                        if(exsist)
                        {
                            foreach(string _tag in trackTag)
                            {
                                bool exsistTag = filterList.Any(tagFind => tagFind.name == _tag);
                                if(exsistTag)
                                {
                                    if (!crossingTag.ContainsKey(tag.name)) crossingTag.Add(tag.name, new Dictionary<string, int>());
                                    if (!crossingTag[tag.name].ContainsKey(_tag)) crossingTag[tag.name].Add(_tag, 1);
                                    else crossingTag[tag.name][_tag] += 1;
                                }
                            }
                        }
                    }
                }
                
                foreach(KeyValuePair<string, Dictionary<string, int>> tag in crossingTag)
                {
                    foreach(Tag uniqTag in tags)
                    {
                        if(!crossingTag[tag.Key].ContainsKey(uniqTag.name)) crossingTag[tag.Key].Add(uniqTag.name, 0);
                    }
                }
                */
            }
        }
        private List<string> GetTrackTags(dynamic tags)
        {
            List<string> trackTags = new List<string>();
            foreach(var tag in tags)
            {
                trackTags.Add((string)tag);
            }
            return trackTags;
        }
        private void AddTag(string tag)
        {
            if (!allTags.ContainsKey(tag)) allTags.Add(tag, 0);
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            foreach (KeyValuePair<string, Dictionary<string, int>> tag in crossingTag)
            {
                List<Tag> intersectionTags = new List<Tag>();
                foreach (var interSectionTag in tag.Value)
                {
                    intersectionTags.Add(new Tag
                    {
                        name = interSectionTag.Key,
                        count = (uint)interSectionTag.Value
                    });
                }

                tagsIntersection.Add(new TagIntersection
                {
                    tag = tag.Key,
                    intersectionTags = intersectionTags,
                });
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
            uint x = 1;
            uint y = 0;
            List<MatrixTag> arrayFinds = new List<MatrixTag>();
            List<string> _tags = new List<string>();
            ResponseMatrixTag first = new ResponseMatrixTag();

            for (uint i = 1; i < tagsIntersection.Count + 1; i++)
            {
                x = i;
                _tags = new List<string>();
                while (true)
                {
                    first = MatrixFind(tagsIntersection, intersectionsTags, x, y, arrayFinds, _tags, new List<string>());
                    if(first.allTags.Count != 1) allTags.Add(new List<string>(first.allTags));
                    if (first.tags.Count == 1) break;
                    x = first.currentI;
                    y = first.currentJ + 1;
                    arrayFinds = first.arrayFinds;
                    _tags = first.tags;
                }
            }

            uint index = 1;
            List<string> _findTags = new List<string>();
            foreach (List<string> s in allTags)
            {
                string _tag = "";
                foreach (string _s in s)
                {
                    _tag += _s + "  (" + tags.Find(findTag => findTag.name == _s).count.ToString() + ") / ";
                }
                _tag += GetEntropy(s).ToString();
                _findTags.Add(_tag);
                index++;
            }
            findTags.ItemsSource = _findTags;
        }
        private float GetEntropy(List<string> tags)
        {
            List<float> numbers = new List<float>();
            float entropy = 0;
            int count = 0;
            float srOtkl = 0;
            foreach(string tag in tags)
            {
                count += (int) this.tags.Find(findTag => findTag.name == tag).count;
            }
            foreach (string tag in tags)
            {
                float otkl = (float)this.tags.Find(findTag => findTag.name == tag).count / count;
                float h = (float)(-otkl * Math.Log(otkl, 2));
                srOtkl += h;
            }
            entropy = (float)(srOtkl / Math.Log(count,2));
            return entropy;
        }
        private float getStandardDeviation(List<float> doubleList)
        {
            float average = doubleList.Average();
            float sumOfDerivation = 0;
            foreach (float value in doubleList)
            {
                sumOfDerivation += (value) * (value);
            }
            double sumOfDerivationAverage = sumOfDerivation / (doubleList.Count - 1);
            return (float)Math.Sqrt(sumOfDerivationAverage - (average * average));
        }
        private uint GetIntersection(List<TagIntersection> tagsIntersection, string firstTag, string secondTag)
        {
            TagIntersection tag = tagsIntersection.Find(_tag => _tag.tag == firstTag);
            Tag intersection = tag.intersectionTags.Find(_tag => _tag.name == secondTag);
            return intersection == null ? 0 : intersection.count;
        }
        private ResponseMatrixTag MatrixFind(List<TagIntersection> arrayTags, string[,] array, uint currentI, uint currentJ, List<MatrixTag> arrayFinds, List<string> tags, List<string> allTags)
        {
            //Если в данный момент нету тегов, то добавляем текущий тег с которого ищем
            if (tags.Count == 0)
            {
                tags.Add(array[currentI, 0]);
                arrayFinds.Add(new MatrixTag { x = currentI, y = 0 });
            }
            for (uint i = currentI; i <= arrayTags.Count; i++)
            {
                for (uint j = currentJ; j <= arrayTags.Count; j++)
                {
                    if (i != 0 && j != 0)
                    {
                        //Если уже координата по X уже последняя, т.е всю матрицу чекнули
                        if (j == arrayTags.Count)
                        {
                            //Если несовместых тег нет, то просто возращаем текущее состояние.
                            if(tags.Count == 1)
                            {

                                return new ResponseMatrixTag { arrayTags = arrayTags, array = array, currentI = i, currentJ = j, arrayFinds = arrayFinds, tags = tags, allTags = new List<string>(tags) };
                            }

                            allTags = new List<string>(tags);
                            tags.RemoveAt(tags.Count - 1);
                            MatrixTag lastTag = arrayFinds[arrayFinds.Count - 1];
                            arrayFinds.RemoveAt(arrayFinds.Count - 1);
                            uint x = lastTag.x;
                            uint y = lastTag.y;
                            return new ResponseMatrixTag { arrayTags = arrayTags, array = array, currentI = x, currentJ = y, arrayFinds = arrayFinds, tags = tags, allTags = allTags };
                        }
                        //Если теги между собой ни разу не пересекаются
                        if (array[i, j] == "0")
                        {
                            //Проверяем, все ли текущие теги несовместные
                            if (isIncompatible(tags, array[0, j]))
                            {
                                //Добавляем координаты тега
                                arrayFinds.Add(new MatrixTag { x = i, y = j });
                                //Добавляем тег
                                tags.Add(array[0, j]);

                            }
                            if (j == arrayTags.Count)
                                return MatrixFind(arrayTags, array, i, j, arrayFinds, tags, allTags);
                            else
                                return MatrixFind(arrayTags, array, i, j + 1, arrayFinds, tags, allTags);
                        }
                    }
                }
            }
            return new ResponseMatrixTag { arrayTags = arrayTags, array = array, currentI = 0, currentJ = 0, arrayFinds = arrayFinds, tags = tags, allTags = allTags };
        }
        /*
        private ResponseMatrixTag MatrixFind(List<TagIntersection> arrayTags, string[,] array, uint currentI, uint currentJ, List<MatrixTag> arrayFinds, List<string> tags, List<string> allTags)
        {
            if (tags.Count == 5)
            {
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
                for (uint j = currentJ; j <= arrayTags.Count; j++)
                {
                    if (i != 0 && j != 0)
                    {
                        //Если теги между собой ни разу не пересекаются
                        if (array[i, j] == "0")
                        {
                            //Если еще локальных тегов нет - добавляем (по идеи это будет тупо тег, с которого мы начали
                            if (tags.Count == 0)
                            {
                                tags.Add(array[i, 0]);
                                arrayFinds.Add(new MatrixTag { x = i, y = 0 });
                            }
                            //Проверяем, что все теги между собой несовместные
                            if (isIncompatible(tags, array[0, j]))
                            {
                                //Добавляем координаты нового тега
                                arrayFinds.Add(new MatrixTag { x = i, y = j });
                                //Добавляем сам тег
                                tags.Add(array[0, j]);

                            }
                            //Если это последний тег, то возвращаем те же самые координа, если же не последний, то возвращем j + 1
                            if (j == arrayTags.Count)
                                return MatrixFind(arrayTags, array, j, j, arrayFinds, tags, allTags);
                            else
                                return MatrixFind(arrayTags, array, j, j + 1, arrayFinds, tags, allTags);
                        }
                        //Если уже координата по X уже последняя, т.е всю матрицу чекнули
                        if (j == arrayTags.Count)
                        {
                            //Если за все это время не найдено ни одного пересечения тега
                            if (tags.Count == 0)
                            {
                                return new ResponseMatrixTag { arrayTags = arrayTags, array = array, currentI = i, currentJ = j, arrayFinds = arrayFinds, tags = tags, allTags = allTags };
                            }
                            //Сохраняем в локальную переменную все найденые теги
                            allTags = new List<string>(tags);
                            //Удаляем последний тег, для того, чтобы потом искать с предпоследнеого
                            tags.RemoveAt(tags.Count - 1);
                            uint x = 0, y = 0;

                            if (tags.Count >= 2)
                            {
                                MatrixTag lastTag = arrayFinds[arrayFinds.Count - 1];
                                arrayFinds.RemoveAt(arrayFinds.Count - 1);
                                x = lastTag.x;
                                y = lastTag.y;
                            }

                            if (arrayFinds[arrayFinds.Count - 1].y == arrayTags.Count && tags.Count == 1) return new ResponseMatrixTag { arrayTags = arrayTags, array = array, currentI = x, currentJ = y + 1, arrayFinds = arrayFinds, tags = tags, allTags = allTags };

                            if (tags.Count < 2)
                            {
                                MatrixTag lastTag = arrayFinds[arrayFinds.Count - 1];
                                arrayFinds.RemoveAt(arrayFinds.Count - 1);
                                x = lastTag.x;
                                y = lastTag.y;
                            }

                            if (y == arrayTags.Count)
                            {
                                MatrixTag lastTag = arrayFinds[arrayFinds.Count - 1];
                                arrayFinds.RemoveAt(arrayFinds.Count - 1);
                                x = lastTag.x;
                                y = lastTag.y;

                                if (tags.Count == 5) allTags = new List<string>(tags);
                                tags.RemoveAt(tags.Count - 1);
                                return new ResponseMatrixTag { arrayTags = arrayTags, array = array, currentI = x, currentJ = y + 1, arrayFinds = arrayFinds, tags = tags, allTags = allTags };
                            }
                            return new ResponseMatrixTag { arrayTags = arrayTags, array = array, currentI = x, currentJ = y + 1, arrayFinds = arrayFinds, tags = tags, allTags = allTags };
                        }
                    }
                }
            }
            return new ResponseMatrixTag { arrayTags = arrayTags, array = array, currentI = 0, currentJ = 0, arrayFinds = arrayFinds, tags = tags, allTags = allTags };
        }
        */
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
