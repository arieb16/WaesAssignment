using System;
using System.Drawing;
using System.IO;
using System.Resources;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;
using Waes.Lib;
using System.Collections.Concurrent;
using WaesAssignment.Properties;

namespace WaesAssignment
{
	public partial class HammingDistanceForm : Form
	{
		// to select random image to compare
		private Random _rand;

		// to avoid same random twice..
		protected int LastUsedID { get; set; }

		// the position of the rect around match QR-Code, use this to erase the rect in the new round..
		protected Point RectPos{ get; set;}


		// 16 picture box, each host QR-Code image
		PictureBox[ ] PictureBoxArray { get; set; }

		// hold the result when user request also the number of bytes that are different
		ConcurrentDictionary<int, HammingDistanceAndPosition> ResultCollection { get; set; }

		// if user choose to get also the amount of different bytes, this tooltip will display the info
		ToolTip ToolTip { get; set; }

		// stop the auto test
		AutoResetEvent StopAutoTestEvent { get; set; }

		// the auto test task
		Task AutoTestTask { get; set; }

		public HammingDistanceForm( )
		{
			InitializeComponent( );

			this.PictureBoxArray = new PictureBox[ ]
			{
				this.pictureBox1,
				this.pictureBox2,
				this.pictureBox3,
				this.pictureBox4,
				this.pictureBox5,
				this.pictureBox6,
				this.pictureBox7,
				this.pictureBox8,
				this.pictureBox9,
				this.pictureBox10,
				this.pictureBox11,
				this.pictureBox12,
				this.pictureBox13,
				this.pictureBox14,
				this.pictureBox15,
				this.pictureBox16,
			};

			this.ResultCollection = new ConcurrentDictionary<int, HammingDistanceAndPosition>( );
			this.ToolTip = new ToolTip( );

			_rand = new Random( );
			this.LastUsedID = 1;

			this.StopAutoTestEvent = new AutoResetEvent( false );
		}

		protected override void OnLoad( EventArgs e )
		{
			base.OnLoad( e );
			pictureBoxMain.SizeMode = PictureBoxSizeMode.StretchImage;

			ResourceManager rm = Resources.ResourceManager;
			pictureBoxMain.Image = ( Bitmap )rm.GetObject( "_1" );
		}

		protected override void OnFormClosing( FormClosingEventArgs e )
		{
			// if auto test is running we need first to stop it - auto test also use the message queue
			// when invoke method that modify UI.
			// avoid deadlocks
			if ( null != this.AutoTestTask )
			{
				MessageBox.Show( this, "Please stop Automatic test first" );
				e.Cancel = true;
			}
			base.OnClosing( e );
		}
		
		private void _btnChangeImg_Click( object sender, EventArgs e )
		{
			this.RemoveMarker( );
			this.ResultCollection.Clear( );
			this.ToolTip.RemoveAll( );

			// under auto test remain disable
			if ( null == this.AutoTestTask )
			{
				this._checkBoxDisplay.Enabled = true;
			}
						
			int id = _rand.Next( 1, 17 );

			this.LastUsedID = this.AvoidSameID( id );

			//display candidate image
			ResourceManager rm = Resources.ResourceManager;
			pictureBoxMain.Image = ( Bitmap )rm.GetObject( "_" + this.LastUsedID.ToString( ) );
		}
		
		private byte[ ] GetImgBytesFromBoxMain( Image img )
		{
			byte[ ] ret;
			using ( MemoryStream ms = new MemoryStream( ) )
			{
				img.Save( ms, ImageFormat.Bmp );
				ret = ms.ToArray( );
			}
			return ret;
		}

		private void _btnSearch_Click( object sender, EventArgs e )
		{
			this._checkBoxDisplay.Enabled = false;
			byte[ ] mainImgByte = this.GetImgBytesFromBoxMain( this.pictureBoxMain.Image );

			// loop bitmaps in parallel
			Parallel.For( 0, this.PictureBoxArray.Length, delegate ( int i )
			{
				byte[ ] selectedImgByte = this.GetImgBytesFromBoxMain( this.PictureBoxArray[ i ].Image );
				int differentBits = 0;

				if ( !this._checkBoxDisplay.Checked )
				{
					// return the number of different bit
					differentBits = HammingDistance.GetHammingDistance( selectedImgByte, mainImgByte );
				}
				else
				{
					// use the HammingDistance version that return the different bits amount & array int => the byte index position
					HammingDistanceAndPosition dp = HammingDistance.GetHammingDistanceAndBytePosition( selectedImgByte, mainImgByte );
					this.ResultCollection[ i ] = dp;
					differentBits = dp.totalDifferents;
				}
				
				if ( 0 == differentBits )
				{
					this.BeginInvoke( new Action<PictureBox>( this.MarkSameItem ), new object[ ] { this.PictureBoxArray[ i ] } );
				}
				
			} );
		}

		// green rect around the match
		private void MarkSameItem( PictureBox obj )
		{
			PictureBox pb = obj as PictureBox;
			
			this.RectPos = new Point( pb.Location.X - 5, pb.Location.Y - 5);

			SolidBrush brush = new SolidBrush( System.Drawing.Color.Green );
			Graphics formGraphics = this.CreateGraphics( );
			formGraphics.FillRectangle( brush, new Rectangle( this.RectPos.X, this.RectPos.Y, 130,130 ) );
			brush.Dispose( );
			formGraphics.Dispose( );
		}


		// remove the green rect around the match
		private void RemoveMarker( )
		{
			SolidBrush brush = new SolidBrush( this.BackColor );
			Graphics formGraphics = this.CreateGraphics( );
			formGraphics.FillRectangle( brush, new Rectangle( this.RectPos.X, this.RectPos.Y, 130, 130 ) );
			brush.Dispose( );
			formGraphics.Dispose( );
		}


		// if rand return same number as before .. 
		private int AvoidSameID( int id )
		{
			if ( id == this.LastUsedID )
			{
				if ( 0 == id )
				{
					id = 5;
				}
				else if ( 16 == id )
				{
					id = 3;
				}
				else
				{
					++id;
				}
			}
			return id;
		}

		private void pictureBox_MouseHover( object sender, EventArgs e )
		{
			// only if user request to return the more detail version of hamming distance
			if ( 0 != this.ResultCollection.Count )
			{
				PictureBox pb = sender as PictureBox;
				HammingDistanceAndPosition dp = this.ResultCollection[ int.Parse( pb.Tag as string ) ];

				string msg = string.Format( "{0} different bytes\n{1} different bits",
					dp.diffrentByteCollection.Length,
					dp.totalDifferents );

				this.ToolTip.SetToolTip( sender as PictureBox, msg );
			}
		}

		// start the automatic test
		private void _btnAutoTest_Click( object sender, EventArgs e )
		{
			if ( null == this.AutoTestTask )
			{
				this._btnChangeImg.Enabled = false;
				this._btnSearch.Enabled = false;
				this._checkBoxDisplay.Enabled = false;
				this._btnAutoTest.Enabled = false;

				this.AutoTestTask = Task.Factory.StartNew( this.AutoTestTaskHandler, TaskCreationOptions.LongRunning );
			}
		}

		// the auto test loop
		private void AutoTestTaskHandler(  )
		{
			bool stop = false;

			while( !stop )
			{
				for ( int i = 0; i < 10; i++  )
				{
					if ( this.CanContinueTest() )
					{
						this.Invoke( new Action<object, EventArgs>( this._btnChangeImg_Click ), new object[ ] { null, null } );
						Task.Delay( 100 ).Wait( );
					}
					// user request to stop
					else
					{
						stop = true;
						break;
					}
				}
				
				// if previous loop not break because of user request to stop
				if ( !stop )
				{
					this.Invoke( new Action<object, EventArgs>( this._btnSearch_Click ), new object[ ] { null, null } );

					// before display the result for 2 seconds, maybe it's redundant .. maybe user request to end..
					if ( this.CanContinueTest( ) )
					{
						Task.Delay( 2000 ).Wait( );
					}
					else
					{
						stop = true;
					}
				}
			}
		}

		// check if user request to stop auto test.
		private bool CanContinueTest( )
		{
			return ( false == this.StopAutoTestEvent.WaitOne( 0 ) );
		}

		// stop automatic test
		private void _btnStopAutomaticTest_Click( object sender, EventArgs e )
		{
			if ( null != this.AutoTestTask )
			{
				this._btnChangeImg.Enabled = true;
				this._btnSearch.Enabled = true;
				this._checkBoxDisplay.Enabled = true;
				this._btnAutoTest.Enabled = true;
				this.StopAutoTestEvent.Set( );
				this.AutoTestTask = null;
			}
		}
	}
}
