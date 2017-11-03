	
using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.IO.Compression;
using System.Collections.Generic;
using Microsoft.Ajax.Utilities;
using UnityEngine;
using UnityEditor;
using BabylonExport.Entities;

namespace System.IO
{
    #region Stream Copy With Compression Extensions
	public enum CopyToOptions
	{
		None,
		Flush,
		FlushFinal
	}
	//
	public class CopyToEventArgs
	{
		public long TotalBytesWritten { get; private set; }
		//
		public CopyToEventArgs(long totalBytesWritten)
		{
			TotalBytesWritten = totalBytesWritten;
		}
	}
	//
	public static class CopyToStreaming
	{
		public static void CopyTo(this Stream source, Stream destination, CopyToOptions options, CompressionMode mode, DecompressionMethods method = DecompressionMethods.Deflate, int bufferSize = 4096, long maxCount = -1, Action<CopyToEventArgs> onProgress = null)
		{
			if (mode == CompressionMode.Compress)
			{
				switch (method)
				{
				case DecompressionMethods.Deflate:
					{
						using (DeflateStream deflaterStream = new DeflateStream(destination, CompressionMode.Compress, true))
						{
							source.CopyTo(deflaterStream, options, bufferSize, maxCount, onProgress);
							deflaterStream.Close();
						}
					}
					break;
				case DecompressionMethods.GZip:
					{
						using (GZipStream gzipStream = new GZipStream(destination, CompressionMode.Compress, true))
						{
							source.CopyTo(gzipStream, options, bufferSize, maxCount, onProgress);
							gzipStream.Close();
						}
					}
					break;
				default:
					{
						source.CopyTo(destination, options, bufferSize, maxCount, onProgress);
					}
					break;
				}
			}
			else
			{
				switch (method)
				{
				case DecompressionMethods.Deflate:
					{
						using (DeflateStream deflaterStream = new DeflateStream(source, CompressionMode.Decompress, true))
						{
							deflaterStream.CopyTo(destination, options, bufferSize, maxCount, onProgress);
							deflaterStream.Close();
						}
					}
					break;
				case DecompressionMethods.GZip:
					{
						using (GZipStream gzipStream = new GZipStream(source, CompressionMode.Decompress, true))
						{
							gzipStream.CopyTo(destination, options, bufferSize, maxCount, onProgress);
							gzipStream.Close();
						}
					}
					break;
				default:
					{
						source.CopyTo(destination, options, bufferSize, maxCount, onProgress);
					}
					break;
				}
			}
		}
		//
		public static void CopyTo(this Stream source, Stream destination, CopyToOptions options, int bufferSize = 4096, long maxCount = -1, Action<CopyToEventArgs> onProgress = null)
		{
			byte[] buffer = new byte[bufferSize];
			//
			long totalBytesWritten = 0;
			//
			while (true)
			{
				int count = buffer.Length;
				//
				if (maxCount > 0)
				{
					if (totalBytesWritten > maxCount - count)
					{
						count = (int)(maxCount - totalBytesWritten);
						if (count <= 0)
						{
							break;
						}
					}
				}
				//
				int read = source.Read(buffer, 0, count);
				if (read <= 0)
				{
					break;
				}
				//
				destination.Write(buffer, 0, read);
				//
				if (options == CopyToOptions.Flush)
				{
					try
					{
						destination.Flush();
					}
					// Analysis disable once EmptyGeneralCatchClause
					catch
					{
						// Do Nothing
					}
				}
				//
				totalBytesWritten += read;
				//
				if (onProgress != null)
				{
					onProgress(new CopyToEventArgs(totalBytesWritten));
				}
			}
			//
			if (options == CopyToOptions.FlushFinal)
			{
				try
				{
					destination.Flush();
				}
				// Analysis disable once EmptyGeneralCatchClause
				catch
				{
					// Do Nothing
				}
			}
		}
	}
	#endregion

	public static class MathTools
	{
		private delegate double RoundingFunction(double value);

		private enum RoundingDirection { Up, Down }

		public static double RoundUp(double value, int precision)
		{
			return Round(value, precision, RoundingDirection.Up);
		}

		public static double RoundDown(double value, int precision)
		{
			return Round(value, precision, RoundingDirection.Down);
		}

		private static double Round(double value, int precision,  RoundingDirection roundingDirection)
		{
			RoundingFunction roundingFunction;
			if (roundingDirection == RoundingDirection.Up)
				roundingFunction = Math.Ceiling;
			else
				roundingFunction = Math.Floor;
			value *= Math.Pow(10, precision);
			value = roundingFunction(value);
			return value * Math.Pow(10, -1 * precision);
		}
	}	
}
    
    
    
    
