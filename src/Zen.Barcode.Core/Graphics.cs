using System;
using System.Drawing;
using System.IO;

namespace Zen.Barcode;

public class Graphics(Bitmap image) : IDisposable
{
    internal static Graphics FromImage(Bitmap image)
    {
        return new Graphics(image);
    }

    internal void FillRectangle(BWColor brush, Rectangle bounds) => FillRectangle(brush, bounds.X, bounds.Y, bounds.Width, bounds.Height);
    internal void FillRectangle(BWColor black, int x, int y, int w, int h)
    {
        for (var i = 0; i < w; i++)
        {
            for (var j = 0; j < h; j++)
            {
                image.SetPixel(x + i, y + j, black);
            }
        }
    }

    public void Dispose()
    {
        // nop
    }
}

internal static class StreamExtensions
{
    public static void WriteInt32(this Stream stream, int value)
    {
        stream.Write(BitConverter.GetBytes(value));
    }

    public static void WriteInt16(this Stream stream, short value)
    {
        stream.Write(BitConverter.GetBytes(value));
    }
}

public abstract class Image
{
    public abstract void WriteBmp(Stream o);
}

public class Bitmap : Image
{
    public Bitmap(int width, int height)
    {
        this.Size = new Size(width, height);
        // BMP has a 4-byte aligned stride
        this.Stride = ((((width + 7) / 8) + 3) / 4) * 4;
        this.Buffer = new byte[Stride * height];
    }

    public int Width => Size.Width;
    public int Height => Size.Height;
    public Size Size { get; }
    public int Stride { get; }
    public byte[] Buffer { get; }

    internal Bitmap Clone()
    {
        var copy = new Bitmap(Width, Height);
        Buffer.CopyTo(copy.Buffer, 0);
        return copy;
    }

    internal BWColor GetPixel(int x, int y)
    {
        var index = y * Stride + x / 8;
        var mask = (byte)(0x80 >> (x % 8));
        return (Buffer[index] & mask) != 0;
    }

    internal void SetPixel(int x, int y, BWColor color)
    {
        var index = y * Stride + x / 8;
        var mask = (byte)(0x80 >> (x % 8));
        if (color)
        {
            Buffer[index] |= mask;
        }
        else
        {
            Buffer[index] &= (byte)~mask;
        }
    }

    internal void RotateFlip(RotateFlipType rotate90FlipNone)
    {
        throw new NotSupportedException();
    }

    public override void WriteBmp(Stream o)
    {
        var oLen = o.Length;

        // BITMAPFILEHEADER
        o.Write([0x42, 0x4D]);                // BM
        o.WriteInt32(62 + Buffer.Length);     // File size
        o.WriteInt32(0);                // Reserved
        o.WriteInt32(62);               // Offset to image data

        // BITMAPINFOHEADER
        o.WriteInt32(40);               // Header size
        o.WriteInt32(Width);             // Image width
        o.WriteInt32(Height);                 // Image height
        o.WriteInt16(1);                // Planes
        o.WriteInt16(1);                // Bits per pixel
        o.WriteInt32(0);                // Compression
        o.WriteInt32(Buffer.Length);          // Compressed Image size
        o.WriteInt32(0);                // X pixels per meter
        o.WriteInt32(0);                // Y pixels per meter
        o.WriteInt32(2);                // Colors in color table
        o.WriteInt32(2);                // Important color count

        // Color table
        o.WriteInt32(0x000000);
        o.WriteInt32(0xffffff);

        // Assert the calculated header length is as expected
        oLen = o.Length - oLen;
        if (oLen != 62) throw new InvalidOperationException("Unexpected header length " + oLen);

        // Image data
        o.Write(Buffer);
    }
}

public class Brushes
{
    public const Brush White = Brush.White;
    public const Brush Black = Brush.Black;
}

public enum Brush 
{
    White,
    Black
}

internal struct BWColor
{
    public bool IsWhite;

    public static implicit operator bool(BWColor color) => color.IsWhite;
    public static implicit operator BWColor(bool color) => new BWColor { IsWhite = color };

    public static implicit operator Color(BWColor color) => color.IsWhite ? Color.White : Color.Black;
    public static implicit operator BWColor(Color color) => color == Color.White ? true : false;

    public static implicit operator Brush(BWColor color) => color.IsWhite ? Brush.White : Brush.Black;
    public static implicit operator BWColor(Brush brush) => brush == Brush.White ? true : false;

    public static implicit operator SolidBrush(BWColor color) => new SolidBrush(color);
    public static implicit operator BWColor(SolidBrush brush) => brush.Color;

    public override string ToString() => IsWhite ? "White" : "Black";

    public override int GetHashCode() => IsWhite.GetHashCode();
    public override bool Equals(object obj) => obj is BWColor other && other.IsWhite == IsWhite;

    public static bool operator ==(BWColor left, BWColor right) => left.IsWhite == right.IsWhite;
    public static bool operator !=(BWColor left, BWColor right) => left.IsWhite != right.IsWhite;
}

public struct SolidBrush
{
    public SolidBrush(Color color)
    {
        Color = color;
    }

    public Color Color { get; internal set; }
}

public enum RotateFlipType
{
    Rotate90FlipNone
}