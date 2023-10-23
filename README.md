# uActivityPub

This package allows you to hook your umbraco website into the fediverse.

Currently there are no settings since the package got developed as a proof of concept for my own blog.
The current roadmap is to extend this package with some settings, then harden it for edge cases.


## How to use this package

1. Install the nuget package to your umbraco (v10) project.
2. Have a content type with the alias `article`
3. In that content type have a property with the alias `authorName` which is a user selector that is mandatory
   - The package uses the user selector to create/read the settings for said user and creates the required table entries so that it can be followed
4. Publish article on your site