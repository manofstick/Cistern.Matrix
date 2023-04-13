using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace Cistern.Matrix;

public class Multiplication<T>
    where T : struct, INumber<T>
{
    public struct Tmp
    {
        public T tmp0_0; public T tmp0_1; public T tmp0_2; public T tmp0_3; public T tmp0_4; public T tmp0_5;
        public T tmp1_0; public T tmp1_1; public T tmp1_2; public T tmp1_3; public T tmp1_4; public T tmp1_5;
        public T tmp2_0; public T tmp2_1; public T tmp2_2; public T tmp2_3; public T tmp2_4; public T tmp2_5;
        public T tmp3_0; public T tmp3_1; public T tmp3_2; public T tmp3_3; public T tmp3_4; public T tmp3_5;
        public T tmp4_0; public T tmp4_1; public T tmp4_2; public T tmp4_3; public T tmp4_4; public T tmp4_5;
        public T tmp5_0; public T tmp5_1; public T tmp5_2; public T tmp5_3; public T tmp5_4; public T tmp5_5;
    }

    ref struct Args
    {
        public Span<Vector256<T>> ARowVector0;
        public Span<Vector256<T>> ARowVector1;
        public Span<Vector256<T>> ARowVector2;
        public Span<Vector256<T>> ARowVector3;
        public Span<Vector256<T>> ARowVector4; 
        public Span<Vector256<T>> ARowVector5; 

        public Span<Vector256<T>> BTRowVector0; 
        public Span<Vector256<T>> BTRowVector1;
        public Span<Vector256<T>> BTRowVector2;
        public Span<Vector256<T>> BTRowVector3; 
        public Span<Vector256<T>> BTRowVector4;
        public Span<Vector256<T>> BTRowVector5;
    }


    static private readonly Vector256<T> Zero = Vector256<T>.Zero;
    static private readonly int ProcessorCount = Environment.ProcessorCount;

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    unsafe private static void InnerLoop(Args args, out Tmp tmp)
    {
        var length = args.ARowVector0.Length;

        Vector256<T> V0_0 = Zero, V0_1 = Zero, V0_2 = Zero, V0_3 = Zero, V0_4 = Zero, V0_5 = Zero;
        Vector256<T> V1_0 = Zero, V1_1 = Zero, V1_2 = Zero, V1_3 = Zero, V1_4 = Zero, V1_5 = Zero;
        Vector256<T> V2_0 = Zero, V2_1 = Zero, V2_2 = Zero, V2_3 = Zero, V2_4 = Zero, V2_5 = Zero;
        Vector256<T> V3_0 = Zero, V3_1 = Zero, V3_2 = Zero, V3_3 = Zero, V3_4 = Zero, V3_5 = Zero;
        Vector256<T> V4_0 = Zero, V4_1 = Zero, V4_2 = Zero, V4_3 = Zero, V4_4 = Zero, V4_5 = Zero;
        Vector256<T> V5_0 = Zero, V5_1 = Zero, V5_2 = Zero, V5_3 = Zero, V5_4 = Zero, V5_5 = Zero;

        fixed (Vector256<T>* A0=args.ARowVector0,  A1=args.ARowVector1,  A2=args.ARowVector2,  A3=args.ARowVector3,  A4=args.ARowVector4,  A5=args.ARowVector5,
                             B0=args.BTRowVector0, B1=args.BTRowVector1, B2=args.BTRowVector2, B3=args.BTRowVector3, B4=args.BTRowVector4, B5=args.BTRowVector5)
        {
            Vector256<T> a;
            for (var i = length-1; i >= 0; --i)
            {
                a = A0[i]; V0_0 += a * B0[i]; V0_1 += a * B1[i]; V0_2 += a * B2[i]; V0_3 += a * B3[i]; V0_4 += a * B4[i]; V0_5 += a * B5[i];
                a = A1[i]; V1_0 += a * B0[i]; V1_1 += a * B1[i]; V1_2 += a * B2[i]; V1_3 += a * B3[i]; V1_4 += a * B4[i]; V1_5 += a * B5[i];
                a = A2[i]; V2_0 += a * B0[i]; V2_1 += a * B1[i]; V2_2 += a * B2[i]; V2_3 += a * B3[i]; V2_4 += a * B4[i]; V2_5 += a * B5[i];
                a = A3[i]; V3_0 += a * B0[i]; V3_1 += a * B1[i]; V3_2 += a * B2[i]; V3_3 += a * B3[i]; V3_4 += a * B4[i]; V3_5 += a * B5[i];
                a = A4[i]; V4_0 += a * B0[i]; V4_1 += a * B1[i]; V4_2 += a * B2[i]; V4_3 += a * B3[i]; V4_4 += a * B4[i]; V4_5 += a * B5[i];
                a = A5[i]; V5_0 += a * B0[i]; V5_1 += a * B1[i]; V5_2 += a * B2[i]; V5_3 += a * B3[i]; V5_4 += a * B4[i]; V5_5 += a * B5[i];
            }
        }

        tmp.tmp0_0 = T.Zero; tmp.tmp0_1 = T.Zero; tmp.tmp0_2 = T.Zero; tmp.tmp0_3 = T.Zero; tmp.tmp0_4 = T.Zero; tmp.tmp0_5 = T.Zero;
        tmp.tmp1_0 = T.Zero; tmp.tmp1_1 = T.Zero; tmp.tmp1_2 = T.Zero; tmp.tmp1_3 = T.Zero; tmp.tmp1_4 = T.Zero; tmp.tmp1_5 = T.Zero;
        tmp.tmp2_0 = T.Zero; tmp.tmp2_1 = T.Zero; tmp.tmp2_2 = T.Zero; tmp.tmp2_3 = T.Zero; tmp.tmp2_4 = T.Zero; tmp.tmp2_5 = T.Zero;
        tmp.tmp3_0 = T.Zero; tmp.tmp3_1 = T.Zero; tmp.tmp3_2 = T.Zero; tmp.tmp3_3 = T.Zero; tmp.tmp3_4 = T.Zero; tmp.tmp3_5 = T.Zero;
        tmp.tmp4_0 = T.Zero; tmp.tmp4_1 = T.Zero; tmp.tmp4_2 = T.Zero; tmp.tmp4_3 = T.Zero; tmp.tmp4_4 = T.Zero; tmp.tmp4_5 = T.Zero;
        tmp.tmp5_0 = T.Zero; tmp.tmp5_1 = T.Zero; tmp.tmp5_2 = T.Zero; tmp.tmp5_3 = T.Zero; tmp.tmp5_4 = T.Zero; tmp.tmp5_5 = T.Zero;
        for (var i = 0; i < Vector256<T>.Count; ++i)
        {
            tmp.tmp0_0 += V0_0[i]; tmp.tmp0_1 += V0_1[i]; tmp.tmp0_2 += V0_2[i]; tmp.tmp0_3 += V0_3[i]; tmp.tmp0_4 += V0_4[i]; tmp.tmp0_5 += V0_5[i];
            tmp.tmp1_0 += V1_0[i]; tmp.tmp1_1 += V1_1[i]; tmp.tmp1_2 += V1_2[i]; tmp.tmp1_3 += V1_3[i]; tmp.tmp1_4 += V1_4[i]; tmp.tmp1_5 += V1_5[i];
            tmp.tmp2_0 += V2_0[i]; tmp.tmp2_1 += V2_1[i]; tmp.tmp2_2 += V2_2[i]; tmp.tmp2_3 += V2_3[i]; tmp.tmp2_4 += V2_4[i]; tmp.tmp2_5 += V2_5[i];
            tmp.tmp3_0 += V3_0[i]; tmp.tmp3_1 += V3_1[i]; tmp.tmp3_2 += V3_2[i]; tmp.tmp3_3 += V3_3[i]; tmp.tmp3_4 += V3_4[i]; tmp.tmp3_5 += V3_5[i];
            tmp.tmp4_0 += V4_0[i]; tmp.tmp4_1 += V4_1[i]; tmp.tmp4_2 += V4_2[i]; tmp.tmp4_3 += V4_3[i]; tmp.tmp4_4 += V4_4[i]; tmp.tmp4_5 += V4_5[i];
            tmp.tmp5_0 += V5_0[i]; tmp.tmp5_1 += V5_1[i]; tmp.tmp5_2 += V5_2[i]; tmp.tmp5_3 += V5_3[i]; tmp.tmp5_4 += V5_4[i]; tmp.tmp5_5 += V5_5[i];
        }
    }

    public static T[][] Multiply(T[][] A, T[][] B)
    {
        if (A[0].Length != B.Length)
            throw new ArgumentException("(A.Columns != B.Rows)");

        var BT = Utils<T>.Transpose(B);

        return MultiplyTransposedB(A, BT);
    }

    private static T[][] MultiplyTransposedB(T[][] A, T[][] BT)
    {
        var rows = A.Length;
        var columns = BT.Length;

        var result = Utils<T>.CreateZero(rows, columns);

        var rowsPerIteration = 6;
        var rowIterationsCount = (rows+rowsPerIteration-1) / rowsPerIteration;
        var rowsBatchesPerIteration = (rowIterationsCount+ProcessorCount-1) / ProcessorCount;
        var batchesCount = (rowIterationsCount+rowsBatchesPerIteration-1) / rowsBatchesPerIteration; 
        
        Parallel.For(0, batchesCount, parallelForIdx => {
            var startIdx = parallelForIdx * rowsBatchesPerIteration * rowsPerIteration;
            var endIdx = Math.Min(rows, (parallelForIdx+1) * rowsBatchesPerIteration * rowsPerIteration);

            var row = startIdx;
            for (; row < endIdx - rowsPerIteration + 1; row += rowsPerIteration)
            {
                Args args = new();
                var ARow0 = A[row + 0].AsSpan();
                var ARow1 = A[row + 1].AsSpan();
                var ARow2 = A[row + 2].AsSpan();
                var ARow3 = A[row + 3].AsSpan();
                var ARow4 = A[row + 4].AsSpan();
                var ARow5 = A[row + 5].AsSpan();
                args.ARowVector0 = MemoryMarshal.Cast<T, Vector256<T>>(ARow0);
                args.ARowVector1 = MemoryMarshal.Cast<T, Vector256<T>>(ARow1);
                args.ARowVector2 = MemoryMarshal.Cast<T, Vector256<T>>(ARow2);
                args.ARowVector3 = MemoryMarshal.Cast<T, Vector256<T>>(ARow3);
                args.ARowVector4 = MemoryMarshal.Cast<T, Vector256<T>>(ARow4);
                args.ARowVector5 = MemoryMarshal.Cast<T, Vector256<T>>(ARow5);
                var column = 0;
                for (; column < columns - 6 + 1; column += 6)
                {
                    var BTRow0 = BT[column + 0].AsSpan();
                    var BTRow1 = BT[column + 1].AsSpan();
                    var BTRow2 = BT[column + 2].AsSpan();
                    var BTRow3 = BT[column + 3].AsSpan();
                    var BTRow4 = BT[column + 4].AsSpan();
                    var BTRow5 = BT[column + 5].AsSpan();
                    args.BTRowVector0 = MemoryMarshal.Cast<T, Vector256<T>>(BTRow0);
                    args.BTRowVector1 = MemoryMarshal.Cast<T, Vector256<T>>(BTRow1);
                    args.BTRowVector2 = MemoryMarshal.Cast<T, Vector256<T>>(BTRow2);
                    args.BTRowVector3 = MemoryMarshal.Cast<T, Vector256<T>>(BTRow3);
                    args.BTRowVector4 = MemoryMarshal.Cast<T, Vector256<T>>(BTRow4);
                    args.BTRowVector5 = MemoryMarshal.Cast<T, Vector256<T>>(BTRow5);

                    InnerLoop(args, out var tmp);

                    for (var i = args.ARowVector0.Length * Vector256<T>.Count; i < ARow0.Length; ++i)
                    {
                        tmp.tmp0_0 += ARow0[i] * BTRow0[i]; tmp.tmp0_1 += ARow0[i] * BTRow1[i]; tmp.tmp0_2 += ARow0[i] * BTRow2[i]; tmp.tmp0_3 += ARow0[i] * BTRow3[i]; tmp.tmp0_4 += ARow0[i] * BTRow4[i]; tmp.tmp0_5 += ARow0[i] * BTRow5[i];
                        tmp.tmp1_0 += ARow1[i] * BTRow0[i]; tmp.tmp1_1 += ARow1[i] * BTRow1[i]; tmp.tmp1_2 += ARow1[i] * BTRow2[i]; tmp.tmp1_3 += ARow1[i] * BTRow3[i]; tmp.tmp1_4 += ARow1[i] * BTRow4[i]; tmp.tmp1_5 += ARow1[i] * BTRow5[i];
                        tmp.tmp2_0 += ARow2[i] * BTRow0[i]; tmp.tmp2_1 += ARow2[i] * BTRow1[i]; tmp.tmp2_2 += ARow2[i] * BTRow2[i]; tmp.tmp2_3 += ARow2[i] * BTRow3[i]; tmp.tmp2_4 += ARow2[i] * BTRow4[i]; tmp.tmp2_5 += ARow2[i] * BTRow5[i];
                        tmp.tmp3_0 += ARow3[i] * BTRow0[i]; tmp.tmp3_1 += ARow3[i] * BTRow1[i]; tmp.tmp3_2 += ARow3[i] * BTRow2[i]; tmp.tmp3_3 += ARow3[i] * BTRow3[i]; tmp.tmp3_4 += ARow3[i] * BTRow4[i]; tmp.tmp3_5 += ARow3[i] * BTRow5[i];
                        tmp.tmp4_0 += ARow4[i] * BTRow0[i]; tmp.tmp4_1 += ARow4[i] * BTRow1[i]; tmp.tmp4_2 += ARow4[i] * BTRow2[i]; tmp.tmp4_3 += ARow4[i] * BTRow3[i]; tmp.tmp4_4 += ARow4[i] * BTRow4[i]; tmp.tmp4_5 += ARow4[i] * BTRow5[i];
                        tmp.tmp5_0 += ARow5[i] * BTRow0[i]; tmp.tmp5_1 += ARow5[i] * BTRow1[i]; tmp.tmp5_2 += ARow5[i] * BTRow2[i]; tmp.tmp5_3 += ARow5[i] * BTRow3[i]; tmp.tmp5_4 += ARow5[i] * BTRow4[i]; tmp.tmp5_5 += ARow5[i] * BTRow5[i];
                    }
                    result[row + 0][column + 0] = tmp.tmp0_0; result[row + 0][column + 1] = tmp.tmp0_1; result[row + 0][column + 2] = tmp.tmp0_2; result[row + 0][column + 3] = tmp.tmp0_3; result[row + 0][column + 4] = tmp.tmp0_4; result[row + 0][column + 5] = tmp.tmp0_5;
                    result[row + 1][column + 0] = tmp.tmp1_0; result[row + 1][column + 1] = tmp.tmp1_1; result[row + 1][column + 2] = tmp.tmp1_2; result[row + 1][column + 3] = tmp.tmp1_3; result[row + 1][column + 4] = tmp.tmp1_4; result[row + 1][column + 5] = tmp.tmp1_5;
                    result[row + 2][column + 0] = tmp.tmp2_0; result[row + 2][column + 1] = tmp.tmp2_1; result[row + 2][column + 2] = tmp.tmp2_2; result[row + 2][column + 3] = tmp.tmp2_3; result[row + 2][column + 4] = tmp.tmp2_4; result[row + 2][column + 5] = tmp.tmp2_5;
                    result[row + 3][column + 0] = tmp.tmp3_0; result[row + 3][column + 1] = tmp.tmp3_1; result[row + 3][column + 2] = tmp.tmp3_2; result[row + 3][column + 3] = tmp.tmp3_3; result[row + 3][column + 4] = tmp.tmp3_4; result[row + 3][column + 5] = tmp.tmp3_5;
                    result[row + 4][column + 0] = tmp.tmp4_0; result[row + 4][column + 1] = tmp.tmp4_1; result[row + 4][column + 2] = tmp.tmp4_2; result[row + 4][column + 3] = tmp.tmp4_3; result[row + 4][column + 4] = tmp.tmp4_4; result[row + 4][column + 5] = tmp.tmp4_5;
                    result[row + 5][column + 0] = tmp.tmp5_0; result[row + 5][column + 1] = tmp.tmp5_1; result[row + 5][column + 2] = tmp.tmp5_2; result[row + 5][column + 3] = tmp.tmp5_3; result[row + 5][column + 4] = tmp.tmp5_4; result[row + 5][column + 5] = tmp.tmp5_5;
                }

                for (; column < columns; column++)
                {
                    var BTRow = BT[column].AsSpan();
                    var BTRowVector = MemoryMarshal.Cast<T, Vector256<T>>(BTRow);
                    var V0 = Vector256<T>.Zero;
                    var V1 = Vector256<T>.Zero;
                    var V2 = Vector256<T>.Zero;
                    var V3 = Vector256<T>.Zero;
                    var V4 = Vector256<T>.Zero;
                    var V5 = Vector256<T>.Zero;
                    for (var i = 0; i < args.ARowVector0.Length; ++i)
                    {
                        V0 += args.ARowVector0[i] * BTRowVector[i];
                        V1 += args.ARowVector1[i] * BTRowVector[i];
                        V2 += args.ARowVector2[i] * BTRowVector[i];
                        V3 += args.ARowVector3[i] * BTRowVector[i];
                        V4 += args.ARowVector4[i] * BTRowVector[i];
                        V5 += args.ARowVector5[i] * BTRowVector[i];
                    }
                    var tmp0 = T.Zero;
                    var tmp1 = T.Zero;
                    var tmp2 = T.Zero;
                    var tmp3 = T.Zero;
                    var tmp4 = T.Zero;
                    var tmp5 = T.Zero;
                    for (var i = 0; i < Vector256<T>.Count; ++i)
                    {
                        tmp0 += V0[i];
                        tmp1 += V1[i];
                        tmp2 += V2[i];
                        tmp3 += V3[i];
                        tmp4 += V4[i];
                        tmp5 += V5[i];
                    }
                    for (var i = args.ARowVector0.Length * Vector256<T>.Count; i < ARow0.Length; ++i)
                    {
                        tmp0 += ARow0[i] * BTRow[i];
                        tmp1 += ARow1[i] * BTRow[i];
                        tmp2 += ARow2[i] * BTRow[i];
                        tmp3 += ARow3[i] * BTRow[i];
                        tmp4 += ARow4[i] * BTRow[i];
                        tmp5 += ARow5[i] * BTRow[i];
                    }
                    result[row + 0][column] = tmp0;
                    result[row + 1][column] = tmp1;
                    result[row + 2][column] = tmp2;
                    result[row + 3][column] = tmp3;
                    result[row + 4][column] = tmp4;
                    result[row + 5][column] = tmp5;
                }
            }
            for (; row < endIdx; ++row)
            {
                var ARow = A[row].AsSpan();
                var ARowVector = MemoryMarshal.Cast<T, Vector256<T>>(ARow);
                for (var column = 0; column < columns; column++)
                {
                    var BTRow = BT[column].AsSpan();
                    var BTRowVector = MemoryMarshal.Cast<T, Vector256<T>>(BTRow);
                    var V = Vector256<T>.Zero;
                    for (var i = 0; i < ARowVector.Length; ++i)
                        V += ARowVector[i] * BTRowVector[i];
                    var tmp = T.Zero;
                    for (var i = 0; i < Vector256<T>.Count; ++i)
                        tmp += V[i];
                    for (var i = ARowVector.Length * Vector256<T>.Count; i < ARow.Length; ++i)
                        tmp += ARow[i] * BTRow[i];
                    result[row][column] = tmp;
                }
            }
        });

        return result;
    }        
}
