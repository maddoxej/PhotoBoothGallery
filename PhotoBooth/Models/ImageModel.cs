using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhotoBooth.Models
{
    public class ImageModel
    {
        public string Name { get; set; }
        public string Path { get; set; }
    }

    public class ImageColumn
    {
        public IEnumerable<ImageModel> Column;        
    }
}