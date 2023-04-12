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
        public T tmp0_0; public T tmp0_1; public T tmp0_2; public T tmp0_3; public T tmp0_4; public T tmp0_5; public T tmp0_6;
        public T tmp1_0; public T tmp1_1; public T tmp1_2; public T tmp1_3; public T tmp1_4; public T tmp1_5; public T tmp1_6;
        public T tmp2_0; public T tmp2_1; public T tmp2_2; public T tmp2_3; public T tmp2_4; public T tmp2_5; public T tmp2_6;
        public T tmp3_0; public T tmp3_1; public T tmp3_2; public T tmp3_3; public T tmp3_4; public T tmp3_5; public T tmp3_6;
        public T tmp4_0; public T tmp4_1; public T tmp4_2; public T tmp4_3; public T tmp4_4; public T tmp4_5; public T tmp4_6;
        public T tmp5_0; public T tmp5_1; public T tmp5_2; public T tmp5_3; public T tmp5_4; public T tmp5_5; public T tmp5_6;
        public T tmp6_0; public T tmp6_1; public T tmp6_2; public T tmp6_3; public T tmp6_4; public T tmp6_5; public T tmp6_6;
    }

    ref struct Args
    {
        public Span<Vector256<T>> ARowVector0;
        public Span<Vector256<T>> ARowVector1;
        public Span<Vector256<T>> ARowVector2;
        public Span<Vector256<T>> ARowVector3;
        public Span<Vector256<T>> ARowVector4; 
        public Span<Vector256<T>> ARowVector5; 
        public Span<Vector256<T>> ARowVector6;

        public Span<Vector256<T>> BTRowVector0; 
        public Span<Vector256<T>> BTRowVector1;
        public Span<Vector256<T>> BTRowVector2;
        public Span<Vector256<T>> BTRowVector3; 
        public Span<Vector256<T>> BTRowVector4;
        public Span<Vector256<T>> BTRowVector5;
        public Span<Vector256<T>> BTRowVector6;
    }


    static private int ProcessorCount = Environment.ProcessorCount;

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    unsafe private static void InnerLoop(Args args, out Tmp tmp)
    {
        var length = args.ARowVector0.Length;

         Vector256<T> tmpVector0_0 = Vector256<T>.Zero, tmpVector0_1 = Vector256<T>.Zero, tmpVector0_2 = Vector256<T>.Zero, tmpVector0_3 = Vector256<T>.Zero, tmpVector0_4 = Vector256<T>.Zero, tmpVector0_5 = Vector256<T>.Zero, tmpVector0_6 = Vector256<T>.Zero;
         Vector256<T> tmpVector1_0 = Vector256<T>.Zero, tmpVector1_1 = Vector256<T>.Zero, tmpVector1_2 = Vector256<T>.Zero, tmpVector1_3 = Vector256<T>.Zero, tmpVector1_4 = Vector256<T>.Zero, tmpVector1_5 = Vector256<T>.Zero, tmpVector1_6 = Vector256<T>.Zero;
         Vector256<T> tmpVector2_0 = Vector256<T>.Zero, tmpVector2_1 = Vector256<T>.Zero, tmpVector2_2 = Vector256<T>.Zero, tmpVector2_3 = Vector256<T>.Zero, tmpVector2_4 = Vector256<T>.Zero, tmpVector2_5 = Vector256<T>.Zero, tmpVector2_6 = Vector256<T>.Zero;
         Vector256<T> tmpVector3_0 = Vector256<T>.Zero, tmpVector3_1 = Vector256<T>.Zero, tmpVector3_2 = Vector256<T>.Zero, tmpVector3_3 = Vector256<T>.Zero, tmpVector3_4 = Vector256<T>.Zero, tmpVector3_5 = Vector256<T>.Zero, tmpVector3_6 = Vector256<T>.Zero;
         Vector256<T> tmpVector4_0 = Vector256<T>.Zero, tmpVector4_1 = Vector256<T>.Zero, tmpVector4_2 = Vector256<T>.Zero, tmpVector4_3 = Vector256<T>.Zero, tmpVector4_4 = Vector256<T>.Zero, tmpVector4_5 = Vector256<T>.Zero, tmpVector4_6 = Vector256<T>.Zero;
         Vector256<T> tmpVector5_0 = Vector256<T>.Zero, tmpVector5_1 = Vector256<T>.Zero, tmpVector5_2 = Vector256<T>.Zero, tmpVector5_3 = Vector256<T>.Zero, tmpVector5_4 = Vector256<T>.Zero, tmpVector5_5 = Vector256<T>.Zero, tmpVector5_6 = Vector256<T>.Zero;
         Vector256<T> tmpVector6_0 = Vector256<T>.Zero, tmpVector6_1 = Vector256<T>.Zero, tmpVector6_2 = Vector256<T>.Zero, tmpVector6_3 = Vector256<T>.Zero, tmpVector6_4 = Vector256<T>.Zero, tmpVector6_5 = Vector256<T>.Zero, tmpVector6_6 = Vector256<T>.Zero;

        fixed (Vector256<T>* A0=args.ARowVector0,  A1=args.ARowVector1,  A2=args.ARowVector2,  A3=args.ARowVector3,  A4=args.ARowVector4,  A5=args.ARowVector5,  A6=args.ARowVector6,
                             B0=args.BTRowVector0, B1=args.BTRowVector1, B2=args.BTRowVector2, B3=args.BTRowVector3, B4=args.BTRowVector4, B5=args.BTRowVector5, B6=args.BTRowVector6)
        {
            Vector256<T> a;
            for (var i = 0; i < length; ++i)
            {
                a = A0[i]; tmpVector0_0 += a * B0[i]; tmpVector0_1 += a * B1[i]; tmpVector0_2 += a * B2[i]; tmpVector0_3 += a * B3[i]; tmpVector0_4 += a * B4[i]; tmpVector0_5 += a * B5[i]; tmpVector0_6 += a * B6[i];
                a = A1[i]; tmpVector1_0 += a * B0[i]; tmpVector1_1 += a * B1[i]; tmpVector1_2 += a * B2[i]; tmpVector1_3 += a * B3[i]; tmpVector1_4 += a * B4[i]; tmpVector1_5 += a * B5[i]; tmpVector1_6 += a * B6[i];
                a = A2[i]; tmpVector2_0 += a * B0[i]; tmpVector2_1 += a * B1[i]; tmpVector2_2 += a * B2[i]; tmpVector2_3 += a * B3[i]; tmpVector2_4 += a * B4[i]; tmpVector2_5 += a * B5[i]; tmpVector2_6 += a * B6[i];
                a = A3[i]; tmpVector3_0 += a * B0[i]; tmpVector3_1 += a * B1[i]; tmpVector3_2 += a * B2[i]; tmpVector3_3 += a * B3[i]; tmpVector3_4 += a * B4[i]; tmpVector3_5 += a * B5[i]; tmpVector3_6 += a * B6[i];
                a = A4[i]; tmpVector4_0 += a * B0[i]; tmpVector4_1 += a * B1[i]; tmpVector4_2 += a * B2[i]; tmpVector4_3 += a * B3[i]; tmpVector4_4 += a * B4[i]; tmpVector4_5 += a * B5[i]; tmpVector4_6 += a * B6[i];
                a = A5[i]; tmpVector5_0 += a * B0[i]; tmpVector5_1 += a * B1[i]; tmpVector5_2 += a * B2[i]; tmpVector5_3 += a * B3[i]; tmpVector5_4 += a * B4[i]; tmpVector5_5 += a * B5[i]; tmpVector5_6 += a * B6[i];
                a = A6[i]; tmpVector6_0 += a * B0[i]; tmpVector6_1 += a * B1[i]; tmpVector6_2 += a * B2[i]; tmpVector6_3 += a * B3[i]; tmpVector6_4 += a * B4[i]; tmpVector6_5 += a * B5[i]; tmpVector6_6 += a * B6[i];
            }
        }

        tmp.tmp0_0 = T.Zero; tmp.tmp0_1 = T.Zero; tmp.tmp0_2 = T.Zero; tmp.tmp0_3 = T.Zero; tmp.tmp0_4 = T.Zero; tmp.tmp0_5 = T.Zero; tmp.tmp0_6 = T.Zero;
        tmp.tmp1_0 = T.Zero; tmp.tmp1_1 = T.Zero; tmp.tmp1_2 = T.Zero; tmp.tmp1_3 = T.Zero; tmp.tmp1_4 = T.Zero; tmp.tmp1_5 = T.Zero; tmp.tmp1_6 = T.Zero;
        tmp.tmp2_0 = T.Zero; tmp.tmp2_1 = T.Zero; tmp.tmp2_2 = T.Zero; tmp.tmp2_3 = T.Zero; tmp.tmp2_4 = T.Zero; tmp.tmp2_5 = T.Zero; tmp.tmp2_6 = T.Zero;
        tmp.tmp3_0 = T.Zero; tmp.tmp3_1 = T.Zero; tmp.tmp3_2 = T.Zero; tmp.tmp3_3 = T.Zero; tmp.tmp3_4 = T.Zero; tmp.tmp3_5 = T.Zero; tmp.tmp3_6 = T.Zero;
        tmp.tmp4_0 = T.Zero; tmp.tmp4_1 = T.Zero; tmp.tmp4_2 = T.Zero; tmp.tmp4_3 = T.Zero; tmp.tmp4_4 = T.Zero; tmp.tmp4_5 = T.Zero; tmp.tmp4_6 = T.Zero;
        tmp.tmp5_0 = T.Zero; tmp.tmp5_1 = T.Zero; tmp.tmp5_2 = T.Zero; tmp.tmp5_3 = T.Zero; tmp.tmp5_4 = T.Zero; tmp.tmp5_5 = T.Zero; tmp.tmp5_6 = T.Zero;
        tmp.tmp6_0 = T.Zero; tmp.tmp6_1 = T.Zero; tmp.tmp6_2 = T.Zero; tmp.tmp6_3 = T.Zero; tmp.tmp6_4 = T.Zero; tmp.tmp6_5 = T.Zero; tmp.tmp6_6 = T.Zero;
        for (var i = 0; i < Vector256<T>.Count; ++i)
        {
            tmp.tmp0_0 += tmpVector0_0[i]; tmp.tmp0_1 += tmpVector0_1[i]; tmp.tmp0_2 += tmpVector0_2[i]; tmp.tmp0_3 += tmpVector0_3[i]; tmp.tmp0_4 += tmpVector0_4[i]; tmp.tmp0_5 += tmpVector0_5[i]; tmp.tmp0_6 += tmpVector0_6[i];
            tmp.tmp1_0 += tmpVector1_0[i]; tmp.tmp1_1 += tmpVector1_1[i]; tmp.tmp1_2 += tmpVector1_2[i]; tmp.tmp1_3 += tmpVector1_3[i]; tmp.tmp1_4 += tmpVector1_4[i]; tmp.tmp1_5 += tmpVector1_5[i]; tmp.tmp1_6 += tmpVector1_6[i];
            tmp.tmp2_0 += tmpVector2_0[i]; tmp.tmp2_1 += tmpVector2_1[i]; tmp.tmp2_2 += tmpVector2_2[i]; tmp.tmp2_3 += tmpVector2_3[i]; tmp.tmp2_4 += tmpVector2_4[i]; tmp.tmp2_5 += tmpVector2_5[i]; tmp.tmp2_6 += tmpVector2_6[i];
            tmp.tmp3_0 += tmpVector3_0[i]; tmp.tmp3_1 += tmpVector3_1[i]; tmp.tmp3_2 += tmpVector3_2[i]; tmp.tmp3_3 += tmpVector3_3[i]; tmp.tmp3_4 += tmpVector3_4[i]; tmp.tmp3_5 += tmpVector3_5[i]; tmp.tmp3_6 += tmpVector3_6[i];
            tmp.tmp4_0 += tmpVector4_0[i]; tmp.tmp4_1 += tmpVector4_1[i]; tmp.tmp4_2 += tmpVector4_2[i]; tmp.tmp4_3 += tmpVector4_3[i]; tmp.tmp4_4 += tmpVector4_4[i]; tmp.tmp4_5 += tmpVector4_5[i]; tmp.tmp4_6 += tmpVector4_6[i];
            tmp.tmp5_0 += tmpVector5_0[i]; tmp.tmp5_1 += tmpVector5_1[i]; tmp.tmp5_2 += tmpVector5_2[i]; tmp.tmp5_3 += tmpVector5_3[i]; tmp.tmp5_4 += tmpVector5_4[i]; tmp.tmp5_5 += tmpVector5_5[i]; tmp.tmp5_6 += tmpVector5_6[i];
            tmp.tmp6_0 += tmpVector6_0[i]; tmp.tmp6_1 += tmpVector6_1[i]; tmp.tmp6_2 += tmpVector6_2[i]; tmp.tmp6_3 += tmpVector6_3[i]; tmp.tmp6_4 += tmpVector6_4[i]; tmp.tmp6_5 += tmpVector6_5[i]; tmp.tmp6_6 += tmpVector6_6[i];
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

        var rowsPerIteration = 7;
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
                var ARow6 = A[row + 6].AsSpan();
                args.ARowVector0 = MemoryMarshal.Cast<T, Vector256<T>>(ARow0);
                args.ARowVector1 = MemoryMarshal.Cast<T, Vector256<T>>(ARow1);
                args.ARowVector2 = MemoryMarshal.Cast<T, Vector256<T>>(ARow2);
                args.ARowVector3 = MemoryMarshal.Cast<T, Vector256<T>>(ARow3);
                args.ARowVector4 = MemoryMarshal.Cast<T, Vector256<T>>(ARow4);
                args.ARowVector5 = MemoryMarshal.Cast<T, Vector256<T>>(ARow5);
                args.ARowVector6 = MemoryMarshal.Cast<T, Vector256<T>>(ARow6);
                var column = 0;
                for (; column < columns - 7 + 1; column += 7)
                {
                    var BTRow0 = BT[column + 0].AsSpan();
                    var BTRow1 = BT[column + 1].AsSpan();
                    var BTRow2 = BT[column + 2].AsSpan();
                    var BTRow3 = BT[column + 3].AsSpan();
                    var BTRow4 = BT[column + 4].AsSpan();
                    var BTRow5 = BT[column + 5].AsSpan();
                    var BTRow6 = BT[column + 6].AsSpan();
                    args.BTRowVector0 = MemoryMarshal.Cast<T, Vector256<T>>(BTRow0);
                    args.BTRowVector1 = MemoryMarshal.Cast<T, Vector256<T>>(BTRow1);
                    args.BTRowVector2 = MemoryMarshal.Cast<T, Vector256<T>>(BTRow2);
                    args.BTRowVector3 = MemoryMarshal.Cast<T, Vector256<T>>(BTRow3);
                    args.BTRowVector4 = MemoryMarshal.Cast<T, Vector256<T>>(BTRow4);
                    args.BTRowVector5 = MemoryMarshal.Cast<T, Vector256<T>>(BTRow5);
                    args.BTRowVector6 = MemoryMarshal.Cast<T, Vector256<T>>(BTRow6);

                    InnerLoop(args, out var tmp);

                    for (var i = args.ARowVector0.Length * Vector256<T>.Count; i < ARow0.Length; ++i)
                    {
                        tmp.tmp0_0 += ARow0[i] * BTRow0[i]; tmp.tmp0_1 += ARow0[i] * BTRow1[i]; tmp.tmp0_2 += ARow0[i] * BTRow2[i]; tmp.tmp0_3 += ARow0[i] * BTRow3[i]; tmp.tmp0_4 += ARow0[i] * BTRow4[i]; tmp.tmp0_5 += ARow0[i] * BTRow5[i]; tmp.tmp0_6 += ARow0[i] * BTRow6[i];
                        tmp.tmp1_0 += ARow1[i] * BTRow0[i]; tmp.tmp1_1 += ARow1[i] * BTRow1[i]; tmp.tmp1_2 += ARow1[i] * BTRow2[i]; tmp.tmp1_3 += ARow1[i] * BTRow3[i]; tmp.tmp1_4 += ARow1[i] * BTRow4[i]; tmp.tmp1_5 += ARow1[i] * BTRow5[i]; tmp.tmp1_6 += ARow1[i] * BTRow6[i];
                        tmp.tmp2_0 += ARow2[i] * BTRow0[i]; tmp.tmp2_1 += ARow2[i] * BTRow1[i]; tmp.tmp2_2 += ARow2[i] * BTRow2[i]; tmp.tmp2_3 += ARow2[i] * BTRow3[i]; tmp.tmp2_4 += ARow2[i] * BTRow4[i]; tmp.tmp2_5 += ARow2[i] * BTRow5[i]; tmp.tmp2_6 += ARow2[i] * BTRow6[i];
                        tmp.tmp3_0 += ARow3[i] * BTRow0[i]; tmp.tmp3_1 += ARow3[i] * BTRow1[i]; tmp.tmp3_2 += ARow3[i] * BTRow2[i]; tmp.tmp3_3 += ARow3[i] * BTRow3[i]; tmp.tmp3_4 += ARow3[i] * BTRow4[i]; tmp.tmp3_5 += ARow3[i] * BTRow5[i]; tmp.tmp3_6 += ARow3[i] * BTRow6[i];
                        tmp.tmp4_0 += ARow4[i] * BTRow0[i]; tmp.tmp4_1 += ARow4[i] * BTRow1[i]; tmp.tmp4_2 += ARow4[i] * BTRow2[i]; tmp.tmp4_3 += ARow4[i] * BTRow3[i]; tmp.tmp4_4 += ARow4[i] * BTRow4[i]; tmp.tmp4_5 += ARow4[i] * BTRow5[i]; tmp.tmp4_6 += ARow4[i] * BTRow6[i];
                        tmp.tmp5_0 += ARow5[i] * BTRow0[i]; tmp.tmp5_1 += ARow5[i] * BTRow1[i]; tmp.tmp5_2 += ARow5[i] * BTRow2[i]; tmp.tmp5_3 += ARow5[i] * BTRow3[i]; tmp.tmp5_4 += ARow5[i] * BTRow4[i]; tmp.tmp5_5 += ARow5[i] * BTRow5[i]; tmp.tmp5_6 += ARow5[i] * BTRow6[i];
                        tmp.tmp6_0 += ARow6[i] * BTRow0[i]; tmp.tmp6_1 += ARow6[i] * BTRow1[i]; tmp.tmp6_2 += ARow6[i] * BTRow2[i]; tmp.tmp6_3 += ARow6[i] * BTRow3[i]; tmp.tmp6_4 += ARow6[i] * BTRow4[i]; tmp.tmp6_5 += ARow6[i] * BTRow5[i]; tmp.tmp6_6 += ARow6[i] * BTRow6[i];
                    }
                    result[row + 0][column + 0] = tmp.tmp0_0; result[row + 0][column + 1] = tmp.tmp0_1; result[row + 0][column + 2] = tmp.tmp0_2; result[row + 0][column + 3] = tmp.tmp0_3; result[row + 0][column + 4] = tmp.tmp0_4; result[row + 0][column + 5] = tmp.tmp0_5; result[row + 0][column + 6] = tmp.tmp0_6;
                    result[row + 1][column + 0] = tmp.tmp1_0; result[row + 1][column + 1] = tmp.tmp1_1; result[row + 1][column + 2] = tmp.tmp1_2; result[row + 1][column + 3] = tmp.tmp1_3; result[row + 1][column + 4] = tmp.tmp1_4; result[row + 1][column + 5] = tmp.tmp1_5; result[row + 1][column + 6] = tmp.tmp1_6;
                    result[row + 2][column + 0] = tmp.tmp2_0; result[row + 2][column + 1] = tmp.tmp2_1; result[row + 2][column + 2] = tmp.tmp2_2; result[row + 2][column + 3] = tmp.tmp2_3; result[row + 2][column + 4] = tmp.tmp2_4; result[row + 2][column + 5] = tmp.tmp2_5; result[row + 2][column + 6] = tmp.tmp2_6;
                    result[row + 3][column + 0] = tmp.tmp3_0; result[row + 3][column + 1] = tmp.tmp3_1; result[row + 3][column + 2] = tmp.tmp3_2; result[row + 3][column + 3] = tmp.tmp3_3; result[row + 3][column + 4] = tmp.tmp3_4; result[row + 3][column + 5] = tmp.tmp3_5; result[row + 3][column + 6] = tmp.tmp3_6;
                    result[row + 4][column + 0] = tmp.tmp4_0; result[row + 4][column + 1] = tmp.tmp4_1; result[row + 4][column + 2] = tmp.tmp4_2; result[row + 4][column + 3] = tmp.tmp4_3; result[row + 4][column + 4] = tmp.tmp4_4; result[row + 4][column + 5] = tmp.tmp4_5; result[row + 4][column + 6] = tmp.tmp4_6;
                    result[row + 5][column + 0] = tmp.tmp5_0; result[row + 5][column + 1] = tmp.tmp5_1; result[row + 5][column + 2] = tmp.tmp5_2; result[row + 5][column + 3] = tmp.tmp5_3; result[row + 5][column + 4] = tmp.tmp5_4; result[row + 5][column + 5] = tmp.tmp5_5; result[row + 5][column + 6] = tmp.tmp5_6;
                    result[row + 6][column + 0] = tmp.tmp6_0; result[row + 6][column + 1] = tmp.tmp6_1; result[row + 6][column + 2] = tmp.tmp6_2; result[row + 6][column + 3] = tmp.tmp6_3; result[row + 6][column + 4] = tmp.tmp6_4; result[row + 6][column + 5] = tmp.tmp6_5; result[row + 6][column + 6] = tmp.tmp6_6;
                }

                for (; column < columns; column++)
                {
                    var BTRow = BT[column].AsSpan();
                    var BTRowVector = MemoryMarshal.Cast<T, Vector256<T>>(BTRow);
                    var tmpVector0 = Vector256<T>.Zero;
                    var tmpVector1 = Vector256<T>.Zero;
                    var tmpVector2 = Vector256<T>.Zero;
                    var tmpVector3 = Vector256<T>.Zero;
                    var tmpVector4 = Vector256<T>.Zero;
                    var tmpVector5 = Vector256<T>.Zero;
                    var tmpVector6 = Vector256<T>.Zero;
                    for (var i = 0; i < args.ARowVector0.Length; ++i)
                    {
                        tmpVector0 += args.ARowVector0[i] * BTRowVector[i];
                        tmpVector1 += args.ARowVector1[i] * BTRowVector[i];
                        tmpVector2 += args.ARowVector2[i] * BTRowVector[i];
                        tmpVector3 += args.ARowVector3[i] * BTRowVector[i];
                        tmpVector4 += args.ARowVector4[i] * BTRowVector[i];
                        tmpVector5 += args.ARowVector5[i] * BTRowVector[i];
                        tmpVector6 += args.ARowVector6[i] * BTRowVector[i];
                    }
                    var tmp0 = T.Zero;
                    var tmp1 = T.Zero;
                    var tmp2 = T.Zero;
                    var tmp3 = T.Zero;
                    var tmp4 = T.Zero;
                    var tmp5 = T.Zero;
                    var tmp6 = T.Zero;
                    for (var i = 0; i < Vector256<T>.Count; ++i)
                    {
                        tmp0 += tmpVector0[i];
                        tmp1 += tmpVector1[i];
                        tmp2 += tmpVector2[i];
                        tmp3 += tmpVector3[i];
                        tmp4 += tmpVector4[i];
                        tmp5 += tmpVector5[i];
                        tmp6 += tmpVector6[i];
                    }
                    for (var i = args.ARowVector0.Length * Vector256<T>.Count; i < ARow0.Length; ++i)
                    {
                        tmp0 += ARow0[i] * BTRow[i];
                        tmp1 += ARow1[i] * BTRow[i];
                        tmp2 += ARow2[i] * BTRow[i];
                        tmp3 += ARow3[i] * BTRow[i];
                        tmp4 += ARow4[i] * BTRow[i];
                        tmp5 += ARow5[i] * BTRow[i];
                        tmp6 += ARow6[i] * BTRow[i];
                    }
                    result[row + 0][column] = tmp0;
                    result[row + 1][column] = tmp1;
                    result[row + 2][column] = tmp2;
                    result[row + 3][column] = tmp3;
                    result[row + 4][column] = tmp4;
                    result[row + 5][column] = tmp5;
                    result[row + 6][column] = tmp6;
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
                    var tmpVector = Vector256<T>.Zero;
                    for (var i = 0; i < ARowVector.Length; ++i)
                        tmpVector += ARowVector[i] * BTRowVector[i];
                    var tmp = T.Zero;
                    for (var i = 0; i < Vector256<T>.Count; ++i)
                        tmp += tmpVector[i];
                    for (var i = ARowVector.Length * Vector256<T>.Count; i < ARow.Length; ++i)
                        tmp += ARow[i] * BTRow[i];
                    result[row][column] = tmp;
                }
            }
        });

        return result;
    }        
}
