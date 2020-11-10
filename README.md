# MCSlimeClusterFinder
## What it is:
MCSlimeClusterFinder is a slime chunk cluster locator for minecraft. It finds clusters in a 128 block radius (max mob spawning distance)

**This algorithm isn't perfect, it's just meant as a starting point to find areas that will be useful.** It all depends on where the player stands to maximize the number of spawning platforms. For instance, one of my outputs was labeled as 56, but when tested in the minecraft world, had 58 chunks in range.

## Future Plans:
* OpenCL support for even faster GPU accelerated speeds (see [OpenCL.NetCore](https://github.com/FracturedCodes/OpenCL.NetCore))
* bugfix memory used display
* threshold command line option
* calculating spawn platforms down to the block and optimizing player position inside the chunk
* move to an inside-out approach
* keyboard interrupt for pausing
* keyboard interrupt for emergency stop (for frozen screen)
* ability to exit the program and resume from file later
* execution speed presets
* progress meter
* device command line option
* time estimation

## How to use:
```
MCSlimeClusterFinder:
Usage: MCSlimeClusterFinder -s WORLD_SEED [OPTIONS]

  -s, --seed=VALUE           world seed, type long
  -i, --in=VALUE             input file to continue saved work
  -o, --out=VALUE            file to save the results
  -h, --help                 show this message and exit
      --start=VALUE          work group step to start at. Learn more in readme (-r)
      --stop=VALUE           work group step to stop at. Learn more in readme (-r)
  -w, --work-size=VALUE      length of the square chunk of work sent to the GPU
                               at once less than 2^14
  -r, --readme               print the readme and exit
  -d, --device=VALUE         the index of the OpenCL device to use

Try `MCSlimeClusterFinder --help' for more information.
```
### Parameters
//TODO

## How to compile and run:
`dotnet run --project MCSlimeClusterFinder [PROGRAM ARGUMENTS (see "How to use")]`

To compile in self-contained single file executable (windows 10 x64):

`dotnet publish -p:PublishSingleFile=true --configuration Release --runtime win10-x64`

## Sample output:
```
TODO
```

## How it works:
The program iterates over every chunk in the specified search area. Each iteration checks how many slime chunks are in range, and compiles the best ones. 
TODO

This run time heavily depends on your specified border.
