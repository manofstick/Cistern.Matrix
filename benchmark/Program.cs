using System.Numerics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Cistern.Matrix;

#if truex
    var m = new Matrices();
    m.L = 128;
    m.M = 128; //10;
    m.N = 128;
    m.GlobalSetup();
    return;
#endif

var summary = BenchmarkRunner.Run<Matrices>();

// BenchmarkDotNet=v0.13.5, OS=ubuntu 22.04
// 12th Gen Intel Core i5-1240P, 1 CPU, 16 logical and 12 physical cores
// .NET SDK=7.0.201
//   [Host]     : .NET 7.0.3 (7.0.323.6910), X64 RyuJIT AVX2
//   DefaultJob : .NET 7.0.3 (7.0.323.6910), X64 RyuJIT AVX2


// | Method | Size |              Mean |           Error |          StdDev |            Median | Ratio | RatioSD |
// |------- |----- |------------------:|----------------:|----------------:|------------------:|------:|--------:|
// |    Mlk |   16 |          3.436 us |       0.0568 us |       0.0851 us |          3.413 us |  1.00 |    0.00 |
// | Stripe |   16 |          3.883 us |       0.0183 us |       0.0171 us |          3.886 us |  1.12 |    0.03 |
// |        |      |                   |                 |                 |                   |       |         |
// |    Mlk |   32 |         10.624 us |       0.1186 us |       0.1109 us |         10.631 us |  1.00 |    0.00 |
// | Stripe |   32 |         10.778 us |       0.1038 us |       0.0867 us |         10.759 us |  1.01 |    0.02 |
// |        |      |                   |                 |                 |                   |       |         |
// |    Mlk |   64 |         68.457 us |       1.3416 us |       1.7910 us |         68.925 us |  1.00 |    0.00 |
// | Stripe |   64 |         35.132 us |       0.1247 us |       0.1166 us |         35.131 us |  0.52 |    0.01 |
// |        |      |                   |                 |                 |                   |       |         |
// |    Mlk |  128 |        313.621 us |      11.1320 us |      32.8230 us |        312.316 us |  1.00 |    0.00 |
// | Stripe |  128 |        150.382 us |       0.9662 us |       0.9038 us |        150.384 us |  0.47 |    0.04 |
// |        |      |                   |                 |                 |                   |       |         |
// |    Mlk |  256 |      1,380.844 us |      26.9113 us |      37.7259 us |      1,389.198 us |  1.00 |    0.00 |
// | Stripe |  256 |        829.894 us |      11.8971 us |      11.1286 us |        831.125 us |  0.60 |    0.02 |
// |        |      |                   |                 |                 |                   |       |         |
// |    Mlk |  512 |      8,195.951 us |     245.9114 us |     725.0752 us |      8,209.742 us |  1.00 |    0.00 |
// | Stripe |  512 |      4,383.820 us |      83.7979 us |     171.1769 us |      4,359.471 us |  0.52 |    0.05 |
// |        |      |                   |                 |                 |                   |       |         |
// |    Mlk | 1024 |     37,576.200 us |   1,390.0292 us |   4,098.5323 us |     36,486.265 us |  1.00 |    0.00 |
// | Stripe | 1024 |     26,917.881 us |     450.8214 us |     421.6986 us |     26,852.488 us |  0.67 |    0.06 |
// |        |      |                   |                 |                 |                   |       |         |
// |    Mlk | 2048 |    215,904.774 us |   2,642.1830 us |   2,342.2264 us |    215,201.703 us |  1.00 |    0.00 |
// | Stripe | 2048 |    183,532.639 us |   3,618.4212 us |   5,415.8825 us |    184,736.025 us |  0.84 |    0.04 |
// |        |      |                   |                 |                 |                   |       |         |
// |    Mlk | 4096 |  1,218,540.173 us |  24,200.5280 us |  28,809.0071 us |  1,209,631.713 us |  1.00 |    0.00 |
// | Stripe | 4096 |  1,375,140.568 us |  26,983.8366 us |  43,573.8664 us |  1,370,477.948 us |  1.13 |    0.05 |
// |        |      |                   |                 |                 |                   |       |         |
// |    Mlk | 8192 |  7,537,006.471 us |  66,328.9045 us |  62,044.0984 us |  7,545,297.986 us |  1.00 |    0.00 |
// | Stripe | 8192 | 12,836,684.861 us | 256,211.5166 us | 448,733.8036 us | 12,951,972.575 us |  1.71 |    0.05 |




public class Matrices
{
    [Params(16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192)]
    public int Size;

    public int L { get; set; }
    public int M { get; set; }
    public int N { get; set; }


    double[][] A = null!;
    double[][] B = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
#if true
        L = Size;
        M = Size;
        N = Size;

        var r = new Random(42);
        A = Utils<double>.Create(L, M, (_, _) => r.NextDouble());
        B = Utils<double>.Create(M, N, (_, _) => r.NextDouble());
#else
        A = Utils<double>.Create(L, M, (r, c) => r*M+c);
        B = Utils<double>.Create(M, N, (r, c) => -(r*N+c));
#endif

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
                var diff = Math.Abs((e[j]-a[j]) / a[j]);
                if (diff > 0.0000000000001)
                    throw new Exception($"e[j] != a[j] ({e[j]} != {a[j]})");
            }
        }
    }

    public void Validate()
    {
        var expected = Mlk();

        EqualOrThrow(expected, EightEight());
        EqualOrThrow(expected, Stripe());
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
            for (j = 0; j < q; j++)
            {
                c[i][j] = cc[i,j];
            }
        }

        return c;
    }


    //[Benchmark]
    public double[][] EightEight() 
    {
        var result = Multiplication<double>.Multiply(A, B);

        return result;
    }


    // BenchmarkDotNet=v0.13.5, OS=ubuntu 22.04
    // 12th Gen Intel Core i5-1240P, 1 CPU, 16 logical and 12 physical cores
    // .NET SDK=7.0.201
    //   [Host]     : .NET 7.0.3 (7.0.323.6910), X64 RyuJIT AVX2
    //   DefaultJob : .NET 7.0.3 (7.0.323.6910), X64 RyuJIT AVX2
    //
    // | Method | Size |              Mean |           Error |          StdDev |            Median | Ratio | RatioSD |
    // |------- |----- |------------------:|----------------:|----------------:|------------------:|------:|--------:|
    // |    Mlk |   16 |          3.361 us |       0.0536 us |       0.0475 us |          3.350 us |  1.00 |    0.00 |
    // | Stripe |   16 |          4.035 us |       0.0327 us |       0.0290 us |          4.029 us |  1.20 |    0.02 |
    // |        |      |                   |                 |                 |                   |       |         |
    // |    Mlk |   32 |         10.833 us |       0.0951 us |       0.0794 us |         10.860 us |  1.00 |    0.00 |
    // | Stripe |   32 |         11.629 us |       0.1941 us |       0.1621 us |         11.579 us |  1.07 |    0.02 |
    // |        |      |                   |                 |                 |                   |       |         |
    // |    Mlk |   64 |         71.346 us |       1.4027 us |       1.5591 us |         71.434 us |  1.00 |    0.00 |
    // | Stripe |   64 |         39.217 us |       0.1181 us |       0.1104 us |         39.185 us |  0.55 |    0.01 |
    // |        |      |                   |                 |                 |                   |       |         |
    // |    Mlk |  128 |        318.269 us |      13.7868 us |      40.6506 us |        311.887 us |  1.00 |    0.00 |
    // | Stripe |  128 |        182.257 us |       0.8005 us |       0.6685 us |        182.232 us |  0.53 |    0.05 |
    // |        |      |                   |                 |                 |                   |       |         |
    // |    Mlk |  256 |      1,375.228 us |      26.4842 us |      42.0068 us |      1,373.367 us |  1.00 |    0.00 |
    // | Stripe |  256 |        973.932 us |      10.1674 us |       8.4902 us |        972.119 us |  0.71 |    0.02 |
    // |        |      |                   |                 |                 |                   |       |         |
    // |    Mlk |  512 |      8,178.014 us |     221.9218 us |     654.3413 us |      8,133.851 us |  1.00 |    0.00 |
    // | Stripe |  512 |      6,207.574 us |     109.4525 us |     102.3819 us |      6,233.176 us |  0.71 |    0.05 |
    // |        |      |                   |                 |                 |                   |       |         |
    // |    Mlk | 1024 |     35,957.144 us |     939.4942 us |   2,740.5488 us |     34,830.235 us |  1.00 |    0.00 |
    // | Stripe | 1024 |     30,075.607 us |     597.0037 us |     558.4376 us |     30,155.376 us |  0.87 |    0.03 |
    // |        |      |                   |                 |                 |                   |       |         |
    // |    Mlk | 2048 |    216,429.255 us |   1,362.2389 us |   1,274.2391 us |    216,319.055 us |  1.00 |    0.00 |
    // | Stripe | 2048 |    194,371.782 us |     912.3539 us |     761.8568 us |    194,514.515 us |  0.90 |    0.01 |
    // |        |      |                   |                 |                 |                   |       |         |
    // |    Mlk | 4096 |  1,229,858.064 us |  24,443.6668 us |  41,507.2013 us |  1,225,273.109 us |  1.00 |    0.00 |
    // | Stripe | 4096 |  1,359,198.073 us |  26,233.3195 us |  36,775.4501 us |  1,358,897.743 us |  1.10 |    0.05 |
    // |        |      |                   |                 |                 |                   |       |         |
    // |    Mlk | 8192 |  8,027,946.989 us | 158,499.4494 us | 251,396.9229 us |  7,973,358.263 us |  1.00 |    0.00 |
    // | Stripe | 8192 | 19,240,664.659 us | 371,806.6019 us | 496,351.1342 us | 19,279,676.106 us |  2.41 |    0.09 |
    [Benchmark]
    public double[][] Stripe() 
    {
        var result = MultiplicationByStripe<double>.Multiply(A, B);

        return result;
    }
}