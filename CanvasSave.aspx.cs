using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Web.Script.Services;
using System.Web.Services;
using System.Drawing;
using Newtonsoft.Json;
using System.Data;

/*
public class SampleData
{
    [JsonProperty("sampleData")]
    public List<sample> sample { get; set; }
}
*/

public class sample
{
    [JsonProperty("no")]
    public int no { get; set; }

    [JsonProperty("x")]
    public int x { get; set; }

    [JsonProperty("y")]
    public int y { get; set; }

    [JsonProperty("w")]
    public int w { get; set; }

    [JsonProperty("h")]
    public int h { get; set; }
}

class CroppedImage
{
    public int no;
    public string image;
}


class PDFInfo
{
    public int total;
    public int index;
}



[ScriptService]
public partial class CanvasSave : System.Web.UI.Page
{

    // static string path = @"D:\My Documents\Visual Studio 2012\WebSites\AllTest\Uploads\";
    static string path = @"D:\My Documents\Visual Studio 2012\WebSites\AllTest\Uploads\";
    
    protected void Page_Load(object sender, EventArgs e)
    {

    }


    [WebMethod()]
    public static void UploadImage(string imageData)
    {

        string fileNameWitPath = path + DateTime.Now.ToString().Replace("/", "-").Replace(" ", "- ").Replace(":", "") + ".png";

        using (FileStream fs = new FileStream(fileNameWitPath, FileMode.Create))
        {

            using (BinaryWriter bw = new BinaryWriter(fs))
            {

                byte[] data = Convert.FromBase64String(imageData);

                bw.Write(data);

                bw.Close();
            }

        }
    }

    [WebMethod()]
    public static string saveSample(string sampleData, string imageData)
    {
        // string path = @"D:\My Documents\Visual Studio 2012\WebSites\AllTest\200206mockh3e_1307221909015679_1511051147042637.jpg";
        // string path = @"D:\Documents\Visual Studio 2012\WebSites\AllTest\200206mockh3e_1307221909015679_1511051147042637.jpg";

        List<CroppedImage> list = new List<CroppedImage>();

        var data = JsonConvert.DeserializeObject<List<sample>>(sampleData);

        byte[] bitmapData = Convert.FromBase64String(imageData);
        System.IO.MemoryStream streamBitmap = new System.IO.MemoryStream(bitmapData);

        foreach (sample i in data)
        {
            var no = i.no;
            var x = i.x;
            var y = i.y;
            var w = i.w + 5;
            var h = i.h;

            using (Bitmap bitImage = new Bitmap((Bitmap)System.Drawing.Image.FromStream(streamBitmap)))
            {
                using (Bitmap newimage = new Bitmap((int)w, (int)h))
                {
                    using (Graphics g = Graphics.FromImage(newimage))
                    {
                        g.DrawImage(bitImage, 0, 0, new Rectangle(x, y, w, h), GraphicsUnit.Pixel);
                        // rectangle 추가
                        // g.FillRectangle(new System.Drawing.SolidBrush(System.Drawing.Color.Yellow), new Rectangle(0, 0, 40, 20));
                        // 글자 추가
                        // g.DrawString((data.IndexOf(i)+1).ToString(), new Font("Tahoma", 11), Brushes.Black, new PointF(10, 0));
                        g.Flush();
                    }

                    // newimage.Save(new System.IO.FileInfo(path).DirectoryName + "_" + data.IndexOf(i).ToString() + "out.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);


                    // Bitmap bImage = newImage;  //Your Bitmap Image
                    System.IO.MemoryStream ms = new System.IO.MemoryStream();
                    newimage.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    byte[] byteImage = ms.ToArray();
                    var SigBase64 = Convert.ToBase64String(byteImage); //Get Base64

                    list.Add(new CroppedImage()
                    {
                        no = no,
                        image = SigBase64
                    });
                }
            }

        }
        
        return JsonConvert.SerializeObject(list);
    }

    [WebMethod()]
    public static string getPageFromPDF(string pdfData, int index)
    {
        // Convert PDF 1st page to PNG file.
        SautinSoft.PdfFocus f = new SautinSoft.PdfFocus();

        // this property is necessary only for registered version.
        //f.Serial = "XXXXXXXXXXX";

        // pdfFileUrl = @"D:\Documents\Visual Studio 2012\WebSites\AllTest\Uploads\200206mockh3e_1307221909015679_1511051147042637.pdf";

        string imagePath = Path.ChangeExtension(pdfData, ".jpg");

        byte[] sPDFDecoded = Convert.FromBase64String(pdfData);

        // saving pdf
        // File.WriteAllBytes(@"D:\Documents\Visual Studio 2012\WebSites\AllTest\Uploads\pdf8.pdf", sPDFDecoded);

        // open pdfFileUrl
        // f.OpenPdf(pdfFileUrl);

        f.OpenPdf(sPDFDecoded);

        return JsonConvert.SerializeObject(new PDFInfo(){total = f.PageCount, index = index});

    }


//*
    [WebMethod()]
    public static string getImageFromPDF(string pdfData, int[] pages)
    {

        List<CroppedImage> list = new List<CroppedImage>();

        // var data = JsonConvert.DeserializeObject<List<int>>(pages);

        SautinSoft.PdfFocus f = new SautinSoft.PdfFocus();

        byte[] sPDFDecoded = Convert.FromBase64String(pdfData);

        f.OpenPdf(sPDFDecoded);

        if (f.PageCount > 0)
        {
            //save 1st page to png file, 120 dpi
            f.ImageOptions.ImageFormat = System.Drawing.Imaging.ImageFormat.Jpeg;
            f.ImageOptions.Dpi = 100;

            foreach (int i in pages)
            {

                Bitmap source_image = (Bitmap)f.ToDrawingImage(i);

                int x = 0;
                int y = 92;
                int w = source_image.Width;
                int h = source_image.Height;


                // 상단 라이센스 부분 처리
                using (Bitmap target_image = new Bitmap(w, h))
                {
                    using (Graphics g = Graphics.FromImage(target_image))
                    {
                        g.DrawImage(source_image, 0, 0, new Rectangle(x, y, w, h), GraphicsUnit.Pixel);
                        // g.Flush();
                    }

                    // Bitmap bImage = newImage;  //Your Bitmap Image
                    System.IO.MemoryStream ms = new System.IO.MemoryStream();
                    target_image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    byte[] byteImage = ms.ToArray();
                    var SigBase64 = Convert.ToBase64String(byteImage); //Get Base64

                    list.Add(new CroppedImage()
                    {
                        no = i,
                        image = SigBase64
                    });
                    
                }
            }

        }

        return JsonConvert.SerializeObject(list);

    }
//*/

    
/*
    [WebMethod()]
    public static string getImageFromPDF(string pdfData, int[] pages)
    {

        List<CroppedImage> list = new List<CroppedImage>();

        // var data = JsonConvert.DeserializeObject<List<int>>(pages);
        
        SautinSoft.PdfFocus f = new SautinSoft.PdfFocus();

        byte[] sPDFDecoded = Convert.FromBase64String(pdfData);

        f.OpenPdf(sPDFDecoded);

        if (f.PageCount > 0)
        {
            //save 1st page to png file, 120 dpi
            f.ImageOptions.ImageFormat = System.Drawing.Imaging.ImageFormat.Jpeg;
            f.ImageOptions.Dpi = 120;


            foreach (int i in pages)
            {

                System.Drawing.Bitmap newimage = (System.Drawing.Bitmap)f.ToDrawingImage(i);

                // Bitmap bImage = newImage;  //Your Bitmap Image
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                newimage.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                byte[] byteImage = ms.ToArray();
                var SigBase64 = Convert.ToBase64String(byteImage); //Get Base64

                list.Add(new CroppedImage()
                {
                    no = i,
                    image = SigBase64
                });
            }

        }

        return JsonConvert.SerializeObject(list);

    }
//*/ 

}