
using UnityEngine;

public static class Extensions
{
	//注意： TaskAwaiter 和 timeSpan 没有任何关系。
	// TaskAwaiter来源于 Task.Delay(). timeSpan只是一个参数而已。
	// 所以， 此扩展函数容易让人不明故里
	// 所以，如果不是通用的方法，请勿使用 Extension

	/// <summary>
	/// 针对int类型的扩展的方法
	/// </summary>
	/// <param name="num"></param>
	/// <returns></returns>
	public static string GetCategory(this string Category, string mode)
	{
		string[] categotylist = Category.Split(';');
		for (int i = 0; i < categotylist.Length; i++)
		{
			string[] keyValue = categotylist[i].Split('=');
			if (keyValue[0] == mode)

				return (keyValue[1]);
		}
		return "0";
	}

	/// <summary>
	/// 删除最后n个字符
	/// </summary>
	/// <param name="sb"></param>
	/// <param name="length"></param>
	/// <returns></returns>
	public static System.Text.StringBuilder RemoveLast(this System.Text.StringBuilder sb, int length)
	{
		int remain = sb.Length - length;
		if (remain <= 0)
		{
			return sb.Clear();
		}
		return sb.Remove(remain, length);
	}

	/// <summary>
	/// Checks if a GameObject has been destroyed.
	/// </summary>
	/// <param name="gameObject">GameObject reference to check for destructedness</param>
	/// <returns>If the game object has been marked as destroyed by UnityEngine</returns>
	public static bool IsDestroyed(this GameObject gameObject)
	{
		// UnityEngine overloads the == opeator for the GameObject type
		// and returns null when the object has been destroyed, but 
		// actually the object is still there but has not been cleaned up yet
		// if we test both we can determine if the object has been destroyed.
		return gameObject == null && !ReferenceEquals(gameObject, null);
	}

	public static UnityEngine.Vector4 Parse(this UnityEngine.Vector4 vector, string name)
	{
		name = name.Replace("(", "").Replace(")", "");
		string[] array = name.Split(',');

		if (array.Length > 0) vector.x = float.Parse(array[0]);
		if (array.Length > 1) vector.y = float.Parse(array[1]);
		if (array.Length > 2) vector.z = float.Parse(array[2]);
		if (array.Length > 3) vector.w = float.Parse(array[3]);

		return vector;
	}

	public static UnityEngine.Vector3 Parse(this UnityEngine.Vector3 vector, string name)
	{
		name = name.Replace("(", "").Replace(")", "");
		string[] array = name.Split(',');

		if (array.Length > 0) vector.x = float.Parse(array[0]);
		if (array.Length > 1) vector.y = float.Parse(array[1]);
		if (array.Length > 2) vector.z = float.Parse(array[2]);

		return vector;
	}

	public static UnityEngine.Vector3 NoZero(this UnityEngine.Vector3 vector)
	{
		if (vector.x == 0) vector.x = UnityEngine.Vector3.kEpsilon;
		if (vector.y == 0) vector.y = UnityEngine.Vector3.kEpsilon;
		if (vector.z == 0) vector.z = UnityEngine.Vector3.kEpsilon;

		return vector;
	}

	public static UnityEngine.Vector2 Parse(this UnityEngine.Vector2 vector, string name)
	{
		name = name.Replace("(", "").Replace(")", "");
		string[] array = name.Split(',');

		if (array.Length > 0) vector.x = float.Parse(array[0]);
		if (array.Length > 1) vector.y = float.Parse(array[1]);

		return vector;
	}

	public static float Max(this UnityEngine.Vector3 vector)
	{
		float reuslt = vector.x;

		if (vector.y > reuslt) reuslt = vector.y;
		if (vector.z > reuslt) reuslt = vector.z;

		return reuslt;
	}

	public static float Min(this UnityEngine.Vector3 vector)
	{
		float reuslt = vector.x;

		if (vector.y < reuslt) reuslt = vector.y;
		if (vector.z < reuslt) reuslt = vector.z;

		return reuslt;
	}

	/// <summary>
	/// 字符串比较
	/// </summary>
	/// <param name="a"></param>
	/// <param name="b"></param>
	/// <returns></returns>
	public static bool CustomEndsWith(this string a, string b)
	{
		int ap = a.Length - 1;
		int bp = b.Length - 1;

		while (ap >= 0 && bp >= 0 && a[ap] == b[bp])
		{
			ap--;
			bp--;
		}

		return (bp < 0);
	}

	/// <summary>
	/// 字符串比较
	/// </summary>
	/// <param name="a"></param>
	/// <param name="b"></param>
	/// <returns></returns>
	public static bool CustomStartsWith(this string a, string b)
	{
		int aLen = a.Length;
		int bLen = b.Length;

		int ap = 0; int bp = 0;

		while (ap < aLen && bp < bLen && a[ap] == b[bp])
		{
			ap++;
			bp++;
		}

		return (bp == bLen);
	}
}
