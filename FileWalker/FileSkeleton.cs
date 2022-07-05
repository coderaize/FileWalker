using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace FileWalker
{
    public class FileSkeleton
    {

        private string[] Drives => Directory.GetLogicalDrives();

        Node Node { get; set; }


        public FileSkeleton()
        {
            this.Node = new Node
            {
                Childs = new List<Node>(),
                Name = "Computer--" + Environment.MachineName
            };
            foreach (string drive in Drives)
            {
                this.Node.Childs.Add(new Node
                {
                    Name = drive,
                    Childs = new List<Node>()
                });
                MakeStructure(drive, Node.Childs.Last());
            }
            //Node = Data;
            //GC.SuppressFinalize(Data);
        }


        public FileSkeleton(string RootDirectory)
        {
            this.Node = new Node
            {
                Childs = new List<Node>() { new Node { Name = "", Childs = new List<Node>() } },
                Name = RootDirectory
            };
            MakeStructure(RootDirectory, this.Node.Childs.LastOrDefault());
        }

        private static void MakeStructure(string path, Node node)
        {
            try
            {
                if (!path.Contains(@"C:\Windows") && !path.Contains(@"C:\Program Files"))
                {
                    node.Childs = new List<Node>();
                    var dirs = Directory.GetDirectories(path);
                    foreach (var folder in dirs)
                    {
                        node.Childs.Add(new Node
                        {
                            Name = Path.GetFileName(folder),
                            Childs = new List<Node>()
                        });
                        try
                        {
                            var files = Directory.GetFiles(folder);
                            foreach (var file in files)
                            {
                                node.Childs.Add(new Node
                                {
                                    Name = Path.GetFileName(file),
                                    Childs = new List<Node>()
                                });
                            }
                        }
                        catch { }
                        MakeStructure(folder, node.Childs.LastOrDefault());//.FirstOrDefault(X => X.Name == Path.GetFileName(folder))
                    }
                }
            }
            catch { }
        }



        ///// <summary>
        ///// Return Item Property As XML Serialized
        ///// </summary>
        ///// <returns></returns>
        //public string ToXML()
        //{
        //    return XML.SerializeObject(Node);
        //}

        ///// <summary>
        ///// Compreses the XML serialized format of Node
        ///// </summary>
        ///// <returns></returns>
        //public byte[] ToCompressedArray()
        //{
        //    return Compress.Zip(XML.SerializeObject(Node));
        //}

        ///// <summary>
        ///// Static Method Only Called for Get Node property From Compressed Array
        ///// </summary>
        ///// <param name="CompressedArrayBytes"></param>
        ///// <returns></returns>
        //public static Node FromCompressedArray(byte[] CompressedArrayBytes)
        //{

        //    return XML.DeserializeObject<Node>(Compress.Unzip(CompressedArrayBytes));
        //}





        /// <summary>
        /// Returns Byte[] of Node Serialized By Binary Formatter
        /// </summary>
        /// <returns></returns>
        public byte[] ToArray()
        {

            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, Node);
            return ms.ToArray();
        }

        /// <summary>
        /// Returns Node Deserialized by ArrayBytes by BinaryFormatter
        /// </summary>
        /// <param name="ArrayBytes"></param>
        /// <returns></returns>
        public static Node FromArray(byte[] ArrayBytes)
        {

            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream(ArrayBytes, 0, ArrayBytes.Length);
            return (Node)bf.Deserialize(ms);
        }
    }
    public class Node
    {
        public string Name { get; set; }
        public List<Node> Childs { get; set; }
    }


}
