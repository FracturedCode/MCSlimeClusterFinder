__kernel void FindClusters2(const int x, const int z, __global int* result, const int worldSeed, const int globalSize) {
    int id = get_global_id(0);

    int rowSize = (int)sqrt((float)globalSize);
    int centerX = id / rowSize + x;
    int centerZ = id % rowSize + z;

    // Populate deltas
    int2 deltas[249];
    int deltaCounter = 0;
    for (int i = -8; i < 9; i++) {
        for (int j = -8; j < 9; j++) {
            if (sqrt((float)(i * i + j * j)) < 9.0) {
                deltas[deltaCounter++] = (int2)(i, j);
            }
        }
    }

    int slimeCounter = 0;

    for (int i = 0; i < deltaCounter; i++) {
        int interestX = centerX + deltas[i].x;
        int interestZ = centerZ + deltas[i].y;
        long seed = ((worldSeed + (long) (interestX * interestX * 4987142) + (long) (interestX * 5947611) + (long) (interestZ * interestZ) * 4392871L + (long) (interestZ * 389711) ^ 987234911L) ^ 0x5DEECE66DL) & ((1L << 48) - 1);
        int bits, val;
        do
        {
            seed = (seed * 0x5DEECE66DL + 0xBL) & ((1L << 48) - 1);
            bits = (int)((ulong)seed >> 17);
            val = bits % 10;
        } while (bits - val + 9 < 0);

        if (val==0) {
            slimeCounter++;
        }
    }
    
    result[id] = slimeCounter;
}

__kernel void KernelDoesShitTest(const int x, const int z, __global int* result, const int worldSeed, const int globalSize) {
    int id = get_global_id(0);
    result[id] = get_global_id(0);
}