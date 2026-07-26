# Temporal Coupling: What Is It and How to Deal With It?

> Identifying Temporal Coupling and Refactoring It Step-by-step.

![It Poisons Your Codebase!](thumb.png)

When working with a legacy codebase, you will likely see a ton of issues with the code. However, we can't just fix everything - we need to find a root cause to attack. Often, many of the problems have one common theme - Temporal Coupling.

Temporal coupling is an implicit expectation on the order of operations (method calls, assignments, etc.) in a codebase. This might sound like a narrow problem, but it spreads throughout a codebase fast and quietly, making it extremely hard to reason about. In this article, we will study an example of code, poisoned with temporal coupling and figure out a step-by-step strategy for dealing with it.

> Or jump straight to the [TL;DR](#tldr) at the end of this article to see the plan cheat sheet.

## Temporal Coupling Examples (Many Variants)

Let me show you the code example we will be dealing with:

```csharp
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
```

## Step 1: Make Temporal Coupling Runtime-Explicit with Exceptions

```csharp
class Cinema(Camera camera)
{
    public void ShowTo(string audience)
    {
        if (camera.Shot == null) throw new Exception("No movie has been shot yet");

        Console.WriteLine($"Showing `{camera.Shot.Content}` to " + audience + " in the cinema");
    }
}

class Marketing(Ads ads, Camera camera)
{
    public void PrepareTrailer()
    {
        var video = camera.Shot;
        if (video == null) throw new Exception("No movie has been shot yet");

        video.Content = video.Content.Split(' ')[1];

        ads.Trailer = video;
        ads.ShowWatermark = video.Copyrighted;
    }
}

class Ads
{
    public Video? Trailer { get; set; }
    public bool? ShowWatermark { get; set; }

    public void ShowTrailer()
    {
        var trailer = Trailer ?? throw new Exception("No trailer has been prepared yet");
        var showWatermark = ShowWatermark ?? throw new Exception("ShowWatermark has not been set yet");

        Console.WriteLine($"Showing: `{trailer.Content}{(showWatermark ? " WM" : "")}` during the ads break");
    }
}
```

## Step 2: Introduce Stateless Methods on Leaf-Services

```csharp
Cinema.Show(camera.Shot!.Content, "academies");

class Cinema
{
    public static void Show(string videoContent, string audience)
    {
        Console.WriteLine($"Showing `{videoContent}` to " + audience + " in the cinema");
    }
}
```

```csharp
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
```

## Step 3: Introduce Stateless Methods on Intermediary Services

```csharp
class Camera
{
    public Video? Shot;

    [Obsolete("Use flow without assignment by calling MakeMovie() instead")]
    public void ShootMovie()
    {
        Shot = MakeMovie();
    }

    public static Video MakeMovie() => 
        new ("An interesting movie from start to finish", true);
}
```

```csharp
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
```

## Step 4: Update the Flow to Use Stateless Methods

```csharp
var movie = Camera.MakeMovie();
Cinema.Show(movie.Content, "academies");
var trailer = Marketing.PrepareTrailer(movie);
Ads.Show(trailer);

// Now showing the movie to the public won't contain bugs:
Cinema.Show(movie.Content, "public");
```

## Step 5: Clean Up by Removing All Statefulness

```csharp
record Video(
    string Content,
    bool Copyrighted
);
```

```csharp
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
```

## TL;DR

In this article, we've refactored a codebase, poisoned with temporal coupling from head to toe. We did it delicately following these steps:

1. Make Temporal Coupling Runtime-Explicit with Exceptions
2. Introduce Stateless Methods on Leaf-Services
3. Introduce Stateless Methods on Intermediary Services
4. Update the Flow to Use Stateless Methods
5. Clean Up by Removing All Statefulness

You should be able to apply this checklist to deal with the temporal coupling when spotting it in your code.

This article, as well as the code samples, is part of the repository called `Hesive`. The repository theme is cohesiveness in software. Don't hesitate to [check it out on GitHub](https://github.com/astordev/hesive) and give it a star. ⭐

Claps for this article are also highly appreciated! 😉