# Optima.Net Optional\<T\>

Optional<T> is a utility class designed to represent a value that may or may not exist. Its purpose is to provide a safer, more expressive alternative to null checks, making code easier to read and maintain.

## Overview

- Some(value): Wraps a value in an Optional.  
- None(): Represents the absence of a value.  
- Accessors: Value, ValueOrDefault, ValueOrNull, TryGetValue  
- Functional operations (sync): Map, Bind, Match, Tap, Where, Or, Zip, Flatten, ToEnumerable  
- Functional operations (async): MapAsync, BindAsync, MatchAsync, TapAsync, WhereAsync, OrAsync, ZipAsync


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

The extensions are divided into 3 name spaces:  

    - Optima.Net.Extensions  
    - Optima.Net.Extensions.Collections  
    - Optima.Net.Extensions.LINQ  
      

**Tap:**  
If optional.HasValue is true, it executes the action with the contained value.
Returns the original optional, which means you continue chaining operations like Map, Bind, or Match.

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

**Or:**  
If this optional has a value, return it. If not, return the fallback optional.
That’s the Optional equivalent of the ?? (null-coalescing) operator, but safer and more expressive.
```
    var maybeUser = userCache.FindUser(id);
    var result = maybeUser.Or(() => userRepository.FindUser(id));
```
**OrAsync:**  
If this optional has a value, return it. If not, return the fallback optional.
That’s the Optional equivalent of the ?? (null-coalescing) operator, but safer and more expressive.
```
    var maybeUser = await userCache.FindUserAsync(id);
    var result = await maybeUser.OrAsync(() => userRepository.FindUserAsync(id));
```
If maybeUser already has a value, you get that.
If not, it returns another Optional\<T\> or it calls the fallback factory userRepository.FindUser(id).

**Flatten:**  
For nested optionals:
Flattens a nested Optional\<Optional\<T\>\> into a single Optional\<T\>.
```
    Optional<Optional<T>> nested = ...;
    var flat = nested.Flatten(); // Optional<T>

```
Useful when you have operations that already return Optional\<T\> and you want to avoid double wrapping.

**MatchAsync:**  
Just like Match, but the handlers (onSome, onNone) can be asynchronous functions.
It’s especially useful when your match handlers do async work (e.g., database calls, HTTP requests, or logging).

Return Result  
```
    var maybeUser = await userRepository.FindUserAsync(id);

    var message = await maybeUser.MatchAsync(
        onSome: async user =>
        {
            await auditService.RecordAccessAsync(user.Id);
            return $"User {user.Name} found";
        },
        onNone: async () =>
        {
            await logger.LogAsync("User not found");
            return "No user available";
        });
```
Side-Effect Only  
```
await maybeUser.MatchAsync(
    onSome: async user => await emailService.SendWelcomeAsync(user),
    onNone: async () => await metrics.IncrementMissingUserAsync());
```
Chainable
```
await Optional<User>.Some(user)
    .TapAsync(async u => await audit.LogAccessAsync(u.Id))
    .WhereAsync(async u => await permissions.IsActiveAsync(u))
    .MatchAsync(
        onSome: async u => await logger.LogAsync($"User {u.Name} processed"),
        onNone: async () => await logger.LogAsync("No user to process"))
    .TapAsync(async _ => await metrics.IncrementProcessedAsync());
```
**MapAsync:**  
If the optional has a value, it applies the async mapper function and wraps the result in a new Optional\<TResult\>.  
  
Transforming values asynchronously while preserving the Optional pipeline.  
```
    Optional<string> maybeApiKey = configRepository.GetApiKey("PaymentService");

    Optional<string> maybeEncryptedKey = await maybeApiKey.MapAsync(async key =>
    {
        await Task.Delay(50); // simulate async work
        return Encrypt(key);
    });

    maybeEncryptedKey.Match(
        onSome: encrypted => Console.WriteLine($"Encrypted key: {encrypted}"),
        onNone: () => Console.WriteLine("No API key present")
    );
```
Chaining Transformations  
```
    Optional<User> maybeUser = await userService.FindUserByEmailAsync(email);

    Optional<OrderSummary> maybeOrderSummary = await maybeUser
        .MapAsync(async u => await orderService.GetLatestOrderForUserSummaryAsync(u.Id));

    await maybeOrderSummary.MatchAsync(
        onSome: async summary => await notificationService.NotifyUserAsync(summary),
        onNone: async () => await logger.LogAsync("No recent order found")
    );
```
Works with BindAsync, WhereAsync, TapAsync, and MatchAsync.
Keeps your optional pipeline fully async without breaking the fluent interface.  
Returns a Task<Optional<TResult>> so it can continue chaining in an await-based async flow.

**BindAsync:**  
It unwraps an Optional<T> and applies an async binder function that itself returns an Optional<TResult>.  
If the original optional is empty, it short-circuits and returns Optional<TResult>.None().  
This is critical for building pipelines where multiple async steps might fail or not produce a value.
  
Chaining async operations  
```
    Optional<User> maybeUser = await userService.FindUserByEmailAsync(email);

    Optional<Order> maybeLatestOrder = await maybeUser.BindAsync(async u =>
    {
        // this step might also return None
        return await orderService.GetLatestOrderForUserAsync(u.Id);
    });

    await maybeLatestOrder.MatchAsync(
        onSome: async order => await notificationService.NotifyUserAsync(order),
        onNone: async () => await logger.LogAsync("No recent order for this user")
    );
```

Mixing MapAsync and BindAsync  
```
    Optional<string> maybeApiKey = configRepository.GetApiKey("PaymentService");

    Optional<EncryptedKey> maybeEncryptedKey = await maybeApiKey
        .MapAsync(async key => await PreEncryptAsync(key))
        .BindAsync(async preEncrypted => await EncryptKeyAsync(preEncrypted));

    await maybeEncryptedKey.MatchAsync(
        onSome: async encrypted => await logger.LogAsync($"Encrypted key ready: {encrypted}"),
        onNone: async () => await logger.LogAsync("No key available")
    );
```

**TryGetValue:**  
TryGetValue is an alternative accessor to Value or ValueOrDefault. It returns a bool indicating whether the optional has a value, and outputs the value via an out parameter.

```
    Optional<User> maybeUser = userService.FindUserByEmail(email);

    if (maybeUser.TryGetValue(out var user))
    {
        Console.WriteLine($"User found: {user.Name}");
    }
    else
    {
        Console.WriteLine("No user found");
    }
```
Async example  
```
    Optional<User> maybeUser = await userService.FindUserByEmailAsync(email);

    if (maybeUser.TryGetValue(out var user))
    {
        await logger.LogAsync($"User {user.Name} found");
    }
    else
    {
        await logger.LogAsync("User not found");
    }
```

**Zip:**  
Zip takes two Optionals: Optional<T1> and Optional<T2>. If both have values, it applies a result selector function and returns Optional<TResult>.  
If either is empty, the result is Optional<TResult>.None().  
  
Eliminates nested if or .HasValue checks for multiple Optionals. Works well with Map, Bind, Where, and other pipeline operations. Provides a functional, declarative way to combine Optionals safely.  
```
    Optional<User> maybeUser = userRepository.FindUserById(id);
    Optional<Profile> maybeProfile = profileRepository.FindProfileByUserId(id);

    Optional<UserProfile> maybeUserProfile = maybeUser.Zip(
        maybeProfile,
        (user, profile) => new UserProfile(user, profile)
    );

    maybeUserProfile.Match(
        onSome: up => Console.WriteLine($"UserProfile ready: {up.User.Name}"),
        onNone: () => Console.WriteLine("UserProfile not available")
    );
```

Chaining with Map  
```
    Optional<User> maybeUser = userRepository.FindUserById(id);
    Optional<Profile> maybeProfile = profileRepository.FindProfileByUserId(id);

    Optional<string> maybeSummary = maybeUser
        .Zip(maybeProfile, (user, profile) => $"{user.Name} - {profile.Bio}")
        .Map(summary => summary.ToUpper());

    maybeSummary.Match(
        onSome: Console.WriteLine,
        onNone: () => Console.WriteLine("No summary available")
    );
```

**ZipAsync:**  
Same as Zip just Async.
```
    Task<Optional<User>> maybeUserTask = userRepository.FindUserByIdAsync(id);
    Task<Optional<Profile>> maybeProfileTask = profileRepository.FindProfileByUserIdAsync(id);

    Optional<UserProfile> maybeUserProfile = await maybeUserTask.ZipAsync(
        maybeProfileTask,
        (user, profile) => new UserProfile(user, profile)
    );

    await maybeUserProfile.MatchAsync(
        onSome: async up => await logger.LogAsync($"UserProfile ready: {up.User.Name}"),
        onNone: async () => await logger.LogAsync("UserProfile not available")
    );
```
Chaining with other async operations  
```
    Optional<User> maybeUser = await userRepository.FindUserByIdAsync(id);
    Optional<Profile> maybeProfile = await profileRepository.FindProfileByUserIdAsync(id);

    Optional<string> maybeSummary = await Task.FromResult(maybeUser)
        .ZipAsync(Task.FromResult(maybeProfile), (user, profile) => $"{user.Name} - {profile.Bio}")
        .MapAsync(async s => (await TranslateAsync(s)).ToUpper());

    await maybeSummary.MatchAsync(
        onSome: async summary => await logger.LogAsync($"Summary: {summary}"),
        onNone: async () => await logger.LogAsync("No summary available")
    );
```

**ToEnumerable:**  

At first this looks a bit counter intuitive as there is no obvious benefit. Using LINQ query syntax or SelectMany works naturally when each Optional can be treated as a sequence of 0 or 1 items. You can combine two Optionals cleanly without writing nested if checks. 

Not so usable:  
```
    Optional<User> maybeUser = userRepository.FindUserById(id);

    foreach (var user in maybeUser.ToEnumerable())
    {
        Console.WriteLine($"User: {user.Name}");
    }
```
Using LINQ to combine to Optionals, an alternative to Zip.
```
    var combined = from user in maybeUser.ToEnumerable()
                from profile in maybeProfile.ToEnumerable()
                select $"{user.Name} - {profile.Bio}";
```
Integrating LINQ Pipelines
```
    var activeUserName = maybeUser
        .ToEnumerable()
        .Where(u => u.IsActive)
        .Select(u => u.Name)
        .FirstOrDefault(); // returns null if maybeUser is None or inactive
```
Flatten a List<Optional\<User\>\> cleanly, resulting in a List<User> with only the present values, automatically skipping all Nones.  
```
    var users = optionalUsers.SelectMany(u => u.ToEnumerable());
```

## LINQ support

**Select, SelectMany:** query syntax:  
```
    var result = from u in maybeUser
                 from o in orderService.GetLatestOrderForUser(u.Id)
                 select o.Total;
```
**Where:** query syntax:  
```
    var activeUser = from u in maybeUser
                     where u.IsActive
                     select u;
```
Makes optionals feel native to C# pipelines.

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
  
## Guideline Matrix

| **Scenario** | **Recommended Approach** | **Why** |
|--------------|---------------------------|---------|
| You just want the value or an exception if missing | `Value` | Simple and direct; throws `NullValueException` if `None`. Best for cases where absence is unexpected. |
| You want the value or a default | `ValueOrDefault(defaultValue)` | Avoids exceptions; allows a fallback value. Good for optional configuration or simple defaults. |
| You want the value or null | `ValueOrNull()` | Useful for nullable reference types. Simpler than `ValueOrDefault(null)`. |
| Traditional C# Try-Get pattern | `TryGetValue(out var value)` | Familiar to C# developers; avoids exceptions and works nicely in conditional statements. |
| Perform side-effects or branching logic | `Match(onSome, onNone)` | Declarative and expressive; keeps code functional. Can also use `MatchAsync` for async side-effects. |
| Perform side-effects but keep fluent chaining | `Tap` / `TapAsync` | Executes a side-effect without breaking the chain; good for logging, metrics, or auditing. |
| Transform a value (sync) | `Map` | Produces a new `Optional<TResult>`; keeps pipeline functional. |
| Transform a value (async) | `MapAsync` | Allows async transformation while keeping pipeline fluent. |
| Chain operations that return Optionals (sync) | `Bind` | Avoids nested Optionals; essential for dependent computations. |
| Chain operations that return Optionals (async) | `BindAsync` | Async equivalent of `Bind`; keeps pipelines fully async and composable. |
| Filter based on a predicate (sync) | `Where` | Returns `None` if predicate fails; keeps pipeline clean. |
| Filter based on an async predicate | `WhereAsync` | Async equivalent; useful for DB/API validations or async checks. |
| Fallback to another Optional (sync) | `Or` | Provides a safe fallback Optional; avoids null coalescing logic. |
| Fallback to another Optional (async) | `OrAsync` | Async fallback; integrates with async pipelines. |
| Combine two Optionals (sync) | `Zip` | Combines two Optionals only if both are present. Useful for dependent values. |
| Combine two Optionals (async) | `ZipAsync` | Async version; waits for both Optionals before combining. |
| Integrate with LINQ pipelines or collections | `ToEnumerable` | Treats Optional as 0-or-1 sequence. Best when working with multiple Optionals, LINQ, or collections. |
| Flatten nested Optionals | `Flatten` | Converts `Optional<Optional<T>> → Optional<T>`; keeps pipeline simple. |

# Optima.Net Result\<T\>

## Overview

Result<T> provides a clean way to represent the outcome of an operation — success or failure — without relying on exceptions for normal control flow.
It wraps either a value (on success) or an error message (on failure), allowing for explicit, predictable handling.

the namespace is Optima.Net.Result

## Createing a successful Results

```
var result = Result<string>.Ok("Operation completed successfully.");

if (result.IsSuccess)
{
    Console.WriteLine(result.Value);
}
```

The Ok() factory method creates a successful result that carries a value.

IsSuccess will be true, and Error will be an empty string.

You can safely access Value because it’s guaranteed to be non-null when IsSuccess is true.

**Output:**
```
Operation completed successfully.
```

## Creating a failed Result

```
var failure = Result<int>.Fail("Something went wrong.");

if (failure.IsFailure)
{
    Console.WriteLine(failure.Error);
}
```

The Fail() factory method creates a failed result with an error message.

IsFailure is simply the negation of IsSuccess.

Accessing Value here would not make sense — it will be the default for T (e.g. 0 for int, null for reference types).

**Output:**
```
Something went wrong.
``` 

## Guarding Against Null Success Values

```
try
{
    var bad = Result<string>.Ok(null); // Throws NullValueException
}
catch (NullValueException ex)
{
    Console.WriteLine(ex.Message);
}
```
The Result\<T\> constructor enforces that a successful result cannot have a null value. This ensures you never accidentally 
treat “no result” as a success.

If you attempt to call Ok(null), a NullValueException (from Optima.Net.Exceptions) is thrown immediately.

**Exception Message:**
```
Cannot create a successful Result with a null value.
```

## Integration Example

**Returning Result from a Service Method:**
```
public class UserService
{
    private readonly Dictionary<int, string> _users = new()
    {
        { 1, "Marcus" },
        { 2, "Elena" }
    };

    public Result<string> GetUser(int id)
    {
        if (_users.TryGetValue(id, out var user))
            return Result<string>.Ok(user);

        return Result<string>.Fail($"User with ID {id} not found.");
    }
}

```

The service returns Result<string> rather than throwing exceptions for missing users. This approach makes the failure 
part of the method’s contract, forcing the caller to handle it explicitly.

It’s a functional alternative to using try/catch in application logic.

**Usage:**
```
var service = new UserService();

var result = service.GetUser(1);

if (result.IsSuccess)
{
    Console.WriteLine($"Found user: {result.Value}");
}
else
{
    Console.WriteLine($"Error: {result.Error}");
}
```

**Output for success:**
```
Found user: Marcus
```

**Output for failure:**
```
Error: User with ID 99 not found.
```

## Chaining Operations (Bind / BindAsync)

Use Bind when you want to chain multiple operations that each return a Result \<U\> — like database calls, validations, or API requests.
If any step fails, the chain short-circuits and propagates the error automatically.

**Syncronous Example:**
```
Result<int> Parse(string input)
{
    if (int.TryParse(input, out var number))
        return Result<int>.Ok(number);
    return Result<int>.Fail("Invalid number format.");
}

Result<double> DivideByTwo(int number)
{
    if (number == 0)
        return Result<double>.Fail("Cannot divide zero.");
    return Result<double>.Ok(number / 2.0);
}

var result = Parse("42")
    .Bind(DivideByTwo);

Console.WriteLine(result.IsSuccess
    ? $"Result: {result.Value}"
    : $"Error: {result.Error}");
```

Each step returns a Result<T>.
Bind() unwraps the value only if the previous step succeeded.
If any step fails, the rest are skipped.

**Output:**
```
Result: 21
```

## Asyncronous Example:**
```
async Task<Result<string>> GetUserAsync(int id)
{
    await Task.Delay(50);
    return id == 1
        ? Result<string>.Ok("Marcus")
        : Result<string>.Fail("User not found.");
}

async Task<Result<string>> SendEmailAsync(string user)
{
    await Task.Delay(50);
    return Result<string>.Ok($"Email sent to {user}");
}

var result = await Result<int>.Ok(1)
    .BindAsync(GetUserAsync)
    .BindAsync(SendEmailAsync);

Console.WriteLine(result.Match(
    success => success,
    error => $"Failed: {error}"
));
```
The async versions behave identically — they just await the inner function.  Perfect for composing async workflows 
without nested try blocks.

## Transforming Values (Map / MapAsync)

Use Map when you want to transform the inner value of a successful result without changing the type to another Result\<T\>.

**Syncronous Example:**

```
var result = Result<int>.Ok(10)
    .Map(x => x * 2)
    .Map(x => $"Final value: {x}");

Console.WriteLine(result.Value);
```
Map projects a successful value into another form.

If the result was a failure, it’s passed through unchanged.

**Output:**
``` 
Final value: 20
```
**Asyncronous Example:**

```
async Task<int> FetchNumberAsync() => await Task.FromResult(21);

var result = await Result<int>.Ok(0)
    .MapAsync(async _ => await FetchNumberAsync())
    .Map(x => $"Got {x * 2}");

Console.WriteLine(result.Value);
```

Use when your transformation itself is async (e.g., calling an external service).

You stay in the Result \<T\> flow without breaking the pipeline.

## Side Effects (Tap / TapAsync / OnFailure)

Use these when you want to perform side effects (like logging, metrics, or notifications) without altering the result itself.

**Syncronous Example:**

```
Result<string>.Ok("Operation successful")
    .Tap(value => Console.WriteLine($"INFO: {value}"))
    .OnFailure(error => Console.WriteLine($"ERROR: {error}"));
```
Tap runs only on success.
OnFailure runs only when the result failed.
Both return the original result, so they can be chained.

**Output:**
```
INFO: Operation successful
```

**Asyncronous Example:**

```
await Result<string>.Ok("Saved")
    .TapAsync(async val =>
    {
        await Task.Delay(100);
        Console.WriteLine($"Logging: {val}");
    });
```
**Output:**
```
Logging: Saved
```

## Pattern Matching (Match / MatchAsync)

Use Match when you want to convert a Result\<T\> into a final output — success and failure are handled explicitly, expression-style.

**Syncronous Example:**
```
var result = Result<int>.Ok(100);

string output = result.Match(
    onSuccess: val => $"The value is {val}",
    onFailure: err => $"Oops: {err}"
);

Console.WriteLine(output);
```

Cleanly handles both cases in a single expression — perfect for returning results from APIs or controllers.

**Output:**
```
The value is 100
```

**Asyncronous Example:**
```
var result = Result<string>.Fail("Network unavailable");

var message = await result.MatchAsync(
    async val => await Task.FromResult($"Processed: {val}"),
    async err => await Task.FromResult($"Handled failure: {err}")
);

Console.WriteLine(message);
```

**Output:**
```
Handled failure: Network unavailable
```

## ResultCollectionExtension — Working with Collections of Results

**namespace Optima.Net.Extensions.Result**

ResultCollectionExtension adds higher-level operators for working with collections of results.
It lets you:

  - Combine multiple Result\<T\> values into one (Sequence / SequenceAsync)

  - Filter success results with predicates (Where / WhereAsync)

  - Flatten nested results (Flatten)


These are ideal for:

  - Batch validation

  - Domain object creation

  - Parallel task processing

  - Handling collections of mixed successes and failures

  ## Aggregating Multiple Results. Sequence

**Syncronous Example:**
```
var results = new List<Result<int>>
{
    Result<int>.Ok(10),
    Result<int>.Ok(20),
    Result<int>.Fail("Third item failed"),
    Result<int>.Ok(40)
};

var aggregated = results.Sequence();

if (aggregated.IsFailure)
{
    Console.WriteLine($"Failed: {aggregated.Error}");
}
else
{
    Console.WriteLine($"All succeeded: {string.Join(", ", aggregated.Value)}");
}
```
Sequence() collapses a collection of Result\<T\> values into one Result<IEnumerable\<T\>\>.
If any element failed, the entire result is a failure.
The Error aggregates all failure messages joined by ; .
If all succeeded, you get a single success containing all Values.

**Output:**
```
Failed: Third item failed
```
**Asyncronous Example:**
```
async Task<Result<int>> ComputeAsync(int x)
{
    await Task.Delay(50);
    return x > 0 ? Result<int>.Ok(x * 2) : Result<int>.Fail($"Invalid: {x}");
}

var tasks = new[]
{
    ComputeAsync(1),
    ComputeAsync(2),
    ComputeAsync(-5)
};

var aggregated = await tasks.SequenceAsync();

Console.WriteLine(aggregated.IsFailure
    ? $"Errors: {aggregated.Error}"
    : $"Values: {string.Join(", ", aggregated.Value)}");
```

SequenceAsync() takes an IEnumerable\<Task\<Result\<T\>\>\>.
It waits for all tasks to complete, then combines them with the same success/failure logic as Sequence().
If any fail, you get a single failure result with combined messages.
Supports CancellationToken if you need to short-circuit early.

**Output:**
```
Errors: Invalid: -5
``` 

## Filtering Results — Where and WhereAsync

**Syncronous Example:**
```
var results = new List<Result<int>>
{
    Result<int>.Ok(10),
    Result<int>.Ok(3),
    Result<int>.Ok(25),
    Result<int>.Fail("Invalid input")
};

var filtered = results.Where(x => x > 5);

foreach (var r in filtered)
{
    Console.WriteLine(r.Match(
        success => $"✅ Kept: {success}",
        error => $"❌ Dropped: {error}"
    ));
}
```
Where() filters only successful results by a predicate.
Failed results stay as failures (preserving original errors).
Successful results that don’t pass the predicate are turned into failures with the message "Filtered out".

**Output:**
```
Kept: 10
Dropped: Filtered out
Kept: 25
Dropped: Invalid input
```

**Asyncronous Example:**
```
async Task<bool> ExpensiveCheck(int value, CancellationToken _) =>
    await Task.FromResult(value % 2 == 0);

var results = new List<Result<int>>
{
    Result<int>.Ok(2),
    Result<int>.Ok(3),
    Result<int>.Ok(4)
};

var filtered = await results.WhereAsync(ExpensiveCheck);

foreach (var r in filtered)
{
    Console.WriteLine(r.Match(
        success => $"Even number: {success}",
        error => $"{error}"
    ));
}
```
Async version supports expensive or IO-bound checks (e.g., database lookups).
You can pass a CancellationToken to cancel mid-stream.
Each element’s success/failure is preserved, and filtered-out items are marked as failures.

**Output:**
```
Even number: 2
Filtered out
Even number: 4
```

## Flattening Nested Results

Flatten() is for collapsing nested results (Result<Result\<T\>\>) into a single layer.
If the outer result failed, the inner result is ignored.
If the outer succeeded, it unwraps and returns the inner result directly.

**Example:**
```
Result<Result<string>> nested =
    Result<Result<string>>.Ok(Result<string>.Ok("Success"));

var flat = nested.Flatten();

Console.WriteLine(flat.IsSuccess
    ? $"Unwrapped value: {flat.Value}"
    : $"Error: {flat.Error}");
```

**Output:**
```
Unwrapped value: Success
```

If the outer result was a failure, Flatten() would return that failure directly, ignoring the inner result.

## ResultLinqExtensions — LINQ Support for Result\<T\>

**namespace Optima.Net.Extensions.Result.LINQ**

Why Where here also?

ResultLinqExtensions.Where exists to support C# LINQ query syntax (from ... where ... select ...) when you are dealing with IEnumerable<Result<T>>.

ResultCollectionExtension.Where (the one you already have) is a collection helper you might call directly in method chains. They look 
similar and intentionally have similar behavior (preserve failures; convert filtered successes into Fail("Filtered out")), but they serve 
different idioms:

- Use ResultLinqExtensions.Where when you prefer LINQ query expressions or want an implementation that yields results (streaming via yield return).

- Use ResultCollectionExtension.Where when you prefer an explicit functional chain or need its particular implementation details (e.g., it's implemented via Bind in your other file).

In short: both exist because one is for LINQ/query-syntax ergonomics and the other is for explicit functional pipelines. C# is picky,if you want from ... where ... to compile,
you must provide a Where with the right signature.

**Simple Select (Map) — Basic usage:**

```
var r1 = Result<int>.Ok(10)
    .Select(x => x * 3); // Result<int>.Ok(30)

Console.WriteLine(r1.Match(
    v => $"Success: {v}",
    e => $"Failure: {e}"
));
```

**SelectMany (Bind + Project) — Chaining two dependent results

```
Result<int> GetUserId() => Result<int>.Ok(7);
Result<string> GetUserName(int id) => id == 7 ? Result<string>.Ok("Marcus") : Result<string>.Fail("No user");

var profile = GetUserId()
    .SelectMany(
        bind: id => GetUserName(id),           // Result<int> -> Result<string>
        project: (id, name) => $"{id}:{name}"  // compose both into V
    );

// profile is Result<string>.Ok("7:Marcus")
Console.WriteLine(profile.Match(s => $"Profile: {s}", e => $"Err: {e}"));
```

SelectMany implements the monadic bind + projection needed for from ... from ... select ... LINQ queries.

It expands Result<T> pipelines while enabling you to project both inputs into a combined result.

**LINQ query syntax — using Select, SelectMany, and Where:**

```
Result<int> FindOrderId(string tracking) => tracking == "abc" ? Result<int>.Ok(101) : Result<int>.Fail("Not found");
Result<decimal> GetOrderAmount(int orderId) => orderId == 101 ? Result<decimal>.Ok(49.99m) : Result<decimal>.Fail("Bad order");

// Using LINQ query expression
var query = from id in FindOrderId("abc")
            from amt in GetOrderAmount(id)
            where amt > 10m      // uses ResultLinqExtensions.Where if the sequence were IEnumerable - see note
            select new { Id = id, Amount = amt };

var output = query.Match(
    success => $"Order {success.Id} -> {success.Amount:C}",
    failure => $"Failure: {failure}"
);

Console.WriteLine(output);
```
**Output:**
```
// if both succeed
Order 101 -> $49.99
```
That where line in a single-Result\<T\> LINQ expression is translated to a SelectMany + filtering logic depending on how 
the compiler desugars it for monadic types.

The presence of Select and SelectMany methods is the core requirement to make such query expressions work for Result\<T\>. 
The Where defined in ResultLinqExtensions handles the pattern when the compiler expects Where on an IEnumerable<Result\<T\>\> 
sequence or when the query syntax resolves to calling Where on the intermediate shape. Implementing Where 
here keeps LINQ syntax intuitive and prevents compilation headaches.

**LINQ over collections of Result\<T\>:streaming Where**

```
var results = new List<Result<int>>
{
    Result<int>.Ok(2),
    Result<int>.Ok(3),
    Result<int>.Fail("bad"),
    Result<int>.Ok(8)
};

// Use LINQ query syntax to filter and project
var q =
    from r in results          // results: IEnumerable<Result<int>>
    where r.IsSuccess && r.Value % 2 == 0 // calls ResultLinqExtensions.Where (IEnumerable<Result<T>>)
    select r.Select(v => v * 10); // select available because Result.Select exists

// q is IEnumerable<Result<int>>
foreach (var rr in q)
{
    Console.WriteLine(rr.Match(s => $"Kept: {s}", e => $"Dropped: {e}"));
}
```
 -  This Where works on IEnumerable<Result<T>> and yields items one-by-one (yield return).

 -  It preserves original failures and marks filtered-out successes as Fail("Filtered out").

 -  This variant is ideal when you want a streaming pipeline that combines LINQ query expression readability with monadic semantics.

**Output:**
```
Kept: 20        // 2 * 10
Dropped: Filtered out  // 3 filtered out -> marked "Filtered out"
Dropped: bad    // original failure preserved
Kept: 80        // 8 * 10
```

## Comparing the two Where implementations; concrete differences & why both exist.

| **Aspect** | **ResultCollectionExtension.Where** | **ResultLinqExtensions.Where** |
|------------|-------------------------------------|--------------------------------|
| Receiver type|IEnumerable<Result<T>> (your functional chain) | IEnumerable<Result<T>> (LINQ/streaming) |
| Implementation style | Usually implemented by Select/Bind pipelines (you used Bind earlier) | yield return streaming approach |
| Intended usage | Functional pipelines, explicit chaining (call it directly)|LINQ query expressions and streaming scenarios |
| Behaviour | Preserves failures, converts filtered successes to Fail("Filtered out")|Same semantics, but fits query syntax ergonomics |
| Why both | One serves the library-style functional chain; the other makes LINQ query syntax compile and behave predictably |  |


##Integration example: filter a batch & then aggregate successes

```
// 1) start with a batch of validations that return Result<T>
IEnumerable<Result<int>> validations = new[]
{
    Result<int>.Ok(15),
    Result<int>.Ok(4),
    Result<int>.Fail("malformed"),
    Result<int>.Ok(30)
};

// 2) Use LINQ-style pipeline to keep even numbers only, then sequence
var filtered = from r in validations
               where r.IsSuccess && r.Value >= 10  // uses LINQ Where
               select r;

// Convert to concrete list
var filteredList = filtered.ToList();

// 3) Now aggregate: Sequence expects IEnumerable<Result<T>>
var aggregated = filteredList.Sequence();

if (aggregated.IsSuccess)
    Console.WriteLine($"Kept values: {string.Join(", ", aggregated.Value)}");
else
    Console.WriteLine($"Errors: {aggregated.Error}");
```

**Output:**
```
Kept values: 15, 30
```
