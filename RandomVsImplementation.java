import java.io.Serializable;
public class RandomVsImplementation {//Typo ik, ik
    public static void main(String[] args) {
        new RandomVsImplementation();
    }
    public RandomVsImplementation() {
        boolean match = new java.util.Random(200).nextInt() == (new Random(200)).nextInt();
        System.out.println(match);
    }
    private class Random implements Serializable
 {
   /**
    * True if the next nextGaussian is available.  This is used by
    * nextGaussian, which generates two gaussian numbers by one call,
    * and returns the second on the second call.
    *
    * @serial whether nextNextGaussian is available
    * @see #nextGaussian()
    * @see #nextNextGaussian
    */
   private boolean haveNextNextGaussian;
 
   /**
    * The next nextGaussian, when available.  This is used by nextGaussian,
    * which generates two gaussian numbers by one call, and returns the
    * second on the second call.
    *
    * @serial the second gaussian of a pair
    * @see #nextGaussian()
    * @see #haveNextNextGaussian
    */
   private double nextNextGaussian;
 
   /**
    * The seed.  This is the number set by setSeed and which is used
    * in next.
    *
    * @serial the internal state of this generator
    * @see #next(int)
    */
   private long seed;
 
   /**
    * Compatible with JDK 1.0+.
    */
   private static final long serialVersionUID = 3905348978240129619L;
 
   /**
    * Creates a new pseudorandom number generator.  The seed is initialized
    * to the current time, as if by
    * <code>setSeed(System.currentTimeMillis());</code>.
    *
    * @see System#currentTimeMillis()
    */
   public Random()
   {
     this(System.currentTimeMillis());
   }
 
   /**
    * Creates a new pseudorandom number generator, starting with the
    * specified seed, using <code>setSeed(seed);</code>.
    *
    * @param seed the initial seed
    */
   public Random(long seed)
   {
     setSeed(seed);
   }
 
   /**
    * Sets the seed for this pseudorandom number generator.  As described
    * above, two instances of the same random class, starting with the
    * same seed, should produce the same results, if the same methods
    * are called.  The implementation for java.util.Random is:
    *
 <pre>public synchronized void setSeed(long seed)
 {
   this.seed = (seed ^ 0x5DEECE66DL) & ((1L &lt;&lt; 48) - 1);
   haveNextNextGaussian = false;
 }</pre>
    *
    * @param seed the new seed
    */
   public synchronized void setSeed(long seed)
   {
     this.seed = (seed ^ 0x5DEECE66DL) & ((1L << 48) - 1);
     haveNextNextGaussian = false;
   }
 
   /**
    * Generates the next pseudorandom number.  This returns
    * an int value whose <code>bits</code> low order bits are
    * independent chosen random bits (0 and 1 are equally likely).
    * The implementation for java.util.Random is:
    *
 <pre>protected synchronized int next(int bits)
 {
   seed = (seed * 0x5DEECE66DL + 0xBL) & ((1L &lt;&lt; 48) - 1);
   return (int) (seed &gt;&gt;&gt; (48 - bits));
 }</pre>
    *
    * @param bits the number of random bits to generate, in the range 1..32
    * @return the next pseudorandom value
    * @since 1.1
    */
   protected synchronized int next(int bits)
   {
     seed = (seed * 0x5DEECE66DL + 0xBL) & ((1L << 48) - 1);
     return (int) (seed >>> (48 - bits));
   }
 
   /**
    * Fills an array of bytes with random numbers.  All possible values
    * are (approximately) equally likely.
    * The JDK documentation gives no implementation, but it seems to be:
    *
 <pre>public void nextBytes(byte[] bytes)
 {
   for (int i = 0; i &lt; bytes.length; i += 4)
   {
     int random = next(32);
     for (int j = 0; i + j &lt; bytes.length && j &lt; 4; j++)
     {
       bytes[i+j] = (byte) (random & 0xff)
       random &gt;&gt;= 8;
     }
   }
 }</pre>
    *
    * @param bytes the byte array that should be filled
    * @throws NullPointerException if bytes is null
    * @since 1.1
    */
   public void nextBytes(byte[] bytes)
   {
     int random;
     // Do a little bit unrolling of the above algorithm.
     int max = bytes.length & ~0x3;
     for (int i = 0; i < max; i += 4)
       {
         random = next(32);
         bytes[i] = (byte) random;
         bytes[i + 1] = (byte) (random >> 8);
         bytes[i + 2] = (byte) (random >> 16);
         bytes[i + 3] = (byte) (random >> 24);
       }
     if (max < bytes.length)
       {
         random = next(32);
         for (int j = max; j < bytes.length; j++)
           {
             bytes[j] = (byte) random;
             random >>= 8;
           }
       }
   }
 
   /**
    * Generates the next pseudorandom number.  This returns
    * an int value whose 32 bits are independent chosen random bits
    * (0 and 1 are equally likely).  The implementation for
    * java.util.Random is:
    * 
 <pre>public int nextInt()
 {
   return next(32);
 }</pre>
    *
    * @return the next pseudorandom value
    */
   public int nextInt()
   {
     return next(32);
   }
 
   /**
    * Generates the next pseudorandom number.  This returns
    * a value between 0(inclusive) and <code>n</code>(exclusive), and
    * each value has the same likelihodd (1/<code>n</code>).
    * (0 and 1 are equally likely).  The implementation for
    * java.util.Random is:
    * 
 <pre>
 public int nextInt(int n)
 {
   if (n &lt;= 0)
     throw new IllegalArgumentException("n must be positive");
 
   if ((n & -n) == n)  // i.e., n is a power of 2
     return (int)((n * (long) next(31)) &gt;&gt; 31);
 
   int bits, val;
   do
   {
     bits = next(31);
     val = bits % n;
   }
   while(bits - val + (n-1) &lt; 0);
 
   return val;
 }</pre>
    *   
    * <p>This algorithm would return every value with exactly the same
    * probability, if the next()-method would be a perfect random number
    * generator.
    *
    * The loop at the bottom only accepts a value, if the random
    * number was between 0 and the highest number less then 1<<31,
    * which is divisible by n.  The probability for this is high for small
    * n, and the worst case is 1/2 (for n=(1<<30)+1).
    *
    * The special treatment for n = power of 2, selects the high bits of
    * the random number (the loop at the bottom would select the low order
    * bits).  This is done, because the low order bits of linear congruential
    * number generators (like the one used in this class) are known to be
    * ``less random'' than the high order bits.
    *
    * @param n the upper bound
    * @throws IllegalArgumentException if the given upper bound is negative
    * @return the next pseudorandom value
    * @since 1.2
    */
   public int nextInt(int n)
   {
     if (n <= 0)
       throw new IllegalArgumentException("n must be positive");
     if ((n & -n) == n) // i.e., n is a power of 2
       return (int) ((n * (long) next(31)) >> 31);
     int bits, val;
     do
       {
         bits = next(31);
         val = bits % n;
       }
     while (bits - val + (n - 1) < 0);
     return val;
   }
 
   /**
    * Generates the next pseudorandom long number.  All bits of this
    * long are independently chosen and 0 and 1 have equal likelihood.
    * The implementation for java.util.Random is:
    *
 <pre>public long nextLong()
 {
   return ((long) next(32) &lt;&lt; 32) + next(32);
 }</pre>
    *
    * @return the next pseudorandom value
    */
   public long nextLong()
   {
     return ((long) next(32) << 32) + next(32);
   }
 
   /**
    * Generates the next pseudorandom boolean.  True and false have
    * the same probability.  The implementation is:
    * 
 <pre>public boolean nextBoolean()
 {
   return next(1) != 0;
 }</pre>
    *
    * @return the next pseudorandom boolean
    * @since 1.2
    */
   public boolean nextBoolean()
   {
     return next(1) != 0;
   }
 
   /**
    * Generates the next pseudorandom float uniformly distributed
    * between 0.0f (inclusive) and 1.0f (exclusive).  The
    * implementation is as follows.
    * 
 <pre>public float nextFloat()
 {
   return next(24) / ((float)(1 &lt;&lt; 24));
 }</pre>
    *
    * @return the next pseudorandom float
    */
   public float nextFloat()
   {
     return next(24) / (float) (1 << 24);
   }
 
   /**
    * Generates the next pseudorandom double uniformly distributed
    * between 0.0 (inclusive) and 1.0 (exclusive).  The
    * implementation is as follows.
    *
 <pre>public double nextDouble()
 {
   return (((long) next(26) &lt;&lt; 27) + next(27)) / (double)(1L &lt;&lt; 53);
 }</pre>
    *
    * @return the next pseudorandom double
    */
   public double nextDouble()
   {
     return (((long) next(26) << 27) + next(27)) / (double) (1L << 53);
   }
 
   /**
    * Generates the next pseudorandom, Gaussian (normally) distributed
    * double value, with mean 0.0 and standard deviation 1.0.
    * The algorithm is as follows.
    * 
 <pre>public synchronized double nextGaussian()
 {
   if (haveNextNextGaussian)
   {
     haveNextNextGaussian = false;
     return nextNextGaussian;
   }
   else
   {
     double v1, v2, s;
     do
     {
       v1 = 2 * nextDouble() - 1; // between -1.0 and 1.0
       v2 = 2 * nextDouble() - 1; // between -1.0 and 1.0
       s = v1 * v1 + v2 * v2;
     }
     while (s >= 1);
 
     double norm = Math.sqrt(-2 * Math.log(s) / s);
     nextNextGaussian = v2 * norm;
     haveNextNextGaussian = true;
     return v1 * norm;
   }
 }</pre>
    *
    * <p>This is described in section 3.4.1 of <em>The Art of Computer
    * Programming, Volume 2</em> by Donald Knuth.
    *
    * @return the next pseudorandom Gaussian distributed double
    */
   public synchronized double nextGaussian()
   {
     if (haveNextNextGaussian)
       {
         haveNextNextGaussian = false;
         return nextNextGaussian;
       }
     double v1, v2, s;
     do
       {
         v1 = 2 * nextDouble() - 1; // Between -1.0 and 1.0.
         v2 = 2 * nextDouble() - 1; // Between -1.0 and 1.0.
         s = v1 * v1 + v2 * v2;
       }
     while (s >= 1);
     double norm = Math.sqrt(-2 * Math.log(s) / s);
     nextNextGaussian = v2 * norm;
     haveNextNextGaussian = true;
     return v1 * norm;
   }
 }
}

