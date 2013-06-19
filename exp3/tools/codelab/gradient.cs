#region UICode
int Amount1=0;	//[0,100]Slider 1 Description
int Amount2=0;	//[0,100]Slider 2 Description
int Amount3=0;	//[0,100]Slider 3 Description
#endregion

void Render(Surface dst, Surface src, Rectangle rect)
{
    ColorBgra tap;
    ColorBgra tapdy;
    ColorBgra tapdx;
    for (int y = rect.Top; y < rect.Bottom; y++)
    {
        int dy = y - 1;
        if ( dy < src.Bounds.Top )
            dy = src.Bounds.Bottom - 1;
        
        for (int x = rect.Left; x < rect.Right; x++)
        {
            int dx = x + 1;
            if ( dx >= src.Bounds.Right )
                dx = src.Bounds.Left + dx % src.Bounds.Width;
            
            tap = src[x,y];
            tapdx = src[dx,y];
            tapdy = src[x,dy];
            
            double h = (double)tap.B;
            double hdx = (double)tapdx.B;
            double hdy = (double)tapdy.B;
            
            // approximate the gradient with forward difference: delta = a[n+1] - a[n]
            tap.R = (byte)( (hdx - h) * 0.5 + 127.5 );
            tap.G = (byte)( (hdy - h) * 0.5 + 127.5 );
            //tap.B = (byte)( 255 - tap.B );
            dst[x,y] = tap;
        }
    }
}
