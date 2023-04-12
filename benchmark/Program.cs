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
// |       Mlk |   16 |         3.440 us |      0.0595 us |      0.0557 us |  1.00 |    0.00 |
// | FourThree |   16 |         4.116 us |      0.0396 us |      0.0310 us |  1.19 |    0.02 |
// |           |      |                  |                |                |       |         |
// |       Mlk |   32 |        11.205 us |      0.1452 us |      0.1287 us |  1.00 |    0.00 |
// | FourThree |   32 |         8.723 us |      0.0550 us |      0.0429 us |  0.78 |    0.01 |
// |           |      |                  |                |                |       |         |
// |       Mlk |   64 |        72.310 us |      1.4037 us |      1.3787 us |  1.00 |    0.00 |
// | FourThree |   64 |        24.351 us |      0.2431 us |      0.2155 us |  0.34 |    0.01 |
// |           |      |                  |                |                |       |         |
// |       Mlk |  128 |       349.724 us |     12.1734 us |     35.8936 us |  1.00 |    0.00 |
// | FourThree |  128 |       109.187 us |      1.2094 us |      1.1313 us |  0.29 |    0.02 |
// |           |      |                  |                |                |       |         |
// |       Mlk |  256 |     1,404.929 us |     27.5375 us |     35.8065 us |  1.00 |    0.00 |
// | FourThree |  256 |       709.576 us |      7.6586 us |      7.1639 us |  0.51 |    0.02 |
// |           |      |                  |                |                |       |         |
// |       Mlk |  512 |     8,179.066 us |    161.5626 us |    387.0936 us |  1.00 |    0.00 |
// | FourThree |  512 |     4,146.243 us |     90.7850 us |    267.6817 us |  0.50 |    0.04 |
// |           |      |                  |                |                |       |         |
// |       Mlk | 1024 |    35,821.914 us |    708.6660 us |  1,555.5391 us |  1.00 |    0.00 |
// | FourThree | 1024 |    28,138.489 us |    557.2877 us |  1,032.9684 us |  0.78 |    0.05 |
// |           |      |                  |                |                |       |         |
// |       Mlk | 2048 |   221,550.597 us |  1,837.3656 us |  1,718.6729 us |  1.00 |    0.00 |
// | FourThree | 2048 |   230,975.782 us |  4,400.8255 us |  4,322.1991 us |  1.04 |    0.02 |
// |           |      |                  |                |                |       |         |
// |       Mlk | 4096 | 1,280,207.056 us | 25,515.8199 us | 34,062.8866 us |  1.00 |    0.00 |
// | FourThree | 4096 | 1,913,193.819 us | 38,224.3966 us | 56,028.8250 us |  1.50 |    0.06 |


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

        EqualOrThrow(expected, FiveThree());
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
    public double[][] FiveThree() 
    {
        var result = Multiplication<double>.Multiply(A, B);
        return result;
    }
}