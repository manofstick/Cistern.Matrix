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
//
// | Method | Size |             Mean |          Error |         StdDev |           Median | Ratio | RatioSD |
// |------- |----- |-----------------:|---------------:|---------------:|-----------------:|------:|--------:|
// |    Mlk |   16 |         3.406 us |      0.0464 us |      0.0434 us |         3.406 us |  1.00 |    0.00 |
// | SixSix |   16 |         4.053 us |      0.0800 us |      0.1706 us |         3.954 us |  1.19 |    0.04 |
// |        |      |                  |                |                |                  |       |         |
// |    Mlk |   32 |        11.018 us |      0.1769 us |      0.1655 us |        10.956 us |  1.00 |    0.00 |
// | SixSix |   32 |        10.069 us |      0.1891 us |      0.2589 us |         9.974 us |  0.91 |    0.02 |
// |        |      |                  |                |                |                  |       |         |
// |    Mlk |   64 |        73.026 us |      1.4440 us |      2.3318 us |        72.683 us |  1.00 |    0.00 |
// | SixSix |   64 |        28.501 us |      0.2253 us |      0.1881 us |        28.527 us |  0.39 |    0.01 |
// |        |      |                  |                |                |                  |       |         |
// |    Mlk |  128 |       336.465 us |     12.8966 us |     38.0258 us |       335.503 us |  1.00 |    0.00 |
// | SixSix |  128 |       133.996 us |      2.5859 us |      2.8742 us |       133.269 us |  0.42 |    0.05 |
// |        |      |                  |                |                |                  |       |         |
// |    Mlk |  256 |     1,517.010 us |     30.1390 us |     48.6688 us |     1,517.476 us |  1.00 |    0.00 |
// | SixSix |  256 |       895.798 us |     16.2942 us |     27.6687 us |       888.562 us |  0.59 |    0.03 |
// |        |      |                  |                |                |                  |       |         |
// |    Mlk |  512 |     9,053.272 us |    263.5428 us |    777.0618 us |     9,280.979 us |  1.00 |    0.00 |
// | SixSix |  512 |     4,468.140 us |     89.0670 us |    255.5498 us |     4,439.134 us |  0.49 |    0.05 |
// |        |      |                  |                |                |                  |       |         |
// |    Mlk | 1024 |    40,621.967 us |  2,091.2375 us |  6,166.0609 us |    39,755.793 us |  1.00 |    0.00 |
// | SixSix | 1024 |    27,031.751 us |    161.9703 us |    143.5825 us |    27,001.684 us |  0.58 |    0.02 |
// |        |      |                  |                |                |                  |       |         |
// |    Mlk | 2048 |   223,201.968 us |  2,571.3208 us |  2,405.2151 us |   222,422.356 us |  1.00 |    0.00 |
// | SixSix | 2048 |   207,325.192 us |  2,286.4180 us |  2,026.8500 us |   207,085.888 us |  0.93 |    0.01 |
// |        |      |                  |                |                |                  |       |         |
// |    Mlk | 4096 | 1,365,824.023 us | 24,189.7069 us | 24,841.0480 us | 1,365,738.219 us |  1.00 |    0.00 |
// | SixSix | 4096 | 1,636,526.371 us | 24,257.0064 us | 21,503.2043 us | 1,642,114.004 us |  1.20 |    0.03 |



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