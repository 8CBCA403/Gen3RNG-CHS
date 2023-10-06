using System;
using System.Collections.Generic;
using System.Linq;

namespace _3genSearch.NewForms;

public static class MyLinq
{
	public static IEnumerable<(R, T2)> Select<T1, T2, R>(this IEnumerable<(T1, T2)> source, Func<T1, R> selector)
	{
		return source.Select(((T1, T2) _) => (selector(_.Item1), _.Item2));
	}

	public static IEnumerable<(R, T2, T3)> Select<T1, T2, T3, R>(this IEnumerable<(T1, T2, T3)> source, Func<T1, R> selector)
	{
		return source.Select(((T1, T2, T3) _) => (selector(_.Item1), _.Item2, _.Item3));
	}

	public static IEnumerable<(T1, T2)> Where<T1, T2>(this IEnumerable<(T1, T2)> source, Func<T1, bool> selector)
	{
		return source.Where(((T1, T2) _) => selector(_.Item1));
	}

	public static IEnumerable<(T1, T2, T3)> Where<T1, T2, T3>(this IEnumerable<(T1, T2, T3)> source, Func<T1, bool> selector)
	{
		return source.Where(((T1, T2, T3) _) => selector(_.Item1));
	}
}
