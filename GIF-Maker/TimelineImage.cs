using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace GIF_Maker
{
    class TimelineImage
    {
        Image originalImage;
        Image resizedImage;

        System.Windows.Forms.PictureBox imageBox;


        public TimelineImage(Image img, int width, int height)
        {
            originalImage = img;
            resizedImage = ImageResize.Resize(img, width, height);

            
        }

        public Image getResizedImage()
        {
            return resizedImage;
        }

    }
}
