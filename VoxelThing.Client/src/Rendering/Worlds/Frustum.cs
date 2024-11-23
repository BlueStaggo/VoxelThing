/*
 * The MIT License
 *
 * Copyright (c) 2015-2024 Kai Burjack
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

// In other words, I couldn't get a proper Frustum implementation working on my own,
// so I just copied JOML's implementation which is what the Java version of Voxel Thing used.

using OpenTK.Mathematics;
using VoxelThing.Game.Maths;

namespace VoxelThing.Client.Rendering.Worlds;

public readonly struct Frustum
{
    private readonly float nxX, nxY, nxZ, nxW;
    private readonly float pxX, pxY, pxZ, pxW;
    private readonly float nyX, nyY, nyZ, nyW;
    private readonly float pyX, pyY, pyZ, pyW;
    private readonly float nzX, nzY, nzZ, nzW;
    private readonly float pzX, pzY, pzZ, pzW;

    private readonly Vector4[] planes = new Vector4[6];

    public Frustum(Matrix4 m)
    {
        nxX = m.M14 + m.M11; nxY = m.M24 + m.M21; nxZ = m.M34 + m.M31; nxW = m.M44 + m.M41;
        planes[0] = new(nxX, nxY, nxZ, nxW);
        pxX = m.M14 - m.M11; pxY = m.M24 - m.M21; pxZ = m.M34 - m.M31; pxW = m.M44 - m.M41;
        planes[1] = new(pxX, pxY, pxZ, pxW);
        nyX = m.M14 + m.M12; nyY = m.M24 + m.M22; nyZ = m.M34 + m.M32; nyW = m.M44 + m.M42;
        planes[2] = new(nyX, nyY, nyZ, nyW);
        pyX = m.M14 - m.M12; pyY = m.M24 - m.M22; pyZ = m.M34 - m.M32; pyW = m.M44 - m.M42;
        planes[3] = new(pyX, pyY, pyZ, pyW);
        nzX = m.M14 + m.M13; nzY = m.M24 + m.M23; nzZ = m.M34 + m.M33; nzW = m.M44 + m.M43;
        planes[4] = new(nzX, nzY, nzZ, nzW);
        pzX = m.M14 - m.M13; pzY = m.M24 - m.M23; pzZ = m.M34 - m.M33; pzW = m.M44 - m.M43;
        planes[5] = new(pzX, pzY, pzZ, pzW);
    }

    public bool TestPoint(Vector3 point) => TestPoint(point.X, point.Y, point.Z);
    
    public bool TestPoint(float x, float y, float z)
        =>  nxX * x + nxY * y + nxZ * z + nxW >= 0 &&
            pxX * x + pxY * y + pxZ * z + pxW >= 0 &&
            nyX * x + nyY * y + nyZ * z + nyW >= 0 &&
            pyX * x + pyY * y + pyZ * z + pyW >= 0 &&
            nzX * x + nzY * y + nzZ * z + nzW >= 0 &&
            pzX * x + pzY * y + pzZ * z + pzW >= 0;

    public bool TestAabb(Aabb aabb)
        =>  nxX * (nxX < 0 ? aabb.MinX : aabb.MaxX)
            + nxY * (nxY < 0 ? aabb.MinY : aabb.MaxY)
            + nxZ * (nxZ < 0 ? aabb.MinZ : aabb.MaxZ) >= -nxW &&
            pxX * (pxX < 0 ? aabb.MinX : aabb.MaxX)
            + pxY * (pxY < 0 ? aabb.MinY : aabb.MaxY)
            + pxZ * (pxZ < 0 ? aabb.MinZ : aabb.MaxZ) >= -pxW &&
            nyX * (nyX < 0 ? aabb.MinX : aabb.MaxX)
            + nyY * (nyY < 0 ? aabb.MinY : aabb.MaxY)
            + nyZ * (nyZ < 0 ? aabb.MinZ : aabb.MaxZ) >= -nyW &&
            pyX * (pyX < 0 ? aabb.MinX : aabb.MaxX)
            + pyY * (pyY < 0 ? aabb.MinY : aabb.MaxY)
            + pyZ * (pyZ < 0 ? aabb.MinZ : aabb.MaxZ) >= -pyW &&
            nzX * (nzX < 0 ? aabb.MinX : aabb.MaxX)
            + nzY * (nzY < 0 ? aabb.MinY : aabb.MaxY)
            + nzZ * (nzZ < 0 ? aabb.MinZ : aabb.MaxZ) >= -nzW &&
            pzX * (pzX < 0 ? aabb.MinX : aabb.MaxX)
            + pzY * (pzY < 0 ? aabb.MinY : aabb.MaxY)
            + pzZ * (pzZ < 0 ? aabb.MinZ : aabb.MaxZ) >= -pzW;

}
