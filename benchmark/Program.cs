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
// |    Mlk |   16 |         3.414 us |      0.0589 us |      0.0551 us |  1.00 |    0.00 |
// | SixSix |   16 |         3.938 us |      0.0424 us |      0.0354 us |  1.15 |    0.02 |
// |        |      |                  |                |                |       |         |
// |    Mlk |   32 |        11.086 us |      0.1477 us |      0.1233 us |  1.00 |    0.00 |
// | SixSix |   32 |         8.947 us |      0.0422 us |      0.0374 us |  0.81 |    0.01 |
// |        |      |                  |                |                |       |         |
// |    Mlk |   64 |        70.044 us |      1.2800 us |      1.1347 us |  1.00 |    0.00 |
// | SixSix |   64 |        24.299 us |      0.0882 us |      0.0688 us |  0.35 |    0.01 |
// |        |      |                  |                |                |       |         |
// |    Mlk |  128 |       366.147 us |     12.8971 us |     38.0274 us |  1.00 |    0.00 |
// | SixSix |  128 |       118.929 us |      1.6709 us |      1.3953 us |  0.31 |    0.04 |
// |        |      |                  |                |                |       |         |
// |    Mlk |  256 |     1,408.355 us |     28.1126 us |     33.4660 us |  1.00 |    0.00 |
// | SixSix |  256 |       745.459 us |     13.7402 us |     12.8526 us |  0.53 |    0.02 |
// |        |      |                  |                |                |       |         |
// |    Mlk |  512 |     8,117.802 us |    182.6804 us |    538.6372 us |  1.00 |    0.00 |
// | SixSix |  512 |     5,113.772 us |    130.8022 us |    385.6732 us |  0.63 |    0.07 |
// |        |      |                  |                |                |       |         |
// |    Mlk | 1024 |    34,939.857 us |    648.4272 us |    606.5392 us |  1.00 |    0.00 |
// | SixSix | 1024 |    27,885.069 us |    553.6036 us |  1,039.8018 us |  0.78 |    0.04 |
// |        |      |                  |                |                |       |         |
// |    Mlk | 2048 |   224,556.310 us |  2,053.9061 us |  1,921.2250 us |  1.00 |    0.00 |
// | SixSix | 2048 |   223,408.791 us |  3,501.5279 us |  3,275.3314 us |  0.99 |    0.02 |
// |        |      |                  |                |                |       |         |
// |    Mlk | 4096 | 1,228,109.955 us |  6,747.6845 us |  5,268.1482 us |  1.00 |    0.00 |
// | SixSix | 4096 | 1,703,089.643 us | 33,724.5477 us | 54,458.8582 us |  1.39 |    0.04 |



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