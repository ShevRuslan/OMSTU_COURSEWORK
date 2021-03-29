using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMSTU_COURSEWORK
{
    class ResponseMatrixTag
    {
        public List<TagIntersection> arrayTags { get; set; }
        public string[,] array { get; set; }
        public uint currentI { get; set; }
        public uint currentJ { get; set; }
        public List<MatrixTag> arrayFinds { get; set; }
        public List<string> tags { get; set; }
        public List<string> allTags { get; set; }
    }
}
