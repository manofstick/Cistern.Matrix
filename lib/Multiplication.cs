using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace Cistern.Matrix;

public class Multiplication<T>
    where T : struct, INumber<T>
{
    const int Rows = 8;
    const int Cols = 8;
    
    public struct Tmp
    {
        public T T0_0; public T T0_1; public T T0_2; public T T0_3; public T T0_4; public T T0_5; public T T0_6; public T T0_7;
        public T T1_0; public T T1_1; public T T1_2; public T T1_3; public T T1_4; public T T1_5; public T T1_6; public T T1_7;
        public T T2_0; public T T2_1; public T T2_2; public T T2_3; public T T2_4; public T T2_5; public T T2_6; public T T2_7;
        public T T3_0; public T T3_1; public T T3_2; public T T3_3; public T T3_4; public T T3_5; public T T3_6; public T T3_7;
        public T T4_0; public T T4_1; public T T4_2; public T T4_3; public T T4_4; public T T4_5; public T T4_6; public T T4_7;
        public T T5_0; public T T5_1; public T T5_2; public T T5_3; public T T5_4; public T T5_5; public T T5_6; public T T5_7;
        public T T6_0; public T T6_1; public T T6_2; public T T6_3; public T T6_4; public T T6_5; public T T6_6; public T T6_7;
        public T T7_0; public T T7_1; public T T7_2; public T T7_3; public T T7_4; public T T7_5; public T T7_6; public T T7_7;
    }

    ref struct Args
    {
        public Span<Vector<T>> A0; public Span<Vector<T>> A1; public Span<Vector<T>> A2; public Span<Vector<T>> A3;
        public Span<Vector<T>> A4; public Span<Vector<T>> A5; public Span<Vector<T>> A6; public Span<Vector<T>> A7; 

        public Span<Vector<T>> BT0; public Span<Vector<T>> BT1; public Span<Vector<T>> BT2; public Span<Vector<T>> BT3; 
        public Span<Vector<T>> BT4; public Span<Vector<T>> BT5; public Span<Vector<T>> BT6; public Span<Vector<T>> BT7;
    }


    static private readonly Vector<T> Zero = Vector<T>.Zero;
    static private readonly int ProcessorCount = Environment.ProcessorCount;

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    unsafe private static void InnerLoop(Args args, out Tmp x)
    {
        var length = args.A0.Length;

        Vector<T> V0_0 = Zero, V0_1 = Zero, V0_2 = Zero, V0_3 = Zero, V0_4 = Zero, V0_5 = Zero, V0_6 = Zero, V0_7 = Zero;
        Vector<T> V1_0 = Zero, V1_1 = Zero, V1_2 = Zero, V1_3 = Zero, V1_4 = Zero, V1_5 = Zero, V1_6 = Zero, V1_7 = Zero;
        Vector<T> V2_0 = Zero, V2_1 = Zero, V2_2 = Zero, V2_3 = Zero, V2_4 = Zero, V2_5 = Zero, V2_6 = Zero, V2_7 = Zero;
        Vector<T> V3_0 = Zero, V3_1 = Zero, V3_2 = Zero, V3_3 = Zero, V3_4 = Zero, V3_5 = Zero, V3_6 = Zero, V3_7 = Zero;
        Vector<T> V4_0 = Zero, V4_1 = Zero, V4_2 = Zero, V4_3 = Zero, V4_4 = Zero, V4_5 = Zero, V4_6 = Zero, V4_7 = Zero;
        Vector<T> V5_0 = Zero, V5_1 = Zero, V5_2 = Zero, V5_3 = Zero, V5_4 = Zero, V5_5 = Zero, V5_6 = Zero, V5_7 = Zero;
        Vector<T> V6_0 = Zero, V6_1 = Zero, V6_2 = Zero, V6_3 = Zero, V6_4 = Zero, V6_5 = Zero, V6_6 = Zero, V6_7 = Zero;
        Vector<T> V7_0 = Zero, V7_1 = Zero, V7_2 = Zero, V7_3 = Zero, V7_4 = Zero, V7_5 = Zero, V7_6 = Zero, V7_7 = Zero;

        fixed (Vector<T>* A0=args.A0,  A1=args.A1,  A2=args.A2,  A3=args.A3,  A4=args.A4,  A5=args.A5,  A6=args.A6,  A7=args.A7,
                          B0=args.BT0, B1=args.BT1, B2=args.BT2, B3=args.BT3, B4=args.BT4, B5=args.BT5, B6=args.BT6, B7=args.BT7)
        {
            Vector<T> a;
            for (var i = length-1; i >= 0; --i)
            {
                a = A0[i]; V0_0 += a * B0[i]; V0_1 += a * B1[i]; V0_2 += a * B2[i]; V0_3 += a * B3[i]; V0_4 += a * B4[i]; V0_5 += a * B5[i]; V0_6 += a * B6[i]; V0_7 += a * B7[i];
                a = A1[i]; V1_0 += a * B0[i]; V1_1 += a * B1[i]; V1_2 += a * B2[i]; V1_3 += a * B3[i]; V1_4 += a * B4[i]; V1_5 += a * B5[i]; V1_6 += a * B6[i]; V1_7 += a * B7[i];
                a = A2[i]; V2_0 += a * B0[i]; V2_1 += a * B1[i]; V2_2 += a * B2[i]; V2_3 += a * B3[i]; V2_4 += a * B4[i]; V2_5 += a * B5[i]; V2_6 += a * B6[i]; V2_7 += a * B7[i];
                a = A3[i]; V3_0 += a * B0[i]; V3_1 += a * B1[i]; V3_2 += a * B2[i]; V3_3 += a * B3[i]; V3_4 += a * B4[i]; V3_5 += a * B5[i]; V3_6 += a * B6[i]; V3_7 += a * B7[i];
                a = A4[i]; V4_0 += a * B0[i]; V4_1 += a * B1[i]; V4_2 += a * B2[i]; V4_3 += a * B3[i]; V4_4 += a * B4[i]; V4_5 += a * B5[i]; V4_6 += a * B6[i]; V4_7 += a * B7[i];
                a = A5[i]; V5_0 += a * B0[i]; V5_1 += a * B1[i]; V5_2 += a * B2[i]; V5_3 += a * B3[i]; V5_4 += a * B4[i]; V5_5 += a * B5[i]; V5_6 += a * B6[i]; V5_7 += a * B7[i];
                a = A6[i]; V6_0 += a * B0[i]; V6_1 += a * B1[i]; V6_2 += a * B2[i]; V6_3 += a * B3[i]; V6_4 += a * B4[i]; V6_5 += a * B5[i]; V6_6 += a * B6[i]; V6_7 += a * B7[i];
                a = A7[i]; V7_0 += a * B0[i]; V7_1 += a * B1[i]; V7_2 += a * B2[i]; V7_3 += a * B3[i]; V7_4 += a * B4[i]; V7_5 += a * B5[i]; V7_6 += a * B6[i]; V7_7 += a * B7[i];
            }
        }

        x.T0_0 = T.Zero; x.T0_1 = T.Zero; x.T0_2 = T.Zero; x.T0_3 = T.Zero; x.T0_4 = T.Zero; x.T0_5 = T.Zero; x.T0_6 = T.Zero; x.T0_7 = T.Zero;
        x.T1_0 = T.Zero; x.T1_1 = T.Zero; x.T1_2 = T.Zero; x.T1_3 = T.Zero; x.T1_4 = T.Zero; x.T1_5 = T.Zero; x.T1_6 = T.Zero; x.T1_7 = T.Zero;
        x.T2_0 = T.Zero; x.T2_1 = T.Zero; x.T2_2 = T.Zero; x.T2_3 = T.Zero; x.T2_4 = T.Zero; x.T2_5 = T.Zero; x.T2_6 = T.Zero; x.T2_7 = T.Zero;
        x.T3_0 = T.Zero; x.T3_1 = T.Zero; x.T3_2 = T.Zero; x.T3_3 = T.Zero; x.T3_4 = T.Zero; x.T3_5 = T.Zero; x.T3_6 = T.Zero; x.T3_7 = T.Zero;
        x.T4_0 = T.Zero; x.T4_1 = T.Zero; x.T4_2 = T.Zero; x.T4_3 = T.Zero; x.T4_4 = T.Zero; x.T4_5 = T.Zero; x.T4_6 = T.Zero; x.T4_7 = T.Zero;
        x.T5_0 = T.Zero; x.T5_1 = T.Zero; x.T5_2 = T.Zero; x.T5_3 = T.Zero; x.T5_4 = T.Zero; x.T5_5 = T.Zero; x.T5_6 = T.Zero; x.T5_7 = T.Zero;
        x.T6_0 = T.Zero; x.T6_1 = T.Zero; x.T6_2 = T.Zero; x.T6_3 = T.Zero; x.T6_4 = T.Zero; x.T6_5 = T.Zero; x.T6_6 = T.Zero; x.T6_7 = T.Zero;
        x.T7_0 = T.Zero; x.T7_1 = T.Zero; x.T7_2 = T.Zero; x.T7_3 = T.Zero; x.T7_4 = T.Zero; x.T7_5 = T.Zero; x.T7_6 = T.Zero; x.T7_7 = T.Zero;
        for (var i = 0; i < Vector<T>.Count; ++i)
        {
            x.T0_0 += V0_0[i]; x.T0_1 += V0_1[i]; x.T0_2 += V0_2[i]; x.T0_3 += V0_3[i]; x.T0_4 += V0_4[i]; x.T0_5 += V0_5[i]; x.T0_6 += V0_6[i]; x.T0_7 += V0_7[i];
            x.T1_0 += V1_0[i]; x.T1_1 += V1_1[i]; x.T1_2 += V1_2[i]; x.T1_3 += V1_3[i]; x.T1_4 += V1_4[i]; x.T1_5 += V1_5[i]; x.T1_6 += V1_6[i]; x.T1_7 += V1_7[i];
            x.T2_0 += V2_0[i]; x.T2_1 += V2_1[i]; x.T2_2 += V2_2[i]; x.T2_3 += V2_3[i]; x.T2_4 += V2_4[i]; x.T2_5 += V2_5[i]; x.T2_6 += V2_6[i]; x.T2_7 += V2_7[i];
            x.T3_0 += V3_0[i]; x.T3_1 += V3_1[i]; x.T3_2 += V3_2[i]; x.T3_3 += V3_3[i]; x.T3_4 += V3_4[i]; x.T3_5 += V3_5[i]; x.T3_6 += V3_6[i]; x.T3_7 += V3_7[i];
            x.T4_0 += V4_0[i]; x.T4_1 += V4_1[i]; x.T4_2 += V4_2[i]; x.T4_3 += V4_3[i]; x.T4_4 += V4_4[i]; x.T4_5 += V4_5[i]; x.T4_6 += V4_6[i]; x.T4_7 += V4_7[i];
            x.T5_0 += V5_0[i]; x.T5_1 += V5_1[i]; x.T5_2 += V5_2[i]; x.T5_3 += V5_3[i]; x.T5_4 += V5_4[i]; x.T5_5 += V5_5[i]; x.T5_6 += V5_6[i]; x.T5_7 += V5_7[i];
            x.T6_0 += V6_0[i]; x.T6_1 += V6_1[i]; x.T6_2 += V6_2[i]; x.T6_3 += V6_3[i]; x.T6_4 += V6_4[i]; x.T6_5 += V6_5[i]; x.T6_6 += V6_6[i]; x.T6_7 += V6_7[i];
            x.T7_0 += V7_0[i]; x.T7_1 += V7_1[i]; x.T7_2 += V7_2[i]; x.T7_3 += V7_3[i]; x.T7_4 += V7_4[i]; x.T7_5 += V7_5[i]; x.T7_6 += V7_6[i]; x.T7_7 += V7_7[i];
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

        var rowsPerIteration = Rows;
        var rowIterationsCount = (rows+rowsPerIteration-1) / rowsPerIteration;
        var rowsBatchesPerIteration = (rowIterationsCount+ProcessorCount-1) / ProcessorCount;
        var batchesCount = (rowIterationsCount+rowsBatchesPerIteration-1) / rowsBatchesPerIteration; 
        
        Parallel.For(0, batchesCount, parallelForIdx => {
            var startIdx = parallelForIdx * rowsBatchesPerIteration * rowsPerIteration;
            var endIdx = Math.Min(rows, (parallelForIdx+1) * rowsBatchesPerIteration * rowsPerIteration);

            var r = startIdx;
            for (; r < endIdx - rowsPerIteration + 1; r += rowsPerIteration)
            {
                Args args = new();
                var A0 = A[r+0].AsSpan();
                var A1 = A[r+1].AsSpan();
                var A2 = A[r+2].AsSpan();
                var A3 = A[r+3].AsSpan();
                var A4 = A[r+4].AsSpan();
                var A5 = A[r+5].AsSpan();
                var A6 = A[r+6].AsSpan();
                var A7 = A[r+7].AsSpan();
                args.A0 = MemoryMarshal.Cast<T, Vector<T>>(A0);
                args.A1 = MemoryMarshal.Cast<T, Vector<T>>(A1);
                args.A2 = MemoryMarshal.Cast<T, Vector<T>>(A2);
                args.A3 = MemoryMarshal.Cast<T, Vector<T>>(A3);
                args.A4 = MemoryMarshal.Cast<T, Vector<T>>(A4);
                args.A5 = MemoryMarshal.Cast<T, Vector<T>>(A5);
                args.A6 = MemoryMarshal.Cast<T, Vector<T>>(A6);
                args.A7 = MemoryMarshal.Cast<T, Vector<T>>(A7);
                var c = 0;
                for (; c < columns - Cols + 1; c += Cols)
                {
                    var BT0 = BT[c+0].AsSpan();
                    var BT1 = BT[c+1].AsSpan();
                    var BT2 = BT[c+2].AsSpan();
                    var BT3 = BT[c+3].AsSpan();
                    var BT4 = BT[c+4].AsSpan();
                    var BT5 = BT[c+5].AsSpan();
                    var BT6 = BT[c+6].AsSpan();
                    var BT7 = BT[c+7].AsSpan();
                    args.BT0 = MemoryMarshal.Cast<T, Vector<T>>(BT0);
                    args.BT1 = MemoryMarshal.Cast<T, Vector<T>>(BT1);
                    args.BT2 = MemoryMarshal.Cast<T, Vector<T>>(BT2);
                    args.BT3 = MemoryMarshal.Cast<T, Vector<T>>(BT3);
                    args.BT4 = MemoryMarshal.Cast<T, Vector<T>>(BT4);
                    args.BT5 = MemoryMarshal.Cast<T, Vector<T>>(BT5);
                    args.BT6 = MemoryMarshal.Cast<T, Vector<T>>(BT6);
                    args.BT7 = MemoryMarshal.Cast<T, Vector<T>>(BT7);

                    InnerLoop(args, out var x);

                    for (var i = args.A0.Length * Vector<T>.Count; i < A0.Length; ++i)
                    {
                        x.T0_0 += A0[i] * BT0[i]; x.T0_1 += A0[i] * BT1[i]; x.T0_2 += A0[i] * BT2[i]; x.T0_3 += A0[i] * BT3[i]; x.T0_4 += A0[i] * BT4[i]; x.T0_5 += A0[i] * BT5[i]; x.T0_6 += A0[i] * BT6[i]; x.T0_7 += A0[i] * BT7[i];
                        x.T1_0 += A1[i] * BT0[i]; x.T1_1 += A1[i] * BT1[i]; x.T1_2 += A1[i] * BT2[i]; x.T1_3 += A1[i] * BT3[i]; x.T1_4 += A1[i] * BT4[i]; x.T1_5 += A1[i] * BT5[i]; x.T1_6 += A1[i] * BT6[i]; x.T1_7 += A1[i] * BT7[i];
                        x.T2_0 += A2[i] * BT0[i]; x.T2_1 += A2[i] * BT1[i]; x.T2_2 += A2[i] * BT2[i]; x.T2_3 += A2[i] * BT3[i]; x.T2_4 += A2[i] * BT4[i]; x.T2_5 += A2[i] * BT5[i]; x.T2_6 += A2[i] * BT6[i]; x.T2_7 += A2[i] * BT7[i];
                        x.T3_0 += A3[i] * BT0[i]; x.T3_1 += A3[i] * BT1[i]; x.T3_2 += A3[i] * BT2[i]; x.T3_3 += A3[i] * BT3[i]; x.T3_4 += A3[i] * BT4[i]; x.T3_5 += A3[i] * BT5[i]; x.T3_6 += A3[i] * BT6[i]; x.T3_7 += A3[i] * BT7[i];
                        x.T4_0 += A4[i] * BT0[i]; x.T4_1 += A4[i] * BT1[i]; x.T4_2 += A4[i] * BT2[i]; x.T4_3 += A4[i] * BT3[i]; x.T4_4 += A4[i] * BT4[i]; x.T4_5 += A4[i] * BT5[i]; x.T4_6 += A4[i] * BT6[i]; x.T4_7 += A4[i] * BT7[i];
                        x.T5_0 += A5[i] * BT0[i]; x.T5_1 += A5[i] * BT1[i]; x.T5_2 += A5[i] * BT2[i]; x.T5_3 += A5[i] * BT3[i]; x.T5_4 += A5[i] * BT4[i]; x.T5_5 += A5[i] * BT5[i]; x.T5_6 += A5[i] * BT6[i]; x.T5_7 += A5[i] * BT7[i];
                        x.T6_0 += A6[i] * BT0[i]; x.T6_1 += A6[i] * BT1[i]; x.T6_2 += A6[i] * BT2[i]; x.T6_3 += A6[i] * BT3[i]; x.T6_4 += A6[i] * BT4[i]; x.T6_5 += A6[i] * BT5[i]; x.T6_6 += A6[i] * BT6[i]; x.T6_7 += A6[i] * BT7[i];
                        x.T7_0 += A7[i] * BT0[i]; x.T7_1 += A7[i] * BT1[i]; x.T7_2 += A7[i] * BT2[i]; x.T7_3 += A7[i] * BT3[i]; x.T7_4 += A7[i] * BT4[i]; x.T7_5 += A7[i] * BT5[i]; x.T7_6 += A7[i] * BT6[i]; x.T7_7 += A7[i] * BT7[i];
                    }
                    result[r+0][c+0] = x.T0_0; result[r+0][c+1] = x.T0_1; result[r+0][c+2] = x.T0_2; result[r+0][c+3] = x.T0_3; result[r+0][c+4] = x.T0_4; result[r+0][c+5] = x.T0_5; result[r+0][c+6] = x.T0_6; result[r+0][c+7] = x.T0_7;
                    result[r+1][c+0] = x.T1_0; result[r+1][c+1] = x.T1_1; result[r+1][c+2] = x.T1_2; result[r+1][c+3] = x.T1_3; result[r+1][c+4] = x.T1_4; result[r+1][c+5] = x.T1_5; result[r+1][c+6] = x.T1_6; result[r+1][c+7] = x.T1_7;
                    result[r+2][c+0] = x.T2_0; result[r+2][c+1] = x.T2_1; result[r+2][c+2] = x.T2_2; result[r+2][c+3] = x.T2_3; result[r+2][c+4] = x.T2_4; result[r+2][c+5] = x.T2_5; result[r+2][c+6] = x.T2_6; result[r+2][c+7] = x.T2_7;
                    result[r+3][c+0] = x.T3_0; result[r+3][c+1] = x.T3_1; result[r+3][c+2] = x.T3_2; result[r+3][c+3] = x.T3_3; result[r+3][c+4] = x.T3_4; result[r+3][c+5] = x.T3_5; result[r+3][c+6] = x.T3_6; result[r+3][c+7] = x.T3_7;
                    result[r+4][c+0] = x.T4_0; result[r+4][c+1] = x.T4_1; result[r+4][c+2] = x.T4_2; result[r+4][c+3] = x.T4_3; result[r+4][c+4] = x.T4_4; result[r+4][c+5] = x.T4_5; result[r+4][c+6] = x.T4_6; result[r+4][c+7] = x.T4_7;
                    result[r+5][c+0] = x.T5_0; result[r+5][c+1] = x.T5_1; result[r+5][c+2] = x.T5_2; result[r+5][c+3] = x.T5_3; result[r+5][c+4] = x.T5_4; result[r+5][c+5] = x.T5_5; result[r+5][c+6] = x.T5_6; result[r+5][c+7] = x.T5_7;
                    result[r+6][c+0] = x.T6_0; result[r+6][c+1] = x.T6_1; result[r+6][c+2] = x.T6_2; result[r+6][c+3] = x.T6_3; result[r+6][c+4] = x.T6_4; result[r+6][c+5] = x.T6_5; result[r+6][c+6] = x.T6_6; result[r+6][c+7] = x.T6_7;
                    result[r+7][c+0] = x.T7_0; result[r+7][c+1] = x.T7_1; result[r+7][c+2] = x.T7_2; result[r+7][c+3] = x.T7_3; result[r+7][c+4] = x.T7_4; result[r+7][c+5] = x.T7_5; result[r+7][c+6] = x.T7_6; result[r+7][c+7] = x.T7_7;
                }

                for (; c < columns; c++)
                {
                    var BTRow = BT[c].AsSpan();
                    var BTRowVector = MemoryMarshal.Cast<T, Vector<T>>(BTRow);
                    var V0 = Vector<T>.Zero;
                    var V1 = Vector<T>.Zero;
                    var V2 = Vector<T>.Zero;
                    var V3 = Vector<T>.Zero;
                    var V4 = Vector<T>.Zero;
                    var V5 = Vector<T>.Zero;
                    var V6 = Vector<T>.Zero;
                    var V7 = Vector<T>.Zero;
                    for (var i = 0; i < args.A0.Length; ++i)
                    {
                        V0 += args.A0[i] * BTRowVector[i];
                        V1 += args.A1[i] * BTRowVector[i];
                        V2 += args.A2[i] * BTRowVector[i];
                        V3 += args.A3[i] * BTRowVector[i];
                        V4 += args.A4[i] * BTRowVector[i];
                        V5 += args.A5[i] * BTRowVector[i];
                        V6 += args.A6[i] * BTRowVector[i];
                        V7 += args.A7[i] * BTRowVector[i];
                    }
                    var tmp0 = T.Zero;
                    var tmp1 = T.Zero;
                    var tmp2 = T.Zero;
                    var tmp3 = T.Zero;
                    var tmp4 = T.Zero;
                    var tmp5 = T.Zero;
                    var tmp6 = T.Zero;
                    var tmp7 = T.Zero;
                    for (var i = 0; i < Vector<T>.Count; ++i)
                    {
                        tmp0 += V0[i];
                        tmp1 += V1[i];
                        tmp2 += V2[i];
                        tmp3 += V3[i];
                        tmp4 += V4[i];
                        tmp5 += V5[i];
                        tmp6 += V6[i];
                        tmp7 += V7[i];
                    }
                    for (var i = args.A0.Length * Vector<T>.Count; i < A0.Length; ++i)
                    {
                        tmp0 += A0[i] * BTRow[i];
                        tmp1 += A1[i] * BTRow[i];
                        tmp2 += A2[i] * BTRow[i];
                        tmp3 += A3[i] * BTRow[i];
                        tmp4 += A4[i] * BTRow[i];
                        tmp5 += A5[i] * BTRow[i];
                        tmp6 += A6[i] * BTRow[i];
                        tmp7 += A7[i] * BTRow[i];
                    }
                    result[r+0][c] = tmp0;
                    result[r+1][c] = tmp1;
                    result[r+2][c] = tmp2;
                    result[r+3][c] = tmp3;
                    result[r+4][c] = tmp4;
                    result[r+5][c] = tmp5;
                    result[r+6][c] = tmp6;
                    result[r+7][c] = tmp7;
                }
            }
            for (; r < endIdx; ++r)
            {
                var ARow = A[r].AsSpan();
                var ARowVector = MemoryMarshal.Cast<T, Vector<T>>(ARow);
                for (var column = 0; column < columns; column++)
                {
                    var BTRow = BT[column].AsSpan();
                    var BTRowVector = MemoryMarshal.Cast<T, Vector<T>>(BTRow);
                    var V = Vector<T>.Zero;
                    for (var i = 0; i < ARowVector.Length; ++i)
                        V += ARowVector[i] * BTRowVector[i];
                    var tmp = T.Zero;
                    for (var i = 0; i < Vector<T>.Count; ++i)
                        tmp += V[i];
                    for (var i = ARowVector.Length * Vector<T>.Count; i < ARow.Length; ++i)
                        tmp += ARow[i] * BTRow[i];
                    result[r][column] = tmp;
                }
            }
        });

        return result;
    }        
}
