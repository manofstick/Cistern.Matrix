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
        public Span<Vector<T>> aRowV;

        public Span<T> bColumn;

        public Span<Vector<T>> result;
    }

    private static Vector<T> Zero => Vector<T>.Zero;

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private unsafe static void InnerLoop(Args args)
    {
        Vector<T> r0 = Zero,  r1 = Zero,  r2 = Zero, r3 = Zero;

        var count = args.aRowV.Length;

        fixed (Vector<T>* aRowV = args.aRowV)
        fixed (T* bColumn = args.bColumn)
        for(var i=0; i < count; ++i)
        {
            var r = aRowV[i];

            var c0 = new Vector<T>(bColumn[(i*Vector<T>.Count)+0]);
            r0 += r * c0;

            var c1 = new Vector<T>(bColumn[(i*Vector<T>.Count)+1]);
            r1 += r * c1;

            var c2 = new Vector<T>(bColumn[(i*Vector<T>.Count)+2]);
            r2 += r * c2;

            var c3 = new Vector<T>(bColumn[(i*Vector<T>.Count)+3]);
            r3 += r * c3;
        }

        args.result[0] = r0;
        args.result[1] = r1;
        args.result[2] = r2;
        args.result[3] = r3;
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

    public static T[][] Multiply(T[][] A, T[][] B)
    {
        var a = StripedA(A);
        var b = StripedB(B);

        var stripedBounds = GetStripedBounds(a, b);

        var results = new T[stripedBounds.L][];
        for(var i=0; i < results.Length; ++i)
            results[i] = new T[stripedBounds.M];

        var args = new Args();
        for(var row=0; row < a.Length; ++row)
        {
            var aRow = a[row].AsSpan();
            var aRowV = MemoryMarshal.Cast<T, Vector<T>>(aRow);

            var rRow = results[row].AsSpan();
            var rRowV = MemoryMarshal.Cast<T, Vector<T>>(rRow);

            args.aRowV = aRowV;

            for (var column=0; column < b.Length; ++column)
            {
                args.bColumn = b[column].AsSpan();

                var resultIdx = (column*Vector<T>.Count);
                args.result = rRowV[resultIdx..(resultIdx+4)];

                InnerLoop(args);
            }
        }

        var bounds = GetBounds(A, B);

        var reshaped = Reshape(results, bounds.L, bounds.N);

        return reshaped;
    }
}
