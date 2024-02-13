# Settings

the uActivityPub package can be configured to change its behavior with a few settings.

## How to change the settings

Currently, settings are read only visable on the umbraco backoffice user interface. 
Under the settings main tab there will be a new entry in the left menu tree called `uActivityPub`
Here you can see the current values of the settings.

Settings are stored in the same SQL database as the umbraco website. And can currently only be changed directly on the SQL database.
The settings are stored in the table `[uActivitySettings]` of the schema you installed umbraco in (default `[dbo]`) 

All setting values are a string tha get parsed runtime to a value

## Supported settings

### Single user mode {id=singleUserMode}

> supported values:
> - true
> - false

_default value:_ __false__

This setting sets the package in either single user mode or multi user mode.

In single user mode the package uses the [Single user mode username](#singleUserModeUserName) setting to determine which user to respond to.

In multi user mode it uses the umbraco User database (not members) to decide which users to respond to. 

### Single user mode username {id=singleUserModeUserName}

> supported values:
> - any url safe string

_default value:_ __uActivityPub__

This setting is used to determine the actor username in [single user mode](#singleUserMode).

### Content type alias

> supported values:
> - any umbraco content type alias

_default value:_ __article__

This setting is used to determine which content type to monitor and post about into the Fediverse.
Currently only one content type at the time is supported.

### List content type alias

> supported values:
> - any umbraco content type alias

_default value:_ __articleList__

This setting is used to determine which content list type to use for post count calculation.
Currently only one content type at the time is supported.

### Author name alias

> supported values:
> - any umbraco property type alias that exists on the chosen content type

_default value:_ __authorName__

This setting is used to determine who the user is of an article. This setting is only used in [multiuser](#singleUserMode) mode.
The alias should point to a property of the type: `Umbraco.UserPicker`. This property does not have to exist in [single user mode](#singleUserMode).