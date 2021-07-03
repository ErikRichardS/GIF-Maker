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
        LinkedList<Image> imageResized = new LinkedList<Image>();
        

        LinkedList<Image>.Enumerator timelineIterator;

        public Form1()
        {
            InitializeComponent();
        }

        private void buttonLoad_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {

                imageTimelineList.AddLast(Image.FromFile(openFileDialog1.FileName));
                updateTimelineList();
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            if(saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                consoleLabel.Text = ""+ numericTickDelay.Value;

                updateResizedImageList();

                GifWriter gifMaker = new GifWriter(saveFileDialog1.FileName, (int)numericWidth.Value, (int)numericHeight.Value);

                gifMaker.WriteFrame(imageResized, (int)numericTickDelay.Value);

                gifMaker.Dispose();
            }
        }

        private void updateTimelineList()
        {

            System.Windows.Forms.PictureBox imageBox = new System.Windows.Forms.PictureBox();
            imageBox.Image = imageTimelineList.Last();
            imageBox.Height = imageTimelinePanel.Height;
            imageBox.Width = imageTimelinePanel.Height;
            imageBox.SizeMode = PictureBoxSizeMode.StretchImage;

            imageTimelinePanel.Controls.Add(imageBox);
            //imageTimelinePanel.Controls.IndexOf(0);
        }

        private void buttonPlay_Click(object sender, EventArgs e)
        {

            if (!timerTimeline.Enabled)
            {
                updateResizedImageList();

                timelineIterator = imageResized.GetEnumerator();

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
                timelineIterator = imageResized.GetEnumerator();

                if (timelineIterator.MoveNext())
                    pictureBoxMain.Image = timelineIterator.Current;
            }
            
        }

        private void updateResizedImageList()
        {
            imageResized.Clear();
            foreach (Image image in imageTimelineList)
            {
                imageResized.AddLast(ImageResize.Resize(image, (int)numericWidth.Value, (int)numericHeight.Value));
            }
        }


    }
}
