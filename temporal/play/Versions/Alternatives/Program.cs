var camera = new Camera();
var ads = new Ads();
var marketing = new Marketing(ads, camera);

camera.ShootMovie();
Cinema.Show(camera.Shot!.Content, "academies");
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

    [Obsolete("Use flow without assignment by calling MakeMovie() instead")]
    public void ShootMovie()
    {
        Shot = new Video("An interesting movie from start to finish", true);
    }

    public static Video MakeMovie()
    {
        return new Video("An interesting movie from start to finish", true);
    }
}

class Cinema(Camera camera)
{
    [Obsolete("Use flow without assignment by calling Show(string videoContent, string audience) instead")]
    public void ShowTo(string audience)
    {
        if (camera.Shot == null) throw new Exception("No movie has been shot yet");

        Show(camera.Shot.Content, audience);
    }

    public static void Show(string videoContent, string audience)
    {
        Console.WriteLine($"Showing `{videoContent}` to " + audience + " in the cinema");
    }
}

class Marketing(Ads ads, Camera camera)
{
    [Obsolete("Use flow without assignment by calling PrepareTrailer(Video video) instead")]
    public void PrepareTrailer()
    {
        var video = camera.Shot ?? throw new("No movie has been shot yet");
        var trailer = PrepareTrailer(video);

        video.Content = trailer.Content;

        ads.Trailer = video;
        ads.ShowWatermark = trailer.ShowWatermark;
    }

    public Trailer PrepareTrailer(Video video) => new(
        Content: video.Content.Split(' ')[1],
        ShowWatermark: video.Copyrighted
    );
}

class Ads
{
    [Obsolete("Use flow without assignment by calling Show(Trailer trailer) instead")]
    public bool ShowWatermark { get; set; }

    [Obsolete("Use flow without assignment by calling Show(Trailer trailer) instead")]
    public Video? Trailer { get; set; }

    [Obsolete("Use flow without assignment by calling Show(Trailer trailer) instead")]
    public void ShowTrailer()
    {
        if (Trailer == null) throw new ("No trailer has been prepared yet");

        var trailer = new Trailer(Trailer.Content, ShowWatermark);
        Show(trailer);
    }

    public static void Show(Trailer trailer)
    {
        Console.WriteLine($"Showing: `{trailer.Content}{(trailer.ShowWatermark ? " WM" : "")}` during the ads break");
    }
}

public record Trailer(string Content, bool ShowWatermark);