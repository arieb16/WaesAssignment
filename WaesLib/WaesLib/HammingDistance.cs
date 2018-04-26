using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;

namespace Waes.Lib
{

	/// <summary>
	/// Use as a return value for "GetHammingDistanceAndBytePosition" - see below
	/// </summary>
	public struct HammingDistanceAndPosition
	{
		public int[ ] diffrentByteCollection;
		public int totalDifferents;
	}

	/// <summary>
	/// This class calculate the hamming distance and return the different number of bits.
	/// The class contain 2 version of implementation:
	/// 
	/// GetHammingDistance - return only the bits amount; 
	/// 
	/// GetHammingDistanceAndBytePosition - return the bits amount and an array of byte index that where diffrent 
	/// </summary>
	public static class HammingDistance
	{
		public static int GetHammingDistance( byte[ ] b1, byte[ ] b2 )
		{
			if ( b1.Length != b2.Length )
			{
				throw new Exception( "Not same length" );
			}
			
			int ret = 0;

			Parallel.For( 0, b1.Length, delegate ( int i )
			{
				int xorVal = b1[ i ] ^ b2[ i ];

				// if XOR not 0.. we have a different..
				if ( 0 != ( b1[ i ] ^ b2[ i ] ) )
				{
					do
					{
						// avoid from concurrent....
						Interlocked.Increment( ref ret );
					} while ( 0 != ( xorVal = xorVal >> 1 ) ); // loop until 0;
				}
			} );
			return ret;
		}

		public static HammingDistanceAndPosition GetHammingDistanceAndBytePosition( byte[ ] b1, byte[ ] b2 )
		{
			HammingDistanceAndPosition ret = new HammingDistanceAndPosition( );

			if ( b1.Length != b2.Length )
			{
				throw new Exception( "Not same length" );
			}

			// thread safe collection
			ConcurrentBag<int> differntsBag = new ConcurrentBag<int>( ); 

			Parallel.For( 0, b1.Length, delegate ( int i )
			{
				int xorVal = b1[ i ] ^ b2[ i ];

				// if XOR not 0.. we have a different..
				if ( 0 != ( b1[ i ] ^ b2[ i ] ) )
				{
					// add the index of the different byte - collection is thread safe
					differntsBag.Add( i );
					do
					{
						// avoid from concurrent
						Interlocked.Increment( ref ret.totalDifferents );
					} while ( 0 != ( xorVal = xorVal >> 1 ) ); // loop until 0;
				}
			} );

			ret.diffrentByteCollection = differntsBag.ToArray( );
			return ret;
		}
	}
}
