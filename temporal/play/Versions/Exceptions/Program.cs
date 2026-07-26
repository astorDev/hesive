var camera = new Camera();
var cinema = new Cinema(camera);
var ads = new Ads();
var marketing = new Marketing(ads, camera);

camera.ShootMovie();
cinema.ShowTo("academies");
marketing.PrepareTrailer();
ads.ShowTrailer();

// Potential Bug. Returns: "Showing `interesting` to public in the cinema"
// cinema.ShowTo("public");

class Video(
    string content,
    bool copyrighted
)
{
    public string Content { get; set; } = content;
    public bool Copyrighted { get; set;} = copyrighted;
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
        if (camera.Shot == null)
        {
            throw new Exception("No movie has been shot yet");
        }

        Console.WriteLine($"Showing `{camera.Shot.Content}` to " + audience + " in the cinema");
    }
}

class Marketing(Ads ads, Camera camera)
{
    public void PrepareTrailer()
    {
        var video = camera.Shot;
        if (video == null)
        {
            throw new Exception("No movie has been shot yet");
        }

        video.Content = video.Content.Split(' ')[1];

        ads.Trailer = video;
        ads.ShowWatermark = video.Copyrighted;
    }
}

class Ads
{
    public bool ShowWatermark { get; set; }

    public Video? Trailer { get; set; }

    public void ShowTrailer()
    {
        if (Trailer == null)
        {
            throw new Exception("No trailer has been prepared yet");
        }

        Console.WriteLine($"Showing: `{Trailer.Content}{(ShowWatermark ? " WM" : "")}` during the ads break");
    }
}