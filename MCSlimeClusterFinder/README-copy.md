# MCSlimeClusterFinder
## What it is:
MCSlimeClusterFinder is a slime chunk cluster locator for minecraft. It finds clusters in a 128 block radius (max mob spawning distance) with an inside-out scanning approach. Using OpenCL, this program can boast 100 million chunks scanned in under two seconds on a modest Vega 56.

**This algorithm isn't perfect (though I am trying to get there soon), it's just meant as a starting point to find areas that will be useful for now.** It all depends on where the player stands to maximize the number of spawning platforms. For instance, one of my outputs was labeled as 56, but when tested in the minecraft world, had 58 chunks in range.

## Future Plans:
* runtime setting changes
* a few minor OpenCL optimizations
* bugfix memory used display
* threshold command line option
* calculating spawn platforms down to the block and optimizing player position inside the chunk
* keyboard interrupt for pausing
* ability to exit the program and resume from file later
* execution speed presets
* progress meter
* time estimation
* an OpenCL kernel rewrite for even faster speeds in v3
* cpu support
* official linux support

## v2.0 Changelog
* OpenCL support for even faster GPU accelerated speeds (see [OpenCL.NetCore](https://github.com/FracturedCodes/OpenCL.NetCore))
* OpenCL work item size option. Adjust the load on your GPU with this.
* OpenCL device command line option
* device selection menu
* search radius has stop AND start options
* most output has been axed since moving to OpenCL. A better replacement is planned for future patches.
* move to an inside-out rectangular spiral scanning approach. This will enable stopping the program and running it at a later time from a save in a future patch

## How to compile and run:
`dotnet run --project MCSlimeClusterFinder [PROGRAM ARGUMENTS (see "How to use")]`

To compile in self-contained single file executable (windows 10 x64):

`dotnet publish -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true --configuration Release --runtime win10-x64`

## How to use:
```
Usage: MCSlimeClusterFinder -s WORLD_SEED [OPTIONS]

  -s, --seed=VALUE           world seed, type long
  -i, --in=VALUE             input file to continue saved work
  -o, --out=VALUE            file to save the results
  -h, --help                 show this message and exit
      --start=VALUE          the start "radius" of the search area in blocks/
                               meters
      --stop=VALUE           the end "radius" of the search area in blocks/
                               meters
  -w, --work-size=VALUE      length of the square chunk of work sent to the GPU
                               at once less than 2^14
  -r, --readme               print the readme and exit. Includes a how-to
  -d, --device=VALUE         the index of the OpenCL device to use

Try `MCSlimeClusterFinder --help' for more information.
```

## Sample output:
```
.\MCSlimeClusterFinder -s 420 -w 4096 -o slimeResults.log --start 0 --stop 10000
Devices:

[0]: gfx900 AMD Accelerated Parallel Processing
Select a device index: 0
```

## How it works:
The program iterates over every chunk in the specified search area in an inside-out spiral approach like [Nukelawe's application](https://youtu.be/0KiXqqdZXbs). Each iteration checks how many slime chunks are in range of the center slime chunk. The program automatically compiles the best options.

This run time heavily depends on your specified border and GPU.
