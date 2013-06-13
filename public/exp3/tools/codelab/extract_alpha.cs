#region UICode
#endregion


// Here is the main render loop function
void Render(Surface dst, Surface src, Rectangle rect)
{
    for (int y = rect.Top; y < rect.Bottom; y++)
    {
        for (int x = rect.Left; x < rect.Right; x++)
        {
            ColorBgra CurrentPixel = src[x,y];

            // TODO: Add additional pixel processing code here
            CurrentPixel.R = CurrentPixel.A;
            CurrentPixel.G = CurrentPixel.A;
            CurrentPixel.B = CurrentPixel.A;
            CurrentPixel.A = 255;

            dst[x,y] = CurrentPixel;
        }
    }
}

