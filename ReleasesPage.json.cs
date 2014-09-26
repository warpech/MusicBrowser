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
                Albums = Db.SQL<Release>("SELECT r FROM MusicBrowser.Release r WHERE Title LIKE ? FETCH ?", "%" + query + "%", limit);
            }
            else
            {
                Albums = Db.SQL<Release>("SELECT r FROM MusicBrowser.Release r FETCH ?", limit);
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
}
