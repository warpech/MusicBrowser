using System;
using Starcounter;
using System.Xml;
using System.Linq;
using System.Xml.Linq;
using System.IO;

namespace MusicBrowser
{
    class Program
    {
        static void Main()
        {
            string markup =
@"<Root>
    <Child>
        <GrandChild/>
    </Child>
</Root>";

            // Create a reader and move to the content.
            using (XmlReader nodeReader = XmlReader.Create(new StringReader(markup)))
            {
                // the reader must be in the Interactive state in order to
                // Create a LINQ to XML tree from it.
                nodeReader.MoveToContent();

                XElement xRoot = XElement.Load(nodeReader, LoadOptions.SetLineInfo);
                Console.WriteLine("{0}{1}{2}",
                    "Element Name".PadRight(20),
                    "Line".PadRight(5),
                    "Position");
                Console.WriteLine("{0}{1}{2}",
                    "------------".PadRight(20),
                    "----".PadRight(5),
                    "--------");
                foreach (XElement e in xRoot.DescendantsAndSelf())
                    Console.WriteLine("{0}{1}{2}",
                        ("".PadRight(e.Ancestors().Count() * 2) + e.Name).PadRight(20),
                        ((IXmlLineInfo)e).LineNumber.ToString().PadRight(5),
                        ((IXmlLineInfo)e).LinePosition);
            }
            Environment.Exit(1);
        }
    }
}
