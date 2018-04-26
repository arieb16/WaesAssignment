using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;


namespace Waes.Lib.Tests
{
	[TestClass( )]
	public class HammingDistanceTests
	{
		[TestMethod( )]
		public void GetHammingDistanceTest_SameString_Scenario( )
		{
			string s1 = "1212121212122121";
			string s2 = s1;
			Exception libException = null;

			int ret = int.MaxValue;
			try
			{
				ret = HammingDistance.GetHammingDistance( Encoding.ASCII.GetBytes( s1 ), Encoding.ASCII.GetBytes( s2 ) );
			}
			catch ( Exception e )
			{
				libException = e;
			}

			Assert.IsNull( libException );
			Assert.IsTrue( 0 == ret );
		}

		[TestMethod( )]
		public void GetHammingDistanceTest_Different_String_Size_Scenario( )
		{
			string s1 = "1212121212122121";
			string s2 = s1.Remove( 0, 1 );

			Exception libException = null;

			int ret = 0;
			try
			{
				ret = HammingDistance.GetHammingDistance( Encoding.ASCII.GetBytes( s1 ), Encoding.ASCII.GetBytes( s2 ) );
			}
			catch ( Exception e )
			{
				libException = e;
			}

			Assert.IsNotNull( libException );
		}

		[TestMethod( )]
		public void GetHammingDistanceTest_All_Bits_Different( )
		{
			Int64 x = 1245646988;
			Int64 y = ~x;

			Exception libException = null;

			int ret = 0;
			try
			{
				ret = HammingDistance.GetHammingDistance( BitConverter.GetBytes( x ), BitConverter.GetBytes( y ) );
			}
			catch ( Exception e )
			{
				libException = e;
			}

			Assert.IsNull( libException );
			Assert.IsTrue( ret == sizeof( Int64 ) * 8 );
		}

		[TestMethod( )]
		public void GetHammingDistanceTest_All_Bits_Different_And_Position( )
		{
			Int64 x = 1245646988;
			Int64 y = ~x;

			Exception libException = null;

			//int ret = 0;
			HammingDistanceAndPosition ret = new HammingDistanceAndPosition();
			try
			{
				ret = HammingDistance.GetHammingDistanceAndBytePosition( BitConverter.GetBytes( x ), BitConverter.GetBytes( y ) );
			}
			catch ( Exception e )
			{
				libException = e;
			}

			Assert.IsNull( libException );
			Assert.IsTrue( ret.totalDiffernts == sizeof( Int64 ) * 8 );
		}
	}
}