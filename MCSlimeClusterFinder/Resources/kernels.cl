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

#define WORK_LENGTH 5000

__kernel void BigKernel(int x, int z, const int worldSeed, const int maxCandidates, __global int3* candidates) {
    int id = get_global_id(0);
    int candidateCount = 0;
    char slimeCounter = 0;

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

    // Initialize search area
    char results[WORK_LENGTH][WORK_LENGTH];

    for (int i = x; i < WORK_LENGTH; i++) {

        for (int i = 0; i < deltaCounter; i++) {
            int interestX = x + deltas[i].x;
            int interestZ = z + deltas[i].y;
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
        results[i][0] = slimeCounter;
    }
    for (int j = z + 1; j < WORK_LENGTH; j++) {
        slimeCounter = 0;

        for (int i = 0; i < deltaCounter; i++) {
            int interestX = x + deltas[i].x;
            int interestZ = z + deltas[i].y;
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
        results[0][j] = slimeCounter;
    }

    // Search the search area
    for (int i = x + 1; i < WORK_LENGTH; i++) {
        for (int j = z + 1; j < WORK_LENGTH; j++) {
            if (results[i-1][j] < 25 || results[i][j-1] < 25) {
                results[i][j] = 0xFF;
            } else {
                slimeCounter = 0;

                for (int i = 0; i < deltaCounter; i++) {
                    int interestX = x + deltas[i].x;
                    int interestZ = z + deltas[i].y;
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
                
                if (slimeCounter > 55) {
                    candidates[maxCandidates*id + candidateCount++] = (int3)(i, j, slimeCounter);
                }
                results[i][j] = slimeCounter;
            }
        }
    }
}

__kernel void BigKernelPreInitialize(int x, int z, const int worldSeed, const int maxCandidates, __global int3* candidates) {
    int id = get_global_id(0);
    int candidateCount = 0;

    // Initialize search area
    bool isSlimeChunk[WORK_LENGTH+16][WORK_LENGTH+16];
    for (int i = x-8; i < x + WORK_LENGTH + 8; i++) {
        for (int j = z-8; j < z + WORK_LENGTH + 8; j++) {
            long seed = ((worldSeed + (long) (i * i * 4987142) + (long) (i * 5947611) + (long) (j * j) * 4392871L + (long) (j * 389711) ^ 987234911L) ^ 0x5DEECE66DL) & ((1L << 48) - 1);
            int bits, val;
            do
            {
                seed = (seed * 0x5DEECE66DL + 0xBL) & ((1L << 48) - 1);
                bits = (int)((ulong)seed >> 17);
                val = bits % 10;
            } while (bits - val + 9 < 0);

            isSlimeChunk[i+8][j+8] = val == 0;
        }
    }

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

    // Search the search area
    for (int i = 8; i < WORK_LENGTH + 8; i++) {
        for (int j = 8; j < WORK_LENGTH + 8; j++) {
            int slimeCounter = 0;
            for (int k = 0; k < deltaCounter; k++) {
                slimeCounter += isSlimeChunk[i + deltas[k].x][j + deltas[k].y];
            }
            if (slimeCounter > 55) {
                candidates[maxCandidates*id + candidateCount++] = (int3)(i-8+x, j-8+z, slimeCounter);
            }
        }
    }
}