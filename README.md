# MCSlimeClusterFinder
### What it is:
MCSlimeClusterFinder is a slime chunk cluster locator for minecraft. It finds clusters in a 128 block radius (max mob spawning distance)

**This algorithm isn't perfect, it's just meant as a starting point to find areas that will be useful.** It all depends on where the player stands to maximize the number of spawning platforms. For instance, one of my outputs was labeled as 56, but when tested in the minecraft world, had 58 chunks in range.

### Future plans:
* OpenCL support for even faster GPU accelerated speeds (see [OpenCL.NetCore](https://github.com/FracturedCodes/OpenCL.NetCore))
* bugfix memory used display
* threshold command line option
* calculating spawn platforms down to the block and optimizing player position inside the chunk

### How to use:
```
Usage: MCSlimeClusterFinder -s WORLD_SEED [OPTIONS]

  -s, --seed=VALUE           the world seed, type long
  -l, --length=VALUE         the length, in blocks, of the square search area
                               centered on 0,0
  -t, --threads=VALUE        the number of cpu threads to run concurrently
  -o, --out=VALUE            the file to save the results
  -h, --help                 show this message and exit
```

### How to compile and run:
`dotnet run --project MCSlimeClusterFinder [PROGRAM ARGUMENTS (see "How to use")]`

To compile in self-contained single file executable (windows 10 x64):

`dotnet publish -p:PublishSingleFile=true --configuration Release --runtime win10-x64`

### Sample output:
```
> ./MCSlimeClusterFinder -s 420 -t 10 -l 200000
Brute force searching. Starting 10 threads
Aggregate: 99.00%   Individual: 99%     99%     99%     99%     99%     99%     99%     99%     99%     99%
Brute force search complete using a maximum of 0GB of memory
BruteForceAllTheChunksLMFAO completed in 00:01:17.052
Found 10 candidates with a max of 47 slime chunks.
Seed: 420       Area: 12500^2 chunks
Saving...Complete
Top 10 List:
5892, -5280, 47
...
1376, -1018, 45
1995, 1013, 45
Total runtime completed in 00:01:17.068
```

### How it works:
The program iterates over every chunk in the specified search area. Each iteration checks how many slime chunks are in range, and compiles the best ones. I have the program set up so that it only records a cluster of 45 slime chunks or higher (_threshold variable).

There's probably a better way to do this that involves nearest neighbor or whatever but this was a very quick little project I wasn't going to put too much time into.


This run time heavily depends on your specified border.
