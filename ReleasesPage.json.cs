using Starcounter;

namespace MusicBrowser
{
    [ReleasesPage_json]
    partial class ReleasesPage : Json
    {
        public void SearchQuery(string query)
        {
            var limit = 50;

            if (query != "")
            {
                FocusedAlbum.Data = (Release)Db.SQL<Release>("SELECT r FROM MusicBrowser.Release r WHERE Title LIKE ? FETCH ?", "%" + query + "%", 1).First;
                Albums = Db.SQL<Release>("SELECT r FROM MusicBrowser.Release r WHERE Title LIKE ? FETCH ? OFFSET ?", "%" + query + "%", limit - 1, 1);
            }
            else
            {
                FocusedAlbum.Data = (Release)Db.SQL<Release>("SELECT r FROM MusicBrowser.Release r FETCH ?", 1).First;
                Albums = Db.SQL<Release>("SELECT r FROM MusicBrowser.Release r FETCH ? OFFSET ?", limit - 1, 1);
            }

            if (FocusedAlbum.Images.Count > 0)
            {
                FocusedAlbum.Image = FocusedAlbum.Images[0].Uri;
                FocusedAlbum.Image = FocusedAlbum.Image.Replace("http://api.discogs.com/images/", "/image/");
                FocusedAlbum.Image = FocusedAlbum.Image.Replace("http://s.pixogs.com/image/", "/image/");
            }
            else
            {
                FocusedAlbum.Image = "/image/default-release.png";
            }

            foreach (var album in Albums)
            {
                if (album.Images.Count > 0)
                {
                    album.Image = album.Images[0].Uri;
                    album.Image = album.Image.Replace("http://api.discogs.com/images/", "/image/");
                    album.Image = album.Image.Replace("http://s.pixogs.com/image/", "/image/");
                }
                else
                {
                    album.Image = "/image/default-release.png";
                }
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
