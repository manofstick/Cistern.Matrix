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


// |    Method | Size |             Mean |          Error |         StdDev | Ratio | RatioSD |
// |---------- |----- |-----------------:|---------------:|---------------:|------:|--------:|
// |       Mlk |   16 |         3.377 us |      0.0637 us |      0.0625 us |  1.00 |    0.00 |
// | FiveThree |   16 |         4.021 us |      0.0302 us |      0.0268 us |  1.19 |    0.02 |
// |           |      |                  |                |                |       |         |
// |       Mlk |   32 |        11.175 us |      0.1253 us |      0.1172 us |  1.00 |    0.00 |
// | FiveThree |   32 |         8.575 us |      0.0613 us |      0.0543 us |  0.77 |    0.01 |
// |           |      |                  |                |                |       |         |
// |       Mlk |   64 |        72.324 us |      1.4365 us |      1.7101 us |  1.00 |    0.00 |
// | FiveThree |   64 |        23.548 us |      0.1133 us |      0.0946 us |  0.32 |    0.01 |
// |           |      |                  |                |                |       |         |
// |       Mlk |  128 |       351.486 us |     12.7122 us |     37.4822 us |  1.00 |    0.00 |
// | FiveThree |  128 |       105.434 us |      1.6013 us |      1.4978 us |  0.28 |    0.02 |
// |           |      |                  |                |                |       |         |
// |       Mlk |  256 |     1,415.847 us |     27.7876 us |     43.2619 us |  1.00 |    0.00 |
// | FiveThree |  256 |       712.571 us |     13.3031 us |     12.4437 us |  0.50 |    0.02 |
// |           |      |                  |                |                |       |         |
// |       Mlk |  512 |     8,091.646 us |    179.2323 us |    528.4705 us |  1.00 |    0.00 |
// | FiveThree |  512 |     4,203.595 us |     83.7231 us |    236.1424 us |  0.52 |    0.04 |
// |           |      |                  |                |                |       |         |
// |       Mlk | 1024 |    34,480.963 us |    270.7440 us |    253.2541 us |  1.00 |    0.00 |
// | FiveThree | 1024 |    29,204.962 us |    579.3579 us |    901.9905 us |  0.83 |    0.04 |
// |           |      |                  |                |                |       |         |
// |       Mlk | 2048 |   212,618.305 us |  1,087.2553 us |    963.8235 us |  1.00 |    0.00 |
// | FiveThree | 2048 |   234,808.803 us |  3,094.2375 us |  2,894.3517 us |  1.11 |    0.01 |
// |           |      |                  |                |                |       |         |
// |       Mlk | 4096 | 1,216,673.895 us | 23,728.0967 us | 35,515.0979 us |  1.00 |    0.00 |
// | FiveThree | 4096 | 1,813,418.061 us | 35,302.3972 us | 52,838.9660 us |  1.49 |    0.07 |



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

        EqualOrThrow(expected, SixThree());
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
    public double[][] SixThree() 
    {
        var result = Multiplication<double>.Multiply(A, B);
        return result;
    }
}