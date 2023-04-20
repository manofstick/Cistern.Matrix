using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Cistern.Matrix;

public class MultiplicationByStripe<T>
    where T : unmanaged, INumber<T>
{
    private static T[][] StripedA(T[][] A)
    {
        var rows = A.Length;
        var columns = A[0].Length;

        var stripedRows = (rows + Vector<T>.Count - 1) / Vector<T>.Count;
        var stripedColumns = ((columns * Vector<T>.Count) + Vector<T>.Count - 1) / Vector<T>.Count * Vector<T>.Count;

        var striped = new T[stripedRows][];
        for (var stripedRowIdx = 0; stripedRowIdx < stripedRows; ++stripedRowIdx)
        {
            var rowIdx = stripedRowIdx * Vector<T>.Count;
            var stripedRow = new T[stripedColumns];
            striped[stripedRowIdx] = stripedRow;
            for (var columnIdx = 0; columnIdx < columns; ++columnIdx)
            {
                for (var i = 0; i < Vector<T>.Count; ++i)
                {
                    if (rowIdx + i < rows)
                        stripedRow[(columnIdx * Vector<T>.Count) + i] = A[rowIdx + i][columnIdx];
                }
            }
        }
        return striped;
    }

    private static T[][] StripedB(T[][] B)
    {
        var rows = B.Length;
        var columns = B[0].Length;

        var stripedRows = (columns + Vector<T>.Count - 1) / Vector<T>.Count;
        var stripedColumns = ((rows * Vector<T>.Count) + Vector<T>.Count - 1) / Vector<T>.Count * Vector<T>.Count;

        var striped = new T[stripedRows][];
        for (var stripedRowIdx = 0; stripedRowIdx < stripedRows; ++stripedRowIdx)
        {
            var columnIdx = stripedRowIdx * Vector<T>.Count;
            var stripedRow = new T[stripedColumns];
            striped[stripedRowIdx] = stripedRow;
            for (var rowIdx = 0; rowIdx < rows; ++rowIdx)
            {
                for (var i = 0; i < Vector<T>.Count; ++i)
                {
                    if (columnIdx + i < columns)
                        stripedRow[(rowIdx * Vector<T>.Count) + i] = B[rowIdx][columnIdx + i];
                }
            }
        }
        return striped;
    }

    static (int L, int M, int N) GetBounds(T[][] A, T[][] B)
    {
        var l = A.Length;
        var m0 = A[0].Length;
        var m1 = B.Length;
        var n = B[0].Length;

        if (m0 != m1)
            throw new ArgumentException("A columns != B rows");

        return (l, m0, n);
    }

    static (int L, int M, int N) GetStripedBounds(T[][] A, T[][] B)
    {
        var l = A.Length;
        var m0 = A[0].Length;
        var m1 = B[0].Length;
        var n = B.Length;

        if (m0 != m1)
            throw new ArgumentException("A columns != B rows");

        return (l, m0, n);
    }

    ref struct Args
    {
        public const int RowBatch = 4;

        public Span<Vector<T>> BatchOfRows;

        public Span<T> Column0;
        public Span<T> Column1;

        public Span<Vector<T>> Result00;
        public Span<Vector<T>> Result10;
        public Span<Vector<T>> Result20;
        public Span<Vector<T>> Result30;
        public Span<Vector<T>> Result01;
        public Span<Vector<T>> Result11;
        public Span<Vector<T>> Result21;
        public Span<Vector<T>> Result31;
    }

    private static Vector<T> Zero => Vector<T>.Zero;

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private unsafe static void InnerLoop(Args args)
    {
        Vector<T> v00_0 = Zero, v00_1 = Zero, v00_2 = Zero, v00_3 = Zero;
        Vector<T> v10_0 = Zero, v10_1 = Zero, v10_2 = Zero, v10_3 = Zero;
        Vector<T> v20_0 = Zero, v20_1 = Zero, v20_2 = Zero, v20_3 = Zero;
        Vector<T> v30_0 = Zero, v30_1 = Zero, v30_2 = Zero, v30_3 = Zero;
        Vector<T> v01_0 = Zero, v01_1 = Zero, v01_2 = Zero, v01_3 = Zero;
        Vector<T> v11_0 = Zero, v11_1 = Zero, v11_2 = Zero, v11_3 = Zero;
        Vector<T> v21_0 = Zero, v21_1 = Zero, v21_2 = Zero, v21_3 = Zero;
        Vector<T> v31_0 = Zero, v31_1 = Zero, v31_2 = Zero, v31_3 = Zero;

        var count = args.BatchOfRows.Length;

        fixed (Vector<T>* R = args.BatchOfRows)
        fixed (T* C0 = args.Column0, C1 = args.Column1)
        for(var i=0; i < count; i += Args.RowBatch)
        {
            var offset = i / Args.RowBatch * Vector<T>.Count;

            v00_0 += R[i+0] * C0[offset+0];
            v10_0 += R[i+1] * C0[offset+0];
            v20_0 += R[i+2] * C0[offset+0];
            v30_0 += R[i+3] * C0[offset+0];

            v00_1 += R[i+0] * C0[offset+1];
            v10_1 += R[i+1] * C0[offset+1];
            v20_1 += R[i+2] * C0[offset+1];
            v30_1 += R[i+3] * C0[offset+1];

            v00_2 += R[i+0] * C0[offset+2];
            v10_2 += R[i+1] * C0[offset+2];
            v20_2 += R[i+2] * C0[offset+2];
            v30_2 += R[i+3] * C0[offset+2];

            v00_3 += R[i+0] * C0[offset+3];
            v10_3 += R[i+1] * C0[offset+3];
            v20_3 += R[i+2] * C0[offset+3];
            v30_3 += R[i+3] * C0[offset+3];

            v01_0 += R[i+0] * C1[offset+0];
            v11_0 += R[i+1] * C1[offset+0];
            v21_0 += R[i+2] * C1[offset+0];
            v31_0 += R[i+3] * C1[offset+0];

            v01_1 += R[i+0] * C1[offset+1];
            v11_1 += R[i+1] * C1[offset+1];
            v21_1 += R[i+2] * C1[offset+1];
            v31_1 += R[i+3] * C1[offset+1];

            v01_2 += R[i+0] * C1[offset+2];
            v11_2 += R[i+1] * C1[offset+2];
            v21_2 += R[i+2] * C1[offset+2];
            v31_2 += R[i+3] * C1[offset+2];

            v01_3 += R[i+0] * C1[offset+3];
            v11_3 += R[i+1] * C1[offset+3];
            v21_3 += R[i+2] * C1[offset+3];
            v31_3 += R[i+3] * C1[offset+3];
        }

        args.Result00[0] = v00_0;
        args.Result00[1] = v00_1;
        args.Result00[2] = v00_2;
        args.Result00[3] = v00_3;

        args.Result10[0] = v10_0;
        args.Result10[1] = v10_1;
        args.Result10[2] = v10_2;
        args.Result10[3] = v10_3;

        args.Result20[0] = v20_0;
        args.Result20[1] = v20_1;
        args.Result20[2] = v20_2;
        args.Result20[3] = v20_3;

        args.Result30[0] = v30_0;
        args.Result30[1] = v30_1;
        args.Result30[2] = v30_2;
        args.Result30[3] = v30_3;

        args.Result01[0] = v01_0;
        args.Result01[1] = v01_1;
        args.Result01[2] = v01_2;
        args.Result01[3] = v01_3;

        args.Result11[0] = v11_0;
        args.Result11[1] = v11_1;
        args.Result11[2] = v11_2;
        args.Result11[3] = v11_3;

        args.Result21[0] = v21_0;
        args.Result21[1] = v21_1;
        args.Result21[2] = v21_2;
        args.Result21[3] = v21_3;

        args.Result31[0] = v31_0;
        args.Result31[1] = v31_1;
        args.Result31[2] = v31_2;
        args.Result31[3] = v31_3;
    }

    // [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    // private unsafe static void InnerLoop(Args args)
    // {
    //     Vector<T> v00_0 = Zero, v00_1 = Zero, v00_2 = Zero, v00_3 = Zero;
    //     Vector<T> v10_0 = Zero, v10_1 = Zero, v10_2 = Zero, v10_3 = Zero;
    //     Vector<T> v20_0 = Zero, v20_1 = Zero, v20_2 = Zero, v20_3 = Zero;
    //     Vector<T> v30_0 = Zero, v30_1 = Zero, v30_2 = Zero, v30_3 = Zero;
    //     Vector<T> v01_0 = Zero, v01_1 = Zero, v01_2 = Zero, v01_3 = Zero;
    //     Vector<T> v11_0 = Zero, v11_1 = Zero, v11_2 = Zero, v11_3 = Zero;
    //     Vector<T> v21_0 = Zero, v21_1 = Zero, v21_2 = Zero, v21_3 = Zero;
    //     Vector<T> v31_0 = Zero, v31_1 = Zero, v31_2 = Zero, v31_3 = Zero;

    //     var count = args.BatchOfRows.Length;

    //     fixed (Vector<T>* R = args.BatchOfRows)
    //     fixed (T* C0 = args.Column0, C1 = args.Column1)
    //     for(var i=0; i < count; i += Args.RowBatch)
    //     {
    //         Vector<T> r0 = R[i+0], r1 = R[i+1], r2 = R[i+2], r3 = R[i+3];
            
    //         var offset = i / Args.RowBatch * Vector<T>.Count;

    //         var c0_0 = new Vector<T>(C0[offset+0]);
    //         var c0_1 = new Vector<T>(C0[offset+1]);
    //         var c0_2 = new Vector<T>(C0[offset+2]);
    //         var c0_3 = new Vector<T>(C0[offset+3]);
    //         var c1_0 = new Vector<T>(C1[offset+0]);
    //         var c1_1 = new Vector<T>(C1[offset+1]);
    //         var c1_2 = new Vector<T>(C1[offset+2]);
    //         var c1_3 = new Vector<T>(C1[offset+3]);

    //         v00_0 += r0 * c0_0;
    //         v10_0 += r1 * c0_0;
    //         v20_0 += r2 * c0_0;
    //         v30_0 += r3 * c0_0;

    //         v00_1 += r0 * c0_1;
    //         v10_1 += r1 * c0_1;
    //         v20_1 += r2 * c0_1;
    //         v30_1 += r3 * c0_1;

    //         v00_2 += r0 * c0_2;
    //         v10_2 += r1 * c0_2;
    //         v20_2 += r2 * c0_2;
    //         v30_2 += r3 * c0_2;

    //         v00_3 += r0 * c0_3;
    //         v10_3 += r1 * c0_3;
    //         v20_3 += r2 * c0_3;
    //         v30_3 += r3 * c0_3;

    //         v01_0 += r0 * c1_0;
    //         v11_0 += r1 * c1_0;
    //         v21_0 += r2 * c1_0;
    //         v31_0 += r3 * c1_0;

    //         v01_1 += r0 * c1_1;
    //         v11_1 += r1 * c1_1;
    //         v21_1 += r2 * c1_1;
    //         v31_1 += r3 * c1_1;

    //         v01_2 += r0 * c1_2;
    //         v11_2 += r1 * c1_2;
    //         v21_2 += r2 * c1_2;
    //         v31_2 += r3 * c1_2;

    //         v01_3 += r0 * c1_3;
    //         v11_3 += r1 * c1_3;
    //         v21_3 += r2 * c1_3;
    //         v31_3 += r3 * c1_3;
    //     }

    //     args.Result00[0] = v00_0;
    //     args.Result00[1] = v00_1;
    //     args.Result00[2] = v00_2;
    //     args.Result00[3] = v00_3;

    //     args.Result10[0] = v10_0;
    //     args.Result10[1] = v10_1;
    //     args.Result10[2] = v10_2;
    //     args.Result10[3] = v10_3;

    //     args.Result20[0] = v20_0;
    //     args.Result20[1] = v20_1;
    //     args.Result20[2] = v20_2;
    //     args.Result20[3] = v20_3;

    //     args.Result30[0] = v30_0;
    //     args.Result30[1] = v30_1;
    //     args.Result30[2] = v30_2;
    //     args.Result30[3] = v30_3;

    //     args.Result01[0] = v01_0;
    //     args.Result01[1] = v01_1;
    //     args.Result01[2] = v01_2;
    //     args.Result01[3] = v01_3;

    //     args.Result11[0] = v11_0;
    //     args.Result11[1] = v11_1;
    //     args.Result11[2] = v11_2;
    //     args.Result11[3] = v11_3;

    //     args.Result21[0] = v21_0;
    //     args.Result21[1] = v21_1;
    //     args.Result21[2] = v21_2;
    //     args.Result21[3] = v21_3;

    //     args.Result31[0] = v31_0;
    //     args.Result31[1] = v31_1;
    //     args.Result31[2] = v31_2;
    //     args.Result31[3] = v31_3;
    // }




    public static T[][] Reshape(T[][] striped, int rows, int columns)
    {
        var results = new T[rows][];
        for(var rowIdx=0; rowIdx < results.Length; ++rowIdx)
            results[rowIdx] = new T[columns];

        for (var rowIdx=0; rowIdx < striped.Length; ++rowIdx)
        {
            for (var columnIdx=0; columnIdx < striped[0].Length; ++columnIdx)
            {
                var column = columnIdx / Vector<T>.Count;
                var rowffset = columnIdx % Vector<T>.Count;

                results[rowIdx*Vector<T>.Count+rowffset][column] = striped[rowIdx][columnIdx];
            }
        }

        return results;
    }

    static private readonly int ProcessorCount = Environment.ProcessorCount;

    static private int RoundBy(int value, int rounding) => ((value + rounding - 1) / rounding) * rounding;  
    

    static private Vector<T>[][] BatchByStride(T[][] t, int stride)
    {
        var rows = (t.Length + stride - 1) / stride;
        var columns = t[0].Length / Vector<T>.Count * stride; 

        var result = new Vector<T>[rows][];
        for (var i=0; i < result.Length; ++i)
            result[i] = new Vector<T>[columns];
        
        for (var i=0; i < t.Length; ++i)
        {
            var rrow = result[i/stride];
            var offset = i % stride;
            var row = MemoryMarshal.Cast<T, Vector<T>>(t[i]);
            for (var j=0; j < row.Length; ++j)
            {
                rrow[j*stride+offset] = row[j];
            }
        }

        return result;
    }

    public static T[][] Multiply(T[][] A, T[][] B)
    {
        var a = StripedA(A);
        var b = StripedB(B);

        var stripedBounds = GetStripedBounds(a, b);

        var results = new T[stripedBounds.L][];
        for(var i=0; i < results.Length; ++i)
            results[i] = new T[stripedBounds.M];

        var batchedA = BatchByStride(a, Args.RowBatch);

        var rows = batchedA.Length;

        var rowsPerIteration = 1;
        var rowIterationsCount = (rows+rowsPerIteration-1) / rowsPerIteration;
        var rowsBatchesPerIteration = (rowIterationsCount+ProcessorCount-1) / ProcessorCount;
        var batchesCount = (rowIterationsCount+rowsBatchesPerIteration-1) / rowsBatchesPerIteration;
        
        var options = new ParallelOptions();
        //options.MaxDegreeOfParallelism = 1;
        Parallel.For(0, batchesCount, options, parallelForIdx => {
            var startIdx = parallelForIdx * rowsBatchesPerIteration * rowsPerIteration;
            var endIdx = Math.Min(rows, (parallelForIdx+1) * rowsBatchesPerIteration * rowsPerIteration);

            var row = startIdx;
            var args = new Args();
            for (; row < endIdx - rowsPerIteration + 1; row += rowsPerIteration)
            {
                args.BatchOfRows = batchedA[row];

                var rRowV0 = MemoryMarshal.Cast<T, Vector<T>>(results[(row*Args.RowBatch)+0]);
                var rRowV1 = MemoryMarshal.Cast<T, Vector<T>>(results[(row*Args.RowBatch)+1]);
                var rRowV2 = MemoryMarshal.Cast<T, Vector<T>>(results[(row*Args.RowBatch)+2]);
                var rRowV3 = MemoryMarshal.Cast<T, Vector<T>>(results[(row*Args.RowBatch)+3]);

                for (var column=0; column < b.Length-2+1; column += 2)
                {
                    args.Column0 = b[column+0].AsSpan();
                    args.Column1 = b[column+1].AsSpan();

                    var result0Idx = ((column+0)*Vector<T>.Count);
                    args.Result00 = rRowV0[result0Idx..(result0Idx+Vector<T>.Count)];
                    args.Result10 = rRowV1[result0Idx..(result0Idx+Vector<T>.Count)];
                    args.Result20 = rRowV2[result0Idx..(result0Idx+Vector<T>.Count)];
                    args.Result30 = rRowV3[result0Idx..(result0Idx+Vector<T>.Count)];

                    var result1Idx = ((column+1)*Vector<T>.Count);
                    args.Result01 = rRowV0[result1Idx..(result1Idx+Vector<T>.Count)];
                    args.Result11 = rRowV1[result1Idx..(result1Idx+Vector<T>.Count)];
                    args.Result21 = rRowV2[result1Idx..(result1Idx+Vector<T>.Count)];
                    args.Result31 = rRowV3[result1Idx..(result1Idx+Vector<T>.Count)];

                    InnerLoop(args);
                }
            }
        });

        var bounds = GetBounds(A, B);

        var reshaped = Reshape(results, bounds.L, bounds.N);

        return reshaped;
    }
}
