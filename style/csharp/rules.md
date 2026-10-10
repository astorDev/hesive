---
description: Rules for C# code
---

1. No method can exceed 5 lines of code.
2. Private and internals methods and classes are NOT allowed: Extract to either extension methods or separate classes.
3. No Tuples Allowed: Create Public Records Instead.
4. Check for "method can be made static" hints. Those method should be either converted to extension or become member of class, where they are won't be static
5. Use expression-bodied members wherever possible.
6. Class can not exceed 50 lines of code.
7. Nesting like this `DoSomething(Parse(x))` is not allowed: Extract intermediate results to variables.
8. Use Primary Constructors wherever possible.
9. Simplify new expressions: Use target-typed `new` wherever possible.
10. Methods can NOT have more than 4 arguments.