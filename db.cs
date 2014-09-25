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
