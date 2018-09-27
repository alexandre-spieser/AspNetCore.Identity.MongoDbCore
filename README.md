# AspNetCore.Identity.MongoDbCore

A MongoDb UserStore and RoleStore adapter for Microsoft.AspNetCore.Identity 2.0.
Allows you to use MongoDb instead of SQL server with Microsoft.AspNetCore.Identity 2.0.

Covered by 737 integration tests and unit tests from the modified [Microsoft.AspNetCore.Identity.EntityFrameworkCore.Test](https://github.com/aspnet/Identity/tree/b865d5878623077eeb715e600d75fa9c24dbb5a1/test/Microsoft.AspNetCore.Identity.EntityFrameworkCore.Test) test suite.

Supports both `netstandard2.0` and `netcoreapp2.0`.

Available as a Nuget package : https://www.nuget.org/packages/AspNetCore.Identity.MongoDbCore/

	Install-Package AspNetCore.Identity.MongoDbCore

# User and Role Entities
Your user and role entities must inherit from `MongoIdentityUser<TKey>` and `MongoIdentityRole<TKey>` in a way similar to the `IdentityUser<TKey>` and the `IdentityRole<TKey>` in `Microsoft.AspNetCore.Identity`, where `TKey` is the type of the primary key of your document.

Here is an example:

```csharp

public class ApplicationUser : MongoIdentityUser<Guid>
{
	public ApplicationUser() : base()
	{
	}

	public ApplicationUser(string userName, string email) : base(userName, email)
	{
	}
}

public class ApplicationRole : MongoIdentityRole<Guid>
{
	public ApplicationRole() : base()
	{
	}

	public ApplicationRole(string roleName) : base(roleName)
	{
	}
}	
```
#### Id Fields
The `Id` field is automatically set at instantiation, this also applies to users inheriting from `MongoIdentityUser<int>`, where a random integer is assigned to the `Id`. It is however not advised to rely on such random mechanism to set the primary key of your document. Using documents inheriting from `MongoIdentityRole` and `MongoIdentityUser`, which both use the `Guid` type for primary keys, is recommended. MongoDB ObjectIds can optionally be used in lieu of GUIDs by passing a key type of `MongoDB.Bson.ObjectId`, e.g. `public class ApplicationUser : MongoIdentityUser<ObjectId>`.

#### Collection Names
MongoDB collection names are set to the plural camel case version of the entity class name, e.g. `ApplicationUser` becomes `applicationUsers`. To override this behavior apply the `CollectionName` attribute from the `MongoDbGenericRepository` nuget package:
```csharp
using MongoDbGenericRepository.Attributes;

namespace App.Entities
{
    // Name this collection Users
    [CollectionName("Users")]
    public class ApplicationUser : MongoIdentityUser<Guid>
    {
	...
```
# Configuration
To add the stores, you can use the `IdentityBuilder` extension like so:

```csharp
services.AddIdentity<ApplicationUser, ApplicationRole>()
	.AddMongoDbStores<ApplicationUser, ApplicationRole, Guid>
	(
		"mongodb://localhost:27017",
		"MongoDbTests"
	)
	.AddDefaultTokenProviders();
```


It is also possible to share a common `IMongoDbContext` across your services (requires https://www.nuget.org/packages/MongoDbGenericRepository/):

```csharp
var mongoDbContext = new MongoDbContext("mongodb://localhost:27017", "MongoDbTests");
services.AddIdentity<ApplicationUser, ApplicationRole>()
	.AddMongoDbStores<IMongoDbContext>(mongoDbContext)
	.AddDefaultTokenProviders();
// Use the mongoDbContext for other things.
```

You can also use the more explicit type declaration:

```csharp
var mongoDbContext = new MongoDbContext("mongodb://localhost:27017", "MongoDbTests");
services.AddIdentity<ApplicationUser, ApplicationRole>()
	.AddMongoDbStores<ApplicationUser, ApplicationRole, Guid>(mongoDbContext)
	.AddDefaultTokenProviders();
// Use the mongoDbContext for other things.
```

Alternatively a full configuration can be done by populating a `MongoDbIdentityConfiguration` object, which can have an `IdentityOptionsAction` property set to an action you want to perform against the `IdentityOptions` (`Action<IdentityOptions>`).

The `MongoDbSettings` object is used to set MongoDb Settings using the `ConnectionString` and the `DatabaseName` properties.

The MongoDb connection is managed using the [mongodb-generic-repository](https://github.com/alexandre-spieser/mongodb-generic-repository), where a repository inheriting `IBaseMongoRepository` is registered as a singleton. Look at the [ServiceCollectionExtension.cs](https://github.com/alexandre-spieser/AspNetCore.Identity.MongoDbCore/blob/master/src/Extensions/ServiceCollectionExtension.cs) file for more details.

```csharp
var mongoDbIdentityConfiguration = new MongoDbIdentityConfiguration
{
	MongoDbSettings = new MongoDbSettings
	{
		ConnectionString = "mongodb://localhost:27017",
		DatabaseName = "MongoDbTests"
	},
	IdentityOptionsAction = options =>
	{
		options.Password.RequireDigit = false;
		options.Password.RequiredLength = 8;
		options.Password.RequireNonAlphanumeric = false;
		options.Password.RequireUppercase = false;
		options.Password.RequireLowercase = false;

		// Lockout settings
		options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
		options.Lockout.MaxFailedAccessAttempts = 10;

		// ApplicationUser settings
		options.User.RequireUniqueEmail = true;
		options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789@.-_";
	}
};
services.ConfigureMongoDbIdentity<ApplicationUser, ApplicationRole, Guid>(mongoDbIdentityConfiguration);
```

# Running the tests

To run the tests, you need a local MongoDb server in default configuration (listening to `localhost:27017`).
Create a database named MongoDbTests for the tests to run.

## Author
**Alexandre Spieser**

## License
AspNetCore.Identity.MongoDbCore is under MIT license - http://www.opensource.org/licenses/mit-license.php

The MIT License (MIT)

Copyright (c) 2016-2017 Alexandre Spieser

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.

## Copyright
Copyright © 2017
