using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GIF_Maker
{
    public partial class Form1 : Form
    {


        LinkedList<Image> imageTimelineList = new LinkedList<Image>();

        // Used for when playing up the GIF
        LinkedList<Image> imageTimelineResized = new LinkedList<Image>();
        LinkedList<Image>.Enumerator timelineIterator;


        System.Windows.Forms.PictureBox selectedImage = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void buttonLoad_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                addImageToTimeline(Image.FromFile(openFileDialog1.FileName));
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            if(saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                consoleLabel.Text = ""+ numericTickDelay.Value;

                updateResizedImageList();

                GifWriter gifMaker = new GifWriter(saveFileDialog1.FileName, (int)numericWidth.Value, (int)numericHeight.Value);

                gifMaker.WriteFrame(imageTimelineResized, (int)numericTickDelay.Value);

                gifMaker.Dispose();
            }
        }

        private void addImageToTimeline(Image img)
        {

            System.Windows.Forms.PictureBox imageBox = new System.Windows.Forms.PictureBox();
            imageBox.Image = img;
            imageBox.Height = imageTimelinePanel.Height-50;
            imageBox.Width = imageTimelinePanel.Height-50;
            imageBox.SizeMode = PictureBoxSizeMode.StretchImage;


            ImageClickEventArgs args = new ImageClickEventArgs();
            args.Index = imageTimelineList.Count;
            imageBox.Click += new EventHandler((s, e) => timelineImage_Click(s, args));

            imageTimelinePanel.Width += imageTimelinePanel.Height;
            imageTimelinePanel.Controls.Add(imageBox);

            imageTimelineList.AddLast(img);
        }

        private void buttonPlay_Click(object sender, EventArgs e)
        {

            if (!timerTimeline.Enabled)
            {
                updateResizedImageList();

                timelineIterator = imageTimelineResized.GetEnumerator();

                if(timelineIterator.MoveNext())
                    pictureBoxMain.Image = timelineIterator.Current;

                buttonPlay.Text = "Stop";


                timerTimeline.Interval = (int) numericTickDelay.Value;
                timerTimeline.Start();
            }
            else
            {
                buttonPlay.Text = "Play";
                timerTimeline.Stop();
                timelineIterator.Dispose();
            }
        }

        private void timerTimeline_Tick(object sender, EventArgs e)
        {
            // If the iterator has a next object
            if(timelineIterator.MoveNext())
            {
                // Update the main pciture box with the next image
                pictureBoxMain.Image = timelineIterator.Current;
            }
            else
            {
                // The iterator reached the end, dispose of current iterator and restart with a new one
                // (This iterator has no reset functionality.)
                timelineIterator.Dispose();
                timelineIterator = imageTimelineResized.GetEnumerator();

                if (timelineIterator.MoveNext())
                    pictureBoxMain.Image = timelineIterator.Current;
            }
            
        }

        private void updateResizedImageList()
        {
            imageTimelineResized.Clear();
            foreach (Image image in imageTimelineList)
            {
                imageTimelineResized.AddLast(ImageResize.Resize(image, (int)numericWidth.Value, (int)numericHeight.Value));
            }
        }


        public void timelineImage_Click(object sender, ImageClickEventArgs e)
        {
            System.Windows.Forms.PictureBox box = sender as System.Windows.Forms.PictureBox;


            if (selectedImage != null)
            {
                selectedImage.BorderStyle = BorderStyle.None;
                selectedImage = box;
                selectedImage.BorderStyle = BorderStyle.Fixed3D;
            }
            else
            {
                selectedImage = box;
                selectedImage.BorderStyle = BorderStyle.Fixed3D;
            }

            pictureBoxMain.Image = ImageResize.Resize( imageTimelineList.ElementAt(e.Index), (int)numericWidth.Value, (int)numericHeight.Value);
            consoleLabel.Text = ""+e.Index;
        }
    }

    public class ImageClickEventArgs : EventArgs
    {
        public int Index { get; set; }
        //public DateTime TimeReached { get; set; }
    }
}
