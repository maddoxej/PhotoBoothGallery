using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PhotoBooth.Models;
using ExifLib;
using System.Threading;

namespace PhotoBooth.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(int id = 0)
        {
            return View(this.GetGallery(page:id));
        }

        public ActionResult Print()
        {
            return View(this.GetGallery());
        }


        public IEnumerable<ImageColumn> GetGallery(int columns = 3, int page = 0, int rows = 4)
        {
            for (int column = 0; column < columns; column++)
            {
                yield return new ImageColumn() { Column = this.GetImages(page + column, rows) };
            }
        }


        /// <summary>
        /// Collects images from gallery 
        /// </summary>
        /// <param name="page">Page to get</param>
        /// <param name="rows">Number of images to retrieve</param>
        /// <returns>Collection of image details</returns>
        public IEnumerable<ImageModel> GetImages(int page = 0, int rows = 4)
        {

            IEnumerable<FileInfo> topFiles = this.GetFiles(page, rows);
            if (topFiles == null)
            {
                return null;
            }

            var images = from f in topFiles
                            select new ImageModel
                            {
                                Name = f.Name,
                                Path = "/images/" + f.Name
                            };

            return images;                 
        }

        /// <summary>
        /// Get a page of the latest images
        /// </summary>
        /// <param name="page">What page we are on, 0 for the first page</param>
        /// <param name="rows">Number of items per page</param>
        /// <returns>An array of the most recent image files. Newest one last.</returns>
        private IEnumerable<FileInfo> GetFiles(int page, int rows)
        {
            var dir = new DirectoryInfo(Server.MapPath("~/Images/"));
            if (dir.Exists)
            {
                var filters = new String[] { ".jpg", ".jpeg", ".png", ".gif", ".tiff", ".bmp" };
                FileInfo[] files = dir.EnumerateFiles().Where(i => filters.Contains(i.Extension.ToLower())).ToArray();

                // continue to return the cached result until a new page of files has been added.
                FileInfo[] cachedFiles = HttpContext.Cache["HomeGetFiles"] as FileInfo[];
                if (cachedFiles != null && (cachedFiles.Count() + rows > files.Count()))
                {
                    files = cachedFiles;
                }
                else
                {
                    Array.Sort<FileInfo>(files, HomeController.exifDateCompare);
                    HttpContext.Cache["HomeGetFiles"] = files;
                }

                // get newest files
                return files.Skip(page * rows).Take(rows).Reverse();
            }
            else
            {
                return null;
            }
        }


        private static int exifDateCompare(FileInfo f1, FileInfo f2)
        {
            return DateTime.Compare(HomeController.exifDate(f2.FullName), HomeController.exifDate(f1.FullName));
        }

        private static DateTime exifDate(string filename)
        {
            try
            {
                using (ExifReader reader = new ExifReader(filename))
                {
                    DateTime datePictureTaken;
                    if (reader.GetTagValue<DateTime>(ExifTags.DateTimeOriginal,
                                        out datePictureTaken))
                    {
                        return datePictureTaken;
                    }
                    else
                    {
                        return DateTime.MinValue;
                    }
                }
            }
            catch (Exception)
            {
                return DateTime.MinValue.AddDays(1);
            }
        }
    }
}