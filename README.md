# DogBot #

DogBot is a Steam bot that will join a chatroom and post a dog once every hour, fulfilling everyone's necessary daily dosage of dogs as recommended by health professionals worldwide.

To use this, just build the project, then run the copy.bat found in the root and head into the build folder and edit the config file to setup your Steam details.

## How is this possible?! ##

This is a bot built using SteamKit split into two components. There's a Core that handles everything related to Steam and listens for commands send via chat or PM. Then there are Modules which is where where the magic happens.

Currently there are five modules:

* **BotManager** (Mandatory) - Allows an admin to change the name of the bot and add/remove users.
* **DogOfTheDay** - Posts a doggo once every hour into chat.
* **ChatJoiner** - Joins a chat room.
* **MapList** - Allows a user to add, remove, and update a map.
* **Server** - Queries a Steam server.

## Okay, great. But how do I do a thing? ##

### Adding a Module ###

When the bot is loaded it will go through the list of module names as specified inside its configuration and will create an instance of each module.

So I my config looks like this:

```
"Modules": 
[
   "ChatJoiner",
   "DogOfTheDay",
   "MapModule",
   "Server",
],
```

I'll end up with those 4 modules being loaded up at startup.

FYI: Modules must be in the namespace 'Modules.ModuleName' and a module must contain a class of the same name that derives from the Module base class. For example, the DogOfTheDay module has the class 'DogOfTheDay' that derives from Module that sits in the namespace 'Modules.DogOfTheDay'.

**...buh?**

Just create a class like 'MyModule' derived from Module. Make sure it's in the namespace 'Modules.MyModule'. Then override the OnInitialize method and start doing cool things. Maybe a Cat of the Day? Who knows. Just an idea.

### Adding a Command ###

There are several ways to add a command. Inside your module, you will have a reference to a CommandListener which will allow you to register a command and assign a callback. 

The simplest usage is to use the Func method overload:

```
protected override void OnInitialize()
{
	CommandListener.AddCommand("!givecat", GiveCat);
}

string GiveCat(CommandSource source)
{
	// This string will be returned to the caller.
	return ":3";
}
```

But you can also register a class that derives from ChatCommand if you want to keep things a bit tidier:

```
protected override void OnInitialize()
{
	CommandListener.AddCommand<GiveCat>("!givecat");
}

public class GiveCat : ChatCommand
{
	public override string Execute(CommandSource source)
	{
		// This string will be returned to the caller.
		return ":3";
	}
}
```
