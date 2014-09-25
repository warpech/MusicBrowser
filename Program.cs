using System;
using Starcounter;
using System.Xml;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using System.IO;
using System.Diagnostics;

namespace MusicBrowser
{
    class Program
    {
        static bool ValueExists(XElement node)
        {
            return (node != null && node.Value.Length > 0);
        }

        static void Main()
        {
            Handle.GET("/load-data", () =>
            {
                var count = 0;
                var limit = 5;

                // Create a reader and move to the content.
                using (XmlReader nodeReader = XmlReader.Create("C:\\Starcounter Projects\\MusicBrowser\\data\\discogs_20140901_masters.xml"))
                {
                    // the reader must be in the Interactive state in order to
                    // Create a LINQ to XML tree from it.
                    nodeReader.MoveToContent();

                    Db.Transaction(() =>
                    {
                        Db.SlowSQL("DELETE FROM MusicBrowser.Release");
                        Db.SlowSQL("DELETE FROM MusicBrowser.ReleaseStyle");
                        Db.SlowSQL("DELETE FROM MusicBrowser.ReleaseGenre");
                        Db.SlowSQL("DELETE FROM MusicBrowser.ReleaseVideo");
                        Db.SlowSQL("DELETE FROM MusicBrowser.ReleaseArtist");
                        Db.SlowSQL("DELETE FROM MusicBrowser.ReleaseImage");
                        Db.SlowSQL("DELETE FROM MusicBrowser.Style");
                        Db.SlowSQL("DELETE FROM MusicBrowser.Genre");
                        Db.SlowSQL("DELETE FROM MusicBrowser.Video");
                        Db.SlowSQL("DELETE FROM MusicBrowser.Artist");
                        Db.SlowSQL("DELETE FROM MusicBrowser.Image");
                    });

                    bool isOnNode = nodeReader.ReadToDescendant("master");
                    while (isOnNode)
                    {
                        var element = (XElement)XNode.ReadFrom(nodeReader);
                        var t = new Transaction();

                        t.Add(() =>
                        {
                            Release release = new Release();
                            release.Id = Convert.ToInt32(element.Attribute("id").Value);

                            XElement node;

                            node = element.XPathSelectElement("title");
                            if (Program.ValueExists(node))
                            {
                                release.Title = node.Value;
                            }

                            node = element.XPathSelectElement("main_release");
                            if (Program.ValueExists(node))
                            {
                                release.MainRelease = Convert.ToInt32(node.Value);
                            }

                            node = element.XPathSelectElement("year");
                            if (Program.ValueExists(node))
                            {
                                release.Year = Convert.ToInt32(node.Value);
                            }

                            node = element.XPathSelectElement("data_quality");
                            if (Program.ValueExists(node))
                            {
                                release.DataQuality = node.Value;
                            }

                            foreach (XElement videoElement in element.XPathSelectElements("videos/video"))
                            {
                                Video video = new Video();
                                video.Duration = Convert.ToInt32(videoElement.Attribute("duration").Value);
                                video.Embed = Convert.ToBoolean(videoElement.Attribute("embed").Value);
                                video.Uri = videoElement.Attribute("src").Value;

                                node = element.XPathSelectElement("title");
                                if (Program.ValueExists(node))
                                {
                                    video.Title = node.Value;
                                }

                                new ReleaseVideo()
                                {
                                    Release = release,
                                    Video = video
                                };
                            }

                            foreach (XElement artistElement in element.XPathSelectElements("artists/artist"))
                            {
                                Artist artist = new Artist();

                                node = element.XPathSelectElement("id");
                                if (Program.ValueExists(node))
                                {
                                    artist.Id = Convert.ToInt32(node.Value);
                                }

                                node = element.XPathSelectElement("name");
                                if (Program.ValueExists(node))
                                {
                                    artist.Name = node.Value;
                                }

                                new ReleaseArtist()
                                {
                                    Release = release,
                                    Artist = artist
                                };
                            }

                            foreach (XElement imageElement in element.XPathSelectElements("images/image"))
                            {
                                Image image = new Image();
                                image.Width = Convert.ToInt32(imageElement.Attribute("width").Value);
                                image.Height = Convert.ToInt32(imageElement.Attribute("height").Value);
                                image.Type = imageElement.Attribute("type").Value;
                                image.Uri = imageElement.Attribute("uri").Value;
                                image.Uri150 = imageElement.Attribute("uri150").Value;

                                new ReleaseImage()
                                {
                                    Release = release,
                                    Image = image
                                };
                            }

                            foreach (XElement styleElement in element.XPathSelectElements("styles/style"))
                            {
                                String name = styleElement.Value;

                                Style style = Db.SQL<Style>("SELECT s FROM MusicBrowser.Style s WHERE Name = ?", name).First;
                                if (style == null)
                                {
                                    style = new Style()
                                    {
                                        Name = name
                                    };
                                }

                                new ReleaseStyle()
                                {
                                    Release = release,
                                    Style = style
                                };
                            }

                            foreach (XElement genreElement in element.XPathSelectElements("genres/genre"))
                            {
                                String name = genreElement.Value;

                                Genre genre = Db.SQL<Genre>("SELECT s FROM MusicBrowser.Genre s WHERE Name = ?", name).First;
                                if (genre == null)
                                {
                                    genre = new Genre()
                                    {
                                        Name = name
                                    };
                                }

                                new ReleaseGenre()
                                {
                                    Release = release,
                                    Genre = genre
                                };
                            }
                        });

                        foreach (XElement e in element.DescendantsAndSelf())
                        {
                            Debug.WriteLine("{0}{1}",
                                ("".PadRight(e.Ancestors().Count() * 2) + e.Name).PadRight(20),
                                (e.Value).PadRight(5));
                        }

                        t.Commit();

                        if (!nodeReader.IsStartElement("master"))
                            isOnNode = nodeReader.ReadToNextSibling("master");

                        count++;
                        if (count == limit)
                        {
                            break;
                        }
                    }
                }
                return 200;
            });
        }
    }
}
