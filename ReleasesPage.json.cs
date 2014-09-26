using Starcounter;
using System;

namespace MusicBrowser
{
    [ReleasesPage_json]
    partial class ReleasesPage : Json
    {
        public void SearchQuery(string query)
        {
            var limit = 10;

            Count = 0;
            if (query != "")
            {
                FocusedAlbum.Data = (Release)Db.SQL<Release>("SELECT r FROM MusicBrowser.Release r WHERE Title LIKE ? FETCH ?", "%" + query + "%", 1).First;
                if (FocusedAlbum.Data != null)
                {
                    Albums = Db.SQL<Release>("SELECT r FROM MusicBrowser.Release r WHERE Title LIKE ? FETCH ? OFFSET ?", "%" + query + "%", limit - 1, 1);
                    Count = 1 + Albums.Count;
                }
            }
            else
            {
                FocusedAlbum.Data = (Release)Db.SQL<Release>("SELECT r FROM MusicBrowser.Release r FETCH ?", 1).First;
                if (FocusedAlbum.Data != null)
                {
                    Count = 1 + Db.SlowSQL<long>("SELECT COUNT(*) FROM MusicBrowser.Release m").First;
                    Albums = Db.SQL<Release>("SELECT r FROM MusicBrowser.Release r FETCH ? OFFSET ?", limit - 1, 1);
                }
            }

            var data = (Release)FocusedAlbum.Data;
            if (data != null)
            {
                if (data.Image != null)
                {
                    FocusedAlbum.ImageUrl = data.Image.Uri;
                    FocusedAlbum.ImageUrl = FocusedAlbum.ImageUrl.Replace("http://api.discogs.com/images/", "/image/");
                    FocusedAlbum.ImageUrl = FocusedAlbum.ImageUrl.Replace("http://s.pixogs.com/image/", "/image/");
                }
                else
                {
                    FocusedAlbum.ImageUrl = "/image/default-release.png";
                }
            }

            foreach (var album in Albums)
            {
                data = (Release)album.Data;
                if (data.Image != null)
                {
                    album.ImageUrl = data.Image.Uri150;
                    album.ImageUrl = album.ImageUrl.Replace("http://api.discogs.com/images/", "/image/");
                    album.ImageUrl = album.ImageUrl.Replace("http://s.pixogs.com/image/", "/image/");
                }
                else
                {
                    album.ImageUrl = "/image/default-release.png";
                }
            }

            if (data.Video != null)
            {
                this.FocusedAlbum.Video.Uri = this.FocusedAlbum.Video.Uri.Replace("http://www.youtube.com/watch?v=", "");
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
}
