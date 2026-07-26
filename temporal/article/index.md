# Temporal Coupling: What Is It And How to Deal With It?

> Identifying Temporal Coupling and Refactoring It Step-By-Step.

![It Poisons Your Codebase!](thumb.png)

When working with a legacy codebase, you will likely see a tons of issues with the code. However, we can't just fix everything - we need to find a root cause to attack. Often many of the problems have one common theme - Temporal Coupling. 

Temporal coupling is an implicit expectation on the order of operations (method calls, assignments, etc.) in a codebase. This might sound like a narrow problem, but it spreads throughout a codebase fast and quietly making it extremely hard to reason about. In this article, we will study an example of a code, poisoned with temporal coupling and figure out a step-by-step strategy for dealing with it.

> Or jump straight to the [TLDR](#tldr) in the end of this article to see the plan cheatsheet.

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

## TLDR;

In this article, we've refactored a codebase, poisoned with temporal coupling from head to toes. We did it delicately following those steps:

1. Make It Explicit With Exceptions
2. Fix Leaves (Parts of the code, that doesn't Have Anything Depending on Them)
3. Introduce Uncoupled Alternatives
4. Fix the Flow
5. Final Cleanup: Introduction of Immutability

You should be able to apply this checklist to deal with the temporal coupling, when spotting one in your code. 

This article, as well, as the code samples, are part of the repository, called `Hesive`. The repository theme is cohesiveness in software. Don't hesitate to [check it out on GitHub](https://github.com/astordev/hesive) and give it a star.  ⭐

Claps for this article are also highly appreciated! 😉