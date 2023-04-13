using System.Numerics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Cistern.Matrix;

var summary = BenchmarkRunner.Run<Matrices>();

// BenchmarkDotNet=v0.13.5, OS=ubuntu 22.04
// 12th Gen Intel Core i5-1240P, 1 CPU, 16 logical and 12 physical cores
// .NET SDK=7.0.201
//   [Host]     : .NET 7.0.3 (7.0.323.6910), X64 RyuJIT AVX2
//   DefaultJob : .NET 7.0.3 (7.0.323.6910), X64 RyuJIT AVX2


// | Method | Size |             Mean |          Error |         StdDev | Ratio | RatioSD |
// |------- |----- |-----------------:|---------------:|---------------:|------:|--------:|
// |    Mlk |   16 |         3.415 us |      0.0566 us |      0.0529 us |  1.00 |    0.00 |
// | SixSix |   16 |         3.942 us |      0.0286 us |      0.0253 us |  1.15 |    0.02 |
// |        |      |                  |                |                |       |         |
// |    Mlk |   32 |        11.064 us |      0.1520 us |      0.1422 us |  1.00 |    0.00 |
// | SixSix |   32 |         8.806 us |      0.0482 us |      0.0402 us |  0.80 |    0.01 |
// |        |      |                  |                |                |       |         |
// |    Mlk |   64 |        71.894 us |      1.3841 us |      1.9850 us |  1.00 |    0.00 |
// | SixSix |   64 |        24.620 us |      0.2094 us |      0.1856 us |  0.34 |    0.01 |
// |        |      |                  |                |                |       |         |
// |    Mlk |  128 |       346.256 us |     12.9472 us |     38.1751 us |  1.00 |    0.00 |
// | SixSix |  128 |       108.635 us |      0.8975 us |      0.8396 us |  0.31 |    0.03 |
// |        |      |                  |                |                |       |         |
// |    Mlk |  256 |     1,391.417 us |     26.7926 us |     32.9037 us |  1.00 |    0.00 |
// | SixSix |  256 |       744.932 us |      9.9372 us |      9.2953 us |  0.53 |    0.01 |
// |        |      |                  |                |                |       |         |
// |    Mlk |  512 |     8,297.379 us |    195.4384 us |    576.2547 us |  1.00 |    0.00 |
// | SixSix |  512 |     4,416.129 us |     96.3518 us |    279.5341 us |  0.54 |    0.05 |
// |        |      |                  |                |                |       |         |
// |    Mlk | 1024 |    38,832.652 us |  1,179.8653 us |  3,478.8595 us |  1.00 |    0.00 |
// | SixSix | 1024 |    26,086.377 us |    516.9180 us |  1,268.0086 us |  0.69 |    0.08 |
// |        |      |                  |                |                |       |         |
// |    Mlk | 2048 |   218,414.801 us |  1,438.3900 us |  1,345.4709 us |  1.00 |    0.00 |
// | SixSix | 2048 |   206,362.595 us |  2,151.8194 us |  1,907.5319 us |  0.94 |    0.01 |
// |        |      |                  |                |                |       |         |
// |    Mlk | 4096 | 1,286,040.549 us |  7,154.3866 us |  6,342.1774 us |  1.00 |    0.00 |
// | SixSix | 4096 | 1,617,340.187 us | 31,538.9503 us | 50,929.5259 us |  1.24 |    0.04 |


public class Matrices
{
    [Params(16, 32, 64, 128, 256, 512, 1024, 2048, 4096)]
    public int Size;

    public int L { get; set; }
    public int M { get; set; }
    public int N { get; set; }


    double[][] A = null!;
    double[][] B = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        L = Size;
        M = Size;
        N = Size;

        var r = new Random(42);

        A = Utils<double>.Create(L, M, (_, _) => r.NextDouble());
        B = Utils<double>.Create(M, N, (_, _) => r.NextDouble());

        Validate();
    }

    public static void EqualOrThrow(double[][] expected, double[][] actual)
    {
        if (expected.Length != actual.Length)
            throw new Exception("expected.Length != actual.Length");

        for(var i=0; i < expected.Length; ++i)
        {
            var e = expected[i];
            var a = actual[i];

            if (e.Length != a.Length)
                throw new Exception("e.Length != a.Length");

            for(var j=0; j < e.Length; ++j)
            {
                var diff = Math.Abs(e[j]-a[j]);
                if (diff > 0.00000000001)
                    throw new Exception($"e[j] != a[j] ({e[j]} != {a[j]})");
            }
        }
    }

    public void Validate()
    {
        var expected = Mlk();

        EqualOrThrow(expected, SixSix());
    }

    [Benchmark(Baseline=true)]
    public double[][] Mlk() 
    {
        int i,j;

        var m = A.Length;
        var n = A[0].Length;
        var p = B.Length;
        var q = B[0].Length;

        var aa = new MKLNET.matrix(m, n, (x, y) => A[x][y]);
        var bb = new MKLNET.matrix(p, q, (x, y) => B[x][y]);

        var cc = (aa * bb).Evaluate();

        double[][] c = new double[m][];
        for (i=0; i<m; ++i)
            c[i] = new double[q];

        for (i = 0; i < m; i++)
        {
            for (j = 0; j < n; j++)
            {
                c[i][j] = cc[i,j];
            }
        }

        return c;
    }

    [Benchmark]
    public double[][] SixSix() 
    {
        var result = Multiplication<double>.Multiply(A, B);
        return result;
    }
}