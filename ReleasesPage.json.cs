using Starcounter;
using System;

namespace MusicBrowser
{
    [ReleasesPage_json]
    partial class ReleasesPage : Json
    {
        public void SearchQuery(string query)
        {
            int limit;

            QueryResultRows<Release> albums;
            if (query != "")
            {
                Search search = Db.SQL<Search>("SELECT s FROM MusicBrowser.Search s WHERE s.Query LIKE ? FETCH ?", query, 1).First;

                if (search == null)
                {
                    limit = 10;
                    albums = Db.SQL<Release>("SELECT r FROM MusicBrowser.Release r WHERE Title LIKE ? ORDER BY Priority DESC FETCH ?", query + "%", limit);

                    var t = new Transaction();
                    t.Add(() =>
                    {
                        search = new Search();
                        search.Query = query;

                        foreach (Release album in albums)
                        {
                            new SearchRelease()
                            {
                                Search = search,
                                Release = album
                            };
                        }
                    });
                    t.Commit();
                }
                else
                {
                    albums = search.Releases;
                }
                
                if (albums.First != null)
                {
                    Albums = albums;
                }
                else
                {
                    for (int i = Convert.ToInt32(Albums.Count) - 1; i >= 0; i--)
                    {
                        Albums.RemoveAt(i);
                    }
                }
                Count = (long)Albums.Count;
            }
            else
            {
                limit = 24;
                albums = Db.SQL<Release>("SELECT r FROM MusicBrowser.Release r ORDER BY Priority DESC FETCH ?", limit);
                Albums = albums;
                Count = (long)Db.SlowSQL<long>("SELECT COUNT(*) FROM MusicBrowser.Release m").First;
            }

            FocusAlbum((Release)albums.First);

            var data = (Release)FocusedAlbum.Data;
            if (data != null)
            {
                foreach (var album in Albums)
                {
                    data = (Release)album.Data;
                    album.ImageUrl = FixImageUrl(data.Image, true);
                }
            }
        }

        public string FixImageUrl(Image image, bool small)
        {
            string url;
            if (image != null)
            {
                if (small == true)
                {
                    url = image.Uri150;
                }
                else
                {
                    url = image.Uri;
                }
                url = url.Replace("http://api.discogs.com/images/", "/image/");
                url = url.Replace("http://s.pixogs.com/image/", "/image/");
            }
            else
            {
                url = "/image/default-release.png";
            }
            return url;
        }

        public string FixVideoUrl(Video video)
        {
            string url;
            if (video != null)
            {
                url = video.Uri;
                url = url.Replace("http://www.youtube.com/watch?v=", "");
            }
            else
            {
                url = "";
            }
            return url;
        }

        public void FocusAlbum(Release album)
        {
            FocusedAlbum.Data = album;
            if (album != null)
            {
                FocusedAlbum.ImageUrl = FixImageUrl(album.Image, false);
                FocusedAlbum.VideoId = FixVideoUrl(album.Video);
            }
            else
            {
                FocusedAlbum.ImageUrl = "";
                FocusedAlbum.VideoId = "";
            }
        }

        void Handle(Input.Query input)
        {
            SearchQuery(input.Value);
        }
    }

    [ReleasesPage_json.FocusedAlbum]
    partial class ReleasesPageFocusedAlbum : Json, IBound<Release>
    {
    }

    [ReleasesPage_json.Albums]
    partial class ReleasesPageAlbum : Json, IBound<Release>
    {
        void Handle(Input.Focus input)
        {
            ((ReleasesPage)Parent.Parent).FocusAlbum((Release)Data);
        }
    }
}
