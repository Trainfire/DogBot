# DogBot

DogBot is a bot built using the SteamKit library that will join a chatroom and post a 'dog of the day' at a specified interval.

When in the same chat as DogBot, you can issue the following commands:
* **!dotd** - Shows the current Dog of the Day
* **!dotdset** - URL | Comment (Optional) - Sets the current Dog of the Day **(Admin Only)**
* **!dotdhelp** - Displays available commands
* **!dotdrnd** - Returns a random previously set DotD
* **!dotdstats** - Displays the number of DoTDs set and the highest contributor
* **!dotdmute** - Mutes the bot **(Admin Only)**
* **!dotdunmute** - Unmutes the bot **(Admin Only)**

## Other Information ##

* DogBot will automatically leave and rejoin chat after a period of time when there's no activity.
* Any messages in chat that are successfully parsed as a command (IE, they begin with any of the commands above) will be logged which includes the Steam ID of the caller. This is for the purpose of identifying abuse.

## Setup ##

Todo.

## Adding New Commands ##

Todo.
