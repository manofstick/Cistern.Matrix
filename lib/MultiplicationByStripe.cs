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
        public Span<Vector<T>> Row0;
        public Span<Vector<T>> Row1;

        public Span<T> Column0;

        public Span<Vector<T>> Result0;
        public Span<Vector<T>> Result1;
    }

    private static Vector<T> Zero => Vector<T>.Zero;

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private unsafe static void InnerLoop(Args args)
    {
        Vector<T> r0_0 = Zero, r0_1 = Zero, r0_2 = Zero, r0_3 = Zero;
        Vector<T> r1_0 = Zero, r1_1 = Zero, r1_2 = Zero, r1_3 = Zero;

        var count = args.Row0.Length;

        fixed (Vector<T>* aRowV0 = args.Row0, aRowV1 = args.Row1)
        fixed (T* bColumn = args.Column0)
        for(var i=0; i < count; ++i)
        {
            r0_0 += aRowV0[i] * bColumn[(i*Vector<T>.Count)+0];
            r1_0 += aRowV1[i] * bColumn[(i*Vector<T>.Count)+0];
            r0_1 += aRowV0[i] * bColumn[(i*Vector<T>.Count)+1];
            r1_1 += aRowV1[i] * bColumn[(i*Vector<T>.Count)+1];
            r0_2 += aRowV0[i] * bColumn[(i*Vector<T>.Count)+2];
            r1_2 += aRowV1[i] * bColumn[(i*Vector<T>.Count)+2];
            r0_3 += aRowV0[i] * bColumn[(i*Vector<T>.Count)+3];
            r1_3 += aRowV1[i] * bColumn[(i*Vector<T>.Count)+3];
        }

        args.Result0[0] = r0_0;
        args.Result0[1] = r0_1;
        args.Result0[2] = r0_2;
        args.Result0[3] = r0_3;

        args.Result1[0] = r1_0;
        args.Result1[1] = r1_1;
        args.Result1[2] = r1_2;
        args.Result1[3] = r1_3;
    }

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

    public static T[][] Multiply(T[][] A, T[][] B)
    {
        var a = StripedA(A);
        var b = StripedB(B);

        var stripedBounds = GetStripedBounds(a, b);

        var results = new T[stripedBounds.L][];
        for(var i=0; i < results.Length; ++i)
            results[i] = new T[stripedBounds.M];

        var rows = a.Length;

        var rowsPerIteration = 2;
        var rowIterationsCount = (rows+rowsPerIteration-1) / rowsPerIteration;
        var rowsBatchesPerIteration = (rowIterationsCount+ProcessorCount-1) / ProcessorCount;
        var batchesCount = (rowIterationsCount+rowsBatchesPerIteration-1) / rowsBatchesPerIteration;
        
        var options = new ParallelOptions();
//        options.MaxDegreeOfParallelism = 1;
        Parallel.For(0, batchesCount, options, parallelForIdx => {
            var startIdx = parallelForIdx * rowsBatchesPerIteration * rowsPerIteration;
            var endIdx = Math.Min(rows, (parallelForIdx+1) * rowsBatchesPerIteration * rowsPerIteration);

            var row = startIdx;
            var args = new Args();
            for (; row < endIdx - rowsPerIteration + 1; row += rowsPerIteration)
            {
                var aRow0 = a[row+0].AsSpan();
                var aRow1 = a[row+1].AsSpan();
                args.Row0 = MemoryMarshal.Cast<T, Vector<T>>(aRow0);
                args.Row1 = MemoryMarshal.Cast<T, Vector<T>>(aRow1);

                var rRow0 = results[row+0].AsSpan();
                var rRow1 = results[row+1].AsSpan();
                var rRowV0 = MemoryMarshal.Cast<T, Vector<T>>(rRow0);
                var rRowV1 = MemoryMarshal.Cast<T, Vector<T>>(rRow1);

                for (var column=0; column < b.Length; ++column)
                {
                    args.Column0 = b[column].AsSpan();

                    var resultIdx = (column*Vector<T>.Count);
                    args.Result0 = rRowV0[resultIdx..(resultIdx+4)];
                    args.Result1 = rRowV1[resultIdx..(resultIdx+4)];

                    InnerLoop(args);
                }
            }
        });

        var bounds = GetBounds(A, B);

        var reshaped = Reshape(results, bounds.L, bounds.N);

        return reshaped;
    }
}
