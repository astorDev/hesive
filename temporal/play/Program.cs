var camera = new Camera();
var cinema = new Cinema(camera);
var ads = new Ads();
var marketing = new Marketing(ads, camera);

camera.ShootMovie();
cinema.ShowTo("academies");
var trailerPrepared = marketing.PrepareTrailer();
if (trailerPrepared)
{
    ads.ShowTrailer();
}

// Potential Bug. Returns: "Showing `interesting` to public in the cinema"
// cinema.ShowTo("public");

class Video(
    string content,
    bool Copyrighted
)
{
    public string Content { get; set; } = content;
    public bool Copyrighted { get; set;} = Copyrighted;
}

class Camera
{
    public Video? Shot;

    public void ShootMovie()
    {
        Shot = new Video("An interesting movie from start to finish", true);
    }
}

class Cinema(Camera camera)
{
    public void ShowTo(string audience)
    {
        if (camera.Shot != null)
        {
            Console.WriteLine($"Showing `{camera.Shot.Content}` to " + audience + " in the cinema");
        }
    }
}

class Marketing(Ads ads, Camera camera)
{
    public bool PrepareTrailer()
    {
        var video = camera.Shot;
        if (video != null)
        {
            video.Content = video.Content.Split(' ')[1];

            ads.Trailer = video;
            ads.ShowWatermark = video.Copyrighted;
            
            return true;
        }

        return false;
    }
}

class Ads
{
    public bool ShowWatermark { get; set; }

    public Video? Trailer { get; set; }

    public void ShowTrailer()
    {
        if (Trailer != null)
        {
            Console.WriteLine($"Showing: `{Trailer.Content}{(ShowWatermark ? " WM" : "")}` during the ads break");
        }
    }
}