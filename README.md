# Modifi 

A command-line Minecraft Mod manager.


## Getting started
To get started, you'll want to download Modifi and throw it somewhere on your system. It's suggested you add it to your `PATH` so you can manage your mods without throwing relative paths all over the place. It makes things easier.

Once you have the binaries in place, run `modifi init` to get started. It'll ask you a few basic questions (such as what Minecraft version you're targeting) and generate a `.modifi` directory in your instance folder.

Now that that's out of the way, let's actually add a mod or two...

## Mod Management
#### Adding Mods
Adding mods is easy once you get the hang of it. The first two commands here are identical, in that they'll install the latest version for your Minecraft version:

- modifi mods add `@curseforge/jei`
- modifi mods add `@curseforge/jei` `latest`

If you want a specific file, you can do that:

- modifi mods add `@curseforge/jei` `2511052`

Maybe you want to look at the latest versions of a mod?

- modifi mod versions `@curseforge/jei`

#### Removing Mods
Simple task, really. Same way you add a mod:
- modifi mods remove `@curseforge/jei`


#### Updating Mods
In the same way you can add mods, you can update them as well. The syntax is fairly similar:

- modifi mod update `@curseforge/jei`
- modifi mod update `@curseforge/jei` `2511052`


## A word about @domains
As you've very lkely noticed by now, all the mods are using an `@curseforge/` prefix. That's to specify a mod is coming from the `CurseForge` domain, which is a built-in handler for the mods on that site. Similarly, you can add your own domains, if you need them:

- modifi domains add `@example` `https://example.com/mods`
- modifi domains remove `@example`

Custom domains are handled as a base-directory for mods. If you've used maven, the concept is similar. To lay it out:

- domain/
	- mod/
		- mod-1.0.0.jar
		- mod-1.1.0.jar
		- mod-1.1.1.jar

In order to install `mod-1.1.1.jar`, you'd add the domain, then the mod:
- modifi domains add `@example` `https://example.com/mods`
- modifi mods add `@example/mod` `1.1.1.jar`

Custom domains work a bit differently, in that the manager would try to download the mod at `https://example.com/mods/mod/1.1.1.jar`. 

In the future, customized domain handling may be implemented. Keep an eye out!

## Pack Management
Okay, so you've got yourself a nice set of mods, they all play nice together- no conflicts and version complaints here! Now what?

Well, now you've got yourself a nice set of files inside of that `.modifi` directory! That file tells Modifi about all the mods and domains you need to create that particular setup again- versions, checksums. You name it. So, you can throw that file set at a friend, and they can now run a new command:

- modifi pack `install`

...I'll give you a moment to figure out what that does.

Still thinking? That command looks for `.modifi` as a directory inside the directory you run the command in. If it finds the files and all looks good, then Modifi will set off on downloading all the versions and mods it needs. It'll stash mods away in a cache, then once it has everything it needs, it'll move everything over into your Minecraft `mods` directory. Nifty.