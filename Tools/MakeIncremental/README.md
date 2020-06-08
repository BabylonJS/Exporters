# Make Incremental scene tool

This tools assists in the creation of `.incremental.babylon` scenes that can be loaded dinamically as the camera navigates through your scene.

If you want the compiled versions for you platform look  `/Dist`.

This project runs on `dotnet core 3.1`. It can be installed from https://dot.net

## Usage: 

```sh
$ MakeIncremental /i:"SOURCE_FILE" /o:"SOURCE_DEST" [/textures]
```

## Building from source

Make sure you have `dotnet core` installed and then just:

```sh
$ dotnet build
```

## Building and packing "/Dist"

Make sure you have `Make` and `dotnet core` installed and then:

```sh
$ make build
```
