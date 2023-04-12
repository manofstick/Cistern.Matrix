namespace Cistern.Matrix;

public static class Utils<T>
{
    public static T[][] CreateZero(int rows, int columns)
    {
        var t = new T[rows][];
        for (var i=0; i < rows; ++i)
            t[i] = new T[columns];
        return t;
    }

    public static T[][] Create(int rows, int columns, Func<int, int, T> create)
    {
        var a = new T[columns][];
        for (var i=0; i < a.Length; ++i)
        {
            a[i] = new T[rows];
            for(var j=0; j < a[i].Length; ++j)
                a[i][j] = create(i, j);
        }
        return a;
    }


    public static T[][] Transpose(T[][] A)
    {
        var rows = A.Length;
        var columns = A[0].Length;

        var t = CreateZero(columns, rows);

        for (var row = 0; row < rows; ++row)
        {
            for(var column=0; column < columns; ++column)
            {
                t[column][row] = A[row][column];
            }
        }
        
        return t;
    }
}