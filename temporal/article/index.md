# Temporal Coupling: What Is It And How to Fix It?

> Identifying Temporal Coupling and Refactoring It Step-By-Step.

![It Poisons Your Codebase!](thumb.png)

When working with a legacy codebase, you will likely see a tons of issues with the code. However, we can't just fix everything - we need to find a root cause to attack. Often many of the problems have one common theme - Temporal Coupling. 

Temporal coupling is an implicit expectation on the order of operations (method calls, assignments, etc.) in a codebase. This might sound like a narrow problem, but it spreads throughout a codebase fast and quietly making it extremely hard to reason about. In this article, we will study an example of a code, poisoned with temporal coupling and figure out a step-by-step strategy for dealing with it.

> Or jump straight to the [TLDR](#tldr) in the end of this article to see the plan cheatsheet.

## Temporal Coupling Examples (Many Variants)

Let me just show you the code:

```csharp
# TODO: Insert Code
```

## TLDR;

In this article, we've refactored a codebase, poisoned with temporal coupling from head to toes. We did, it delicately, however, following those steps:

1. Make It Explicit With Exceptions
2. Fix Leaves
3. Introduce Uncoupled Alternatives
4. Fix the Flow
5. Clean Up

