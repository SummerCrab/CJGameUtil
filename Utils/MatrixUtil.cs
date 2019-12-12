using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatrixUtil {

    public class Matrix
    {
        public Matrix(int size)
        {
            matrix = new float[size, size];
        }
        public Matrix(int row, int col)
        {
            matrix = new float[row, col];
        }
        float[,] matrix = new float[0, 0];

        public int Rows
        {
            get
            {
                return matrix.GetLength(0);
            }
        }

        public int Columns
        {
            get
            {
                return matrix.GetLength(1);
            }
        }

        public int Size
        {
            get
            {
                return GetMatrixSize();
            }
        }

        public Matrix(int size, float[] numbs) : this(size)
        {
            Fill(numbs);
        }
        public Matrix(int row, int col, float[] numbs) : this(row, col)
        {
            Fill(numbs);
        }
        public float[] ToArray()
        {
            float[] a = new float[matrix.GetLength(0) * matrix.GetLength(1)];
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    a[i * matrix.Length + j] = matrix[i, j];
                }
            }
            return a;
        }
        public override string ToString()
        {
            System.Text.StringBuilder result = new System.Text.StringBuilder();

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    result.Append('[');
                    result.Append(matrix[i, j]);
                    result.Append(']');
                }
                result.Append('\n');
            }
            return result.ToString();
        }

        public void Fill(float[] numbs)
        {

            if (!CheckEnoughNumber(numbs))
            {
                numbs = SupplyZero(numbs);
            }

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    matrix[i, j] = numbs[i * matrix.GetLength(0) + j];
                }
            }
        }

        int GetMatrixSize()
        {
            return matrix.GetLength(0) * matrix.GetLength(1);
        }

        public bool CheckEnoughNumber(float[] numbs)
        {
            return numbs.Length >= GetMatrixSize();
        }

        float[] SupplyZero(float[] numbs)
        {
            int matrixSize = GetMatrixSize();
            if (numbs.Length >= matrixSize)
            {
                return numbs;
            }
            float[] enoughNumbs = new float[matrixSize];
            numbs.CopyTo(enoughNumbs, 0);
            return enoughNumbs;
        }


        public static Matrix operator *(Matrix lhs, Matrix rhs)
        {

            if (lhs.Columns!=rhs.Rows)
            {
                Debug.LogError("左矩阵列不等于右矩阵列，不能相乘");
                return null;
            }


            int row = lhs.Rows;
            int col = rhs.Columns;

        

            Matrix result = new Matrix(row, col);

            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    float sum = 0;
                    for (int k = 0; k < col; k++)
                    {
                        sum += lhs.matrix[i, k] * rhs.matrix[k, i];
                    }
                    result.matrix[i, j] = sum;
                }
            }

            return result;
        }
        public float this[int i,int j]
        {
            get { return matrix[i, j]; }
            set { matrix[i, j] = value; }
        }
    }

    //[UnityEditor.MenuItem("Matrix/test Matrix")]
    //public static void TestMatrix()
    //{
    //    var mat = new Matrix(3, new float[] { 1, 1, 1, 0, 0, 0, 1, 1, 1 });
    //    Debug.LogError((mat*mat).ToString());
    //    Debug.LogError(Determinate(mat));
    //}

    //创建单位矩阵
    public static Matrix CreatUnitMatrix(int size)
    {
        Matrix result = new Matrix(size);
        for (int i = 0; i < result.Rows; i++)
        {
            for (int j = 0; j < result.Columns; j++)
            {
                result[i, j] = (i == j) ? 1 : 0;
            }
        }
        return null;
    }
    //求转置矩阵
    public static Matrix TransposeMatrix(Matrix matrix)
    {
        Matrix result = new Matrix(matrix.Rows, matrix.Columns);
        for (int i = 0; i < result.Rows; i++)
        {
            for (int j = 0; j < result.Columns;j++)
            {
                result[i, j] = matrix[j, i];
            }
        }
        return result;
    }
    //计算行列式值
    public static float Determinate(Matrix matrix)
    {
        if (matrix.Rows!=matrix.Columns)
        {
            return 0;
        }

        int length = matrix.Rows;

        float mainDiagonalSum = 0;
		float mainDiagonalLine = 1;
        for (int i = 0; i < length; i++)
        {
            mainDiagonalLine = 1;
            for (int j = 0; j < length; j++)
            {
                int index = (i + j) % length;
                mainDiagonalLine *= matrix[index, j];
                //Debug.LogErrorFormat("主对角线元素a[{0},{2}]：{1}", index, matrix[index, j],j);
                if (System.Math.Abs(mainDiagonalLine) < Mathf.Epsilon)
                {
                    break;
                }
            }
            mainDiagonalSum += mainDiagonalLine;
        }
        float counterDiagonalSum = 0;
        float counterDiagonalLine = 1;
        for (int i = length - 1; i >=0; i--)
        {
            counterDiagonalLine = 1;
            for (int j = 0 ; j <length; j++)
            {
                int index = (i - j) % length;

                index = index < 0 ? (index + length - 1) : index;

                counterDiagonalLine *= matrix[index, j];

                //Debug.LogErrorFormat("副对角线元素a[{0},{2}]：{1}", index, matrix[index, j],j);
                if (System.Math.Abs(mainDiagonalLine) < Mathf.Epsilon)
                {
                    break;
                }
            }
            counterDiagonalSum += counterDiagonalLine;
        }

        return mainDiagonalSum - counterDiagonalSum;
    }

}
