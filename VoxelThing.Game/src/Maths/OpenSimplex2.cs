/**
 * K.jpg's OpenSimplex 2, faster variant
 */

namespace VoxelThing.Game.Maths;

using System.Runtime.CompilerServices;

public static class OpenSimplex2
{
    private const long PrimeX = 0x5205402B9270C86FL;
    private const long PrimeY = 0x598CD327003817B5L;
    private const long PrimeZ = 0x5BCC226E9FA0BACBL;
    private const long PrimeW = 0x56CC5227E58F554BL;
    private const long HashMultiplier = 0x53A3F72DEEC546F5L;
    private const long SeedFlip3D = -0x52D547B2E96ED629L;
    private const long SeedOffset4D = 0xE83DC3E0DA7164DL;

    private const double Root2Over2 = 0.7071067811865476;
    private const double Skew2D = 0.366025403784439;
    private const double Unskew2D = -0.21132486540518713;

    private const double Root3Over3 = 0.577350269189626;
    private const double FallbackRotate3D = 2.0 / 3.0;
    private const double Rotate3DOrthogonalizer = Unskew2D;

    private const float Skew4D = -0.138196601125011f;
    private const float Unskew4D = 0.309016994374947f;
    private const float LatticeStep4D = 0.2f;

    private const int NGrads2DExponent = 7;
    private const int NGrads3DExponent = 8;
    private const int NGrads4DExponent = 9;
    private const int NGrads2D = 1 << NGrads2DExponent;
    private const int NGrads3D = 1 << NGrads3DExponent;
    private const int NGrads4D = 1 << NGrads4DExponent;

    private const double Normalizer2D = 0.01001634121365712;
    private const double Normalizer3D = 0.07969837668935331;
    private const double Normalizer4D = 0.0220065933241897;

    private const float Rsquared2D = 0.5f;
    private const float Rsquared3D = 0.6f;
    private const float Rsquared4D = 0.6f;


    /*
     * Noise Evaluators
     */

    /**
     * 2D Simplex noise, standard lattice orientation.
     */
    public static float Noise2(long seed, double x, double y)
    {
        // Get points for A2* lattice
        double s = Skew2D * (x + y);
        double xs = x + s, ys = y + s;

        return Noise2_UnskewedBase(seed, xs, ys);
    }

    /**
     * 2D Simplex noise, with Y pointing down the main diagonal.
     * Might be better for a 2D sandbox style game, where Y is vertical.
     * Probably slightly less optimal for heightmaps or continent maps,
     * unless your map is centered around an equator. It's a subtle
     * difference, but the option is here to make it an easy choice.
     */
    public static float Noise2_ImproveX(long seed, double x, double y)
    {
        // Skew transform and rotation baked into one.
        double xx = x * Root2Over2;
        double yy = y * (Root2Over2 * (1 + 2 * Skew2D));

        return Noise2_UnskewedBase(seed, yy + xx, yy - xx);
    }

    /**
     * 2D Simplex noise base.
     */
    private static float Noise2_UnskewedBase(long seed, double xs, double ys)
    {
        // Get base points and offsets.
        int xsb = FastFloor(xs), ysb = FastFloor(ys);
        float xi = (float)(xs - xsb), yi = (float)(ys - ysb);

        // Prime pre-multiplication for hash.
        long xsbp = xsb * PrimeX, ysbp = ysb * PrimeY;

        // Unskew.
        float t = (xi + yi) * (float)Unskew2D;
        float dx0 = xi + t, dy0 = yi + t;

        // First vertex.
        float value = 0;
        float a0 = Rsquared2D - dx0 * dx0 - dy0 * dy0;
        if (a0 > 0)
        {
            value = (a0 * a0) * (a0 * a0) * Grad(seed, xsbp, ysbp, dx0, dy0);
        }

        // Second vertex.
        float a1 = (float)(2 * (1 + 2 * Unskew2D) * (1 / Unskew2D + 2)) * t + ((float)(-2 * (1 + 2 * Unskew2D) * (1 + 2 * Unskew2D)) + a0);
        if (a1 > 0)
        {
            float dx1 = dx0 - (float)(1 + 2 * Unskew2D);
            float dy1 = dy0 - (float)(1 + 2 * Unskew2D);
            value += (a1 * a1) * (a1 * a1) * Grad(seed, xsbp + PrimeX, ysbp + PrimeY, dx1, dy1);
        }

        // Third vertex.
        if (dy0 > dx0)
        {
            float dx2 = dx0 - (float)Unskew2D;
            float dy2 = dy0 - (float)(Unskew2D + 1);
            float a2 = Rsquared2D - dx2 * dx2 - dy2 * dy2;
            if (a2 > 0)
            {
                value += (a2 * a2) * (a2 * a2) * Grad(seed, xsbp, ysbp + PrimeY, dx2, dy2);
            }
        }
        else
        {
            float dx2 = dx0 - (float)(Unskew2D + 1);
            float dy2 = dy0 - (float)Unskew2D;
            float a2 = Rsquared2D - dx2 * dx2 - dy2 * dy2;
            if (a2 > 0)
            {
                value += (a2 * a2) * (a2 * a2) * Grad(seed, xsbp + PrimeX, ysbp, dx2, dy2);
            }
        }

        return value;
    }

    /**
     * 3D OpenSimplex2 noise, with better visual isotropy in (X, Y).
     * Recommended for 3D terrain and time-varied animations.
     * The Z coordinate should always be the "different" coordinate in whatever your use case is.
     * If Y is vertical in world coordinates, call Noise3_ImproveXZ(x, z, Y) or use noise3_XZBeforeY.
     * If Z is vertical in world coordinates, call Noise3_ImproveXZ(x, y, Z).
     * For a time varied animation, call Noise3_ImproveXY(x, y, T).
     */
    public static float Noise3_ImproveXY(long seed, double x, double y, double z)
    {
        // Re-orient the cubic lattices without skewing, so Z points up the main lattice diagonal,
        // and the planes formed by XY are moved far out of alignment with the cube faces.
        // Orthonormal rotation. Not a skew transform.
        double xy = x + y;
        double s2 = xy * Rotate3DOrthogonalizer;
        double zz = z * Root3Over3;
        double xr = x + s2 + zz;
        double yr = y + s2 + zz;
        double zr = xy * -Root3Over3 + zz;

        // Evaluate both lattices to form a BCC lattice.
        return Noise3_UnrotatedBase(seed, xr, yr, zr);
    }

    /**
     * 3D OpenSimplex2 noise, with better visual isotropy in (X, Z).
     * Recommended for 3D terrain and time-varied animations.
     * The Y coordinate should always be the "different" coordinate in whatever your use case is.
     * If Y is vertical in world coordinates, call Noise3_ImproveXZ(x, Y, z).
     * If Z is vertical in world coordinates, call Noise3_ImproveXZ(x, Z, y) or use Noise3_ImproveXY.
     * For a time varied animation, call Noise3_ImproveXZ(x, T, y) or use Noise3_ImproveXY.
     */
    public static float Noise3_ImproveXZ(long seed, double x, double y, double z)
    {
        // Re-orient the cubic lattices without skewing, so Y points up the main lattice diagonal,
        // and the planes formed by XZ are moved far out of alignment with the cube faces.
        // Orthonormal rotation. Not a skew transform.
        double xz = x + z;
        double s2 = xz * Rotate3DOrthogonalizer;
        double yy = y * Root3Over3;
        double xr = x + s2 + yy;
        double zr = z + s2 + yy;
        double yr = xz * -Root3Over3 + yy;

        // Evaluate both lattices to form a BCC lattice.
        return Noise3_UnrotatedBase(seed, xr, yr, zr);
    }

    /**
     * 3D OpenSimplex2 noise, fallback rotation option
     * Use Noise3_ImproveXY or Noise3_ImproveXZ instead, wherever appropriate.
     * They have less diagonal bias. This function's best use is as a fallback.
     */
    public static float Noise3_Fallback(long seed, double x, double y, double z)
    {
        // Re-orient the cubic lattices via rotation, to produce a familiar look.
        // Orthonormal rotation. Not a skew transform.
        double r = FallbackRotate3D * (x + y + z);
        double xr = r - x, yr = r - y, zr = r - z;

        // Evaluate both lattices to form a BCC lattice.
        return Noise3_UnrotatedBase(seed, xr, yr, zr);
    }

    /**
     * Generate overlapping cubic lattices for 3D OpenSimplex2 noise.
     */
    private static float Noise3_UnrotatedBase(long seed, double xr, double yr, double zr)
    {
        // Get base points and offsets.
        int xrb = FastRound(xr), yrb = FastRound(yr), zrb = FastRound(zr);
        float xri = (float)(xr - xrb), yri = (float)(yr - yrb), zri = (float)(zr - zrb);

        // -1 if positive, 1 if negative.
        int xNSign = (int)(-1.0f - xri) | 1, yNSign = (int)(-1.0f - yri) | 1, zNSign = (int)(-1.0f - zri) | 1;

        // Compute absolute values, using the above as a shortcut. This was faster in my tests for some reason.
        float ax0 = xNSign * -xri, ay0 = yNSign * -yri, az0 = zNSign * -zri;

        // Prime pre-multiplication for hash.
        long xrbp = xrb * PrimeX, yrbp = yrb * PrimeY, zrbp = zrb * PrimeZ;

        // Loop: Pick an edge on each lattice copy.
        float value = 0;
        float a = (Rsquared3D - xri * xri) - (yri * yri + zri * zri);
        for (int l = 0; ; l++)
        {

            // Closest point on cube.
            if (a > 0)
            {
                value += (a * a) * (a * a) * Grad(seed, xrbp, yrbp, zrbp, xri, yri, zri);
            }

            // Second-closest point.
            if (ax0 >= ay0 && ax0 >= az0)
            {
                float b = a + ax0 + ax0;
                if (b > 1)
                {
                    b -= 1;
                    value += (b * b) * (b * b) * Grad(seed, xrbp - xNSign * PrimeX, yrbp, zrbp, xri + xNSign, yri, zri);
                }
            }
            else if (ay0 > ax0 && ay0 >= az0)
            {
                float b = a + ay0 + ay0;
                if (b > 1)
                {
                    b -= 1;
                    value += (b * b) * (b * b) * Grad(seed, xrbp, yrbp - yNSign * PrimeY, zrbp, xri, yri + yNSign, zri);
                }
            }
            else
            {
                float b = a + az0 + az0;
                if (b > 1)
                {
                    b -= 1;
                    value += (b * b) * (b * b) * Grad(seed, xrbp, yrbp, zrbp - zNSign * PrimeZ, xri, yri, zri + zNSign);
                }
            }

            // Break from loop if we're done, skipping updates below.
            if (l == 1) break;

            // Update absolute value.
            ax0 = 0.5f - ax0;
            ay0 = 0.5f - ay0;
            az0 = 0.5f - az0;

            // Update relative coordinate.
            xri = xNSign * ax0;
            yri = yNSign * ay0;
            zri = zNSign * az0;

            // Update falloff.
            a += (0.75f - ax0) - (ay0 + az0);

            // Update prime for hash.
            xrbp += (xNSign >> 1) & PrimeX;
            yrbp += (yNSign >> 1) & PrimeY;
            zrbp += (zNSign >> 1) & PrimeZ;

            // Update the reverse sign indicators.
            xNSign = -xNSign;
            yNSign = -yNSign;
            zNSign = -zNSign;

            // And finally update the seed for the other lattice copy.
            seed ^= SeedFlip3D;
        }

        return value;
    }

    /**
     * 4D OpenSimplex2 noise, with XYZ oriented like Noise3_ImproveXY
     * and W for an extra degree of freedom. W repeats eventually.
     * Recommended for time-varied animations which texture a 3D object (W=time)
     * in a space where Z is vertical
     */
    public static float Noise4_ImproveXYZ_ImproveXY(long seed, double x, double y, double z, double w)
    {
        double xy = x + y;
        double s2 = xy * -0.21132486540518699998;
        double zz = z * 0.28867513459481294226;
        double ww = w * 0.2236067977499788;
        double xr = x + (zz + ww + s2), yr = y + (zz + ww + s2);
        double zr = xy * -0.57735026918962599998 + (zz + ww);
        double wr = z * -0.866025403784439 + ww;

        return Noise4_UnskewedBase(seed, xr, yr, zr, wr);
    }

    /**
     * 4D OpenSimplex2 noise, with XYZ oriented like Noise3_ImproveXZ
     * and W for an extra degree of freedom. W repeats eventually.
     * Recommended for time-varied animations which texture a 3D object (W=time)
     * in a space where Y is vertical
     */
    public static float Noise4_ImproveXYZ_ImproveXZ(long seed, double x, double y, double z, double w)
    {
        double xz = x + z;
        double s2 = xz * -0.21132486540518699998;
        double yy = y * 0.28867513459481294226;
        double ww = w * 0.2236067977499788;
        double xr = x + (yy + ww + s2), zr = z + (yy + ww + s2);
        double yr = xz * -0.57735026918962599998 + (yy + ww);
        double wr = y * -0.866025403784439 + ww;

        return Noise4_UnskewedBase(seed, xr, yr, zr, wr);
    }

    /**
     * 4D OpenSimplex2 noise, with XYZ oriented like Noise3_Fallback
     * and W for an extra degree of freedom. W repeats eventually.
     * Recommended for time-varied animations which texture a 3D object (W=time)
     * where there isn't a clear distinction between horizontal and vertical
     */
    public static float Noise4_ImproveXYZ(long seed, double x, double y, double z, double w)
    {
        double xyz = x + y + z;
        double ww = w * 0.2236067977499788;
        double s2 = xyz * -0.16666666666666666 + ww;
        double xs = x + s2, ys = y + s2, zs = z + s2, ws = -0.5 * xyz + ww;

        return Noise4_UnskewedBase(seed, xs, ys, zs, ws);
    }

    /**
     * 4D OpenSimplex2 noise, fallback lattice orientation.
     */
    public static float Noise4_Fallback(long seed, double x, double y, double z, double w)
    {
        // Get points for A4 lattice
        double s = Skew4D * (x + y + z + w);
        double xs = x + s, ys = y + s, zs = z + s, ws = w + s;

        return Noise4_UnskewedBase(seed, xs, ys, zs, ws);
    }

    /**
    * 4D OpenSimplex2 noise base.
    */
    private static float Noise4_UnskewedBase(long seed, double xs, double ys, double zs, double ws)
    {
        // Get base points and offsets
        int xsb = FastFloor(xs), ysb = FastFloor(ys), zsb = FastFloor(zs), wsb = FastFloor(ws);
        float xsi = (float)(xs - xsb), ysi = (float)(ys - ysb), zsi = (float)(zs - zsb), wsi = (float)(ws - wsb);

        // Determine which lattice we can be confident has a contributing point its corresponding cell's base simplex.
        // We only look at the spaces between the diagonal planes. This proved effective in all of my tests.
        float siSum = (xsi + ysi) + (zsi + wsi);
        int startingLattice = (int)(siSum * 1.25);

        // Offset for seed based on first lattice copy.
        seed += startingLattice * SeedOffset4D;

        // Offset for lattice point relative positions (skewed)
        float startingLatticeOffset = startingLattice * -LatticeStep4D;
        xsi += startingLatticeOffset; ysi += startingLatticeOffset; zsi += startingLatticeOffset; wsi += startingLatticeOffset;

        // Prep for vertex contributions.
        float ssi = (siSum + startingLatticeOffset * 4) * Unskew4D;

        // Prime pre-multiplication for hash.
        long xsvp = xsb * PrimeX, ysvp = ysb * PrimeY, zsvp = zsb * PrimeZ, wsvp = wsb * PrimeW;

        // Five points to add, total, from five copies of the A4 lattice.
        float value = 0;
        for (int i = 0; ; i++)
        {

            // Next point is the closest vertex on the 4-simplex whose base vertex is the aforementioned vertex.
            double score0 = 1.0 + ssi * (-1.0 / Unskew4D); // Seems slightly faster than 1.0-xsi-ysi-zsi-wsi
            if (xsi >= ysi && xsi >= zsi && xsi >= wsi && xsi >= score0)
            {
                xsvp += PrimeX;
                xsi -= 1;
                ssi -= Unskew4D;
            }
            else if (ysi > xsi && ysi >= zsi && ysi >= wsi && ysi >= score0)
            {
                ysvp += PrimeY;
                ysi -= 1;
                ssi -= Unskew4D;
            }
            else if (zsi > xsi && zsi > ysi && zsi >= wsi && zsi >= score0)
            {
                zsvp += PrimeZ;
                zsi -= 1;
                ssi -= Unskew4D;
            }
            else if (wsi > xsi && wsi > ysi && wsi > zsi && wsi >= score0)
            {
                wsvp += PrimeW;
                wsi -= 1;
                ssi -= Unskew4D;
            }

            // Gradient contribution with falloff.
            float dx = xsi + ssi, dy = ysi + ssi, dz = zsi + ssi, dw = wsi + ssi;
            float a = (dx * dx + dy * dy) + (dz * dz + dw * dw);
            if (a < Rsquared4D)
            {
                a -= Rsquared4D;
                a *= a;
                value += a * a * Grad(seed, xsvp, ysvp, zsvp, wsvp, dx, dy, dz, dw);
            }

            // Break from loop if we're done, skipping updates below.
            if (i == 4) break;

            // Update for next lattice copy shifted down by <-0.2, -0.2, -0.2, -0.2>.
            xsi += LatticeStep4D; ysi += LatticeStep4D; zsi += LatticeStep4D; wsi += LatticeStep4D;
            ssi += LatticeStep4D * 4 * Unskew4D;
            seed -= SeedOffset4D;

            // Because we don't always start on the same lattice copy, there's a special reset case.
            if (i == startingLattice)
            {
                xsvp -= PrimeX;
                ysvp -= PrimeY;
                zsvp -= PrimeZ;
                wsvp -= PrimeW;
                seed += SeedOffset4D * 5;
            }
        }

        return value;
    }

    /*
     * Utility
     */

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float Grad(long seed, long xsvp, long ysvp, float dx, float dy)
    {
        long hash = seed ^ xsvp ^ ysvp;
        hash *= HashMultiplier;
        hash ^= hash >> (64 - NGrads2DExponent + 1);
        int gi = (int)hash & ((NGrads2D - 1) << 1);
        return Gradients2D[gi | 0] * dx + Gradients2D[gi | 1] * dy;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float Grad(long seed, long xrvp, long yrvp, long zrvp, float dx, float dy, float dz)
    {
        long hash = (seed ^ xrvp) ^ (yrvp ^ zrvp);
        hash *= HashMultiplier;
        hash ^= hash >> (64 - NGrads3DExponent + 2);
        int gi = (int)hash & ((NGrads3D - 1) << 2);
        return Gradients3D[gi | 0] * dx + Gradients3D[gi | 1] * dy + Gradients3D[gi | 2] * dz;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float Grad(long seed, long xsvp, long ysvp, long zsvp, long wsvp, float dx, float dy, float dz, float dw)
    {
        long hash = seed ^ (xsvp ^ ysvp) ^ (zsvp ^ wsvp);
        hash *= HashMultiplier;
        hash ^= hash >> (64 - NGrads4DExponent + 2);
        int gi = (int)hash & ((NGrads4D - 1) << 2);
        return (Gradients4D[gi | 0] * dx + Gradients4D[gi | 1] * dy) + (Gradients4D[gi | 2] * dz + Gradients4D[gi | 3] * dw);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int FastFloor(double x)
    {
        int xi = (int)x;
        return x < xi ? xi - 1 : xi;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int FastRound(double x)
    {
        return x < 0 ? (int)(x - 0.5) : (int)(x + 0.5);
    }

    /*
     * Gradients
     */

    private static readonly float[] Gradients2D;
    private static readonly float[] Gradients3D;
    private static readonly float[] Gradients4D;
    static OpenSimplex2()
    {

        Gradients2D = new float[NGrads2D * 2];
        float[] grad2 = {
             0.38268343236509f,   0.923879532511287f,
             0.923879532511287f,  0.38268343236509f,
             0.923879532511287f, -0.38268343236509f,
             0.38268343236509f,  -0.923879532511287f,
            -0.38268343236509f,  -0.923879532511287f,
            -0.923879532511287f, -0.38268343236509f,
            -0.923879532511287f,  0.38268343236509f,
            -0.38268343236509f,   0.923879532511287f,
            //-------------------------------------//
             0.130526192220052f,  0.99144486137381f,
             0.608761429008721f,  0.793353340291235f,
             0.793353340291235f,  0.608761429008721f,
             0.99144486137381f,   0.130526192220051f,
             0.99144486137381f,  -0.130526192220051f,
             0.793353340291235f, -0.60876142900872f,
             0.608761429008721f, -0.793353340291235f,
             0.130526192220052f, -0.99144486137381f,
            -0.130526192220052f, -0.99144486137381f,
            -0.608761429008721f, -0.793353340291235f,
            -0.793353340291235f, -0.608761429008721f,
            -0.99144486137381f,  -0.130526192220052f,
            -0.99144486137381f,   0.130526192220051f,
            -0.793353340291235f,  0.608761429008721f,
            -0.608761429008721f,  0.793353340291235f,
            -0.130526192220052f,  0.99144486137381f,
        };
        for (int i = 0; i < grad2.Length; i++)
        {
            grad2[i] = (float)(grad2[i] / Normalizer2D);
        }
        for (int i = 0, j = 0; i < Gradients2D.Length; i++, j++)
        {
            if (j == grad2.Length) j = 0;
            Gradients2D[i] = grad2[j];
        }

        Gradients3D = new float[NGrads3D * 4];
        float[] grad3 = {
             2.22474487139f,       2.22474487139f,      -1.0f,                 0.0f,
             2.22474487139f,       2.22474487139f,       1.0f,                 0.0f,
             3.0862664687972017f,  1.1721513422464978f,  0.0f,                 0.0f,
             1.1721513422464978f,  3.0862664687972017f,  0.0f,                 0.0f,
            -2.22474487139f,       2.22474487139f,      -1.0f,                 0.0f,
            -2.22474487139f,       2.22474487139f,       1.0f,                 0.0f,
            -1.1721513422464978f,  3.0862664687972017f,  0.0f,                 0.0f,
            -3.0862664687972017f,  1.1721513422464978f,  0.0f,                 0.0f,
            -1.0f,                -2.22474487139f,      -2.22474487139f,       0.0f,
             1.0f,                -2.22474487139f,      -2.22474487139f,       0.0f,
             0.0f,                -3.0862664687972017f, -1.1721513422464978f,  0.0f,
             0.0f,                -1.1721513422464978f, -3.0862664687972017f,  0.0f,
            -1.0f,                -2.22474487139f,       2.22474487139f,       0.0f,
             1.0f,                -2.22474487139f,       2.22474487139f,       0.0f,
             0.0f,                -1.1721513422464978f,  3.0862664687972017f,  0.0f,
             0.0f,                -3.0862664687972017f,  1.1721513422464978f,  0.0f,
            //--------------------------------------------------------------------//
            -2.22474487139f,      -2.22474487139f,      -1.0f,                 0.0f,
            -2.22474487139f,      -2.22474487139f,       1.0f,                 0.0f,
            -3.0862664687972017f, -1.1721513422464978f,  0.0f,                 0.0f,
            -1.1721513422464978f, -3.0862664687972017f,  0.0f,                 0.0f,
            -2.22474487139f,      -1.0f,                -2.22474487139f,       0.0f,
            -2.22474487139f,       1.0f,                -2.22474487139f,       0.0f,
            -1.1721513422464978f,  0.0f,                -3.0862664687972017f,  0.0f,
            -3.0862664687972017f,  0.0f,                -1.1721513422464978f,  0.0f,
            -2.22474487139f,      -1.0f,                 2.22474487139f,       0.0f,
            -2.22474487139f,       1.0f,                 2.22474487139f,       0.0f,
            -3.0862664687972017f,  0.0f,                 1.1721513422464978f,  0.0f,
            -1.1721513422464978f,  0.0f,                 3.0862664687972017f,  0.0f,
            -1.0f,                 2.22474487139f,      -2.22474487139f,       0.0f,
             1.0f,                 2.22474487139f,      -2.22474487139f,       0.0f,
             0.0f,                 1.1721513422464978f, -3.0862664687972017f,  0.0f,
             0.0f,                 3.0862664687972017f, -1.1721513422464978f,  0.0f,
            -1.0f,                 2.22474487139f,       2.22474487139f,       0.0f,
             1.0f,                 2.22474487139f,       2.22474487139f,       0.0f,
             0.0f,                 3.0862664687972017f,  1.1721513422464978f,  0.0f,
             0.0f,                 1.1721513422464978f,  3.0862664687972017f,  0.0f,
             2.22474487139f,      -2.22474487139f,      -1.0f,                 0.0f,
             2.22474487139f,      -2.22474487139f,       1.0f,                 0.0f,
             1.1721513422464978f, -3.0862664687972017f,  0.0f,                 0.0f,
             3.0862664687972017f, -1.1721513422464978f,  0.0f,                 0.0f,
             2.22474487139f,      -1.0f,                -2.22474487139f,       0.0f,
             2.22474487139f,       1.0f,                -2.22474487139f,       0.0f,
             3.0862664687972017f,  0.0f,                -1.1721513422464978f,  0.0f,
             1.1721513422464978f,  0.0f,                -3.0862664687972017f,  0.0f,
             2.22474487139f,      -1.0f,                 2.22474487139f,       0.0f,
             2.22474487139f,       1.0f,                 2.22474487139f,       0.0f,
             1.1721513422464978f,  0.0f,                 3.0862664687972017f,  0.0f,
             3.0862664687972017f,  0.0f,                 1.1721513422464978f,  0.0f,
        };
        for (int i = 0; i < grad3.Length; i++)
        {
            grad3[i] = (float)(grad3[i] / Normalizer3D);
        }
        for (int i = 0, j = 0; i < Gradients3D.Length; i++, j++)
        {
            if (j == grad3.Length) j = 0;
            Gradients3D[i] = grad3[j];
        }

        Gradients4D = new float[NGrads4D * 4];
        float[] grad4 = {
            -0.6740059517812944f,   -0.3239847771997537f,   -0.3239847771997537f,    0.5794684678643381f,
            -0.7504883828755602f,   -0.4004672082940195f,    0.15296486218853164f,   0.5029860367700724f,
            -0.7504883828755602f,    0.15296486218853164f,  -0.4004672082940195f,    0.5029860367700724f,
            -0.8828161875373585f,    0.08164729285680945f,   0.08164729285680945f,   0.4553054119602712f,
            -0.4553054119602712f,   -0.08164729285680945f,  -0.08164729285680945f,   0.8828161875373585f,
            -0.5029860367700724f,   -0.15296486218853164f,   0.4004672082940195f,    0.7504883828755602f,
            -0.5029860367700724f,    0.4004672082940195f,   -0.15296486218853164f,   0.7504883828755602f,
            -0.5794684678643381f,    0.3239847771997537f,    0.3239847771997537f,    0.6740059517812944f,
            -0.6740059517812944f,   -0.3239847771997537f,    0.5794684678643381f,   -0.3239847771997537f,
            -0.7504883828755602f,   -0.4004672082940195f,    0.5029860367700724f,    0.15296486218853164f,
            -0.7504883828755602f,    0.15296486218853164f,   0.5029860367700724f,   -0.4004672082940195f,
            -0.8828161875373585f,    0.08164729285680945f,   0.4553054119602712f,    0.08164729285680945f,
            -0.4553054119602712f,   -0.08164729285680945f,   0.8828161875373585f,   -0.08164729285680945f,
            -0.5029860367700724f,   -0.15296486218853164f,   0.7504883828755602f,    0.4004672082940195f,
            -0.5029860367700724f,    0.4004672082940195f,    0.7504883828755602f,   -0.15296486218853164f,
            -0.5794684678643381f,    0.3239847771997537f,    0.6740059517812944f,    0.3239847771997537f,
            -0.6740059517812944f,    0.5794684678643381f,   -0.3239847771997537f,   -0.3239847771997537f,
            -0.7504883828755602f,    0.5029860367700724f,   -0.4004672082940195f,    0.15296486218853164f,
            -0.7504883828755602f,    0.5029860367700724f,    0.15296486218853164f,  -0.4004672082940195f,
            -0.8828161875373585f,    0.4553054119602712f,    0.08164729285680945f,   0.08164729285680945f,
            -0.4553054119602712f,    0.8828161875373585f,   -0.08164729285680945f,  -0.08164729285680945f,
            -0.5029860367700724f,    0.7504883828755602f,   -0.15296486218853164f,   0.4004672082940195f,
            -0.5029860367700724f,    0.7504883828755602f,    0.4004672082940195f,   -0.15296486218853164f,
            -0.5794684678643381f,    0.6740059517812944f,    0.3239847771997537f,    0.3239847771997537f,
             0.5794684678643381f,   -0.6740059517812944f,   -0.3239847771997537f,   -0.3239847771997537f,
             0.5029860367700724f,   -0.7504883828755602f,   -0.4004672082940195f,    0.15296486218853164f,
             0.5029860367700724f,   -0.7504883828755602f,    0.15296486218853164f,  -0.4004672082940195f,
             0.4553054119602712f,   -0.8828161875373585f,    0.08164729285680945f,   0.08164729285680945f,
             0.8828161875373585f,   -0.4553054119602712f,   -0.08164729285680945f,  -0.08164729285680945f,
             0.7504883828755602f,   -0.5029860367700724f,   -0.15296486218853164f,   0.4004672082940195f,
             0.7504883828755602f,   -0.5029860367700724f,    0.4004672082940195f,   -0.15296486218853164f,
             0.6740059517812944f,   -0.5794684678643381f,    0.3239847771997537f,    0.3239847771997537f,
            //------------------------------------------------------------------------------------------//
            -0.753341017856078f,    -0.37968289875261624f,  -0.37968289875261624f,  -0.37968289875261624f,
            -0.7821684431180708f,   -0.4321472685365301f,   -0.4321472685365301f,    0.12128480194602098f,
            -0.7821684431180708f,   -0.4321472685365301f,    0.12128480194602098f,  -0.4321472685365301f,
            -0.7821684431180708f,    0.12128480194602098f,  -0.4321472685365301f,   -0.4321472685365301f,
            -0.8586508742123365f,   -0.508629699630796f,     0.044802370851755174f,  0.044802370851755174f,
            -0.8586508742123365f,    0.044802370851755174f, -0.508629699630796f,     0.044802370851755174f,
            -0.8586508742123365f,    0.044802370851755174f,  0.044802370851755174f, -0.508629699630796f,
            -0.9982828964265062f,   -0.03381941603233842f,  -0.03381941603233842f,  -0.03381941603233842f,
            -0.37968289875261624f,  -0.753341017856078f,    -0.37968289875261624f,  -0.37968289875261624f,
            -0.4321472685365301f,   -0.7821684431180708f,   -0.4321472685365301f,    0.12128480194602098f,
            -0.4321472685365301f,   -0.7821684431180708f,    0.12128480194602098f,  -0.4321472685365301f,
             0.12128480194602098f,  -0.7821684431180708f,   -0.4321472685365301f,   -0.4321472685365301f,
            -0.508629699630796f,    -0.8586508742123365f,    0.044802370851755174f,  0.044802370851755174f,
             0.044802370851755174f, -0.8586508742123365f,   -0.508629699630796f,     0.044802370851755174f,
             0.044802370851755174f, -0.8586508742123365f,    0.044802370851755174f, -0.508629699630796f,
            -0.03381941603233842f,  -0.9982828964265062f,   -0.03381941603233842f,  -0.03381941603233842f,
            -0.37968289875261624f,  -0.37968289875261624f,  -0.753341017856078f,    -0.37968289875261624f,
            -0.4321472685365301f,   -0.4321472685365301f,   -0.7821684431180708f,    0.12128480194602098f,
            -0.4321472685365301f,    0.12128480194602098f,  -0.7821684431180708f,   -0.4321472685365301f,
             0.12128480194602098f,  -0.4321472685365301f,   -0.7821684431180708f,   -0.4321472685365301f,
            -0.508629699630796f,     0.044802370851755174f, -0.8586508742123365f,    0.044802370851755174f,
             0.044802370851755174f, -0.508629699630796f,    -0.8586508742123365f,    0.044802370851755174f,
             0.044802370851755174f,  0.044802370851755174f, -0.8586508742123365f,   -0.508629699630796f,
            -0.03381941603233842f,  -0.03381941603233842f,  -0.9982828964265062f,   -0.03381941603233842f,
            -0.37968289875261624f,  -0.37968289875261624f,  -0.37968289875261624f,  -0.753341017856078f,
            -0.4321472685365301f,   -0.4321472685365301f,    0.12128480194602098f,  -0.7821684431180708f,
            -0.4321472685365301f,    0.12128480194602098f,  -0.4321472685365301f,   -0.7821684431180708f,
             0.12128480194602098f,  -0.4321472685365301f,   -0.4321472685365301f,   -0.7821684431180708f,
            -0.508629699630796f,     0.044802370851755174f,  0.044802370851755174f, -0.8586508742123365f,
             0.044802370851755174f, -0.508629699630796f,     0.044802370851755174f, -0.8586508742123365f,
             0.044802370851755174f,  0.044802370851755174f, -0.508629699630796f,    -0.8586508742123365f,
            -0.03381941603233842f,  -0.03381941603233842f,  -0.03381941603233842f,  -0.9982828964265062f,
            -0.3239847771997537f,   -0.6740059517812944f,   -0.3239847771997537f,    0.5794684678643381f,
            -0.4004672082940195f,   -0.7504883828755602f,    0.15296486218853164f,   0.5029860367700724f,
             0.15296486218853164f,  -0.7504883828755602f,   -0.4004672082940195f,    0.5029860367700724f,
             0.08164729285680945f,  -0.8828161875373585f,    0.08164729285680945f,   0.4553054119602712f,
            -0.08164729285680945f,  -0.4553054119602712f,   -0.08164729285680945f,   0.8828161875373585f,
            -0.15296486218853164f,  -0.5029860367700724f,    0.4004672082940195f,    0.7504883828755602f,
             0.4004672082940195f,   -0.5029860367700724f,   -0.15296486218853164f,   0.7504883828755602f,
             0.3239847771997537f,   -0.5794684678643381f,    0.3239847771997537f,    0.6740059517812944f,
            -0.3239847771997537f,   -0.3239847771997537f,   -0.6740059517812944f,    0.5794684678643381f,
            -0.4004672082940195f,    0.15296486218853164f,  -0.7504883828755602f,    0.5029860367700724f,
             0.15296486218853164f,  -0.4004672082940195f,   -0.7504883828755602f,    0.5029860367700724f,
             0.08164729285680945f,   0.08164729285680945f,  -0.8828161875373585f,    0.4553054119602712f,
            -0.08164729285680945f,  -0.08164729285680945f,  -0.4553054119602712f,    0.8828161875373585f,
            -0.15296486218853164f,   0.4004672082940195f,   -0.5029860367700724f,    0.7504883828755602f,
             0.4004672082940195f,   -0.15296486218853164f,  -0.5029860367700724f,    0.7504883828755602f,
             0.3239847771997537f,    0.3239847771997537f,   -0.5794684678643381f,    0.6740059517812944f,
            -0.3239847771997537f,   -0.6740059517812944f,    0.5794684678643381f,   -0.3239847771997537f,
            -0.4004672082940195f,   -0.7504883828755602f,    0.5029860367700724f,    0.15296486218853164f,
             0.15296486218853164f,  -0.7504883828755602f,    0.5029860367700724f,   -0.4004672082940195f,
             0.08164729285680945f,  -0.8828161875373585f,    0.4553054119602712f,    0.08164729285680945f,
            -0.08164729285680945f,  -0.4553054119602712f,    0.8828161875373585f,   -0.08164729285680945f,
            -0.15296486218853164f,  -0.5029860367700724f,    0.7504883828755602f,    0.4004672082940195f,
             0.4004672082940195f,   -0.5029860367700724f,    0.7504883828755602f,   -0.15296486218853164f,
             0.3239847771997537f,   -0.5794684678643381f,    0.6740059517812944f,    0.3239847771997537f,
            -0.3239847771997537f,   -0.3239847771997537f,    0.5794684678643381f,   -0.6740059517812944f,
            -0.4004672082940195f,    0.15296486218853164f,   0.5029860367700724f,   -0.7504883828755602f,
             0.15296486218853164f,  -0.4004672082940195f,    0.5029860367700724f,   -0.7504883828755602f,
             0.08164729285680945f,   0.08164729285680945f,   0.4553054119602712f,   -0.8828161875373585f,
            -0.08164729285680945f,  -0.08164729285680945f,   0.8828161875373585f,   -0.4553054119602712f,
            -0.15296486218853164f,   0.4004672082940195f,    0.7504883828755602f,   -0.5029860367700724f,
             0.4004672082940195f,   -0.15296486218853164f,   0.7504883828755602f,   -0.5029860367700724f,
             0.3239847771997537f,    0.3239847771997537f,    0.6740059517812944f,   -0.5794684678643381f,
            -0.3239847771997537f,    0.5794684678643381f,   -0.6740059517812944f,   -0.3239847771997537f,
            -0.4004672082940195f,    0.5029860367700724f,   -0.7504883828755602f,    0.15296486218853164f,
             0.15296486218853164f,   0.5029860367700724f,   -0.7504883828755602f,   -0.4004672082940195f,
             0.08164729285680945f,   0.4553054119602712f,   -0.8828161875373585f,    0.08164729285680945f,
            -0.08164729285680945f,   0.8828161875373585f,   -0.4553054119602712f,   -0.08164729285680945f,
            -0.15296486218853164f,   0.7504883828755602f,   -0.5029860367700724f,    0.4004672082940195f,
             0.4004672082940195f,    0.7504883828755602f,   -0.5029860367700724f,   -0.15296486218853164f,
             0.3239847771997537f,    0.6740059517812944f,   -0.5794684678643381f,    0.3239847771997537f,
            -0.3239847771997537f,    0.5794684678643381f,   -0.3239847771997537f,   -0.6740059517812944f,
            -0.4004672082940195f,    0.5029860367700724f,    0.15296486218853164f,  -0.7504883828755602f,
             0.15296486218853164f,   0.5029860367700724f,   -0.4004672082940195f,   -0.7504883828755602f,
             0.08164729285680945f,   0.4553054119602712f,    0.08164729285680945f,  -0.8828161875373585f,
            -0.08164729285680945f,   0.8828161875373585f,   -0.08164729285680945f,  -0.4553054119602712f,
            -0.15296486218853164f,   0.7504883828755602f,    0.4004672082940195f,   -0.5029860367700724f,
             0.4004672082940195f,    0.7504883828755602f,   -0.15296486218853164f,  -0.5029860367700724f,
             0.3239847771997537f,    0.6740059517812944f,    0.3239847771997537f,   -0.5794684678643381f,
             0.5794684678643381f,   -0.3239847771997537f,   -0.6740059517812944f,   -0.3239847771997537f,
             0.5029860367700724f,   -0.4004672082940195f,   -0.7504883828755602f,    0.15296486218853164f,
             0.5029860367700724f,    0.15296486218853164f,  -0.7504883828755602f,   -0.4004672082940195f,
             0.4553054119602712f,    0.08164729285680945f,  -0.8828161875373585f,    0.08164729285680945f,
             0.8828161875373585f,   -0.08164729285680945f,  -0.4553054119602712f,   -0.08164729285680945f,
             0.7504883828755602f,   -0.15296486218853164f,  -0.5029860367700724f,    0.4004672082940195f,
             0.7504883828755602f,    0.4004672082940195f,   -0.5029860367700724f,   -0.15296486218853164f,
             0.6740059517812944f,    0.3239847771997537f,   -0.5794684678643381f,    0.3239847771997537f,
             0.5794684678643381f,   -0.3239847771997537f,   -0.3239847771997537f,   -0.6740059517812944f,
             0.5029860367700724f,   -0.4004672082940195f,    0.15296486218853164f,  -0.7504883828755602f,
             0.5029860367700724f,    0.15296486218853164f,  -0.4004672082940195f,   -0.7504883828755602f,
             0.4553054119602712f,    0.08164729285680945f,   0.08164729285680945f,  -0.8828161875373585f,
             0.8828161875373585f,   -0.08164729285680945f,  -0.08164729285680945f,  -0.4553054119602712f,
             0.7504883828755602f,   -0.15296486218853164f,   0.4004672082940195f,   -0.5029860367700724f,
             0.7504883828755602f,    0.4004672082940195f,   -0.15296486218853164f,  -0.5029860367700724f,
             0.6740059517812944f,    0.3239847771997537f,    0.3239847771997537f,   -0.5794684678643381f,
             0.03381941603233842f,   0.03381941603233842f,   0.03381941603233842f,   0.9982828964265062f,
            -0.044802370851755174f, -0.044802370851755174f,  0.508629699630796f,     0.8586508742123365f,
            -0.044802370851755174f,  0.508629699630796f,    -0.044802370851755174f,  0.8586508742123365f,
            -0.12128480194602098f,   0.4321472685365301f,    0.4321472685365301f,    0.7821684431180708f,
             0.508629699630796f,    -0.044802370851755174f, -0.044802370851755174f,  0.8586508742123365f,
             0.4321472685365301f,   -0.12128480194602098f,   0.4321472685365301f,    0.7821684431180708f,
             0.4321472685365301f,    0.4321472685365301f,   -0.12128480194602098f,   0.7821684431180708f,
             0.37968289875261624f,   0.37968289875261624f,   0.37968289875261624f,   0.753341017856078f,
             0.03381941603233842f,   0.03381941603233842f,   0.9982828964265062f,    0.03381941603233842f,
            -0.044802370851755174f,  0.044802370851755174f,  0.8586508742123365f,    0.508629699630796f,
            -0.044802370851755174f,  0.508629699630796f,     0.8586508742123365f,   -0.044802370851755174f,
            -0.12128480194602098f,   0.4321472685365301f,    0.7821684431180708f,    0.4321472685365301f,
             0.508629699630796f,    -0.044802370851755174f,  0.8586508742123365f,   -0.044802370851755174f,
             0.4321472685365301f,   -0.12128480194602098f,   0.7821684431180708f,    0.4321472685365301f,
             0.4321472685365301f,    0.4321472685365301f,    0.7821684431180708f,   -0.12128480194602098f,
             0.37968289875261624f,   0.37968289875261624f,   0.753341017856078f,     0.37968289875261624f,
             0.03381941603233842f,   0.9982828964265062f,    0.03381941603233842f,   0.03381941603233842f,
            -0.044802370851755174f,  0.8586508742123365f,   -0.044802370851755174f,  0.508629699630796f,
            -0.044802370851755174f,  0.8586508742123365f,    0.508629699630796f,    -0.044802370851755174f,
            -0.12128480194602098f,   0.7821684431180708f,    0.4321472685365301f,    0.4321472685365301f,
             0.508629699630796f,     0.8586508742123365f,   -0.044802370851755174f, -0.044802370851755174f,
             0.4321472685365301f,    0.7821684431180708f,   -0.12128480194602098f,   0.4321472685365301f,
             0.4321472685365301f,    0.7821684431180708f,    0.4321472685365301f,   -0.12128480194602098f,
             0.37968289875261624f,   0.753341017856078f,     0.37968289875261624f,   0.37968289875261624f,
             0.9982828964265062f,    0.03381941603233842f,   0.03381941603233842f,   0.03381941603233842f,
             0.8586508742123365f,   -0.044802370851755174f, -0.044802370851755174f,  0.508629699630796f,
             0.8586508742123365f,   -0.044802370851755174f,  0.508629699630796f,    -0.044802370851755174f,
             0.7821684431180708f,   -0.12128480194602098f,   0.4321472685365301f,    0.4321472685365301f,
             0.8586508742123365f,    0.508629699630796f,    -0.044802370851755174f, -0.044802370851755174f,
             0.7821684431180708f,    0.4321472685365301f,   -0.12128480194602098f,   0.4321472685365301f,
             0.7821684431180708f,    0.4321472685365301f,    0.4321472685365301f,   -0.12128480194602098f,
             0.753341017856078f,     0.37968289875261624f,   0.37968289875261624f,   0.37968289875261624f,
        };
        for (int i = 0; i < grad4.Length; i++)
        {
            grad4[i] = (float)(grad4[i] / Normalizer4D);
        }
        for (int i = 0, j = 0; i < Gradients4D.Length; i++, j++)
        {
            if (j == grad4.Length) j = 0;
            Gradients4D[i] = grad4[j];
        }
    }
}