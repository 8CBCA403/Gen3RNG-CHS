namespace _3genSearch.Lecagy;

internal delegate TResult RefFunc<T, out TResult>(ref T seed);
internal delegate TResult RefFunc<T1, T2, out TResult>(ref T1 seed, T2 arg2);
internal delegate TResult RefFunc<T1, T2, T3, out TResult>(ref T1 seed, T2 arg2, T3 arg3);
