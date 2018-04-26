using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;

namespace Waes.Lib
{
	public struct HammingDistanceAndPosition
	{
		public int[ ] diffrentByteCollection;
		public int totalDiffernts;
	}

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

				if ( 0 != ( b1[ i ] ^ b2[ i ] ) )
				{
					do
					{
						// avoid from concurrent....
						Interlocked.Increment( ref ret );
					} while ( 0 != ( xorVal = xorVal >> 1 ) );
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

				if ( 0 != ( b1[ i ] ^ b2[ i ] ) )
				{
					differntsBag.Add( i );

					do
					{
						// avoid from concurrent
						Interlocked.Increment( ref ret.totalDiffernts );
					} while ( 0 != ( xorVal = xorVal >> 1 ) );
				}
			} );

			ret.diffrentByteCollection = differntsBag.ToArray( );
			return ret;
		}
	}
}
