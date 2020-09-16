import java.io.IOException;
// Author FracturedCode
// This program simply brute force checks all chunks in -length to +length in both x and z directions and saves all slime chunks
// Works for 1.15.2. Unverified for other versions, though I think I have heard the slime chunk algorithm hasn't changed in awhile.
// Length is meters so /16 for chunks
public class Main {
    public static void main(String[] args) {
        new ChunkFinder(200000, 423338365327502521L).Run();
    }
    public static void clearConsole() {
        try {
            new ProcessBuilder("cmd", "/c", "cls").inheritIO().start().waitFor();
        } catch (IOException | InterruptedException e) {
            e.printStackTrace();
        }
    }
}
