using Zen.Barcode;

var barcode = BarcodeDrawFactory.Code128WithChecksum.Draw("123456", 20, 1);
using (var fs = File.Create("out.bmp"))
{
    barcode.WriteBmp(fs);
}