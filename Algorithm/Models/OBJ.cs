using System.IO;
using Algorithm.Lib;

namespace Algorithm.Models
{
    public class OBJ
    {
        public static OBJ OBJFromSTL(STL mesh)
        {
            return new OBJ();
        }

        public void SaveAsBinary(string path)
        {
            using Stream stream = File.Create(path);
        }
    }
}