import java.io.File;
import java.io.FileWriter;
import java.io.IOException;
import java.util.ArrayList;
import java.util.Random;

/*
This algorithm was derived from this function and function call in MCP

boolean flag = SharedSeedRandom.seedSlimeChunk(chunkpos.x, chunkpos.z, ((ISeedReader)p_223366_1_).getSeed(), 987234911L).nextInt(10) == 0;

public static Random seedSlimeChunk(int p_205190_0_, int p_205190_1_, long p_205190_2_, long p_205190_4_) {
      return new Random(p_205190_2_ + (long)(p_205190_0_ * p_205190_0_ * 4987142) + (long)(p_205190_0_ * 5947611) + (long)(p_205190_1_ * p_205190_1_) * 4392871L + (long)(p_205190_1_ * 389711) ^ p_205190_4_);
}
*/

public class ChunkFinder {
    public final ArrayList<Chunk> SlimeChunks;
    private final int chunkLength;
    private final long seed;

    public ChunkFinder(int length, long seed) {
        SlimeChunks = new ArrayList<Chunk>();
        this.chunkLength = length / 16;
        this.seed = seed;
    }

    public void Run() {
        PopulateChunkList();
        SaveChunkListToFile();
    }

    public void PopulateChunkList() {
        int lastPercent = 0;
        System.out.println("Finding chunks\n0%");
        for (int i = -chunkLength; i < chunkLength; i++) {
            if ((int)((double)(i + chunkLength) / (double)(chunkLength * 2) * 100) > lastPercent) {
                Main.clearConsole();
                System.out.println("Finding chunks\n" + ++lastPercent + "%"); // Only works with large borders, ie > a few thousand
            }
            for (int j = -chunkLength; j < chunkLength; j++) {
                if (new Random(seed + (long) (i * i * 4987142) + (long) (i * 5947611) + (long) (j * j) * 4392871L + (long) (j * 389711) ^ 987234911L).nextInt(10) == 0) {
                    SlimeChunks.add(new Chunk(i, j));
                }
            }
        }
        System.out.println("All slime chunks found within " + chunkLength * 16 + " block border.");
        System.out.println("Final count is " + SlimeChunks.size() + " slime chunks which is " + (double)SlimeChunks.size() / (double)(chunkLength * chunkLength * 4) + " of all chunks");
    }

    public void SaveChunkListToFile() {
        System.out.println("Saving all to file...");
        try {
            File file = new File("slimeChunks.txt");
            file.createNewFile();
            FileWriter fr = new FileWriter("slimeChunks.txt");
            for (Chunk chunk : SlimeChunks) {
                fr.write(chunk.x + ", " + chunk.z + "\n");
            }
            fr.close();
        } catch (IOException e) {
            e.printStackTrace();
        }
        System.out.println("Success");
    }

}
