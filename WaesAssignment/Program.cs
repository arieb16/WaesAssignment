using System;
using System.Text;
using Waes.Lib;



namespace WaesAssignment
{
	class Program
	{
		static void Main( string[ ] args )
		{
			Console.WriteLine( "Hamming Binary Distance" );
			Console.WriteLine( "=======================\n" );

			int distance = 0;
			
			while ( true )
			{
				Console.WriteLine( "Insert first string" );
				string s1 = Console.ReadLine();
				Console.WriteLine( "Insert second string" );
				string s2 = Console.ReadLine( );

				try
				{
					distance = HammingDistance.GetHammingDistance( Encoding.ASCII.GetBytes( s1 ), Encoding.ASCII.GetBytes( s2 ) );

					Console.WriteLine( "============================================\n" );
					Console.WriteLine( "binary distance between {0} and {1} is {2}\n", s1, s2, distance );
					Console.WriteLine( "============================================\n" );
				}
				catch ( Exception e )
				{
					Console.WriteLine( e.Message );
				}

				Console.WriteLine( "ESC to exit - other key to continue\n" );

				if ( 27 == Console.ReadKey( true ).KeyChar ) 
				{
					break;
				}
				else
				{
					Console.Clear();
				}
			}
		}
	}
}
