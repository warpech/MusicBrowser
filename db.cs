using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Starcounter;

namespace MusicBrowser
{
    [Database]
    public class Release
    {
        public Nullable<Int32> Id;
        public string Title;
        public Nullable<Int32> MainRelease;
        public Nullable<Int32> Year;
        public string DataQuality;
        public QueryResultRows<Style> Styles { get { return Db.SQL<Style>("SELECT r.Style FROM ReleaseStyle r WHERE r.Release=?", this); } }
        public Style Style { get { return Db.SQL<Style>("SELECT r.Style FROM ReleaseStyle r WHERE r.Release=? FETCH ?", this, 1).First; } }
        public QueryResultRows<Genre> Genres { get { return Db.SQL<Genre>("SELECT r.Genre FROM ReleaseGenre r WHERE r.Release=?", this); } }
        public Genre Genre { get { return Db.SQL<Genre>("SELECT r.Genre FROM ReleaseGenre r WHERE r.Release=? FETCH ?", this, 1).First; } }
        public QueryResultRows<Video> Videos { get { return Db.SQL<Video>("SELECT r.Video FROM ReleaseVideo r WHERE r.Release=?", this); } }
        public Video Video { get { return Db.SQL<Video>("SELECT r.Video FROM ReleaseVideo r WHERE r.Release=? FETCH ?", this, 1).First; } }
        public QueryResultRows<Artist> Artists { get { return Db.SQL<Artist>("SELECT r.Artist FROM ReleaseArtist r WHERE r.Release=?", this); } }
        public Artist Artist { get { return Db.SQL<Artist>("SELECT r.Artist FROM ReleaseArtist r WHERE r.Release=? FETCH ?", this, 1).First; } }
        public QueryResultRows<Image> Images { get { return Db.SQL<Image>("SELECT r.Image FROM ReleaseImage r WHERE r.Release=?", this); } }
        public Image Image { get { return Db.SQL<Image>("SELECT r.Image FROM ReleaseImage r WHERE r.Release=? FETCH ?", this, 1).First; } }
        }

    [Database]
    public class ReleaseStyle
    {
        public Release Release;
        public Style Style;
    }

    [Database]
    public class ReleaseGenre
    {
        public Release Release;
        public Genre Genre;
    }

    [Database]
    public class ReleaseVideo
    {
        public Release Release;
        public Video Video;
    }

    [Database]
    public class ReleaseArtist
    {
        public Release Release;
        public Artist Artist;
    }

    [Database]
    public class ReleaseImage
    {
        public Release Release;
        public Image Image;
    }

    [Database]
    public class Style
    {
        public string Name;
    }

    [Database]
    public class Genre
    {
        public string Name;
    }

    [Database]
    public class Video
    {
        public Nullable<Int32> Duration;
        public bool Embed;
        public string Uri;
        public string Title;
    }

    [Database]
    public class Artist
    {
        public int Id;
        public string Name;
    }

    [Database]
    public class Image
    {
        public string Uri;
        public string Uri150;
        public string Type;
        public Nullable<Int32> Height;
        public Nullable<Int32> Width;
    }
}
