var movie = Camera.MakeMovie();
Cinema.Show(movie.Content, "academies");
var trailer = Marketing.PrepareTrailer(movie);
Ads.Show(trailer);

// Now showing the movie to the public won't contain bugs:
Cinema.Show(movie.Content, "public");

record Video(
    string Content,
    bool Copyrighted
);

class Camera
{
    public static Video MakeMovie() => 
        new("An interesting movie from start to finish", true);
}

class Cinema
{
    public static void Show(string videoContent, string audience) => 
        Console.WriteLine($"Showing `{videoContent}` to " + audience + " in the cinema");
}

class Marketing
{
    public static Trailer PrepareTrailer(Video video) => new(
        Content: video.Content.Split(' ')[1],
        ShowWatermark: video.Copyrighted
    );
}

class Ads
{
    public static void Show(Trailer trailer) => 
        Console.WriteLine($"Showing: `{trailer.Content}{(trailer.ShowWatermark ? " WM" : "")}` during the ads break");
}

public record Trailer(string Content, bool ShowWatermark);