# uActivityPub

[![NuGet Version](http://img.shields.io/nuget/v/uActivityPub.svg?style=flat)](https://www.nuget.org/packages/uActivityPub/) [![NuGet Downloads](https://img.shields.io/nuget/dt/uActivityPub.svg)](https://www.nuget.org/packages/uActivityPub/) [![GitHub License](https://img.shields.io/github/license/eduwardpost/uActivityPub)](https://github.com/eduwardpost/uActivityPub)

![uActivityPub](uActivityPub.svg)

 
This package allows you to hook your umbraco website into the fediverse.

Currently the settings or only settable in the database, you can however view them in the umbraco backoffice
The current roadmap is to extend this package with some settings, then harden it for edge cases.


## How to use this package

1. Install the nuget package to your umbraco (v13) project.
2. Have a content type with the alias `article`
3. In that content type have a property with the alias `authorName` which is a user selector that is mandatory
    - The package uses the user selector to create/read the settings for said user and creates the required table entries so that it can be followed
    - Or it can be configured with a few settings in the database to change its behavior. See [Settings](Settings.md) for more information.
4. Publish article on your site