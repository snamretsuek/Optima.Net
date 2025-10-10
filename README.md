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

<TABLE>
<TR><TD><B>Scenario</B></TD><TD><B>Recommended Approach</B></TD><TD><B>Why</B></TD></TR>
<TR><TD>You just want the value or an exception if missing	</TD><TD>Value							</TD><TD>Simple and direct; throws 							
																											NullValueException if None. Best for cases 		
																											where absence is unexpected.						</TD></TR>
<TR><TD>You want the value or a default                   	</TD><TD>ValueOrDefault(defaultValue)	</TD><TD>Avoids exceptions; allows a fallback value. 			
																											Good for optional configuration or simple defaults.</TD></TR>
<TR><TD>You want the value or null	                       	</TD><TD>ValueOrNull()					</TD><TD>Useful for nullable reference types. 				
																											Simpler than ValueOrDefault(null).					</TD></TR>
<TR><TD>Traditional C# Try-Get pattern                    	</TD><TD>TryGetValue(out var value)		</TD><TD>Familiar to C# developers; avoids exceptions 		
																											and works nicely in conditional statements.		</TD></TR>
<TR><TD>Perform side-effects or branching logic           	</TD><TD>Match(onSome, onNone)			</TD><TD>Declarative and expressive; keeps code functional.	
																											Can also use MatchAsync for async side-effects.	</TD></TR>
<TR><TD>Perform side-effects but keep fluent chaining     	</TD><TD>Tap / TapAsync					</TD><TD>Executes a side-effect without breaking the chain;
																											good for logging, metrics, or auditing.			</TD></TR>
<TR><TD>Transform a value (sync)                          	</TD><TD>Map							</TD><TD>Produces a new Optional<TResult>; keeps pipeline	
																											functional.										</TD></TR>
<TR><TD>Transform a value (async)                         	</TD><TD>MapAsync						</TD><TD>Allows async transformation while keeping 			
																											pipeline fluent.									</TD></TR>
<TR><TD>Chain operations that return Optionals (sync)     	</TD><TD>Bind							</TD><TD>Avoids nested Optionals; essential for dependent	
																											computations.										</TD></TR>
<TR><TD>Chain operations that return Optionals (async)    	</TD><TD>BindAsync						</TD><TD>Async equivalent of Bind; keeps pipelines fully 	
																											async and composable.								</TD></TR>
<TR><TD>Filter based on a predicate (sync)                	</TD><TD>Where							</TD><TD>Returns None if predicate fails; keeps pipeline	
																											clean.												</TD></TR>		
<TR><TD>Filter based on an async predicate                	</TD><TD>WhereAsync						</TD><TD>Async equivalent; useful for DB/API validations	
																											or async checks.									</TD></TR>
<TR><TD>Fallback to another Optional (sync)               	</TD><TD>Or								</TD><TD>Provides a safe fallback Optional; avoids null		
																											coalescing logic.									</TD></TR>
<TR><TD>Fallback to another Optional (async)              	</TD><TD>OrAsync						</TD><TD>Async fallback; integrates with async pipelines.	</TD></TR>
<TR><TD>Combine two Optionals (sync)                      	</TD><TD>Zip							</TD><TD>Combines two Optionals only if both are present.	
																											Useful for dependent values.						</TD></TR>
<TR><TD>Combine two Optionals (async)                     	</TD><TD>ZipAsync						</TD><TD>Async version; waits for both Optionals before		
																										combining.												</TD></TR>
<TR><TD>Integrate with LINQ pipelines or collections      	</TD><TD>ToEnumerable					</TD><TD>Treats Optional as 0-or-1 sequence. Best when		
																											working with multiple Optionals, LINQ, or 			
																											collections.										</TD></TR>
<TR><TD>Flatten nested Optionals                          	</TD><TD>Flatten						</TD><TD>Converts Optional<Optional<T>> → Optional<T>;		
																											keeps pipeline simple.								</TD></TR>
</TABLE>
