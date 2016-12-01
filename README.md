# GithubDocGen

Given a managed dll this program will try to create a bunch of .md files to be used on a github wiki.

Here's an example of its output:

https://github.com/Facepunch/Facepunch.Steamworks/wiki/Facepunch.Steamworks

# Code Quality

The code is terrible, this started as a proof of concept and didn't evolve much from there.

# Command Line

``` 
[Option( 'l', "library", Required = true, HelpText = "Libraries to be parsed" )]
[Option( 'o', "output", Required = true, HelpText = "Output folder" )]
[Option( 'd', "delete", Required = false, HelpText = "Clear everything" )]
 ```

Example:

```
-l "C:\Test\MyLibrary.dll" -o "C:\Test\Generated" -d
```


# License

MIT. Do whatever you want.
