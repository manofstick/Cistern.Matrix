using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace Cistern.Matrix;

public class Multiplication<T>
    where T : struct, INumber<T>
{
    public struct Tmp4_3
    {
        public T tmp0_0;
        public T tmp0_1;
        public T tmp0_2;
        public T tmp1_0;
        public T tmp1_1;
        public T tmp1_2;
        public T tmp2_0;
        public T tmp2_1;
        public T tmp2_2;
        public T tmp3_0;
        public T tmp3_1;
        public T tmp3_2;
    }


    static private int ProcessorCount = Environment.ProcessorCount;

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    unsafe private static void InnerLoop(Span<Vector256<T>> ARowVector0, Span<Vector256<T>> ARowVector1, Span<Vector256<T>> ARowVector2, Span<Vector256<T>> ARowVector3, Span<Vector256<T>> BTRowVector0, Span<Vector256<T>> BTRowVector1, Span<Vector256<T>> BTRowVector2, out Tmp4_3 tmp)
    {
        var length = ARowVector0.Length;

        var tmpVector0_0 = Vector256<T>.Zero; var tmpVector0_1 = Vector256<T>.Zero; var tmpVector0_2 = Vector256<T>.Zero;
        var tmpVector1_0 = Vector256<T>.Zero; var tmpVector1_1 = Vector256<T>.Zero; var tmpVector1_2 = Vector256<T>.Zero;
        var tmpVector2_0 = Vector256<T>.Zero; var tmpVector2_1 = Vector256<T>.Zero; var tmpVector2_2 = Vector256<T>.Zero;
        var tmpVector3_0 = Vector256<T>.Zero; var tmpVector3_1 = Vector256<T>.Zero; var tmpVector3_2 = Vector256<T>.Zero;

        fixed (Vector256<T>* A0=ARowVector0, A1=ARowVector1, A2=ARowVector2, A3=ARowVector3, B0=BTRowVector0, B1=BTRowVector1, B2=BTRowVector2)
        {
            Vector256<T> x;
            for (var i = 0; i < length; ++i)
            {
                x = A0[i]; tmpVector0_0 += x * B0[i]; tmpVector0_1 += x * B1[i]; tmpVector0_2 += x * B2[i];
                x = A1[i]; tmpVector1_0 += x * B0[i]; tmpVector1_1 += x * B1[i]; tmpVector1_2 += x * B2[i];
                x = A2[i]; tmpVector2_0 += x * B0[i]; tmpVector2_1 += x * B1[i]; tmpVector2_2 += x * B2[i];
                x = A3[i]; tmpVector3_0 += x * B0[i]; tmpVector3_1 += x * B1[i]; tmpVector3_2 += x * B2[i];
            }
        }

        tmp.tmp0_0 = T.Zero; tmp.tmp0_1 = T.Zero; tmp.tmp0_2 = T.Zero;
        tmp.tmp1_0 = T.Zero; tmp.tmp1_1 = T.Zero; tmp.tmp1_2 = T.Zero;
        tmp.tmp2_0 = T.Zero; tmp.tmp2_1 = T.Zero; tmp.tmp2_2 = T.Zero;
        tmp.tmp3_0 = T.Zero; tmp.tmp3_1 = T.Zero; tmp.tmp3_2 = T.Zero;
        for (var i = 0; i < Vector256<T>.Count; ++i)
        {
            tmp.tmp0_0 += tmpVector0_0[i]; tmp.tmp0_1 += tmpVector0_1[i]; tmp.tmp0_2 += tmpVector0_2[i];
            tmp.tmp1_0 += tmpVector1_0[i]; tmp.tmp1_1 += tmpVector1_1[i]; tmp.tmp1_2 += tmpVector1_2[i];
            tmp.tmp2_0 += tmpVector2_0[i]; tmp.tmp2_1 += tmpVector2_1[i]; tmp.tmp2_2 += tmpVector2_2[i];
            tmp.tmp3_0 += tmpVector3_0[i]; tmp.tmp3_1 += tmpVector3_1[i]; tmp.tmp3_2 += tmpVector3_2[i];
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

        var rowsPerIteration = 4;
        var rowIterationsCount = (rows+rowsPerIteration-1) / rowsPerIteration;
        var rowsBatchesPerIteration = (rowIterationsCount+ProcessorCount-1) / ProcessorCount;
        var batchesCount = (rowIterationsCount+rowsBatchesPerIteration-1) / rowsBatchesPerIteration; 
        
        Parallel.For(0, batchesCount, parallelForIdx => {
            var startIdx = parallelForIdx * rowsBatchesPerIteration * 4;
            var endIdx = Math.Min(rows, (parallelForIdx+1) * rowsBatchesPerIteration * 4);

            var row = startIdx;
            for (; row < endIdx - 4 + 1; row += 4)
            {
                var ARow0 = A[row + 0].AsSpan();
                var ARow1 = A[row + 1].AsSpan();
                var ARow2 = A[row + 2].AsSpan();
                var ARow3 = A[row + 3].AsSpan();
                var ARowVector0 = MemoryMarshal.Cast<T, Vector256<T>>(ARow0);
                var ARowVector1 = MemoryMarshal.Cast<T, Vector256<T>>(ARow1);
                var ARowVector2 = MemoryMarshal.Cast<T, Vector256<T>>(ARow2);
                var ARowVector3 = MemoryMarshal.Cast<T, Vector256<T>>(ARow3);
                var column = 0;
                for (; column < columns - 3 + 1; column += 3)
                {
                    var BTRow0 = BT[column + 0].AsSpan();
                    var BTRow1 = BT[column + 1].AsSpan();
                    var BTRow2 = BT[column + 2].AsSpan();
                    var BTRowVector0 = MemoryMarshal.Cast<T, Vector256<T>>(BTRow0);
                    var BTRowVector1 = MemoryMarshal.Cast<T, Vector256<T>>(BTRow1);
                    var BTRowVector2 = MemoryMarshal.Cast<T, Vector256<T>>(BTRow2);

                    InnerLoop(ARowVector0, ARowVector1, ARowVector2, ARowVector3, BTRowVector0, BTRowVector1, BTRowVector2, out var tmp);

                    for (var i = ARowVector0.Length * Vector256<T>.Count; i < ARow0.Length; ++i)
                    {
                        tmp.tmp0_0 += ARow0[i] * BTRow0[i]; tmp.tmp0_1 += ARow0[i] * BTRow1[i]; tmp.tmp0_2 += ARow0[i] * BTRow2[i];
                        tmp.tmp1_0 += ARow1[i] * BTRow0[i]; tmp.tmp1_1 += ARow1[i] * BTRow1[i]; tmp.tmp1_2 += ARow1[i] * BTRow2[i];
                        tmp.tmp2_0 += ARow2[i] * BTRow0[i]; tmp.tmp2_1 += ARow2[i] * BTRow1[i]; tmp.tmp2_2 += ARow2[i] * BTRow2[i];
                        tmp.tmp3_0 += ARow3[i] * BTRow0[i]; tmp.tmp3_1 += ARow3[i] * BTRow1[i]; tmp.tmp3_2 += ARow3[i] * BTRow2[i];
                    }
                    result[row + 0][column + 0] = tmp.tmp0_0; result[row + 0][column + 1] = tmp.tmp0_1; result[row + 0][column + 2] = tmp.tmp0_2;
                    result[row + 1][column + 0] = tmp.tmp1_0; result[row + 1][column + 1] = tmp.tmp1_1; result[row + 1][column + 2] = tmp.tmp1_2;
                    result[row + 2][column + 0] = tmp.tmp2_0; result[row + 2][column + 1] = tmp.tmp2_1; result[row + 2][column + 2] = tmp.tmp2_2;
                    result[row + 3][column + 0] = tmp.tmp3_0; result[row + 3][column + 1] = tmp.tmp3_1; result[row + 3][column + 2] = tmp.tmp3_2;
                }

                for (; column < columns; column++)
                {
                    var BTRow = BT[column].AsSpan();
                    var BTRowVector = MemoryMarshal.Cast<T, Vector256<T>>(BTRow);
                    var tmpVector0 = Vector256<T>.Zero;
                    var tmpVector1 = Vector256<T>.Zero;
                    var tmpVector2 = Vector256<T>.Zero;
                    var tmpVector3 = Vector256<T>.Zero;
                    for (var i = 0; i < ARowVector0.Length; ++i)
                    {
                        tmpVector0 += ARowVector0[i] * BTRowVector[i];
                        tmpVector1 += ARowVector1[i] * BTRowVector[i];
                        tmpVector2 += ARowVector2[i] * BTRowVector[i];
                        tmpVector3 += ARowVector3[i] * BTRowVector[i];
                    }
                    var tmp0 = T.Zero;
                    var tmp1 = T.Zero;
                    var tmp2 = T.Zero;
                    var tmp3 = T.Zero;
                    for (var i = 0; i < Vector256<T>.Count; ++i)
                    {
                        tmp0 += tmpVector0[i];
                        tmp1 += tmpVector1[i];
                        tmp2 += tmpVector2[i];
                        tmp3 += tmpVector3[i];
                    }
                    for (var i = ARowVector0.Length * Vector256<T>.Count; i < ARow0.Length; ++i)
                    {
                        tmp0 += ARow0[i] * BTRow[i];
                        tmp1 += ARow1[i] * BTRow[i];
                        tmp2 += ARow2[i] * BTRow[i];
                        tmp3 += ARow3[i] * BTRow[i];
                    }
                    result[row + 0][column] = tmp0;
                    result[row + 1][column] = tmp1;
                    result[row + 2][column] = tmp2;
                    result[row + 3][column] = tmp3;
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
