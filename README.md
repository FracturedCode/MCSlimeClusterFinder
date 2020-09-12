# MCSlimeClusterFinder
### What it is:
MCSlimeClusterFinder is a slime chunk cluster locator for minecraft. It finds clusters in a 128 block radius (max mob spawning distance)

**This algorithm isn't perfect, it's just meant as a starting point to find areas that will be useful.** It all depends on where the player stands to maximize the number of spawning platforms. For instance, one of my outputs was labeled as 56, but when tested in the minecraft world, had 58 chunks in range.
### How it works:
A more detailed description is located in Program.cs, but here is a brief explanation.

The java portion simply finds every slime chunk in the specified border and saves it to `slimeChunks.txt`.
The C# portion iterates over every chunk inside the border (- an 8 chunk buffer.) Each iteration tallies all slime chunks in about a 128 block radius. Every tally >= the cluster threshold is saved in `candidates.txt`
There's probably a better way to do this that involves nearest neighbor or whatever but this was a very quick little project I wasn't going to put too much time into.
### How to use:
1. In Main.java place your "radius" (more accurately half-length) of your search area from (x=0, z=0) and your world seed in the ChunkFinder constructor.
2. Compile with `javac *.java`, run with `java Main`
3. Once finished, open up `Program.cs` and change any necessary parameters including the thread count, cluster threshold, and the border length you set in the java file.
4. Save and run with `dotnet run`

This run time heavily depends on your specified border, but with a 200k half-length and a Ryzen 5 3600 on 8 threads this took me about 1 hour.

