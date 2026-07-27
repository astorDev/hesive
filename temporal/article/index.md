# Temporal Coupling: What Is It and How to Deal With It?

> Identifying Temporal Coupling and Refactoring It Step-by-step.

![It Poisons Your Codebase!](thumb.png)

When working with a legacy codebase, you will likely see a ton of issues with the code. However, we can't just fix everything - we need to find a root cause to attack. Often, many of the problems have one common theme: temporal coupling.

Temporal coupling is an implicit expectation on the order of operations (method calls, assignments, etc.) in a codebase. 

This might sound like a narrow problem, but it spreads throughout a codebase fast and quietly, making it extremely hard to reason about. In this article, we will study an example of code, poisoned with temporal coupling, and figure out a step-by-step strategy for dealing with it.

> Or jump straight to the [TL;DR](#tldr) at the end of this article to see the plan cheat sheet.

## Temporal Coupling Example (Many Variants Inside)

Perhaps the main issue with temporal coupling is that the code overall looks somewhat reasonable. Every class on its own doesn't look too bad - fixing it seems like a lot of work, while the benefits are unclear. Not to mention that temporal coupling takes many forms, so it's hard to refactor it semi-automatically.

For that reason, we will refactor a complete "Program", rather than an individual thing. Let me give you our initial code:

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

As you might have noticed, the code is held together by an unwritten contract: shoot the movie, then show it, then prepare the trailer, then show the ads - in that exact order. Skip a step, or reorder two lines, and nothing tells you anything went wrong. 

`Marketing.PrepareTrailer` even mutates `Shot.Content` in place, which is exactly why re-running `cinema.ShowTo("public")` afterwards prints the trimmed trailer text instead of the movie - the bug hiding in the comment at the top of the snippet. Every class here is only as safe as the order it happens to be called in.

It's important to keep in mind that this is test code. Unlike with real legacy code, we won't see tens of dependencies when clicking on a method. 

So what should we do first?

## Step 1: Make Temporal Coupling Runtime-Explicit with Exceptions

Let's say we forgot to call `Camera.ShootMovie()`. The program will run without any errors, but neither the public nor critics will see anything. The code will swallow the problem. The least we can do is make the problems explicit at runtime by throwing an exception when things don't go as planned. Here's how our methods will look after:

> Note: This change is a breaking change, and it will be painful. Don't forget to do regression testing. The good news is that the change is 100% isolated - you can fix it in as few places as you want and still enjoy the effect.

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

We gained our first "win". However, the goal is of course to replace runtime errors with compile-time ones. This is a long journey though. What should be our first step?

## Step 2: Introduce Stateless Methods on Leaf-Services

Refactoring a legacy system is hard because changing anything requires tracking a ton of dependent classes. However, in every system there are leaves - methods or classes that do a "final" action. Methods on which other classes don't really depend. Fixing those first is usually a good first step. In our case, there are two examples of such services: `Cinema` and `Ads`.

Refactoring `Cinema` is quite trivial, and we can move it right to the final version by redoing its `Show` method and fixing its single call in the program flow:

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

> Note that `Show` became a static method. This clearly shows that it no longer relies on any state. In real system, the method likely won't become static due to having dependent services or configuration, but the important part is to remove its reliance on other objects' state.

With `Ads`, the situation is more complicated, though. It's intertwined with `Marketing`, feeling its properties. In a real codebase, it may not just be `Marketing`, but a whole set of other services. Refactoring it in one go can be too big a task. The thing we can do quite easily, though, is ADD a stateless method and hint other services to use it with an `Obsolete` attribute. Here's how the code will look:

> We will also add an immutable object, called Trailer, to serve as a better representation of what an `Ads` needs

```csharp
public record Trailer(string Content, bool ShowWatermark);

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

The "easy" part lays an important foundation for our next step. 

Both leaf services are now free of any dependency on `Camera` or on their own fields - `Cinema.Show` and `Ads.Show` are pure functions you could unit test without constructing anything else first. The `[Obsolete]` attributes are doing real work here too: they let the old, coupled API keep functioning for any caller we haven't migrated yet, so this refactor can ship gradually instead of as one big-bang rewrite.

## Step 3: Introduce Stateless Methods on Intermediary Services

The same idea we used with `Ads` can be applied to other services, including those having dependants. Here's how we can introduce a stateless alternative to `Camera`:

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

One benefit of fixing leaf services first is that it begs the introduction of new models for the data those services need. With the `Trailer` model in place, refactoring of `Marketing` is quite easy:

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

Note that after step 1, we didn't really change any behavior at all. However, we already extracted much smaller, clearer, and easier-to-test methods, which can also serve as a foundation for the proper flow.

## Step 4: Update the Flow to Use Stateless Methods

Every class now has a pure, stateless alternative sitting right next to its old stateful methods. The last piece is rewiring the actual program to use them:

```csharp
var movie = Camera.MakeMovie();
Cinema.Show(movie.Content, "academies");
var trailer = Marketing.PrepareTrailer(movie);
Ads.Show(trailer);

// Now showing the movie to the public won't contain bugs:
Cinema.Show(movie.Content, "public");
```

> Notice that the bug from the very beginning of the article - showing an already-trimmed trailer content to the public - is now structurally impossible. `movie.Content` is never mutated in place anymore, so every call to `Cinema.Show` gets the full movie, exactly as expected.

The code is now good. However, we won't get the number of lines reduced, as we should typically expect from a good refactoring. Also, we haven't taken preventive measures for our code not to degrade again. Let's do it in our final step: Clean Up.

## Step 5: Clean Up by Removing All Statefulness

A fundamental building block of code with temporal coupling is mutable models. Unfortunately, we can't normally start with refactoring them because they are used in an enormous number of places. Gladly, now we have refactored our methods and can make our `Video` model immutable:

```csharp
record Video(
    string Content,
    bool Copyrighted
);
```

If that model were immutable in the first place, all the other problems would be significantly harder to introduce. Probably we could even get proper code in the first place. Here's how our Program looks without temporal coupling after removing all the methods marked as `Obsolete`:

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

As you might see, now our methods don't even look like they have "logic". They didn't really have it in the first place - temporal coupling, however, masquarades to bussiness logic very well. Let's recap!

## TL;DR

In this article, we've refactored a codebase, poisoned with temporal coupling from head to toe. We did it delicately following these steps:

1. Make Temporal Coupling Runtime-Explicit with Exceptions
2. Introduce Stateless Methods on Leaf Services
3. Introduce Stateless Methods on Intermediary Services
4. Update the Flow to Use Stateless Methods
5. Clean Up by Removing All Statefulness

You should be able to apply this checklist to deal with the temporal coupling when spotting it in your code.

This article, as well as the code samples, is part of the repository called `Hesive`. The repository theme is cohesiveness in software. Don't hesitate to [check it out on GitHub](https://github.com/astordev/hesive) and give it a star. ⭐

Claps for this article are also highly appreciated! 😉
