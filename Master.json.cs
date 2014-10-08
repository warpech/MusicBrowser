using System;
using Starcounter;
using System.Xml;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using System.IO;
using System.Diagnostics;
using System.Net;

namespace MusicBrowser
{
    [Master_json]
    partial class Master : Page
    {
        static bool ValueExists(XElement node)
        {
            return (node != null && node.Value.Length > 0);
        }

        static void Main()
        {
            Handle.GET("/master", (Request req) =>
            {
                Master m;
                if (Session.Current == null)
                {
                    m = new Master()
                    {
                        Html = "/master.html",
                    };
                    m.Session = new Session();
                }
                else
                {
                    m = (Master)Session.Current.Data;
                }
                
                return m;
            });

            Handle.GET("/", () =>
            {
                Master m = Master.GET("/master");
                ReleasesPage p = new ReleasesPage();
                p.SearchQuery("");
                m.Application = p;
                return m;
            });

            Handle.GET("/image/{?}", (string ImageId) =>
            {
                var url = "http://s.pixogs.com/image/" + ImageId;

                HttpWebRequest myWebClient = (HttpWebRequest)WebRequest.Create(url);
                myWebClient.Headers.Add("Pragma", "no-cache");
                myWebClient.Headers.Add("Cache-Control", "no-cache");
                myWebClient.Accept = "image/jpeg,text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                myWebClient.UserAgent = "Mozilla/5.0 (Windows; U; MSIE 7.0; Windows NT 6.0; en-US)";
                myWebClient.Headers.Add("Accept-Encoding", "gzip,deflate,sdch");
                myWebClient.Headers.Add("Accept-Language", "en-US,en;q=0.8,pl;q=0.6");
                myWebClient.Referer = "http://www.discogs.com/";

                HttpWebResponse myWebResponse = (HttpWebResponse)myWebClient.GetResponse();
                MemoryStream memoryStream = new MemoryStream(0x10000);
                Byte[] data = null;
                try
                {
                    using (Stream responseStream = myWebResponse.GetResponseStream())
                    {
                        byte[] buffer = new byte[0x1000];
                        int bytes;
                        while ((bytes = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            memoryStream.Write(buffer, 0, bytes);
                        }
                    }
                    data = memoryStream.ToArray();
                }
                catch (Exception e)
                {
                }

                Response resp = new Response();

                if (data != null)
                {
                    resp.BodyBytes = data;

                    WebHeaderCollection myWebHeaderCollection = myWebResponse.Headers;
                    for (int i = 0; i < myWebHeaderCollection.Count; i++)
                    {
                        if (myWebHeaderCollection.GetKey(i) == "Content-Type")
                        {
                            resp.ContentType = myWebHeaderCollection.Get(i);
                            break;
                        }
                    }
                }

                resp.CacheControl = "max-age=99936000, public";
                
                return resp;
            });

            Handle.GET("/create-indexes", () =>
            {
                if (null == Db.SlowSQL("SELECT i FROM MATERIALIZEDINDEX i WHERE Name = ?", "Release_Index").First)
                    Db.SlowSQL("CREATE INDEX Release_Index ON MusicBrowser.Release (Title)");

                if (null == Db.SlowSQL("SELECT i FROM MATERIALIZEDINDEX i WHERE Name = ?", "ReleasePriorityDesc_Index").First)
                    Db.SlowSQL("CREATE INDEX ReleasePriorityDesc_Index ON MusicBrowser.Release (Priority DESC)");

                if (null == Db.SlowSQL("SELECT i FROM MATERIALIZEDINDEX i WHERE Name = ?", "ReleasePriority_Index").First)
                    Db.SlowSQL("CREATE INDEX ReleasePriority_Index ON MusicBrowser.Release (Priority)");

                if (null == Db.SlowSQL("SELECT i FROM MATERIALIZEDINDEX i WHERE Name = ?", "ReleasePriorityTitle_Index").First)
                    Db.SlowSQL("CREATE INDEX ReleasePriorityTitle_Index ON MusicBrowser.Release (Priority DESC, Title)");

                if (null == Db.SlowSQL("SELECT i FROM MATERIALIZEDINDEX i WHERE Name = ?", "ReleaseStyle_Index").First)
                    Db.SlowSQL("CREATE INDEX ReleaseStyle_Index ON MusicBrowser.ReleaseStyle (Release)");

                if (null == Db.SlowSQL("SELECT i FROM MATERIALIZEDINDEX i WHERE Name = ?", "ReleaseGenre_Index").First)
                    Db.SlowSQL("CREATE INDEX ReleaseGenre_Index ON MusicBrowser.ReleaseGenre (Release)");

                if (null == Db.SlowSQL("SELECT i FROM MATERIALIZEDINDEX i WHERE Name = ?", "ReleaseVideo_Index").First)
                    Db.SlowSQL("CREATE INDEX ReleaseVideo_Index ON MusicBrowser.ReleaseVideo (Release)");

                if (null == Db.SlowSQL("SELECT i FROM MATERIALIZEDINDEX i WHERE Name = ?", "ReleaseArtist_Index").First)
                    Db.SlowSQL("CREATE INDEX ReleaseArtist_Index ON MusicBrowser.ReleaseArtist (Release)");

                if (null == Db.SlowSQL("SELECT i FROM MATERIALIZEDINDEX i WHERE Name = ?", "ReleaseImage_Index").First)
                    Db.SlowSQL("CREATE INDEX ReleaseImage_Index ON MusicBrowser.ReleaseImage (Release)");
                return 200;
            });

            Handle.GET("/load-data", () =>
            {
                var count = 0;
                var skip = 0;
                var limit = 10000;
                //var limit = 1000;

                var untilTitle = "";

                var importGenres = false;
                var importStyles = false;
                var onlyFullInfo = true;

                StreamWriter filewriter = File.AppendText("log.txt");
                Log("Importing records from XML...", filewriter);

                // Create a reader and move to the content.
                // download XML from http://www.discogs.com/data/discogs_20140901_masters.xml.gz (98 MB compressed)

                using (XmlReader nodeReader = XmlReader.Create(Starcounter.Application.Current.WorkingDirectory + "..\\data\\discogs_masters.xml"))
                {
                    // the reader must be in the Interactive state in order to
                    // Create a LINQ to XML tree from it.
                    nodeReader.MoveToContent();

                    if (skip == 0)
                    {
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
                            Db.SlowSQL("DELETE FROM MusicBrowser.Search");
                            Db.SlowSQL("DELETE FROM MusicBrowser.SearchRelease");
                        });
                    }

                    bool isOnNode = nodeReader.ReadToDescendant("master");
                    while (isOnNode)
                    {
                        var element = (XElement)XNode.ReadFrom(nodeReader);

                        if (onlyFullInfo)
                        {
                            var str = element.ToString();
                            if (
                                str.IndexOf("<year") == -1
                           /* ||
                                (str.IndexOf("Funk") == -1 && str.IndexOf("Queen") == -1)*/
                            ||
                                str.IndexOf("<video") == -1
                            ||
                                str.IndexOf("<image") == -1
                                )
                            {
                                if (!nodeReader.IsStartElement("master"))
                                    isOnNode = nodeReader.ReadToNextSibling("master");

                                continue;
                            }
                        }

                        count++;

                        if (count < skip)
                        {
                            if (!nodeReader.IsStartElement("master"))
                                isOnNode = nodeReader.ReadToNextSibling("master");

                            continue;
                        }

                        var t = new Transaction();

                        t.Add(() =>
                        {
                            Release release = new Release();
                            release.Id = Convert.ToInt32(element.Attribute("id").Value);

                            XElement node;

                            node = element.XPathSelectElement("title");
                            if (Master.ValueExists(node))
                            {
                                release.Title = node.Value;
                            }

                            node = element.XPathSelectElement("main_release");
                            if (Master.ValueExists(node))
                            {
                                release.MainRelease = Convert.ToInt32(node.Value);
                            }

                            node = element.XPathSelectElement("year");
                            if (Master.ValueExists(node))
                            {
                                release.Year = Convert.ToInt32(node.Value);
                            }

                            node = element.XPathSelectElement("data_quality");
                            if (Master.ValueExists(node))
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
                                if (Master.ValueExists(node))
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
                                Int32 id;
                                string name;

                                node = artistElement.XPathSelectElement("id");
                                if (Master.ValueExists(node))
                                {
                                    id = Convert.ToInt32(node.Value);

                                    node = artistElement.XPathSelectElement("name");
                                    if (Master.ValueExists(node))
                                    {
                                        name = node.Value;

                                        Artist artist = Db.SQL<Artist>("SELECT a FROM MusicBrowser.Artist a WHERE Id = ?", id).First;
                                        if (artist == null)
                                        {
                                            artist = new Artist()
                                            {
                                                Id = id,
                                                Name = name
                                            };
                                        }

                                        new ReleaseArtist()
                                        {
                                            Release = release,
                                            Artist = artist
                                        };
                                    }
                                }
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

                            if (importStyles)
                            {
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
                            }

                            if (importGenres)
                            {
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
                            }
                        });

                        if (count % 1000 == 0)
                        {
                            Log("Adding " + count, filewriter);
                        }

                        t.Commit();

                        if (untilTitle.Length > 0)
                        {
                            if (element.XPathSelectElement("title").Value == untilTitle)
                            {
                                break;
                            }
                        }

                        if (!nodeReader.IsStartElement("master"))
                            isOnNode = nodeReader.ReadToNextSibling("master");

                        if (count == limit + skip)
                        {
                            break;
                        }
                    }
                }
                Log("Import finished", filewriter);
                filewriter.Close();
                return 200;
            });
        }
        
        public static void Log(string logMessage, TextWriter w)
        {
            w.WriteLine(DateTime.Now.ToString("o") + "\t" + logMessage);
            w.Flush();
        }
    }
}

