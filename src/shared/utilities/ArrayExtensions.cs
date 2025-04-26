﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Godot;
using Godot.NativeInterop;

public static class ArrayExtensions
{
    public static byte At(this Image array, int x, int y)
    {
        return array.Data["data"].As<byte[]>()[y * array.GetWidth() + x];
    }

    public static Span<T> AsSpan<T>(this T[,] array)
    {
        return MemoryMarshal.CreateSpan(ref Unsafe.As<byte, T>(ref MemoryMarshal.GetArrayDataReference(array)), array.Length);
    }

    public static ReadOnlySpan<T> AsReadOnlySpan<T>(this T[,] array)
    {
        return MemoryMarshal.CreateSpan(ref Unsafe.As<byte, T>(ref MemoryMarshal.GetArrayDataReference(array)), array.Length);
    }

    public static Span<T> AsSpan<T>(this T[,,] array)
    {
        return MemoryMarshal.CreateSpan(ref Unsafe.As<byte, T>(ref MemoryMarshal.GetArrayDataReference(array)), array.Length);
    }

    public static Span<int[]> AsIntArraySpan(this int[,,] array)
    {
        return MemoryMarshal.CreateSpan(ref Unsafe.As<byte, int[]>(ref MemoryMarshal.GetArrayDataReference(array)), array.Length);
    }

    public static ReadOnlySpan<T> AsReadOnlySpan<T>(this T[,,] array)
    {
        return MemoryMarshal.CreateSpan(ref Unsafe.As<byte, T>(ref MemoryMarshal.GetArrayDataReference(array)), array.Length);
    }

    public static IEnumerable<T> Slice<T>(this T[,,] array, int x1, int x2)
    {
        for (int x3 = 0; x3 < array.GetLength(2); x3++)
            yield return array[x1, x2, x3];
    }


    public static void Print<T>(this T[,] array)
    {
        for (int i = 0; i < array.GetLength(0); i++)
        {
            for (int j = 0; j < array.GetLength(1); j++)
            {
                Console.Write($"{array[i, j]:0.00} ");
            }

            Console.WriteLine();
        }
    }

// 	[WriteAccessRequired]
// 	public static unsafe void CopyFrom<natT, arrT>(this NativeArray<natT> dst, arrT[] src)
// 		where natT : struct
// 		where arrT : struct
// 	{
// 		SanityCheck<natT, arrT>(dst, src, src.Length);
// 		GCHandle gCHandle = GCHandle.Alloc(src, GCHandleType.Pinned);
// 		IntPtr intPtr = gCHandle.AddrOfPinnedObject();
// 		UnsafeUtility.MemCpy(
// 			(byte*)dst.GetUnsafePtr(),
// 			(byte*)(void*)intPtr,
// 			dst.Length * UnsafeUtility.SizeOf<natT>());
// 		gCHandle.Free();
// 	}
//
// 	[WriteAccessRequired]
// 	public static unsafe void CopyFrom<natT, arrT>(this NativeArray<natT> dst, arrT[,] src)
// 		where natT : struct
// 		where arrT : struct
// 	{
// 		SanityCheck<natT, arrT>(dst, src, src.Length);
// 		GCHandle gCHandle = GCHandle.Alloc(src, GCHandleType.Pinned);
// 		IntPtr intPtr = gCHandle.AddrOfPinnedObject();
// 		UnsafeUtility.MemCpy(
// 			(byte*)dst.GetUnsafePtr(),
// 			(byte*)(void*)intPtr,
// 			dst.Length * UnsafeUtility.SizeOf<natT>());
// 		gCHandle.Free();
// 	}
//
// 	[WriteAccessRequired]
// 	public static unsafe void CopyTo<natT, arrT>(this NativeArray<natT> src, arrT[] dst)
// 		where natT : struct
// 		where arrT : struct
// 	{
// 		SanityCheck<natT, arrT>(src, dst, dst.Length);
// 		GCHandle gCHandle = GCHandle.Alloc(dst, GCHandleType.Pinned);
// 		IntPtr intPtr = gCHandle.AddrOfPinnedObject();
// 		UnsafeUtility.MemCpy(
// 			(byte*)(void*)intPtr,
// 			(byte*)src.GetUnsafePtr(),
// 			src.Length * UnsafeUtility.SizeOf<natT>());
// 		gCHandle.Free();
// 	}
//
// 	[WriteAccessRequired]
// 	public static unsafe void CopyTo<natT, arrT>(this NativeArray<natT> src, arrT[,] dst)
// 		where natT : struct
// 		where arrT : struct
// 	{
// 		SanityCheck<natT, arrT>(src, dst, dst.Length);
// 		GCHandle gCHandle = GCHandle.Alloc(dst, GCHandleType.Pinned);
// 		IntPtr intPtr = gCHandle.AddrOfPinnedObject();
// 		UnsafeUtility.MemCpy(
// 			(byte*)(void*)intPtr,
// 			(byte*)src.GetUnsafePtr(),
// 			src.Length * UnsafeUtility.SizeOf<natT>());
// 		gCHandle.Free();
// 	}
//
// 	// Common
// 	static void SanityCheck<natT, arrT>(NativeArray<natT> nat, object array, int arrayLength)
// 		where natT : struct
// 		where arrT : struct
// 	{
// 		if (array == null) {
// 			throw new ArgumentNullException(nameof(array));
// 		}
//
// 		if (arrayLength < 0) {
// 			throw new ArgumentOutOfRangeException(nameof(arrayLength), "length must be equal or greater than zero.");
// 		}
//
// 		if (arrayLength != nat.Length) {
// 			throw new Exception("Array length and NativeArray length do not match.");
// 		}
//
// 		if (UnsafeUtility.SizeOf<arrT>() != UnsafeUtility.SizeOf<natT>()) {
// 			throw new Exception("Element types of array and NativeArray are not of same size.");
// 		}
}