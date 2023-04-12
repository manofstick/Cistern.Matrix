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


// |   Method | Size |             Mean |          Error |         StdDev |           Median | Ratio | RatioSD |
// |--------- |----- |-----------------:|---------------:|---------------:|-----------------:|------:|--------:|
// |      Mlk |   16 |         3.404 us |      0.0387 us |      0.0362 us |         3.401 us |  1.00 |    0.00 |
// | SixThree |   16 |         3.947 us |      0.0461 us |      0.0409 us |         3.934 us |  1.16 |    0.02 |
// |          |      |                  |                |                |                  |       |         |
// |      Mlk |   32 |        11.138 us |      0.1312 us |      0.1228 us |        11.114 us |  1.00 |    0.00 |
// | SixThree |   32 |         8.601 us |      0.0781 us |      0.0692 us |         8.571 us |  0.77 |    0.01 |
// |          |      |                  |                |                |                  |       |         |
// |      Mlk |   64 |        72.269 us |      1.4376 us |      1.9192 us |        72.284 us |  1.00 |    0.00 |
// | SixThree |   64 |        24.329 us |      0.0645 us |      0.0572 us |        24.339 us |  0.33 |    0.01 |
// |          |      |                  |                |                |                  |       |         |
// |      Mlk |  128 |       354.824 us |     13.0750 us |     38.5519 us |       352.145 us |  1.00 |    0.00 |
// | SixThree |  128 |       107.633 us |      1.7935 us |      1.6777 us |       107.648 us |  0.29 |    0.03 |
// |          |      |                  |                |                |                  |       |         |
// |      Mlk |  256 |     1,399.640 us |     24.8282 us |     33.9852 us |     1,385.323 us |  1.00 |    0.00 |
// | SixThree |  256 |       740.569 us |      8.7869 us |      7.7894 us |       743.230 us |  0.53 |    0.01 |
// |          |      |                  |                |                |                  |       |         |
// |      Mlk |  512 |     8,121.539 us |    191.6598 us |    565.1134 us |     8,135.032 us |  1.00 |    0.00 |
// | SixThree |  512 |     4,734.269 us |    142.4357 us |    419.9749 us |     4,729.093 us |  0.59 |    0.07 |
// |          |      |                  |                |                |                  |       |         |
// |      Mlk | 1024 |    35,894.401 us |    713.9747 us |  1,737.9132 us |    35,551.733 us |  1.00 |    0.00 |
// | SixThree | 1024 |    27,630.748 us |    544.2382 us |  1,061.4948 us |    27,969.999 us |  0.76 |    0.04 |
// |          |      |                  |                |                |                  |       |         |
// |      Mlk | 2048 |   224,815.497 us |  1,358.9447 us |  1,271.1577 us |   224,413.231 us |  1.00 |    0.00 |
// | SixThree | 2048 |   219,254.111 us |  3,675.1111 us |  3,437.7012 us |   220,180.746 us |  0.98 |    0.02 |
// |          |      |                  |                |                |                  |       |         |
// |      Mlk | 4096 | 1,271,955.953 us | 24,916.7709 us | 34,106.3573 us | 1,291,641.864 us |  1.00 |    0.00 |
// | SixThree | 4096 | 1,718,712.861 us | 33,802.3646 us | 53,614.1324 us | 1,725,532.018 us |  1.35 |    0.06 |




public class Matrices
{
    [Params(16, 32, 64, 128, 256, 512, 1024, 2048, 4096)]
    public int Size;

    public int L { get; set; }
    public int M { get; set; }
    public int N { get; set; }


    double[][] A;
    double[][] B;

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