# Optima.Net Optional<T>

Optional<T> is a utility class designed to represent a value that may or may not exist. Its purpose is to provide a safer, more expressive alternative to null checks, making code easier to read and maintain.

## Overview

- Some(value): Wraps a value in an Optional.  
- None(): Represents the absence of a value.  
- Accessors: Value, ValueOrDefault, ValueOrNull  
- Functional operations: Map, Bind, Match, Tap, Where,  

Optional<T> is immutable and thread-safe. It is intended to be used wherever a value might not be present, allowing developers to handle both cases explicitly and safely.

## Creating Optionals

Optional with a value:
```
    var maybeUser = Optional<User>.Some(user);
```

Optional with no value:
```
    var noUser = Optional<User>.None();
```

Some(value) throws ArgumentNullException if the value is null.  
None() is a singleton representing no value.

## Accessing Values

**Value:**
```
    var user = maybeUser.Value; // Throws NullValueException if None
```

**ValueOrDefault:**
```
    var user = maybeUser.ValueOrDefault(defaultUser);
```

**ValueOrNull:**
```
    var user = maybeUser.ValueOrNull();
```

## Functional Operations

**Map:**  
Map transforms the inner value without unwrapping the Optional.
Think of it like saying: “If I have a value, do something to it; if not, just keep None.”
Example:
```
    Optional<int> maybeNumber = Optional<int>.Some(10);
    Optional<string> maybeString = maybeNumber.Map(n => $"Number is {n}");
    Console.WriteLine(maybeString.Value); // "Number is 10"
```
Input: Optional<T>
Mapper: Func<T, TResult>
Output: Optional<TResult>

If maybeNumber was None(), the result would also be None(). No exceptions.

**Bind:**  
(also called FlatMap in some languages) Is for chaining operations that themselves return an Optional.
Why not use Map? Because Map would give you Optional<Optional<TResult>>—nested Optionals. Bind flattens it.
Example:
```
    Optional<int> maybeNumber = Optional<int>.Some(10);

    Optional<string> maybeString = maybeNumber.Bind(n =>
    {
        if (n > 5) return Optional<string>.Some($"Big number: {n}");
        return Optional<string>.None();
    });

    Console.WriteLine(maybeString.HasValue); // true
    Console.WriteLine(maybeString.Value);    // "Big number: 10"
```
Input: Optional<T>
Binder: Func<T, Optional<TResult>>
Output: Optional<TResult>

“If I have a value, I may or may not return another Optional; if I don’t have a value, the result is None.”

**Match:**  
Is a safe way to handle both cases, Some and None, without throwing exceptions.
```
    maybeUser.Match(
	onSome: u => Console.WriteLine(u.Name),
	onNone: () => Console.WriteLine("No user found")
	);
```
You provide two functions:

onSome(T): what to do if there’s a value

onNone(): what to do if there isn’t

It returns the result of the function you call, so no need to unwrap manually or risk exceptions.

It’s basically a “pattern match” for Optionals.

## Optional\<T\> Extensions

**Tap:**  
If optional.HasValue is true, it executes the action with the contained value.
Returns the original optional, which menas you continue chaining operations like Map, Bind, or Match.

Does not modify the optional or create a new one, it is purely a side-effect hook.
```
    var maybeUser = Optional<User>.Some(user);

    maybeUser
    .Tap(u => Console.WriteLine($"Found user: {u.Name}"))
    .Map(u => u.Orders.Count)
    .Match(
        onSome: count => Console.WriteLine($"User has {count} orders"),
        onNone: () => Console.WriteLine("No user found")
    );
```
Logs the user if it exists.
Continues mapping to get order count.
Still handles the None case cleanly.

**TapAsync:**  
Checks if optional.HasValue is true.
If so, executes the provided async action (Func<T, Task>).
Returns the original optional wrapped in a Task<Optional\<T\>> so you can continue chaining.

```
    var maybeUser = Optional<User>.Some(user);

    await maybeUser
    .TapAsync(async u => await notificationService.NotifyUserAsync(u))
    .Map(u => u.Orders.Count)
    .TapAsync(async count => await logger.LogAsync($"User has {count} orders"))
    .Match(
        onSome: count => Console.WriteLine($"Processed {count} orders"),
        onNone: () => Console.WriteLine("No user to process")
    );


```
Sends an async notification if the user exists. Logs the order count asynchronously.
Still handles the None case safely.


**Where:**  
If the optional is None, it returns None immediately. If the optional has a value, it evaluates the predicate:  
-  True: returns the original optional.
-  False: returns None.
  
This lets you chain filters safely without null checks.
```
    var maybeUser = Optional<User>.Some(user);

    // Only keep active users
    var activeUser = maybeUser.Where(u => u.IsActive);

    activeUser.Match(
        onSome: u => Console.WriteLine($"{u.Name} is active"),
        onNone: () => Console.WriteLine("User is inactive or missing")
    );
```
If user is inactive, activeUser becomes None. The pipeline remains clean and expressive, no if statements needed.  

**Where:** (for a collection of Optional\<T\>)  
Same as a single the where on a single Optional\<T\>.  Now you can filter the collection and remove the None(s)
```
    var users = new List<Optional<User>>
    {
        Optional<User>.Some(new User("Alice", true)),
        Optional<User>.Some(new User("Bob", false)),
        Optional<User>.None(),
        Optional<User>.Some(new User("Charlie", true))
    };

    // Keep only active users
    var activeUsers = users.Where(u => u.IsActive);

    foreach (var userOpt in activeUsers)
    {
        userOpt.Match(
            onSome: u => Console.WriteLine($"Active: {u.Name}"),
            onNone: () => {} // This won't run, because Where already filtered out Nones
        );
    }

    // Or get a flattened list of active users
    var activeUsersList = users
        .Where(u => u.IsActive)
        .Flatten()
        .ToList();
```
Where filters both Nones and values that fail the predicate. Flatten gives a clean IEnumerable<T> with only the values you care about.
This is much better than iterating and checking each optional manually.

**WhereAsync:**  
WhereAsync preserves the Optional pipeline model; no leaking null, no messy .Any() or await tangles.
It’s safe and clean even with async I/O operations.
```
    var maybeUser = Optional<User>.Some(user);

    var maybeActiveUser = await maybeUser.WhereAsync(async u =>
    {
        // Pretend this checks a database or cache
        await Task.Delay(10);
        return u.IsActive;
    });

    maybeActiveUser.Match(
        onSome: u => Console.WriteLine($"Active user: {u.Name}"),
        onNone: () => Console.WriteLine("User inactive or not found")
    );
```

**WhereAsync:** (for a collection of Optional\<T\>)  
WhereAsync preserves the Optional pipeline model; no leaking null, no messy .Any() or await tangles.
It’s safe and clean even with async I/O operations.
```
var users = new List<Optional<User>>
{
    Optional<User>.Some(new User("Alice", true)),
    Optional<User>.Some(new User("Bob", false)),
    Optional<User>.None(),
    Optional<User>.Some(new User("Charlie", true))
};

var activeUsers = await users.WhereAsync(async u =>
{
    await Task.Delay(5);
    return u.IsActive;
});

foreach (var user in activeUsers)
{
    user.Tap(u => Console.WriteLine($"Active: {u.Name}"));
}
```
It parallelizes using Task.WhenAll() for max efficiency.
## More Examples

1. Fetching Optional Configuration:
```
    Optional<string> maybeApiKey = configRepository.GetApiKey("PaymentService");

    maybeApiKey
    .Map(key => Encrypt(key))
    .Match(
    onSome: encryptedKey => logger.Log($"Using API key: {encryptedKey}"),
    onNone: () => logger.Log("No API key configured")
    );
```

2. Chaining Service Calls:
```
    Optional<User> maybeUser = userService.FindUserByEmail(email);

    Optional<Order> maybeLatestOrder = maybeUser
    .Bind(u => orderService.GetLatestOrderForUser(u.Id));

    maybeLatestOrder.Match(
    onSome: order => notificationService.NotifyUser(order),
    onNone: () => logger.Log("No recent order for this user")
    );
```
3. Input Validation and Transformation:
```
    Optional<string> maybeInput = Optional<string>.Some(request["age"]);

    Optional<int> maybeAge = maybeInput
    .Map(s => int.TryParse(s, out var n) ? n : throw new FormatException())
    .Bind(age => age > 0 ? Optional<int>.Some(age) : Optional<int>.None());

    maybeAge.Match(
    onSome: age => domainService.RegisterAge(age),
    onNone: () => logger.Log("Invalid age provided")
    );
```
4. Conditional Feature Toggles:
```
    Optional<FeatureConfig> maybeFeature = featureService.GetFeatureConfig("NewDashboard");

    maybeFeature
    .Map(cfg => cfg.IsEnabled)
    .Match(
    onSome: enabled => enabled ? dashboardService.Show() : dashboardService.Hide(),
    onNone: () => dashboardService.Hide()
    );
```
5. Event-Driven Data Enrichment:
```
    Optional<string> maybeCustomerId = eventPayload.CustomerId;

    maybeCustomerId
    .Bind(id => customerRepository.FindCustomer(id))
    .Map(customer => new EnrichedEvent(eventPayload, customer))
    .Match(
    onSome: enriched => eventProcessor.Process(enriched),
    onNone: () => logger.Log("Event skipped: missing or invalid customer")
    );
```
## Notes

- Use Optional<T> whenever a value might be absent, instead of null.  
- Functional operations (Map, Bind, Match) make pipelines readable and safe.  
- Optional<T> integrates cleanly with domain-driven designs and event-driven architectures.