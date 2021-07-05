using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;

namespace GIF_Maker
{

    // Much of the codde taken from https://stackoverflow.com/questions/1196322/how-to-create-an-animated-gif-in-net
    // Rewritten to be more understandable
    class GifWriter
    {

        const long SourceGlobalColorInfoPosition = 10, 
                   SourceImageBlockPosition = 789;


        int gifHeight;
        int gifWidth;

        BinaryWriter writer;


        public GifWriter(string fileName, int width, int height)
        {

            gifHeight = height;
            gifWidth = width;

            writer = new BinaryWriter(new FileStream(fileName, FileMode.Create));
        }


        public void WriteFrame(LinkedList<Image> imageList, int delay)
        {
            bool firstFrame = true;

            foreach (Image image in imageList)
            {
                using (var gifStream = new MemoryStream())
                {
                
                    image.Save( gifStream, ImageFormat.Gif );

                    // Steal the global color table info
                    if (firstFrame)
                        InitHeader(gifStream);

                    WriteGraphicControlBlock(gifStream, writer, delay);
                    WriteImageBlock(gifStream, writer, !firstFrame, 0, 0);

                    
                }
                firstFrame = false;
            }
        }

        

        private void InitHeader(Stream SourceGif)
        {
            // File Header
            writer.Write("GIF".ToCharArray()); // File type
            writer.Write("89a".ToCharArray()); // File Version

            writer.Write( (short)(gifWidth) ); // Initial Logical Width
            writer.Write( (short)(gifHeight) ); // Initial Logical Height

            SourceGif.Position = SourceGlobalColorInfoPosition;
            writer.Write( (byte)SourceGif.ReadByte() ); // Global Color Table Info
            writer.Write( (byte)0 ); // Background Color Index
            writer.Write( (byte)0 ); // Pixel aspect ratio
            WriteColorTable( SourceGif, writer);


            writer.Write( unchecked((short)0xff21) ); // Application Extension Block Identifier
            writer.Write( (byte)0x0b ); // Application Block Size
            writer.Write( "NETSCAPE2.0".ToCharArray() ); // Application Identifier
            writer.Write( (byte)3 ); // Application block length
            writer.Write( (byte)1 );
            writer.Write( (short)0 ); // Repeat count for images.
            writer.Write( (byte)0 ); // terminator
        }

        private void WriteColorTable(Stream SourceGif, BinaryWriter Writer)
        {
            SourceGif.Position = 13; // Locating the image color table
            var colorTable = new byte[768];
            SourceGif.Read(colorTable, 0, colorTable.Length);
            Writer.Write(colorTable, 0, colorTable.Length);
        }

        private void WriteGraphicControlBlock(Stream SourceGif, BinaryWriter Writer, int FrameDelay)
        {
            SourceGif.Position = 781; // Locating the source GCE
            var blockhead = new byte[8];
            SourceGif.Read(blockhead, 0, blockhead.Length); // Reading source GCE

            Writer.Write(unchecked((short)0xf921)); // Identifier
            Writer.Write((byte)0x04); // Block Size
            Writer.Write((byte)(blockhead[3] & 0xf7 | 0x08)); // Setting disposal flag
            Writer.Write((short)(FrameDelay / 10)); // Setting frame delay
            Writer.Write(blockhead[6]); // Transparent color index
            Writer.Write((byte)0); // Terminator
        }

        private void WriteImageBlock(Stream SourceGif, BinaryWriter Writer, bool IncludeColorTable, int X, int Y)
        {
            SourceGif.Position = SourceImageBlockPosition; // Locating the image block
            var header = new byte[11];
            SourceGif.Read(header, 0, header.Length);
            Writer.Write(header[0]); // Separator
            Writer.Write((short)X); // Position X
            Writer.Write((short)Y); // Position Y
            Writer.Write((short)gifWidth); // Width
            Writer.Write((short)gifHeight); // Height

            if (IncludeColorTable) // If first frame, use global color table - else use local
            {
                SourceGif.Position = SourceGlobalColorInfoPosition;
                Writer.Write((byte)(SourceGif.ReadByte() & 0x3f | 0x80)); // Enabling local color table
                WriteColorTable(SourceGif, Writer);
            }
            else Writer.Write((byte)(header[9] & 0x07 | 0x07)); // Disabling local color table

            Writer.Write(header[10]); // LZW Min Code Size

            // Read/Write image data
            SourceGif.Position = SourceImageBlockPosition + header.Length;

            var dataLength = SourceGif.ReadByte();
            while (dataLength > 0)
            {
                var imgData = new byte[dataLength];
                SourceGif.Read(imgData, 0, dataLength);

                Writer.Write((byte)dataLength);
                Writer.Write(imgData, 0, dataLength);
                dataLength = SourceGif.ReadByte();
            }

            Writer.Write((byte)0); // Terminator
        }

        /// <summary>
        /// Frees all resources used by this object.
        /// </summary>
        public void Dispose()
        {
            // Complete File
            writer.Write((byte)0x3b); // File Trailer

            writer.BaseStream.Dispose();
            writer.Dispose();
        }
    }
}
