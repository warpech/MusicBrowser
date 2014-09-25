using System;
using Starcounter;
using System.Xml;
using System.Linq;
using System.Xml.Linq;
using System.IO;
using System.Diagnostics;

namespace MusicBrowser
{
    class Program
    {
        static void Main()
        {
            // Create a reader and move to the content.
            //using (XmlReader nodeReader = XmlReader.Create("\\psf\Home\Downloads\discogs_20140901_masters.xml"))
            using (XmlReader nodeReader = XmlReader.Create("C:\\Starcounter Projects\\MusicBrowser\\data\\books.xml"))
            {
                // the reader must be in the Interactive state in order to
                // Create a LINQ to XML tree from it.
                nodeReader.MoveToContent();

                XElement xRoot = XElement.Load(nodeReader, LoadOptions.SetLineInfo);
                Debug.WriteLine("{0}{1}{2}",
                    "Element Name".PadRight(20),
                    "Line".PadRight(5),
                    "Position");
                Debug.WriteLine("{0}{1}{2}",
                    "------------".PadRight(20),
                    "----".PadRight(5),
                    "--------");
                foreach (XElement e in xRoot.DescendantsAndSelf())
                    Debug.WriteLine("{0}{1}{2}",
                        ("".PadRight(e.Ancestors().Count() * 2) + e.Name).PadRight(20),
                        ((IXmlLineInfo)e).LineNumber.ToString().PadRight(5),
                        ((IXmlLineInfo)e).LinePosition);
            }
            Environment.Exit(0);
        }
    }
}
